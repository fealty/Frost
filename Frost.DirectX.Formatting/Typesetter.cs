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
	/// <summary>
	///   This class typesets shaped cluster and glyph data provided by a <see cref="ShaperSink" /> .
	/// </summary>
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

		/// <summary>
		///   This constructor creates a new instance of this class linked to an output sink.
		/// </summary>
		/// <param name="outputSink"> This parameter references the output sink to link. </param>
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

		/// <summary>
		///   This method typesets a paragraph from the shaped input.
		/// </summary>
		/// <typeparam name="T"> This parameter indicates the type of enumeration. </typeparam>
		/// <param name="input"> This parameter references the input sink. </param>
		/// <param name="paragraph"> This parameter references the paragraph. </param>
		/// <param name="region"> This parameter contains the region to typeset the paragraph within. </param>
		/// <param name="boxes"> This parameter references an enumeration of floating regions. </param>
		public void Typeset<T>(ShaperSink input, Paragraph paragraph, Rectangle region, T boxes)
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

			// the indentation caps to half of the layout region
			float indentationCap = _LayoutRegion.Width / 2.0f;

			// determine the amount of indentation in pixels
			float indentationValue = paragraph.Indentation * _LineHeight;

			// cap the indentation
			float indentation = Math.Min(indentationCap, indentationValue);

			_OutputSink.Indentation = indentation;

			AnalyzeItems(input, paragraph.Alignment, paragraph.Spacing, paragraph.Tracking, indentation);

			DetermineBreakingPoints(boxes);

			SaveBreakingPoints();

			OutputClusters(input, paragraph.Alignment);

			_OutputSink.Lines.AddRange(_ComputedLines);
		}

		/// <summary>
		///   This method processes floating regions.
		/// </summary>
		/// <param name="item"> This parameter references the current line item. </param>
		private void ProcessFloaters(ref LineItem item)
		{
			if(item.Position >= 0)
			{
				ShapedCluster cluster = _InputSink.Clusters[item.Position];

				if(cluster.ContentType == ContentType.Floater)
				{
					// determine the total height of each line
					float lineHeight = _LineHeight + _Leading;

					// determine how many lines exist in the occupied space
					float occupiedLines = cluster.Floater.Height / lineHeight;

					// round to a whole number of lines
					occupiedLines = Convert.ToSingle(Math.Ceiling(occupiedLines));

					float occupiedX = _LayoutRegion.Left;
					float occupiedY = _LayoutRegion.Top + _LineOffset;
					float occupiedWidth = cluster.Floater.Width;
					float occupiedHeight = occupiedLines * lineHeight;

					Rectangle occupiedRegion = new Rectangle(occupiedX, occupiedY, occupiedWidth, occupiedHeight);

					Debug.Assert(cluster.HAlignment != Alignment.Stretch);

					// determine the horizonal region the occupied region can reside within
					Rectangle slideRegion = new Rectangle(
						occupiedX, occupiedY, _LayoutRegion.Width, occupiedHeight);

					LayoutDirection direction = cluster.BidiLevel % 2 == 0
					                            	? LayoutDirection.LeftToRight
					                            	: LayoutDirection.RightToLeft;

					// align the occupied region within the slide region
					occupiedRegion = occupiedRegion.AlignWithin(
						slideRegion, cluster.HAlignment, Axis.Horizontal, direction);

					// align the floater vertically within the occupied region
					cluster.Floater = cluster.Floater.AlignWithin(
						occupiedRegion, cluster.VAlignment, Axis.Vertical, direction);

					// align the floater horizontally within the occupied region
					cluster.Floater = cluster.Floater.AlignWithin(
						occupiedRegion, cluster.HAlignment, Axis.Horizontal, direction);

					_InputSink.Clusters[item.Position] = cluster;

					_Obstructions.Add(cluster.Floater);
				}
			}
		}

		/// <summary>
		///   This method determines the maximum line height in the paragraph.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
		/// <param name="leadingEm"> This parameter indicates the leading in EM units. </param>
		private void FindLineHeight(ShaperSink input, float leadingEm)
		{
			Contract.Requires(input != null);
			Contract.Requires(Check.IsPositive(leadingEm));

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

			// convert from EMs to pixels; cap value to a minimum of the gap
			_Leading = (_LineHeight * leadingEm) + gap;
		}

		/// <summary>
		///   This method computes the region for the line at the given index.
		/// </summary>
		/// <param name="lineIndex"> This parameter indicates the line index. </param>
		/// <returns> This method returns the computed line region. </returns>
		private Rectangle GetLineRegion(int lineIndex)
		{
			Contract.Requires(lineIndex >= 0);

			// a line has already been computed for this index
			if(lineIndex < _ComputedLines.Count)
			{
				return _ComputedLines[lineIndex];
			}

			Rectangle lineRegion = new Rectangle(
				_LayoutRegion.Left, _LayoutRegion.Top + _LineOffset, _LayoutRegion.Width, _LineHeight);

			IdentifyFreeSegments(lineRegion);

			// advance over the line and its leading to next line position
			_LineOffset += _LineHeight + _Leading;

			return GetLineRegion(lineIndex);
		}

		/// <summary>
		///   This method processes a free line segment.
		/// </summary>
		/// <param name="index"> This parameter references the index of the free segment. </param>
		/// <param name="box"> This parameter contains the box to test against. </param>
		private void ProcessFreeSegment(ref int index, Rectangle box)
		{
			Contract.Requires(index >= 0);

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

		/// <summary>
		///   This method identifies free segments within a line region.
		/// </summary>
		/// <param name="lineRegion"> This parameter contains the region of the line. </param>
		private void IdentifyFreeSegments(Rectangle lineRegion)
		{
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
					segment.Position, lineRegion.Y, segment.Length, lineRegion.Height);

				// reject lines that occupy less space than the line height
				if(newRegion.Width > _LineHeight)
				{
					_ComputedLines.Add(newRegion);
				}
			}
		}

		/// <summary>
		///   This method resets the line state to allow for mutation.
		/// </summary>
		private void ResetLinesState()
		{
			_LineOffset = 0.0f;

			_Obstructions.Clear();
			_ComputedLines.Clear();
		}

		/// <summary>
		///   This method adds obstructions to the line state.
		/// </summary>
		/// <typeparam name="T"> This type parameter indicates the type of the obstruction enumeration. </typeparam>
		/// <param name="boxes"> This parameter references an instance of the obstruction enumeration. </param>
		private void AddObstructions<T>(T boxes) where T : class, IEnumerable<Rectangle>
		{
			if(boxes != null)
			{
				foreach(Rectangle box in boxes)
				{
					_Obstructions.Add(box);
				}
			}
		}

		/// <summary>
		///   This method determines the breaking points, taking into account the given obstructions.
		/// </summary>
		/// <typeparam name="T"> This type parameter indicates the type of the obstruction enumeration. </typeparam>
		/// <param name="boxes"> This parameter references an instance of the obstruction enumeration. </param>
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

		/// <summary>
		///   This method saves the breaking points in the proper order.
		/// </summary>
		private void SaveBreakingPoints()
		{
			_ClusterBreaks.Clear();

			// save the breaking points in reverse iteration order
			for(int i = _LineBreaker.Breakpoints.Count - 1; i >= 1; --i)
			{
				_ClusterBreaks.Push(_LineBreaker.Breakpoints[i]);
			}
		}

		/// <summary>
		///   This method determines whether the line is broken before the given cluster.
		/// </summary>
		/// <param name="index"> This parameter indicates the index of the current item. </param>
		/// <param name="clusters"> This parameter references the shaped clusters. </param>
		/// <param name="isForced"> This parameter indicates whether the break is forced. </param>
		/// <param name="isAlone"> This parameter indicates whether the call to the method is recursive. </param>
		/// <returns> This method returns <c>true</c> if the line is broken before the given cluster; otherwise, this method returns <c>false</c> . </returns>
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

		/// <summary>
		///   This method determines whether the line is broken after the given cluster.
		/// </summary>
		/// <param name="index"> This parameter indicates the index of the current item. </param>
		/// <param name="clusters"> This parameter references the shaped clusters. </param>
		/// <param name="isForced"> This parameter indicates whether the break is forced. </param>
		/// <param name="isAlone"> This parameter indicates whether the call to the method is recursive. </param>
		/// <returns> This method returns <c>true</c> if the line is broken after the given cluster; otherwise, this method returns <c>false</c> . </returns>
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

		/// <summary>
		///   This method adds a soft hyphen to the line breaker.
		/// </summary>
		/// <param name="index"> This parameter indicates the index of the item representing the soft hyphen. </param>
		/// <param name="advance"> This parameter indicates the advance width of the soft hyphen. </param>
		/// <param name="isRagged"> This parameter indicates whether the text is being set ragged or justified. </param>
		private void AddSoftHyphen(int index, double advance, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(Check.IsPositive(advance));

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

		/// <summary>
		///   This method adds a forced break to the line breaker.
		/// </summary>
		/// <param name="index"> This parameter indicates the index of the item representing the forced break. </param>
		/// <param name="isRagged"> This parameter indicates whether the text is being set ragged or justified. </param>
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

		/// <summary>
		///   This method adds a variable space to the line breaker.
		/// </summary>
		/// <param name="index"> This parameter indicates the index of the item representing the variable space. </param>
		/// <param name="length"> This parameter indicates the length of the space. </param>
		/// <param name="isRagged"> This parameter indicates whether the text is being set ragged or justified. </param>
		private void AddVariableSpace(int index, double length, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(Check.IsPositive(length));

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

		/// <summary>
		///   This method adds whitespace to the line breaker.
		/// </summary>
		/// <param name="index"> This parameter indicates the index of the item representing the whitespace. </param>
		/// <param name="length"> This parameter indicates the length of the space. </param>
		/// <param name="input"> This parameter references the input sink. </param>
		/// <param name="isRagged"> This parameter indicates whether the text is being set ragged or justified. </param>
		private void AddWhitespace(int index, double length, ShaperSink input, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(Check.IsPositive(length));
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

		/// <summary>
		///   This method adds a cluster to the line breaker.
		/// </summary>
		/// <param name="index"> This parameter indicates the index of the item representing the cluster. </param>
		/// <param name="advance"> This parameter indicates the advance width of the cluster. </param>
		/// <param name="input"> This parameter references the input sink. </param>
		/// <param name="isRagged"> This parameter indicates whether the text is being set ragged or justified. </param>
		private void AddCluster(int index, double advance, ShaperSink input, bool isRagged)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(Check.IsPositive(advance));
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

		/// <summary>
		///   This method produces the typeset glyphs.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
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

		/// <summary>
		///   This method produces the typeset clusters.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
		/// <param name="alignment"> This parameter indicates the paragraph alignment. </param>
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

					float newWidth = newCluster.Advance.Width;
					float newHeight = cluster.Advance.Height;

					newCluster.Characters = cluster.Characters;
					newCluster.Glyphs = cluster.Glyphs;
					newCluster.ContentType = cluster.ContentType;
					newCluster.Font = cluster.Font;
					newCluster.Advance = new Size(newWidth, newHeight);
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

		/// <summary>
		///   This method produces line items from analysis of the shaped data.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
		/// <param name="alignment"> This parameter indicates the paragraph alignment. </param>
		/// <param name="spacingEm"> This parameter indicates the spacing length in EM units. </param>
		/// <param name="trackingEm"> This parameter indicates the tracking length in EM units. </param>
		/// <param name="indentation"> This parameter indicates the indentation in pixels. </param>
		private void AnalyzeItems(
			ShaperSink input, Alignment alignment, double spacingEm, double trackingEm, double indentation)
		{
			Contract.Requires(input != null);
			Contract.Requires(Check.IsPositive(spacingEm));
			Contract.Requires(Check.IsPositive(trackingEm));
			Contract.Requires(Check.IsPositive(indentation));

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

		/// <summary>
		///   This method computes the advance width for a shaped cluster.
		/// </summary>
		/// <param name="cluster"> This parameter references the shaped cluster. </param>
		/// <returns> This method returns the advance width for the shaped cluster. </returns>
		private static double ComputeAdvance(ref ShapedCluster cluster)
		{
			return cluster.Advance.Width;
		}

		/// <summary>
		///   This method computes the spacing for a shaped cluster.
		/// </summary>
		/// <param name="spacingEm"> This parameter indicates the spacing length in EM units. </param>
		/// <param name="cluster"> This parameter references the shaped cluster. </param>
		/// <returns> This method returns the computed spacing length for the cluster. </returns>
		private double ComputeSpacing(double spacingEm, ref ShapedCluster cluster)
		{
			double spacing = cluster.Advance.Height * spacingEm;

			// cap spacing at a quarter of the layout length
			return Math.Min(spacing, _LayoutRegion.Width / 4.0);
		}

		/// <summary>
		///   This method computes the tracking for a shaped cluster.
		/// </summary>
		/// <param name="trackingEm"> This parameter indicates the tracking length in EM units. </param>
		/// <param name="cluster"> This parameter references the shaped cluster. </param>
		/// <returns> This method returns the computed tracking length for the cluster. </returns>
		private double ComputeTracking(double trackingEm, ref ShapedCluster cluster)
		{
			double tracking = trackingEm * cluster.Advance.Height;

			// cap tracking at a quarter of the layout length
			return Math.Min(_LayoutRegion.Width / 4.0, tracking);
		}

		/// <summary>
		///   This method computes the inline length.
		/// </summary>
		/// <param name="inlineLength"> This parameter indicates the length of an inline object. </param>
		/// <returns> This method returns the computed length of the inline object. </returns>
		private double ComputeInline(double inlineLength)
		{
			// cap inlines at half the layout length
			return Math.Min(_LayoutRegion.Width / 2.0, inlineLength);
		}

		/// <summary>
		///   This struct stores data for segments of lines.
		/// </summary>
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