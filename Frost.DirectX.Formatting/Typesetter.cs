// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Frost.Formatting;

using DxFontMetrics = SharpDX.DirectWrite.FontMetrics;

namespace Frost.DirectX.Formatting
{
	internal sealed class Typesetter : ILineProvider
	{
		private static readonly Comparison<Segment> mSegmentComparison;

		private readonly Stack<BreakIndex> mClusterBreaks;

		private readonly List<Rectangle> mComputedLines;
		private readonly List<Segment> mFreeSegments;
		private readonly LineBreaker mLineBreaker;
		private readonly List<Rectangle> mObstructions;

		private readonly TypesetterSink mOutputSink;
		private ShaperSink mInputSink;

		private Rectangle mLayoutRegion;

		private double mLeading;
		private double mLineHeight;
		private double mLineOffset;

		static Typesetter()
		{
			mSegmentComparison = (c1, c2) => c1.Position.CompareTo(c2.Position);
		}

		public Typesetter(TypesetterSink outputSink)
		{
			Contract.Requires(outputSink != null);

			mOutputSink = outputSink;

			mLineBreaker = new LineBreaker(this);

			mFreeSegments = new List<Segment>();
			mComputedLines = new List<Rectangle>();
			mObstructions = new List<Rectangle>();
			mClusterBreaks = new Stack<BreakIndex>();

			mLineBreaker.ExaminingItem += ProcessFloaters;
		}

		double ILineProvider.ProduceLine(int lineIndex)
		{
			return GetLineRegion(lineIndex).Width;
		}

		public void Break<T>(ShaperSink input, Paragraph paragraph, Rectangle region, T boxes)
			where T : class, IEnumerable<Rectangle>
		{
			Contract.Requires(input != null);
			Contract.Requires(paragraph != null);
			Contract.Requires(region.X >= double.MinValue && region.X <= double.MaxValue);
			Contract.Requires(region.Y >= double.MinValue && region.Y <= double.MaxValue);
			Contract.Requires(region.Width >= 0.0 && region.Width <= double.MaxValue);
			Contract.Requires(region.Height >= 0.0 && region.Height <= double.MaxValue);

			Contract.Ensures(mOutputSink.Glyphs.Count == input.Glyphs.Count);
			Contract.Ensures(mOutputSink.Clusters.Count == input.Clusters.Count);

			mInputSink = input;
			mLayoutRegion = region;

			mOutputSink.Reset(input.FullText);

			mOutputSink.PreallocateClusters(input.Clusters.Count);
			mOutputSink.PreallocateGlyphs(input.Glyphs.Count);

			mOutputSink.LayoutRegion = region;
			mOutputSink.Alignment = paragraph.Alignment;

			FindLineHeight(input, paragraph.Leading);

			mOutputSink.Leading = mLeading;
			mOutputSink.LineHeight = mLineHeight;

			// determine the amount of indentation in pixels
			double indentation = Math.Min(mLayoutRegion.Width / 2.0, paragraph.Indentation * mLineHeight);

			mOutputSink.Indentation = indentation;

			AnalyzeItems(input, paragraph.Alignment, paragraph.Spacing, paragraph.Tracking, indentation);

			DetermineBreakingPoints(boxes);

			SaveBreakingPoints();

			OutputClusters(input, paragraph.Alignment);

			mOutputSink.Lines.AddRange(mComputedLines);
		}

		private void ProcessFloaters(ref LineItem item)
		{
			if(item.Position >= 0)
			{
				ShapedCluster cluster = mInputSink.Clusters[item.Position];

				if(cluster.ContentType == ContentType.Floater)
				{
					Rectangle floaterRegion = Rectangle.Empty;

					double occupiedHeight = Math.Ceiling(cluster.Floater.Height / (mLineHeight + mLeading)) *
					                        (mLineHeight + mLeading);

					switch(cluster.VAlignment)
					{
						case Alignment.Stretch:
							floaterRegion.Width = cluster.Floater.Width + (occupiedHeight - cluster.Floater.Height);
							floaterRegion.Height = occupiedHeight;
							floaterRegion.Y = mLayoutRegion.Top + mLineOffset;
							break;
						case Alignment.Trailing:
							floaterRegion.Width = cluster.Floater.Width;
							floaterRegion.Height = cluster.Floater.Height;
							floaterRegion.Y = mLayoutRegion.Top + mLineOffset;
							break;
						case Alignment.Center:
							floaterRegion.Width = cluster.Floater.Width;
							floaterRegion.Height = cluster.Floater.Height;
							floaterRegion.Y = ((mLayoutRegion.Top + mLineOffset) + (occupiedHeight / 2.0)) -
							                  (floaterRegion.Height / 2.0);
							break;
						case Alignment.Leading:
							floaterRegion.Width = cluster.Floater.Width;
							floaterRegion.Height = cluster.Floater.Height;
							floaterRegion.Y = (mLayoutRegion.Top + mLineOffset + occupiedHeight) - floaterRegion.Height;
							break;
					}

					switch(cluster.HAlignment)
					{
						case Alignment.Stretch:
							throw new InvalidOperationException();
						case Alignment.Leading:
							if(cluster.BidiLevel % 2 == 0)
							{
								floaterRegion.X = mLayoutRegion.Right - floaterRegion.Width;
							}
							else
							{
								floaterRegion.X = mLayoutRegion.Left;
							}
							break;
						case Alignment.Center:
							floaterRegion.X = (mLayoutRegion.X + (mLayoutRegion.Width / 2.0)) -
							                  (floaterRegion.Width / 2.0);
							break;
						case Alignment.Trailing:
							if(cluster.BidiLevel % 2 == 0)
							{
								floaterRegion.X = mLayoutRegion.Left;
							}
							else
							{
								floaterRegion.X = mLayoutRegion.Right - floaterRegion.Width;
							}
							break;
					}

					cluster.Floater = floaterRegion;

					mInputSink.Clusters[item.Position] = cluster;

					mObstructions.Add(floaterRegion);
				}
			}
		}

		private void FindLineHeight(ShaperSink input, double leadingEm)
		{
			Contract.Requires(input != null);
			Contract.Requires(leadingEm >= 0.0 && leadingEm <= double.MaxValue);

			ResetLinesState();

			double gap = 0.0;

			mLineHeight = 0.0;
			mLineOffset = 0.0;

			foreach(ShapedCluster cluster in input.Clusters)
			{
				if(cluster.ContentType != ContentType.Floater)
				{
					DxFontMetrics metrics = cluster.Font.ResolveFace().Metrics;

					double units = metrics.DesignUnitsPerEm;

					// find the maximum line gap in the paragraph
					gap = Math.Max(gap, FontMetrics.ToPixels(metrics.LineGap, cluster.PointSize, units));

					double height = FontMetrics.ToPixels(metrics.Descent, cluster.PointSize, units);

					// find the maximum line height in the paragraph
					mLineHeight = Math.Max(mLineHeight, cluster.Advance.Height + height);
				}
			}

			// convert from EMs to pixels, cap value to a minimum of the gap
			mLeading = (mLineHeight * leadingEm) + gap;
		}

		private Rectangle GetLineRegion(int lineIndex)
		{
			Contract.Requires(lineIndex >= 0);

			// a line has already been computed for this index
			if(lineIndex < mComputedLines.Count)
			{
				return mComputedLines[lineIndex];
			}

			Rectangle lineRegion = Rectangle.Empty;

			lineRegion.X = mLayoutRegion.Left;
			lineRegion.Y = mLayoutRegion.Top + mLineOffset;
			lineRegion.Width = mLayoutRegion.Width;
			lineRegion.Height = mLineHeight;

			IdentifyFreeSegments(lineRegion);

			// advance over the line and its leading to next line position
			mLineOffset += mLineHeight + mLeading;

			return GetLineRegion(lineIndex);
		}

		private void ProcessFreeSegment(ref int index, Rectangle box)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(box.X >= double.MinValue && box.X <= double.MaxValue);
			Contract.Requires(box.Y >= double.MinValue && box.Y <= double.MaxValue);
			Contract.Requires(box.Width >= 0.0 && box.Width <= double.MaxValue);
			Contract.Requires(box.Height >= 0.0 && box.Height <= double.MaxValue);

			Segment segment = mFreeSegments[index];

			// the segment entirely contains the box
			if(segment.Contains(box.Left) && segment.Contains(box.Right))
			{
				Segment left;

				left.Position = segment.Position;
				left.Length = box.Left - segment.Position;

				Debug.Assert(left.Length >= 0.0);

				Segment right;

				right.Position = box.Right;
				right.Length = (segment.Position + segment.Length) - box.Right;

				Debug.Assert(right.Length >= 0.0);

				mFreeSegments.RemoveAt(index--);

				mFreeSegments.Add(left);
				mFreeSegments.Add(right);

				Debug.Assert(index >= -1);
			}

				// the segment contains only the left portion of the box
			else if(segment.Contains(box.Left))
			{
				segment.Length -= (segment.Position + segment.Length) - box.Left;

				Debug.Assert(segment.Length >= 0.0);

				mFreeSegments[index] = segment;
			}

				// the segment contains only the right portion of the box
			else if(segment.Contains(box.Right))
			{
				segment.Length -= box.Right - segment.Position;

				Debug.Assert(segment.Length >= 0.0);

				segment.Position = box.Right;

				mFreeSegments[index] = segment;
			}
		}

		private void IdentifyFreeSegments(Rectangle lineRegion)
		{
			Contract.Requires(lineRegion.X >= double.MinValue && lineRegion.X <= double.MaxValue);
			Contract.Requires(lineRegion.Y >= double.MinValue && lineRegion.Y <= double.MaxValue);
			Contract.Requires(lineRegion.Width >= 0.0 && lineRegion.Width <= double.MaxValue);
			Contract.Requires(lineRegion.Height >= 0.0 && lineRegion.Height <= double.MaxValue);

			mFreeSegments.Clear();

			Segment fullLine;

			fullLine.Length = lineRegion.Width;
			fullLine.Position = lineRegion.Left;

			mFreeSegments.Add(fullLine);

			// remove space for each box that intersects the line region
			foreach(Rectangle obstruction in mObstructions)
			{
				if(obstruction.Top < lineRegion.Bottom)
				{
					if(obstruction.Bottom > lineRegion.Top)
					{
						if(!obstruction.IsEmpty)
						{
							for(int i = 0; i < mFreeSegments.Count; ++i)
							{
								ProcessFreeSegment(ref i, obstruction);
							}
						}
					}
				}
			}

			// sort free segments to order left to right
			mFreeSegments.Sort(mSegmentComparison);

			foreach(Segment segment in mFreeSegments)
			{
				Rectangle newRegion = Rectangle.Empty;

				newRegion.X = segment.Position;
				newRegion.Y = lineRegion.Y;
				newRegion.Width = segment.Length;
				newRegion.Height = lineRegion.Height;

				// reject lines that occupy less space than the line height
				if(newRegion.Width > mLineHeight)
				{
					mComputedLines.Add(newRegion);
				}
			}
		}

		private void ResetLinesState()
		{
			mLineOffset = 0.0;

			mObstructions.Clear();
			mComputedLines.Clear();
		}

		private void AddObstructions<T>(T boxes) where T : class, IEnumerable<Rectangle>
		{
			if(boxes != null)
			{
				foreach(Rectangle box in boxes)
				{
					Contract.Assert(box.X >= double.MinValue && box.X <= double.MaxValue);
					Contract.Assert(box.Y >= double.MinValue && box.Y <= double.MaxValue);
					Contract.Assert(box.Width >= 0.0 && box.Width <= double.MaxValue);
					Contract.Assert(box.Height >= 0.0 && box.Height <= double.MaxValue);

					mObstructions.Add(box);
				}
			}
		}

		private void DetermineBreakingPoints<T>(T boxes) where T : class, IEnumerable<Rectangle>
		{
			AddObstructions(boxes);

			// five attempts to find a feasible set of breaking points
			for(int i = 1; i <= 5; ++i)
			{
				if(mLineBreaker.FindBreakpoints(((i - 1) * 2) + 1))
				{
					break;
				}

				ResetLinesState();

				AddObstructions(boxes);

				if(i == 5)
				{
					// give up by forcing the text to set
					mLineBreaker.FindBreakpoints(20, true);
				}
			}
		}

		private void SaveBreakingPoints()
		{
			mClusterBreaks.Clear();

			// save the breaking points in reverse iteration order
			for(int i = mLineBreaker.Breakpoints.Count - 1; i >= 1; --i)
			{
				mClusterBreaks.Push(mLineBreaker.Breakpoints[i]);
			}
		}

		private static bool IsBrokenBefore(
			int index, List<ShapedCluster> clusters, ref bool isForced, bool isAlone = true)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(clusters != null);

			ShapedCluster cluster;

			if(clusters.TryCurrentOrDefault(index, out cluster))
			{
				switch(cluster.Breakpoint.BreakConditionBefore)
				{
					case BreakCondition.MustBreak:
						isForced = true;
						return true;
					case BreakCondition.CanBreak:
						// check the neighboring break condition
						return !isAlone || IsBrokenAfter(index - 1, clusters, ref isForced, false);
					case BreakCondition.Neutral:
						// check the neighboring break condition
						return !isAlone || IsBrokenAfter(index + 1, clusters, ref isForced, false);
				}
			}

			return false;
		}

		private static bool IsBrokenAfter(
			int index, List<ShapedCluster> clusters, ref bool isForced, bool isAlone = true)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(clusters != null);

			ShapedCluster cluster;

			if(clusters.TryCurrentOrDefault(index, out cluster))
			{
				switch(cluster.Breakpoint.BreakConditionAfter)
				{
					case BreakCondition.MustBreak:
						isForced = true;
						return true;
					case BreakCondition.CanBreak:
						// check the neighboring break condition
						return !isAlone || IsBrokenBefore(index + 1, clusters, ref isForced, false);
					case BreakCondition.Neutral:
						// check the neighboring break condition
						return !isAlone || IsBrokenBefore(index + 1, clusters, ref isForced, false);
				}
			}

			return false;
		}

		private void AddSoftHyphen(int index, double advance, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(advance >= 0.0 && advance <= double.MaxValue);

			if(isRagged)
			{
				mLineBreaker.AddPenalty(0.0, 0.0, 0.0);
				mLineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
				mLineBreaker.AddPenalty(advance, index, 5.0, 1.0);
				mLineBreaker.AddGlue(0.0, -LineItem.Infinity, 0.0);
			}
			else
			{
				mLineBreaker.AddPenalty(advance, index, 5.0, 1.0);
			}
		}

		private void AddForcedBreak(int index, bool isRagged)
		{
			Contract.Requires(index >= 0);

			if(isRagged)
			{
				mLineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
				mLineBreaker.AddPenalty(0.0, index, -LineItem.Infinity);
			}
			else
			{
				mLineBreaker.AddPenalty(0.0, index, -LineItem.Infinity);
			}
		}

		private void AddVariableSpace(int index, double length, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(length >= 0.0 && length <= double.MaxValue);

			if(isRagged)
			{
				mLineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
				mLineBreaker.AddPenalty(0.0, 0.0);
				mLineBreaker.AddGlue(length, index, -LineItem.Infinity, 0.0);
			}
			else
			{
				mLineBreaker.AddGlue(length, index, length / 2.0, length / 3.0);
			}
		}

		private void AddWhitespace(int index, double length, ShaperSink input, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(length >= 0.0 && length <= double.MaxValue);
			Contract.Requires(input != null);

			bool isForced = false;

			if(IsBrokenAfter(index, input.Clusters, ref isForced))
			{
				if(isForced)
				{
					AddForcedBreak(index, isRagged);
				}
				else
				{
					AddVariableSpace(index, length, isRagged);
				}
			}
			else
			{
				ShapedCluster cluster = input.Clusters[index];

				mLineBreaker.AddBox(ComputeAdvance(ref cluster), index);
			}
		}

		private void AddCluster(int index, double advance, ShaperSink input, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(advance >= 0.0 && advance <= double.MaxValue);
			Contract.Requires(input != null);

			mLineBreaker.AddBox(advance, index);

			bool isForced = false;

			if(IsBrokenAfter(index, input.Clusters, ref isForced))
			{
				if(isForced)
				{
					AddForcedBreak(-1, isRagged);
				}
				else
				{
					if(isRagged)
					{
						mLineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
						mLineBreaker.AddPenalty(0.0, 0.0);
					}
					else
					{
						mLineBreaker.AddPenalty(0.0, 0.0);
					}
				}
			}
		}

		private void OutputGlyphs(ShaperSink input)
		{
			Contract.Requires(input != null);

			foreach(ShapedGlyph glyph in input.Glyphs)
			{
				TypesetGlyph newGlyph;

				newGlyph.Advance = glyph.Advance;
				newGlyph.Index = glyph.Index;
				newGlyph.Offset = glyph.Offset;

				mOutputSink.Glyphs.Add(newGlyph);
			}
		}

		private void OutputClusters(ShaperSink input, Alignment alignment)
		{
			Contract.Requires(input != null);

			int lineNumber = 0;

			double lineLength = 0.0;

			bool isLineStart = true;

			for(int i = 0; i < mLineBreaker.Items.Count; ++i)
			{
				LineItem item = mLineBreaker.Items[i];

				// line ends are always at breaking points
				bool isLineEnd = false;

				if(mClusterBreaks.Count > 0)
				{
					isLineEnd = mClusterBreaks.Peek().Index == i;
				}

				if(item.Position >= 0)
				{
					TypesetCluster newCluster = new TypesetCluster();

					// the breaking point contains the spacing ratio for the line
					double ratio = 1.0;

					newCluster.LineNumber = lineNumber;

					if(mClusterBreaks.Count > 0)
					{
						ratio = mClusterBreaks.Peek().Ratio;
					}

					if(isLineEnd)
					{
						if(item.IsGlue)
						{
							// suppress whitespaces at the end of each line
							newCluster.Advance.Width = 0.0;

							newCluster.Display = DisplayMode.Suppressed;
						}
						else
						{
							// breaking on a penalty enables that cluster
							newCluster.Advance.Width = item.ComputeWidth(ratio, true);

							newCluster.Display = DisplayMode.Neutral;
						}
					}
					else if(isLineStart)
					{
						if(item.IsGlue)
						{
							// suppress whitespaces at the start of each line
							newCluster.Advance.Width = 0.0;

							newCluster.Display = DisplayMode.Suppressed;
						}
						else
						{
							// penalties cannot display at the start of the line
							newCluster.Advance.Width = item.ComputeWidth(ratio, false);

							newCluster.Display = DisplayMode.Neutral;
						}
					}
					else
					{
						if(alignment == Alignment.Stretch)
						{
							// spacing length within the line depends on the line ratio
							newCluster.Advance.Width = item.ComputeWidth(ratio, false);

							newCluster.Display = DisplayMode.Neutral;
						}
						else
						{
							if(item.IsPenalty)
							{
								// suppress penalties within the line
								newCluster.Advance.Width = 0.0;

								newCluster.Display = DisplayMode.Suppressed;
							}
							else
							{
								// other items retain their widths
								newCluster.Advance.Width = item.Width;

								newCluster.Display = DisplayMode.Neutral;
							}
						}
					}

					ShapedCluster cluster = input.Clusters[item.Position];

					newCluster.Characters = cluster.Characters;
					newCluster.Glyphs = cluster.Glyphs;
					newCluster.ContentType = cluster.ContentType;
					newCluster.Font = cluster.Font;
					newCluster.Advance.Height = cluster.Advance.Height;
					newCluster.BidiLevel = cluster.BidiLevel;
					newCluster.PointSize = cluster.PointSize;

					switch(cluster.ContentType)
					{
						case ContentType.Floater:
							newCluster.Floater = cluster.Floater;
							newCluster.Display = DisplayMode.Suppressed;
							break;
						case ContentType.Inline:
							newCluster.Display = DisplayMode.Neutral;
							break;
					}

					// track the actual length of the line
					lineLength += newCluster.Advance.Width;

					mOutputSink.Clusters.Add(newCluster);
				}

				if(isLineEnd)
				{
					isLineStart = true;

					// move to the next line
					lineNumber++;

					// save the actual length of the line
					mOutputSink.LineLengths.Add(lineLength);

					lineLength = 0.0;

					// remove the used breakpoint
					mClusterBreaks.Pop();
				}
				else
				{
					isLineStart = false;
				}
			}

			// save the actual length of the line
			mOutputSink.LineLengths.Add(lineLength);
		}

		private void AnalyzeItems(
			ShaperSink input, Alignment alignment, double spacingEm, double trackingEm, double indentation)
		{
			Contract.Requires(input != null);
			Contract.Requires(spacingEm >= 0.0 && spacingEm <= double.MaxValue);
			Contract.Requires(trackingEm >= 0.0 && trackingEm <= double.MaxValue);
			Contract.Requires(indentation >= 0.0 && indentation <= double.MaxValue);

			OutputGlyphs(input);

			mLineBreaker.BeginParagraph();

			mLineBreaker.AddBox(indentation);

			for(int i = 0; i < input.Clusters.Count; ++i)
			{
				bool isTrackingSuppressed = false;

				ShapedCluster nextCluster;

				if(input.Clusters.TryGetNextItem(i, out nextCluster))
				{
					if(nextCluster.Breakpoint.IsWhitespace)
					{
						// suppress letter spacing if the next cluster is whitespace
						isTrackingSuppressed = true;
					}
				}

				ShapedCluster cluster = input.Clusters[i];

				// suppress letter spacing for floaters
				if(cluster.ContentType == ContentType.Floater)
				{
					isTrackingSuppressed = true;
				}

				// consider all alignments other than 'stretch' as ragged
				bool isRagged = alignment != Alignment.Stretch;

				ShapedCluster previousCluster;

				if(input.Clusters.TryGetPreviousItem(i, out previousCluster))
				{
					if(previousCluster.ContentType == ContentType.Floater)
					{
						if(cluster.Breakpoint.IsWhitespace)
						{
							mLineBreaker.AddBox(0.0, i);

							continue;
						}
					}
				}

				if(cluster.Breakpoint.IsSoftHyphen)
				{
					double advance = ComputeAdvance(ref cluster);

					AddSoftHyphen(i, advance, isRagged);
				}
				else if(cluster.Breakpoint.IsWhitespace)
				{
					double spacing = ComputeSpacing(spacingEm, ref cluster);

					spacing += ComputeTracking(trackingEm, ref cluster);

					AddWhitespace(i, spacing, input, isRagged);
				}
				else
				{
					if(cluster.ContentType != ContentType.Format && cluster.ContentType != ContentType.Floater)
					{
						double advance = ComputeAdvance(ref cluster);

						if(!isTrackingSuppressed)
						{
							advance += ComputeTracking(trackingEm, ref cluster);
						}

						if(cluster.ContentType == ContentType.Inline)
						{
							advance = ComputeInline(advance);
						}

						AddCluster(i, advance, input, isRagged);
					}
					else
					{
						mLineBreaker.AddBox(0.0, i);
					}
				}
			}

			// add the final items required by the line breaking algorithm
			mLineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
			mLineBreaker.AddPenalty(0.0, -LineItem.Infinity, 1.0);

			mLineBreaker.EndParagraph();
		}

		private static double ComputeAdvance(ref ShapedCluster cluster)
		{
			return cluster.Advance.Width;
		}

		private double ComputeSpacing(double spacingEm, ref ShapedCluster cluster)
		{
			double spacing = cluster.Advance.Height * spacingEm;

			// cap spacing at a quarter of the layout length
			return Math.Min(spacing, mLayoutRegion.Width / 4.0);
		}

		private double ComputeTracking(double trackingEm, ref ShapedCluster cluster)
		{
			double tracking = trackingEm * cluster.Advance.Height;

			// cap tracking at a quarter of the layout length
			return Math.Min(mLayoutRegion.Width / 4.0, tracking);
		}

		private double ComputeInline(double inlineLength)
		{
			// cap inlines at half the layout length
			return Math.Min(mLayoutRegion.Width / 2.0, inlineLength);
		}

		private struct Segment : IEquatable<Segment>
		{
			public double Length;
			public double Position;

			public bool Equals(Segment other)
			{
				return other.Length.Equals(Length) && other.Position.Equals(Position);
			}

			public bool Contains(double position)
			{
				return position >= Position && position <= Position + Length;
			}

			public override bool Equals(object obj)
			{
				if(ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is Segment && Equals((Segment)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (Length.GetHashCode() * 397) ^ Position.GetHashCode();
				}
			}

			public static bool operator ==(Segment left, Segment right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Segment left, Segment right)
			{
				return !left.Equals(right);
			}
		}
	}
}