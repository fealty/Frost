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
	internal sealed class Formatter
	{
		private readonly FormatterSink _OutputSink;

		private FormattedCluster[] _Clusters;

		public Formatter(FormatterSink outputSink)
		{
			Contract.Requires(outputSink != null);

			_OutputSink = outputSink;
		}

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

		private static IEnumerable<FormattedRun> ProduceFinalRuns(TypesetterSink input)
		{
			Contract.Requires(input != null);

			FormattedRun activeRun;

			activeRun.BidiLevel = input.Clusters[0].BidiLevel;
			activeRun.Clusters = ClusterRange.Empty;
			activeRun.EmSize = Convert.ToSingle(input.Clusters[0].Advance.Height);
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
				currentRun.EmSize = Convert.ToSingle(input.Clusters[i].Advance.Height);
				currentRun.Font = input.Clusters[i].Font;
				currentRun.TextRange = activeRun.TextRange;
				currentRun.PointSize = input.Clusters[i].PointSize;
				currentRun.LineNumber = input.Clusters[i].LineNumber;

				if(currentRun == activeRun)
				{
					ClusterRange range = activeRun.Clusters;

					activeRun.Clusters = new ClusterRange(range.Start, range.Length + 1);
				}
				else
				{
					start = input.Clusters[activeRun.Clusters.Start].Characters.StartIndex;
					end = input.Clusters[activeRun.Clusters.End].Characters.LastIndex;

					activeRun.TextRange = new IndexedRange(start, (end - start) + 1);

					yield return activeRun;

					activeRun = currentRun;

					activeRun.Clusters = new ClusterRange(i, 1);
				}
			}

			start = input.Clusters[activeRun.Clusters.Start].Characters.StartIndex;
			end = input.Clusters[activeRun.Clusters.End].Characters.LastIndex;

			activeRun.TextRange = new IndexedRange(start, (end - start) + 1);

			yield return activeRun;
		}

		private static IEnumerable<FormattedRun> ProduceBidiRuns(TypesetterSink input)
		{
			Contract.Requires(input != null);

			FormattedRun activeRun;

			activeRun.BidiLevel = input.Clusters[0].BidiLevel;
			activeRun.Clusters = ClusterRange.Empty;
			activeRun.EmSize = Convert.ToSingle(input.Clusters[0].Advance.Height);
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
					ClusterRange range = activeRun.Clusters;

					activeRun.Clusters = new ClusterRange(range.Start, range.Length + 1);
				}
				else
				{
					yield return activeRun;

					activeRun = currentRun;

					activeRun.Clusters = new ClusterRange(i, 1);
				}
			}

			yield return activeRun;
		}

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
					if(lineNumber == -1)
					{
						// the line bidi level is determined by the first run
						lineBidiLevel = run.BidiLevel;

						if(input.Alignment == Alignment.Leading)
						{
							// reverse the bidi level of the line for leading alignment
							lineBidiLevel++;
						}
					}

					lineNumber = run.LineNumber;

					if(input.Alignment != Alignment.Center)
					{
						if(lineNumber == 0)
						{
							// indent only the first line
							penPosition = lineBidiLevel % 2 == 0
							              	? input.Indentation
							              	: input.Lines[lineNumber].Right - input.Indentation;
						}
						else
						{
							penPosition = lineBidiLevel % 2 == 0 ? 0.0f : input.Lines[lineNumber].Right;
						}
					}
					else
					{
						float lineWidth = input.Lines[lineNumber].Width;

						lineWidth -= input.LineLengths[lineNumber];

						lineWidth /= 2.0f;

						penPosition = lineBidiLevel % 2 == 0 ? lineWidth : input.Lines[lineNumber].Right - lineWidth;
					}
				}

				ProduceClusters(run, lineBidiLevel, ref penPosition, input);
			}

			_OutputSink.Clusters.AddRange(_Clusters);

			_OutputSink.Runs.AddRange(ProduceFinalRuns(input));
		}

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

		private void ProduceClusters(
			FormattedRun run, int lineBidiLevel, ref float penPosition, TypesetterSink input)
		{
			Contract.Requires(input != null);

			// the run and the line have the same direction
			if((run.BidiLevel % 2 == 0) == (lineBidiLevel % 2 == 0))
			{
				for(int i = run.Clusters.Start; i <= run.Clusters.End; ++i)
				{
					FormattedCluster newCluster;

					newCluster.Region = input.Lines[run.LineNumber];

					TypesetCluster cluster = input.Clusters[i];

					newCluster.Characters = cluster.Characters;
					newCluster.Glyphs = cluster.Glyphs;
					newCluster.Display = cluster.Display;

					newCluster.Region = new Rectangle(
						newCluster.Region.Location, new Size(cluster.Advance.Width, newCluster.Region.Height));

					// the line reads from left to right
					if(lineBidiLevel % 2 == 0)
					{
						newCluster.Region =
							new Rectangle(
								new Point(newCluster.Region.X + penPosition, newCluster.Region.Y), newCluster.Region.Size);

						penPosition += newCluster.Region.Width;
					}
						// the line reads from right to left
					else
					{
						newCluster.Region =
							new Rectangle(
								new Point(penPosition - newCluster.Region.Width, newCluster.Region.Y),
								newCluster.Region.Size);

						penPosition -= newCluster.Region.Width;
					}

					if(cluster.ContentType == ContentType.Floater)
					{
						newCluster.Region = cluster.Floater;
					}

					_Clusters[i] = newCluster;
				}
			}
			else
			{
				// iterate over the clusters in reverse
				for(int i = run.Clusters.End; i >= run.Clusters.Start; --i)
				{
					FormattedCluster newCluster;

					newCluster.Region = input.Lines[run.LineNumber];

					TypesetCluster cluster = input.Clusters[i];

					newCluster.Characters = cluster.Characters;
					newCluster.Glyphs = cluster.Glyphs;
					newCluster.Display = cluster.Display;

					newCluster.Region = new Rectangle(
						newCluster.Region.Location, new Size(cluster.Advance.Width, newCluster.Region.Height));

					// the line reads from left to right
					if(lineBidiLevel % 2 == 0)
					{
						newCluster.Region =
							new Rectangle(
								new Point(newCluster.Region.X + penPosition, newCluster.Region.Y), newCluster.Region.Size);

						penPosition += newCluster.Region.Width;
					}

						// the line reads from right to left
					else
					{
						newCluster.Region =
							new Rectangle(
								new Point(penPosition - newCluster.Region.Width, newCluster.Region.Y),
								newCluster.Region.Size);

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

		private static float TakeAbsMax(float n1, float n2)
		{
			double n1Abs = Math.Abs(n1);
			double n2Abs = Math.Abs(n2);

			return n1Abs > n2Abs ? n1 : n2;
		}

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