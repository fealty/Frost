using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using DxFontMetrics = SharpDX.DirectWrite.FontMetrics;

namespace Frost.DirectX.Formatting
{
	internal sealed class Formatter
	{
		private readonly FormatterSink mOutputSink;

		private FormattedCluster[] mClusters;

		public Formatter(FormatterSink outputSink)
		{
			Contract.Requires(outputSink != null);

			mOutputSink = outputSink;
		}

		public void Format(TypesetterSink input)
		{
			Contract.Requires(input != null);

			Contract.Ensures(mOutputSink.Clusters.Count == input.Clusters.Count);
			Contract.Ensures(mOutputSink.Glyphs.Count == input.Glyphs.Count);

			mClusters = new FormattedCluster[input.Clusters.Count];

			mOutputSink.Reset(input.FullText);

			mOutputSink.LayoutRegion = input.LayoutRegion;
			mOutputSink.LineHeight = input.LineHeight;
			mOutputSink.Leading = input.Leading;

			mOutputSink.PreallocateGlyphs(input.Glyphs.Count);
			mOutputSink.PreallocateClusters(input.Clusters.Count);

			OutputGlyphs(input);

			ProduceLines(input);

			DetermineBaselines();
		}

		private static IEnumerable<FormattedRun> ProduceFinalRuns(
			TypesetterSink input)
		{
			Contract.Requires(input != null);

			FormattedRun activeRun;

			activeRun.BidiLevel = input.Clusters[0].BidiLevel;
			activeRun.Clusters = ClusterRange.Empty;
			activeRun.EmSize = Convert.ToSingle(input.Clusters[0].Advance.Height);
			activeRun.Font = input.Clusters[0].Font;
			activeRun.TextRange = TextRange.Empty;
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
					start = input.Clusters[activeRun.Clusters.Start].Characters.Start;
					end = input.Clusters[activeRun.Clusters.End].Characters.End;

					activeRun.TextRange = new TextRange(start, (end - start) + 1);

					yield return activeRun;

					activeRun = currentRun;

					activeRun.Clusters = new ClusterRange(i, 1);
				}
			}

			start = input.Clusters[activeRun.Clusters.Start].Characters.Start;
			end = input.Clusters[activeRun.Clusters.End].Characters.End;

			activeRun.TextRange = new TextRange(start, (end - start) + 1);

			yield return activeRun;
		}

		private static IEnumerable<FormattedRun> ProduceBidiRuns(
			TypesetterSink input)
		{
			Contract.Requires(input != null);

			FormattedRun activeRun;

			activeRun.BidiLevel = input.Clusters[0].BidiLevel;
			activeRun.Clusters = ClusterRange.Empty;
			activeRun.EmSize = Convert.ToSingle(input.Clusters[0].Advance.Height);
			activeRun.Font = input.Clusters[0].Font;
			activeRun.PointSize = input.Clusters[0].PointSize;
			activeRun.TextRange = TextRange.Empty;
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

			double penPosition = 0.0;

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
							              	: input.Lines[lineNumber].Right -
							              	  input.Indentation;
						}
						else
						{
							penPosition = lineBidiLevel % 2 == 0
							              	? 0.0
							              	: input.Lines[lineNumber].Right;
						}
					}
					else
					{
						double lineWidth = input.Lines[lineNumber].Width;

						lineWidth -= input.LineLengths[lineNumber];

						lineWidth /= 2.0;

						penPosition = lineBidiLevel % 2 == 0
						              	? lineWidth
						              	: input.Lines[lineNumber].Right - lineWidth;
					}
				}

				ProduceClusters(run, lineBidiLevel, ref penPosition, input);
			}

			mOutputSink.Clusters.AddRange(mClusters);

			mOutputSink.Runs.AddRange(ProduceFinalRuns(input));
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

				mOutputSink.Glyphs.Add(newGlyph);
			}
		}

		private void ProduceClusters(
			FormattedRun run,
			int lineBidiLevel,
			ref double penPosition,
			TypesetterSink input)
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
					newCluster.Region.Width = cluster.Advance.Width;

					// the line reads from left to right
					if(lineBidiLevel % 2 == 0)
					{
						newCluster.Region.X += penPosition;

						penPosition += newCluster.Region.Width;
					}
						// the line reads from right to left
					else
					{
						newCluster.Region.X = penPosition - newCluster.Region.Width;

						penPosition -= newCluster.Region.Width;
					}

					if(cluster.ContentType == ContentType.Floater)
					{
						newCluster.Region = cluster.Floater;
					}

					mClusters[i] = newCluster;
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
					newCluster.Region.Width = cluster.Advance.Width;

					// the line reads from left to right
					if(lineBidiLevel % 2 == 0)
					{
						newCluster.Region.X += penPosition;

						penPosition += newCluster.Region.Width;
					}

						// the line reads from right to left
					else
					{
						newCluster.Region.X = penPosition - newCluster.Region.Width;

						penPosition -= newCluster.Region.Width;
					}

					if(cluster.ContentType == ContentType.Floater)
					{
						newCluster.Region = cluster.Floater;
					}

					mClusters[i] = newCluster;
				}
			}
		}

		private static double TakeAbsMax(double n1, double n2)
		{
			double n1Abs = Math.Abs(n1);
			double n2Abs = Math.Abs(n2);

			return n1Abs > n2Abs ? n1 : n2;
		}

		private void DetermineBaselines()
		{
			double baselineOffset = 0.0;
			double strikethroughOffset = 0.0;
			double underlineOffset = 0.0;

			double strikethroughThickness = 0.0;
			double underlineThickness = 0.0;

			foreach(FormattedRun run in mOutputSink.Runs)
			{
				DxFontMetrics metrics = run.Font.ResolveFace().Metrics;

				double unitsPerEm = metrics.DesignUnitsPerEm;

				double blo = FontMetrics.ToPixels(
					metrics.Ascent + metrics.Descent, run.PointSize, unitsPerEm);
				double sto = FontMetrics.ToPixels(
					metrics.StrikethroughPosition, run.PointSize, unitsPerEm);
				double ulo = FontMetrics.ToPixels(
					metrics.UnderlinePosition, run.PointSize, unitsPerEm);
				double stt = FontMetrics.ToPixels(
					metrics.StrikethroughThickness, run.PointSize, unitsPerEm);
				double ult = FontMetrics.ToPixels(
					metrics.UnderlineThickness, run.PointSize, unitsPerEm);

				baselineOffset = TakeAbsMax(baselineOffset, blo);
				strikethroughOffset = TakeAbsMax(strikethroughOffset, sto);
				underlineOffset = TakeAbsMax(underlineOffset, ulo);
				strikethroughThickness = TakeAbsMax(strikethroughThickness, stt);
				underlineThickness = TakeAbsMax(underlineThickness, ult);
			}

			mOutputSink.BaselineOffset = baselineOffset;
			mOutputSink.StrikethroughOffset = strikethroughOffset;
			mOutputSink.StrikethroughThickness = Convert.ToSingle(strikethroughThickness);
			mOutputSink.UnderlineOffset = underlineOffset;
			mOutputSink.UnderlineThickness = Convert.ToSingle(underlineThickness);
		}
	}
}