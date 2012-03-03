// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;
using Frost.Formatting;

using DxFontMetrics = SharpDX.DirectWrite.FontMetrics;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class formats typeset clusters and glyphs, producing positioning information and outputting results to an output sink.
	/// </summary>
	internal sealed class Formatter
	{
		private readonly FormatterSink _OutputSink;

		private FormattedCluster[] _Clusters;

		/// <summary>
		///   This constructor links a new instance of this class to an output sink.
		/// </summary>
		/// <param name="outputSink"> This parameter references the output sink. </param>
		public Formatter(FormatterSink outputSink)
		{
			Contract.Requires(outputSink != null);

			_OutputSink = outputSink;
		}

		/// <summary>
		///   This method formats the input clusters and glyphs.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
		public void Format(TypesetterSink input)
		{
			Contract.Requires(input != null);

			Contract.Ensures(_OutputSink.Clusters.Count == input.Clusters.Count);
			Contract.Ensures(_OutputSink.Glyphs.Count == input.Glyphs.Count);

			_Clusters = new FormattedCluster[input.Clusters.Count];

			_OutputSink.Reset(input.FullText);

			_OutputSink.LayoutRegion = input.LayoutRegion;
			_OutputSink.LineHeight = input.LineHeight;
			_OutputSink.Leading = input.Leading;

			_OutputSink.PreallocateGlyphs(input.Glyphs.Count);
			_OutputSink.PreallocateClusters(input.Clusters.Count);

			OutputGlyphs(input);
			ProduceLines(input);

			DetermineBaselines();
		}

		/// <summary>
		///   This method produces the final formatted cluster runs.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
		/// <returns> This method returns an enumeration of the results. </returns>
		private static IEnumerable<FormattedRun> ProduceFinalRuns(TypesetterSink input)
		{
			Contract.Requires(input != null);

			FormattedRun activeRun;

			activeRun.BidiLevel = input.Clusters[0].BidiLevel;
			activeRun.Clusters = IndexedRange.Empty;
			activeRun.EmSize = input.Clusters[0].Advance.Height;
			activeRun.Font = input.Clusters[0].Font;
			activeRun.TextRange = IndexedRange.Empty;
			activeRun.PointSize = input.Clusters[0].PointSize;
			activeRun.LineNumber = input.Clusters[0].LineNumber;

			int start;
			int end;

			for(int i = 0; i < input.Clusters.Count; ++i)
			{
				FormattedRun currentRun;

				currentRun.BidiLevel = input.Clusters[i].BidiLevel;
				currentRun.Clusters = activeRun.Clusters;
				currentRun.EmSize = input.Clusters[i].Advance.Height;
				currentRun.Font = input.Clusters[i].Font;
				currentRun.TextRange = activeRun.TextRange;
				currentRun.PointSize = input.Clusters[i].PointSize;
				currentRun.LineNumber = input.Clusters[i].LineNumber;

				// the runs are identical
				if(currentRun == activeRun)
				{
					// extend the current cluster range by one cluster
					activeRun.Clusters = activeRun.Clusters.Extend(1);
				}
				else
				{
					// determine the text range from the clusters
					start = input.Clusters[activeRun.Clusters.StartIndex].Characters.StartIndex;
					end = input.Clusters[activeRun.Clusters.LastIndex].Characters.LastIndex;

					// convert the valid index range to a length
					activeRun.TextRange = new IndexedRange(start, (end - start) + 1);

					yield return activeRun;

					activeRun = currentRun;

					activeRun.Clusters = new IndexedRange(i, 1);
				}
			}

			// determine the text range from the clusters
			start = input.Clusters[activeRun.Clusters.StartIndex].Characters.StartIndex;
			end = input.Clusters[activeRun.Clusters.LastIndex].Characters.LastIndex;

			// convert the valid index range to a length
			activeRun.TextRange = new IndexedRange(start, (end - start) + 1);

			yield return activeRun;
		}

		/// <summary>
		///   This method produces the bidi formatted cluster runs.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
		/// <returns> This method returns an enumeration of the results. </returns>
		private static IEnumerable<FormattedRun> ProduceBidiRuns(TypesetterSink input)
		{
			Contract.Requires(input != null);

			FormattedRun activeRun;

			activeRun.BidiLevel = input.Clusters[0].BidiLevel;
			activeRun.Clusters = IndexedRange.Empty;
			activeRun.EmSize = input.Clusters[0].Advance.Height;
			activeRun.Font = input.Clusters[0].Font;
			activeRun.PointSize = input.Clusters[0].PointSize;
			activeRun.TextRange = IndexedRange.Empty;
			activeRun.LineNumber = input.Clusters[0].LineNumber;

			for(int i = 0; i < input.Clusters.Count; ++i)
			{
				FormattedRun currentRun;

				currentRun.BidiLevel = input.Clusters[i].BidiLevel;
				currentRun.Clusters = activeRun.Clusters;
				currentRun.EmSize = activeRun.EmSize;
				currentRun.Font = activeRun.Font;
				currentRun.PointSize = activeRun.PointSize;
				currentRun.TextRange = activeRun.TextRange;
				currentRun.LineNumber = input.Clusters[i].LineNumber;

				if(currentRun == activeRun)
				{
					// extend the current cluster range by one cluster
					activeRun.Clusters = activeRun.Clusters.Extend(1);
				}
				else
				{
					yield return activeRun;

					activeRun = currentRun;

					activeRun.Clusters = new IndexedRange(i, 1);
				}
			}

			yield return activeRun;
		}

		/// <summary>
		///   This method produces formatted lines of clusters.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
		private void ProduceLines(TypesetterSink input)
		{
			Contract.Requires(input != null);

			int lineNumber = -1;
			int lineBidiLevel = -1;

			float penPosition = 0.0f;

			foreach(FormattedRun run in ProduceBidiRuns(input))
			{
				if(run.LineNumber != lineNumber)
				{
					lineNumber = run.LineNumber;

					if(lineNumber == 0)
					{
						// the first run determines the line bidi level
						lineBidiLevel = run.BidiLevel;

						if(input.Alignment == Alignment.Leading)
						{
							// reverse the bidi level for leading alignment
							lineBidiLevel++;
						}
					}

					bool isLeftToRight = lineBidiLevel % 2 == 0;

					if(input.Alignment != Alignment.Center)
					{
						penPosition = 0.0f;

						if(!isLeftToRight)
						{
							// position relative to the right edge
							penPosition = input.Lines[lineNumber].Right;
						}

						// indent only the first line in the paragraph
						if(lineNumber == 0)
						{
							if(isLeftToRight)
							{
								// position relative to the left edge
								penPosition = input.Indentation;
							}
							else
							{
								// position relative to the right edge
								penPosition = penPosition - input.Indentation;
							}
						}
					}
					else
					{
						float lineWidth = input.Lines[lineNumber].Width;

						// determine unused space on the line
						lineWidth -= input.LineLengths[lineNumber];

						// divide the unused space in half
						lineWidth /= 2.0f;

						// distribute the halved space to center the line
						penPosition = lineWidth;

						if(!isLeftToRight)
						{
							// position relative to the right edge
							penPosition = input.Lines[lineNumber].Right - penPosition;
						}
					}
				}

				ProduceClusters(run, lineBidiLevel, ref penPosition, input);
			}

			_OutputSink.Clusters.AddRange(_Clusters);

			_OutputSink.Runs.AddRange(ProduceFinalRuns(input));
		}

		/// <summary>
		///   This method produces the formatted glyphs.
		/// </summary>
		/// <param name="input"> This parameter references the input sink. </param>
		private void OutputGlyphs(TypesetterSink input)
		{
			Contract.Requires(input != null);

			foreach(TypesetGlyph glyph in input.Glyphs)
			{
				FormattedGlyph newGlyph;

				newGlyph.Advance = glyph.Advance;
				newGlyph.Index = glyph.Index;
				newGlyph.Offset = glyph.Offset;

				_OutputSink.Glyphs.Add(newGlyph);
			}
		}

		/// <summary>
		///   This method produces the formatted clusters for each line from each run.
		/// </summary>
		/// <param name="run"> This parameter contains the formatted run. </param>
		/// <param name="lineBidiLevel"> This parameter indicates the bidi level of the line. </param>
		/// <param name="penPosition"> This parameter indicates the pen position. </param>
		/// <param name="input"> This parameter references the input sink. </param>
		private void ProduceClusters(
			FormattedRun run, int lineBidiLevel, ref float penPosition, TypesetterSink input)
		{
			Contract.Requires(input != null);

			// the run and the line have the same direction
			if((run.BidiLevel % 2 == 0) == (lineBidiLevel % 2 == 0))
			{
				foreach(int clusterIndex in run.Clusters)
				{
					FormattedCluster newCluster;

					newCluster.Region = input.Lines[run.LineNumber];

					TypesetCluster cluster = input.Clusters[clusterIndex];

					newCluster.Characters = cluster.Characters;
					newCluster.Glyphs = cluster.Glyphs;
					newCluster.Display = cluster.Display;

					float clusterWidth = cluster.Advance.Width;
					float clusterHeight = newCluster.Region.Height;

					Size clusterSize = new Size(clusterWidth, clusterHeight);

					newCluster.Region = newCluster.Region.Resize(clusterSize);

					// the line reads from left to right
					if(lineBidiLevel % 2 == 0)
					{
						Size amount = new Size(penPosition, 0.0f);

						newCluster.Region = newCluster.Region.Translate(amount);

						penPosition += newCluster.Region.Width;
					}
						// the line reads from right to left
					else
					{
						float x = penPosition - newCluster.Region.Width;

						Point location = new Point(x, newCluster.Region.Y);

						newCluster.Region = newCluster.Region.Relocate(location);

						penPosition -= newCluster.Region.Width;
					}

					if(cluster.ContentType == ContentType.Floater)
					{
						newCluster.Region = cluster.Floater;
					}

					_Clusters[clusterIndex] = newCluster;
				}
			}
			else
			{
				// iterate over the clusters in reverse
				for(int i = run.Clusters.LastIndex; i >= run.Clusters.StartIndex; --i)
				{
					FormattedCluster newCluster;

					newCluster.Region = input.Lines[run.LineNumber];

					TypesetCluster cluster = input.Clusters[i];

					newCluster.Characters = cluster.Characters;
					newCluster.Glyphs = cluster.Glyphs;
					newCluster.Display = cluster.Display;

					float clusterWidth = cluster.Advance.Width;
					float clusterHeight = newCluster.Region.Height;

					Size clusterSize = new Size(clusterWidth, clusterHeight);

					newCluster.Region = newCluster.Region.Resize(clusterSize);

					// the line reads from left to right
					if(lineBidiLevel % 2 == 0)
					{
						Size amount = new Size(penPosition, 0.0f);

						newCluster.Region = newCluster.Region.Translate(amount);

						penPosition += newCluster.Region.Width;
					}
						// the line reads from right to left
					else
					{
						float x = penPosition - newCluster.Region.Width;

						Point location = new Point(x, newCluster.Region.Y);

						newCluster.Region = newCluster.Region.Relocate(location);

						penPosition -= newCluster.Region.Width;
					}

					if(cluster.ContentType == ContentType.Floater)
					{
						newCluster.Region = cluster.Floater;
					}

					_Clusters[i] = newCluster;
				}
			}
		}

		/// <summary>
		///   This method determines the max of two absolute values.
		/// </summary>
		/// <param name="n1"> This parameter contains the first value to test. </param>
		/// <param name="n2"> This parameter contains the second value to test. </param>
		/// <returns> This method returns the max absolute value. </returns>
		private static float TakeAbsMax(float n1, float n2)
		{
			double n1Abs = Math.Abs(n1);
			double n2Abs = Math.Abs(n2);

			return n1Abs > n2Abs ? n1 : n2;
		}

		/// <summary>
		///   This method determines the common baselines for the paragraph.
		/// </summary>
		private void DetermineBaselines()
		{
			float baselineOffset = 0.0f;
			float strikethroughOffset = 0.0f;
			float underlineOffset = 0.0f;
			float strikethroughThickness = 0.0f;
			float underlineThickness = 0.0f;

			foreach(FormattedRun run in _OutputSink.Runs)
			{
				DxFontMetrics dxMetrics = run.Font.ResolveFace().Metrics;

				FontMetrics metrics = new FontMetrics(
					dxMetrics.Ascent, dxMetrics.Descent, dxMetrics.DesignUnitsPerEm);

				float blo = metrics.Measure(dxMetrics.Ascent + dxMetrics.Descent, run.PointSize);
				float sto = metrics.Measure(dxMetrics.StrikethroughPosition, run.PointSize);
				float ulo = metrics.Measure(dxMetrics.UnderlinePosition, run.PointSize);
				float stt = metrics.Measure(dxMetrics.StrikethroughThickness, run.PointSize);
				float ult = metrics.Measure(dxMetrics.UnderlineThickness, run.PointSize);

				baselineOffset = TakeAbsMax(baselineOffset, blo);
				strikethroughOffset = TakeAbsMax(strikethroughOffset, sto);
				underlineOffset = TakeAbsMax(underlineOffset, ulo);
				strikethroughThickness = TakeAbsMax(strikethroughThickness, stt);
				underlineThickness = TakeAbsMax(underlineThickness, ult);
			}

			_OutputSink.BaselineOffset = baselineOffset;
			_OutputSink.StrikethroughOffset = strikethroughOffset;
			_OutputSink.StrikethroughThickness = strikethroughThickness;
			_OutputSink.UnderlineOffset = underlineOffset;
			_OutputSink.UnderlineThickness = underlineThickness;
		}
	}
}