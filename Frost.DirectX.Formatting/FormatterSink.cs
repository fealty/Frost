using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal sealed class FormatterSink
	{
		private readonly List<FormattedCluster> mClusters;
		private readonly List<FormattedGlyph> mGlyphs;
		private readonly List<FormattedRun> mRuns;
		private double mBaselineOffset;
		private string mFullText;
		private Rectangle mLayoutRegion;
		private double mLeading;
		private double mLineHeight;
		private double mStrikethroughOffset;
		private float mStrikethroughThickness;
		private double mUnderlineOffset;
		private float mUnderlineThickness;

		public FormatterSink()
		{
			mRuns = new List<FormattedRun>();

			mGlyphs = new List<FormattedGlyph>();

			mClusters = new List<FormattedCluster>();
		}

		public List<FormattedRun> Runs
		{
			get { return mRuns; }
		}

		public List<FormattedGlyph> Glyphs
		{
			get { return mGlyphs; }
		}

		public List<FormattedCluster> Clusters
		{
			get { return mClusters; }
		}

		public Rectangle LayoutRegion
		{
			get { return mLayoutRegion; }
			set
			{
				Contract.Requires(value.X >= double.MinValue && value.X <= double.MaxValue);
				Contract.Requires(value.Y >= double.MinValue && value.Y <= double.MaxValue);
				Contract.Requires(value.Width >= 0.0 && value.Width <= double.MaxValue);
				Contract.Requires(value.Height >= 0.0 && value.Height <= double.MaxValue);

				mLayoutRegion = value;
			}
		}

		public double LineHeight
		{
			get { return mLineHeight; }
			set
			{
				Contract.Requires(value >= 0.0 && value <= double.MaxValue);

				mLineHeight = value;
			}
		}

		public double Leading
		{
			get { return mLeading; }
			set
			{
				Contract.Requires(value >= 0.0 && value <= double.MaxValue);

				mLeading = value;
			}
		}

		public string FullText
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);

				return mFullText;
			}
			set
			{
				Contract.Requires(!string.IsNullOrEmpty(value));

				mFullText = value;
			}
		}

		public double BaselineOffset
		{
			get { return mBaselineOffset; }
			set
			{
				Contract.Requires(value >= double.MinValue && value <= double.MaxValue);

				mBaselineOffset = value;
			}
		}

		public double StrikethroughOffset
		{
			get { return mStrikethroughOffset; }
			set
			{
				Contract.Requires(value >= double.MinValue && value <= double.MaxValue);

				mStrikethroughOffset = value;
			}
		}

		public double UnderlineOffset
		{
			get { return mUnderlineOffset; }
			set
			{
				Contract.Requires(value >= double.MinValue && value <= double.MaxValue);

				mUnderlineOffset = value;
			}
		}

		public float StrikethroughThickness
		{
			get { return mStrikethroughThickness; }
			set
			{
				Contract.Requires(value >= 0.0f && value <= float.MaxValue);

				mStrikethroughThickness = value;
			}
		}

		public float UnderlineThickness
		{
			get { return mUnderlineThickness; }
			set
			{
				Contract.Requires(value >= 0.0f && value <= float.MaxValue);

				mUnderlineThickness = value;
			}
		}

		public void PreallocateGlyphs(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Glyphs.Capacity)
			{
				Glyphs.Capacity = count * 2;
			}
		}

		public void PreallocateClusters(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Clusters.Capacity)
			{
				Clusters.Capacity = count * 2;
			}
		}

		public void Reset(string fullText)
		{
			Contract.Requires(!string.IsNullOrEmpty(fullText));

			mRuns.Clear();
			mGlyphs.Clear();
			mClusters.Clear();

			mFullText = fullText;
		}
	}
}