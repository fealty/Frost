// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal sealed class ShaperSink
	{
		private readonly List<ShapedCluster> _Clusters;
		private readonly List<ShapedGlyph> _Glyphs;

		private string _FullText;

		public ShaperSink()
		{
			_Glyphs = new List<ShapedGlyph>();

			_Clusters = new List<ShapedCluster>();
		}

		public List<ShapedGlyph> Glyphs
		{
			get { return _Glyphs; }
		}

		public List<ShapedCluster> Clusters
		{
			get { return _Clusters; }
		}

		public string FullText
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);

				return _FullText;
			}
			set
			{
				Contract.Requires(!string.IsNullOrEmpty(FullText));

				_FullText = value;
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

			_Glyphs.Clear();
			_Clusters.Clear();

			_FullText = fullText;
		}
	}
}