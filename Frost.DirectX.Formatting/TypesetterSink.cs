// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides storage for the typeset results from a <see cref="Typesetter" /> .
	/// </summary>
	internal sealed class TypesetterSink
	{
		private readonly List<TypesetCluster> _Clusters;
		private readonly List<TypesetGlyph> _Glyphs;
		private readonly List<float> _LineLengths;
		private readonly List<Rectangle> _Lines;

		private Alignment _Alignment;
		private string _FullText;
		private float _Indentation;
		private Rectangle _LayoutRegion;
		private float _Leading;
		private float _LineHeight;

		public TypesetterSink()
		{
			_Lines = new List<Rectangle>();
			_Glyphs = new List<TypesetGlyph>();
			_Clusters = new List<TypesetCluster>();
			_LineLengths = new List<float>();
		}

		/// <summary>
		///   This property exposes the typeset glyphs.
		/// </summary>
		public List<TypesetGlyph> Glyphs
		{
			get
			{
				Contract.Ensures(Contract.Result<List<TypesetGlyph>>() != null);

				return _Glyphs;
			}
		}

		/// <summary>
		///   This property exposes the typeset clusters.
		/// </summary>
		public List<TypesetCluster> Clusters
		{
			get
			{
				Contract.Ensures(Contract.Result<List<TypesetCluster>>() != null);

				return _Clusters;
			}
		}

		/// <summary>
		///   This property exposes the typeset lines.
		/// </summary>
		public List<Rectangle> Lines
		{
			get
			{
				Contract.Ensures(Contract.Result<List<Rectangle>>() != null);

				return _Lines;
			}
		}

		/// <summary>
		///   This property indicates the typeset paragraph alignment.
		/// </summary>
		public Alignment Alignment
		{
			get { return _Alignment; }
			set { _Alignment = value; }
		}

		/// <summary>
		///   This property indicates the layout region the paragraph was typeset within.
		/// </summary>
		public Rectangle LayoutRegion
		{
			get { return _LayoutRegion; }
			set { _LayoutRegion = value; }
		}

		/// <summary>
		///   This property indicates the height of each typeset line.
		/// </summary>
		public float LineHeight
		{
			get { return _LineHeight; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_LineHeight = value;
			}
		}

		/// <summary>
		///   This property indicates the indentation at the beginning of the paragraph.
		/// </summary>
		public float Indentation
		{
			get { return _Indentation; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_Indentation = value;
			}
		}

		/// <summary>
		///   This property indicates the leading for each typeset line.
		/// </summary>
		public float Leading
		{
			get { return _Leading; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_Leading = value;
			}
		}

		/// <summary>
		///   This property exposes the length of each typeset line.
		/// </summary>
		public List<float> LineLengths
		{
			get
			{
				Contract.Ensures(Contract.Result<List<float>>() != null);

				return _LineLengths;
			}
		}

		/// <summary>
		///   This property contains the text that was typeset.
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
				Contract.Requires(!string.IsNullOrEmpty(value));

				_FullText = value;
			}
		}

		/// <summary>
		///   This method ensures that the clusters list can hold given count of clusters.
		/// </summary>
		/// <param name="count"> This parameter indicates how many clusters the list must hold. </param>
		public void PreallocateClusters(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Clusters.Capacity)
			{
				Clusters.Capacity = count * 2;
			}
		}

		/// <summary>
		///   This method ensures that the glyph list can hold the given count of glyphs.
		/// </summary>
		/// <param name="count"> This parameter indicates how many glyphs the list must hold. </param>
		public void PreallocateGlyphs(int count)
		{
			Contract.Requires(count >= 0);

			if(count > Glyphs.Capacity)
			{
				Glyphs.Capacity = count * 2;
			}
		}

		/// <summary>
		///   This method prepares the sink for formatting output.
		/// </summary>
		/// <param name="fullText"> This parameter references the text to be typeset. </param>
		public void Reset(string fullText)
		{
			Contract.Requires(!string.IsNullOrEmpty(fullText));

			_Lines.Clear();
			_Glyphs.Clear();
			_Clusters.Clear();
			_LineLengths.Clear();

			_FullText = fullText;
		}
	}
}