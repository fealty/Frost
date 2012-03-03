// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides storage for results from an <see cref="Shaper" /> .
	/// </summary>
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

		/// <summary>
		///   This property exposes the shaped glyphs.
		/// </summary>
		public List<ShapedGlyph> Glyphs
		{
			get
			{
				Contract.Ensures(Contract.Result<List<ShapedGlyph>>() != null);

				return _Glyphs;
			}
		}

		/// <summary>
		///   This property exposes the shaped clusters.
		/// </summary>
		public List<ShapedCluster> Clusters
		{
			get
			{
				Contract.Ensures(Contract.Result<List<ShapedCluster>>() != null);

				return _Clusters;
			}
		}

		/// <summary>
		///   This property contains the text that was formatted.
		/// </summary>
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

		/// <summary>
		///   This method ensures that the glyph list can hold the given count of glyphs.
		/// </summary>
		/// <param name="count"> This parameter indicates how many glyphs the list must hold. </param>
		public void PreallocateGlyphs(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Glyphs.Capacity - Glyphs.Count)
			{
				Glyphs.Capacity = count + Glyphs.Count;
			}
		}

		/// <summary>
		///   This method ensures that the clusters list can hold given count of clusters.
		/// </summary>
		/// <param name="count"> This parameter indicates how many clusters the list must hold. </param>
		public void PreallocateClusters(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Clusters.Capacity - Clusters.Count)
			{
				Clusters.Capacity = count + Clusters.Count;
			}
		}

		/// <summary>
		///   This method prepares the sink for shaping output.
		/// </summary>
		/// <param name="fullText"> This parameter references the text to be shaped. </param>
		public void Reset(string fullText)
		{
			Contract.Requires(!string.IsNullOrEmpty(fullText));

			_Glyphs.Clear();
			_Clusters.Clear();

			_FullText = fullText;
		}
	}
}