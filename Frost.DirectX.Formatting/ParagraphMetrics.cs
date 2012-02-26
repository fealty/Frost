using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	public sealed class ParagraphMetrics
		: ITextMetrics,
		  ILightweightList<Rectangle>,
		  ILightweightList<TextRange>,
		  ILightweightList<Outline>
	{
		private static readonly List<TextRange> mLineBuilder;

		private readonly double mBaselineOffset;
		private readonly byte[] mClusterBidiLevels;
		private readonly Outline[] mOutlines;

		private readonly FormattedCluster[] mClusters;
		private readonly Rectangle mLayoutRegion;
		private readonly double mLeading;
		private readonly TextRange[] mLines;
		private readonly Paragraph mParagraph;

		private readonly Rectangle mTextRegion;
		private readonly int[] mTextToCluster;

		static ParagraphMetrics()
		{
			mLineBuilder = new List<TextRange>();
		}

		internal ParagraphMetrics(
			Paragraph paragraph,
			FormatterSink formattedData,
			TextGeometryCache geometryCache)
		{
			Contract.Requires(paragraph != null);
			Contract.Requires(formattedData != null);
			Contract.Requires(geometryCache != null);

			mLineBuilder.Clear();

			mParagraph = paragraph;

			mLeading = formattedData.Leading;
			mLayoutRegion = formattedData.LayoutRegion;
			mBaselineOffset = formattedData.BaselineOffset;

			mClusters = formattedData.Clusters.ToArray();

			mClusterBidiLevels = new byte[formattedData.Clusters.Count];

			mOutlines = new Outline[formattedData.Clusters.Count];

			int geometryClusterIndex = 0;

			for(int i = 0; i < formattedData.Runs.Count; ++i)
			{
				FormattedRun run = formattedData.Runs[i];

				FontFace face = run.Font.ResolveFace();

				double emSize = face.Metrics.Ascent + face.Metrics.Descent;

				double baseline = face.Metrics.Ascent / emSize;

				for(int j = run.Clusters.Start; j <= run.Clusters.End; ++j)
				{
					Geometry geometry = geometryCache.Retrieve(
						j, run.BidiLevel, run.Font, formattedData);

					mOutlines[geometryClusterIndex] =
						new Outline(geometry, run.EmSize, baseline);

					mClusterBidiLevels[geometryClusterIndex] = run.BidiLevel;

					geometryClusterIndex++;
				}
			}

			mTextToCluster = new int[formattedData.FullText.Length];

			for(int i = 0; i < mClusters.Length; ++i)
			{
				TextRange characters = mClusters[i].Characters;

				for(int j = characters.Start; j <= characters.End; ++j)
				{
					mTextToCluster[j] = i;
				}
			}

			double left = double.MaxValue;
			double top = double.MaxValue;
			double right = double.MinValue;
			double bottom = double.MinValue;

			for(int i = 0; i < mClusters.Length; ++i)
			{
				left = Math.Min(left, mClusters[i].Region.Left);
				top = Math.Min(top, mClusters[i].Region.Top);
				right = Math.Max(right, mClusters[i].Region.Right);
				bottom = Math.Max(bottom, mClusters[i].Region.Bottom);
			}

			mTextRegion = new Rectangle(left, top, right - left, bottom - top);

			int lastLine = 0;

			ClusterRange range = ClusterRange.Empty;

			for(int i = 0; i < formattedData.Runs.Count; ++i)
			{
				if(formattedData.Runs[i].LineNumber > lastLine)
				{
					mLineBuilder.Add(ToTextRange(range));

					range = new ClusterRange(formattedData.Runs[i].Clusters.Start, 0);

					lastLine = formattedData.Runs[i].LineNumber;
				}

				range = new ClusterRange(
					range.Start, range.Length + formattedData.Runs[i].Clusters.Length);
			}

			if(range.Length > 0)
			{
				mLineBuilder.Add(ToTextRange(range));
			}

			mLines = mLineBuilder.ToArray();
		}

		Outline ILightweightList<Outline>.this[int index]
		{
			get { return mOutlines[index]; }
		}

		int ILightweightList<Outline>.Count
		{
			get { return mOutlines.Length; }
		}

		IEnumerator<Outline> IEnumerable<Outline>.GetEnumerator()
		{
			ILightweightList<Outline> @this = this;

			return @this.GetEnumerator();
		}

		int ILightweightList<Rectangle>.Count
		{
			get { return mTextToCluster.Length; }
		}

		Rectangle ILightweightList<Rectangle>.this[int index]
		{
			get { return mClusters[mTextToCluster[index]].Region; }
		}

		IEnumerator<Rectangle> IEnumerable<Rectangle>.GetEnumerator()
		{
			ILightweightList<Rectangle> @this = this;

			for(int i = 0; i < @this.Count; ++i)
			{
				yield return @this[i];
			}
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotSupportedException();
		}

		TextRange ILightweightList<TextRange>.this[int index]
		{
			get { return mLines[index]; }
		}

		int ILightweightList<TextRange>.Count
		{
			get { return mLines.Length; }
		}

		IEnumerator<TextRange> IEnumerable<TextRange>.GetEnumerator()
		{
			ILightweightList<TextRange> @this = this;

			for(int i = 0; i < @this.Count; ++i)
			{
				yield return @this[i];
			}
		}

		ILightweightList<TextRange> ITextMetrics.Lines
		{
			get { return this; }
		}

		public ILightweightList<Rectangle> Regions
		{
			get { return this; }
		}

		public ILightweightList<Outline> Outlines
		{
			get { return this; }
		}

		public Paragraph Paragraph
		{
			get { return mParagraph; }
		}

		public Rectangle TextRegion
		{
			get { return mTextRegion; }
		}

		public Rectangle LayoutRegion
		{
			get { return mLayoutRegion; }
		}

		public double Leading
		{
			get { return mLeading; }
		}

		public Size BaselineOffset
		{
			get { return new Size(0.0, mBaselineOffset); }
		}

		public bool IsRightToLeft(int textIndex)
		{
			int clusterIndex = mTextToCluster[textIndex];

			return Convert.ToBoolean(mClusterBidiLevels[clusterIndex] & 1);
		}

		public bool IsClusterStart(int textIndex)
		{
			if(textIndex > 0)
			{
				if(mTextToCluster[textIndex] == mTextToCluster[textIndex - 1])
				{
					return false;
				}
			}

			return true;
		}

		public bool IsClusterVisible(int textIndex)
		{
			int clusterIndex = mTextToCluster[textIndex];

			if(mClusters[clusterIndex].Display != DisplayMode.Suppressed)
			{
				if(mClusters[clusterIndex].Display == DisplayMode.Neutral)
				{
					if(!mClusters[clusterIndex].Region.IsEmpty)
					{
						return true;
					}
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		public bool FindIndexForPoint(Point position, out int textIndex)
		{
			int nearestCluster = 0;

			for(int i = 0; i < mClusters.Length; ++i)
			{
				if(mClusters[i].Region.Contains(position))
				{
					textIndex = mClusters[i].Characters.Start;

					return true;
				}

				Point reference = mClusters[i].Region.FindCenter();

				double referenceX = position.X - reference.X;
				double referenceY = position.Y - reference.Y;

				referenceX *= referenceX;
				referenceY *= referenceY;

				Point nearest = mClusters[nearestCluster].Region.FindCenter();

				double nearestX = position.X - nearest.X;
				double nearestY = position.Y - nearest.Y;

				nearestX *= nearestX;
				nearestY *= nearestY;

				double referenceTotal = referenceX + referenceY;
				double nearestTotal = nearestX + nearestY;

				nearestCluster = referenceTotal > nearestTotal ? i : nearestCluster;
			}

			textIndex = mClusters[nearestCluster].Characters.Start;

			return false;
		}

		public void ComputeRegion(TextRange range, out Rectangle region)
		{
			double left = double.MaxValue;
			double top = double.MaxValue;
			double right = double.MinValue;
			double bottom = double.MinValue;

			for(int i = range.Start; i <= range.End; ++i)
			{
				// exclude invisible clusters from consideration
				if(IsClusterVisible(i))
				{
					int cluster = mTextToCluster[i];

					left = Math.Min(left, mClusters[cluster].Region.Left);
					top = Math.Min(top, mClusters[cluster].Region.Top);
					right = Math.Max(right, mClusters[cluster].Region.Right);
					bottom = Math.Max(bottom, mClusters[cluster].Region.Bottom);
				}
			}

			region = new Rectangle(left, top, right - left, bottom - top);
		}

		private TextRange ToTextRange(ClusterRange clusters)
		{
			int textLength = 0;

			for(int i = clusters.Start; i <= clusters.End; ++i)
			{
				textLength += mClusters[i].Characters.Length;
			}

			return new TextRange(
				mClusters[clusters.Start].Characters.Start, textLength);
		}
	}
}