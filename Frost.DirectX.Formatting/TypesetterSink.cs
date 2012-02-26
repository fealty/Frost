using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal sealed class TypesetterSink
	{
		private readonly List<TypesetCluster> mClusters;
		private readonly List<TypesetGlyph> mGlyphs;
		private readonly List<double> mLineLengths;
		private readonly List<Rectangle> mLines;

		private string mFullText;
		private double mIndentation;
		private Rectangle mLayoutRegion;
		private double mLeading;
		private double mLineHeight;

		public TypesetterSink()
		{
			mLines = new List<Rectangle>();

			mGlyphs = new List<TypesetGlyph>();

			mClusters = new List<TypesetCluster>();

			mLineLengths = new List<double>();
		}

		public List<TypesetGlyph> Glyphs
		{
			get { return mGlyphs; }
		}

		public List<TypesetCluster> Clusters
		{
			get { return mClusters; }
		}

		public List<Rectangle> Lines
		{
			get { return mLines; }
		}

		public Alignment Alignment { get; set; }

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

		public double Indentation
		{
			get { return mIndentation; }
			set
			{
				Contract.Requires(value >= 0.0 && value <= double.MaxValue);

				mIndentation = value;
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

		public List<double> LineLengths
		{
			get { return mLineLengths; }
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

		public void PreallocateClusters(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Clusters.Capacity)
			{
				Clusters.Capacity = count * 2;
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

		public void Reset(string fullText)
		{
			Contract.Requires(!string.IsNullOrEmpty(fullText));

			mLines.Clear();
			mGlyphs.Clear();
			mClusters.Clear();
			mLineLengths.Clear();

			mFullText = fullText;
		}
	}
}