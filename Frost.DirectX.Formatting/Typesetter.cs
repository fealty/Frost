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
		private static readonly Comparison<Segment> _SegmentComparison;

		private readonly Stack<BreakIndex> _ClusterBreaks;

		private readonly List<Rectangle> _ComputedLines;
		private readonly List<Segment> _FreeSegments;
		private readonly LineBreaker _LineBreaker;
		private readonly List<Rectangle> _Obstructions;

		private readonly TypesetterSink _OutputSink;
		private ShaperSink _InputSink;

		private Rectangle _LayoutRegion;

		private float _Leading;
		private float _LineHeight;
		private float _LineOffset;

		static Typesetter()
		{
			_SegmentComparison = (c1, c2) => c1.Position.CompareTo(c2.Position);
		}

		public Typesetter(TypesetterSink outputSink)
		{
			Contract.Requires(outputSink != null);

			_OutputSink = outputSink;

			_LineBreaker = new LineBreaker(this);

			_FreeSegments = new List<Segment>();
			_ComputedLines = new List<Rectangle>();
			_Obstructions = new List<Rectangle>();
			_ClusterBreaks = new Stack<BreakIndex>();

			_LineBreaker.ExaminingItem += ProcessFloaters;
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

			Contract.Ensures(_OutputSink.Glyphs.Count == input.Glyphs.Count);
			Contract.Ensures(_OutputSink.Clusters.Count == input.Clusters.Count);

			_InputSink = input;
			_LayoutRegion = region;

			_OutputSink.Reset(input.FullText);

			_OutputSink.PreallocateClusters(input.Clusters.Count);
			_OutputSink.PreallocateGlyphs(input.Glyphs.Count);

			_OutputSink.LayoutRegion = region;
			_OutputSink.Alignment = paragraph.Alignment;

			FindLineHeight(input, paragraph.Leading);

			_OutputSink.Leading = _Leading;
			_OutputSink.LineHeight = _LineHeight;

			// determine the amount of indentation in pixels
			float indentation = Math.Min(_LayoutRegion.Width / 2.0f, paragraph.Indentation * _LineHeight);

			_OutputSink.Indentation = indentation;

			AnalyzeItems(input, paragraph.Alignment, paragraph.Spacing, paragraph.Tracking, indentation);

			DetermineBreakingPoints(boxes);

			SaveBreakingPoints();

			OutputClusters(input, paragraph.Alignment);

			_OutputSink.Lines.AddRange(_ComputedLines);
		}

		private void ProcessFloaters(ref LineItem item)
		{
			if(item.Position >= 0)
			{
				ShapedCluster cluster = _InputSink.Clusters[item.Position];

				if(cluster.ContentType == ContentType.Floater)
				{
					float floaterX = 0.0f;
					float floaterY = 0.0f;
					float floaterWidth = 0.0f;
					float floaterHeight = 0.0f;

					float occupiedHeight = (float)Math.Ceiling(cluster.Floater.Height / (_LineHeight + _Leading)) *
					                       (_LineHeight + _Leading);

					switch(cluster.VAlignment)
					{
						case Alignment.Stretch:
							floaterWidth = cluster.Floater.Width + (occupiedHeight - cluster.Floater.Height);
							floaterHeight = occupiedHeight;
							floaterY = _LayoutRegion.Top + _LineOffset;
							break;
						case Alignment.Trailing:
							floaterWidth = cluster.Floater.Width;
							floaterHeight = cluster.Floater.Height;
							floaterY = _LayoutRegion.Top + _LineOffset;
							break;
						case Alignment.Center:
							floaterWidth = cluster.Floater.Width;
							floaterHeight = cluster.Floater.Height;
							floaterY = ((_LayoutRegion.Top + _LineOffset) + (occupiedHeight / 2.0f)) -
							           (floaterHeight / 2.0f);
							break;
						case Alignment.Leading:
							floaterWidth = cluster.Floater.Width;
							floaterHeight = cluster.Floater.Height;
							floaterY = (_LayoutRegion.Top + _LineOffset + occupiedHeight) - floaterHeight;
							break;
					}

					switch(cluster.HAlignment)
					{
						case Alignment.Stretch:
							throw new InvalidOperationException();
						case Alignment.Leading:
							if(cluster.BidiLevel % 2 == 0)
							{
								floaterX = _LayoutRegion.Right - floaterWidth;
							}
							else
							{
								floaterX = _LayoutRegion.Left;
							}
							break;
						case Alignment.Center:
							floaterX = (_LayoutRegion.X + (_LayoutRegion.Width / 2.0f)) - (floaterWidth / 2.0f);
							break;
						case Alignment.Trailing:
							if(cluster.BidiLevel % 2 == 0)
							{
								floaterX = _LayoutRegion.Left;
							}
							else
							{
								floaterX = _LayoutRegion.Right - floaterWidth;
							}
							break;
					}

					cluster.Floater = new Rectangle(
						floaterX, floaterY, floaterX + floaterWidth, floaterY + floaterHeight);

					_InputSink.Clusters[item.Position] = cluster;

					_Obstructions.Add(cluster.Floater);
				}
			}
		}

		private void FindLineHeight(ShaperSink input, float leadingEm)
		{
			Contract.Requires(input != null);
			Contract.Requires(leadingEm >= 0.0 && leadingEm <= double.MaxValue);

			ResetLinesState();

			float gap = 0.0f;

			_LineHeight = 0.0f;
			_LineOffset = 0.0f;

			foreach(ShapedCluster cluster in input.Clusters)
			{
				if(cluster.ContentType != ContentType.Floater)
				{
					DxFontMetrics dxMetrics = cluster.Font.ResolveFace().Metrics;

					FontMetrics metrics = new FontMetrics(
						dxMetrics.Ascent, dxMetrics.Descent, dxMetrics.DesignUnitsPerEm);

					// find the maximum line gap in the paragraph
					gap = Math.Max(gap, metrics.Measure(dxMetrics.LineGap, cluster.PointSize));

					float height = metrics.Measure(dxMetrics.Descent, cluster.PointSize);

					// find the maximum line height in the paragraph
					_LineHeight = Math.Max(_LineHeight, cluster.Advance.Height + height);
				}
			}

			// convert from EMs to pixels, cap value to a minimum of the gap
			_Leading = (_LineHeight * leadingEm) + gap;
		}

		private Rectangle GetLineRegion(int lineIndex)
		{
			Contract.Requires(lineIndex >= 0);

			// a line has already been computed for this index
			if(lineIndex < _ComputedLines.Count)
			{
				return _ComputedLines[lineIndex];
			}

			Rectangle lineRegion =
				new Rectangle(
					new Point(_LayoutRegion.Left, _LayoutRegion.Top + _LineOffset),
					new Size(_LayoutRegion.Width, _LineHeight));

			IdentifyFreeSegments(lineRegion);

			// advance over the line and its leading to next line position
			_LineOffset += _LineHeight + _Leading;

			return GetLineRegion(lineIndex);
		}

		private void ProcessFreeSegment(ref int index, Rectangle box)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(box.X >= double.MinValue && box.X <= double.MaxValue);
			Contract.Requires(box.Y >= double.MinValue && box.Y <= double.MaxValue);
			Contract.Requires(box.Width >= 0.0 && box.Width <= double.MaxValue);
			Contract.Requires(box.Height >= 0.0 && box.Height <= double.MaxValue);

			Segment segment = _FreeSegments[index];

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

				_FreeSegments.RemoveAt(index--);

				_FreeSegments.Add(left);
				_FreeSegments.Add(right);

				Debug.Assert(index >= -1);
			}

				// the segment contains only the left portion of the box
			else if(segment.Contains(box.Left))
			{
				segment.Length -= (segment.Position + segment.Length) - box.Left;

				Debug.Assert(segment.Length >= 0.0);

				_FreeSegments[index] = segment;
			}

				// the segment contains only the right portion of the box
			else if(segment.Contains(box.Right))
			{
				segment.Length -= box.Right - segment.Position;

				Debug.Assert(segment.Length >= 0.0);

				segment.Position = box.Right;

				_FreeSegments[index] = segment;
			}
		}

		private void IdentifyFreeSegments(Rectangle lineRegion)
		{
			Contract.Requires(lineRegion.X >= double.MinValue && lineRegion.X <= double.MaxValue);
			Contract.Requires(lineRegion.Y >= double.MinValue && lineRegion.Y <= double.MaxValue);
			Contract.Requires(lineRegion.Width >= 0.0 && lineRegion.Width <= double.MaxValue);
			Contract.Requires(lineRegion.Height >= 0.0 && lineRegion.Height <= double.MaxValue);

			_FreeSegments.Clear();

			Segment fullLine;

			fullLine.Length = lineRegion.Width;
			fullLine.Position = lineRegion.Left;

			_FreeSegments.Add(fullLine);

			// remove space for each box that intersects the line region
			foreach(Rectangle obstruction in _Obstructions)
			{
				if(obstruction.Top < lineRegion.Bottom)
				{
					if(obstruction.Bottom > lineRegion.Top)
					{
						if(obstruction.Area > 0)
						{
							for(int i = 0; i < _FreeSegments.Count; ++i)
							{
								ProcessFreeSegment(ref i, obstruction);
							}
						}
					}
				}
			}

			// sort free segments to order left to right
			_FreeSegments.Sort(_SegmentComparison);

			foreach(Segment segment in _FreeSegments)
			{
				Rectangle newRegion = new Rectangle(
					new Point(segment.Position, lineRegion.Y), new Size(segment.Length, lineRegion.Height));

				// reject lines that occupy less space than the line height
				if(newRegion.Width > _LineHeight)
				{
					_ComputedLines.Add(newRegion);
				}
			}
		}

		private void ResetLinesState()
		{
			_LineOffset = 0.0f;

			_Obstructions.Clear();
			_ComputedLines.Clear();
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

					_Obstructions.Add(box);
				}
			}
		}

		private void DetermineBreakingPoints<T>(T boxes) where T : class, IEnumerable<Rectangle>
		{
			AddObstructions(boxes);

			// five attempts to find a feasible set of breaking points
			for(int i = 1; i <= 5; ++i)
			{
				if(_LineBreaker.FindBreakpoints(((i - 1) * 2) + 1))
				{
					break;
				}

				ResetLinesState();

				AddObstructions(boxes);

				if(i == 5)
				{
					// give up by forcing the text to set
					_LineBreaker.FindBreakpoints(20, true);
				}
			}
		}

		private void SaveBreakingPoints()
		{
			_ClusterBreaks.Clear();

			// save the breaking points in reverse iteration order
			for(int i = _LineBreaker.Breakpoints.Count - 1; i >= 1; --i)
			{
				_ClusterBreaks.Push(_LineBreaker.Breakpoints[i]);
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
				_LineBreaker.AddPenalty(0.0, 0.0, 0.0);
				_LineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
				_LineBreaker.AddPenalty(advance, index, 5.0, 1.0);
				_LineBreaker.AddGlue(0.0, -LineItem.Infinity, 0.0);
			}
			else
			{
				_LineBreaker.AddPenalty(advance, index, 5.0, 1.0);
			}
		}

		private void AddForcedBreak(int index, bool isRagged)
		{
			Contract.Requires(index >= 0);

			if(isRagged)
			{
				_LineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
				_LineBreaker.AddPenalty(0.0, index, -LineItem.Infinity);
			}
			else
			{
				_LineBreaker.AddPenalty(0.0, index, -LineItem.Infinity);
			}
		}

		private void AddVariableSpace(int index, double length, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(length >= 0.0 && length <= double.MaxValue);

			if(isRagged)
			{
				_LineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
				_LineBreaker.AddPenalty(0.0, 0.0);
				_LineBreaker.AddGlue(length, index, -LineItem.Infinity, 0.0);
			}
			else
			{
				_LineBreaker.AddGlue(length, index, length / 2.0, length / 3.0);
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

				_LineBreaker.AddBox(ComputeAdvance(ref cluster), index);
			}
		}

		private void AddCluster(int index, double advance, ShaperSink input, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(advance >= 0.0 && advance <= double.MaxValue);
			Contract.Requires(input != null);

			_LineBreaker.AddBox(advance, index);

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
						_LineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
						_LineBreaker.AddPenalty(0.0, 0.0);
					}
					else
					{
						_LineBreaker.AddPenalty(0.0, 0.0);
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

				_OutputSink.Glyphs.Add(newGlyph);
			}
		}

		private void OutputClusters(ShaperSink input, Alignment alignment)
		{
			Contract.Requires(input != null);

			int lineNumber = 0;

			float lineLength = 0.0f;

			bool isLineStart = true;

			for(int i = 0; i < _LineBreaker.Items.Count; ++i)
			{
				LineItem item = _LineBreaker.Items[i];

				// line ends are always at breaking points
				bool isLineEnd = false;

				if(_ClusterBreaks.Count > 0)
				{
					isLineEnd = _ClusterBreaks.Peek().Index == i;
				}

				if(item.Position >= 0)
				{
					TypesetCluster newCluster = new TypesetCluster();

					// the breaking point contains the spacing ratio for the line
					double ratio = 1.0;

					newCluster.LineNumber = lineNumber;

					if(_ClusterBreaks.Count > 0)
					{
						ratio = _ClusterBreaks.Peek().Ratio;
					}

					if(isLineEnd)
					{
						if(item.IsGlue)
						{
							// suppress whitespaces at the end of each line
							newCluster.Advance = new Size(0.0f, newCluster.Advance.Height);

							newCluster.Display = DisplayMode.Suppressed;
						}
						else
						{
							// breaking on a penalty enables that cluster
							newCluster.Advance = new Size(
								Convert.ToSingle(item.ComputeWidth(ratio, true)), newCluster.Advance.Height);

							newCluster.Display = DisplayMode.Neutral;
						}
					}
					else if(isLineStart)
					{
						if(item.IsGlue)
						{
							// suppress whitespaces at the start of each line
							newCluster.Advance = new Size(0.0f, newCluster.Advance.Height);

							newCluster.Display = DisplayMode.Suppressed;
						}
						else
						{
							// penalties cannot display at the start of the line
							newCluster.Advance = new Size(
								Convert.ToSingle(item.ComputeWidth(ratio, false)), newCluster.Advance.Height);

							newCluster.Display = DisplayMode.Neutral;
						}
					}
					else
					{
						if(alignment == Alignment.Stretch)
						{
							// spacing length within the line depends on the line ratio
							newCluster.Advance = new Size(
								Convert.ToSingle(item.ComputeWidth(ratio, false)), newCluster.Advance.Height);

							newCluster.Display = DisplayMode.Neutral;
						}
						else
						{
							if(item.IsPenalty)
							{
								// suppress penalties within the line
								newCluster.Advance = new Size(0.0f, newCluster.Advance.Height);

								newCluster.Display = DisplayMode.Suppressed;
							}
							else
							{
								// other items retain their widths
								newCluster.Advance = new Size(Convert.ToSingle(item.Width), newCluster.Advance.Height);

								newCluster.Display = DisplayMode.Neutral;
							}
						}
					}

					ShapedCluster cluster = input.Clusters[item.Position];

					newCluster.Characters = cluster.Characters;
					newCluster.Glyphs = cluster.Glyphs;
					newCluster.ContentType = cluster.ContentType;
					newCluster.Font = cluster.Font;
					newCluster.Advance = new Size(newCluster.Advance.Width, cluster.Advance.Height);
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

					_OutputSink.Clusters.Add(newCluster);
				}

				if(isLineEnd)
				{
					isLineStart = true;

					// move to the next line
					lineNumber++;

					// save the actual length of the line
					_OutputSink.LineLengths.Add(lineLength);

					lineLength = 0.0f;

					// remove the used breakpoint
					_ClusterBreaks.Pop();
				}
				else
				{
					isLineStart = false;
				}
			}

			// save the actual length of the line
			_OutputSink.LineLengths.Add(lineLength);
		}

		private void AnalyzeItems(
			ShaperSink input, Alignment alignment, double spacingEm, double trackingEm, double indentation)
		{
			Contract.Requires(input != null);
			Contract.Requires(spacingEm >= 0.0 && spacingEm <= double.MaxValue);
			Contract.Requires(trackingEm >= 0.0 && trackingEm <= double.MaxValue);
			Contract.Requires(indentation >= 0.0 && indentation <= double.MaxValue);

			OutputGlyphs(input);

			_LineBreaker.BeginParagraph();

			_LineBreaker.AddBox(indentation);

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
							_LineBreaker.AddBox(0.0, i);

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
						_LineBreaker.AddBox(0.0, i);
					}
				}
			}

			// add the final items required by the line breaking algorithm
			_LineBreaker.AddGlue(0.0, LineItem.Infinity, 0.0);
			_LineBreaker.AddPenalty(0.0, -LineItem.Infinity, 1.0);

			_LineBreaker.EndParagraph();
		}

		private static double ComputeAdvance(ref ShapedCluster cluster)
		{
			return cluster.Advance.Width;
		}

		private double ComputeSpacing(double spacingEm, ref ShapedCluster cluster)
		{
			double spacing = cluster.Advance.Height * spacingEm;

			// cap spacing at a quarter of the layout length
			return Math.Min(spacing, _LayoutRegion.Width / 4.0);
		}

		private double ComputeTracking(double trackingEm, ref ShapedCluster cluster)
		{
			double tracking = trackingEm * cluster.Advance.Height;

			// cap tracking at a quarter of the layout length
			return Math.Min(_LayoutRegion.Width / 4.0, tracking);
		}

		private double ComputeInline(double inlineLength)
		{
			// cap inlines at half the layout length
			return Math.Min(_LayoutRegion.Width / 2.0, inlineLength);
		}

		private struct Segment : IEquatable<Segment>
		{
			public float Length;
			public float Position;

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