using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal sealed class ShaperSink
	{
		private readonly List<ShapedCluster> mClusters;
		private readonly List<ShapedGlyph> mGlyphs;
		private string mFullText;

		public ShaperSink()
		{
			mGlyphs = new List<ShapedGlyph>();

			mClusters = new List<ShapedCluster>();
		}

		public List<ShapedGlyph> Glyphs
		{
			get { return mGlyphs; }
		}

		public List<ShapedCluster> Clusters
		{
			get { return mClusters; }
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
				Contract.Requires(!string.IsNullOrEmpty(FullText));

				mFullText = value;
			}
		}

		public void PreallocateGlyphs(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Glyphs.Capacity - Glyphs.Count)
			{
				Glyphs.Capacity = count + Glyphs.Count;
			}
		}

		public void PreallocateClusters(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Clusters.Capacity - Clusters.Count)
			{
				Clusters.Capacity = count + Clusters.Count;
			}
		}

		public void Reset(string fullText)
		{
			Contract.Requires(!string.IsNullOrEmpty(fullText));

			mGlyphs.Clear();
			mClusters.Clear();

			mFullText = fullText;
		}
	}
}