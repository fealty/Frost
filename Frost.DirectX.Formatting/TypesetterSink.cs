// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal sealed class TypesetterSink
	{
		private readonly List<TypesetCluster> _Clusters;
		private readonly List<TypesetGlyph> _Glyphs;
		private readonly List<float> _LineLengths;
		private readonly List<Rectangle> _Lines;

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

		public List<TypesetGlyph> Glyphs
		{
			get { return _Glyphs; }
		}

		public List<TypesetCluster> Clusters
		{
			get { return _Clusters; }
		}

		public List<Rectangle> Lines
		{
			get { return _Lines; }
		}

		public Alignment Alignment { get; set; }

		public Rectangle LayoutRegion
		{
			get { return _LayoutRegion; }
			set { _LayoutRegion = value; }
		}

		public float LineHeight
		{
			get { return _LineHeight; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_LineHeight = value;
			}
		}

		public float Indentation
		{
			get { return _Indentation; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_Indentation = value;
			}
		}

		public float Leading
		{
			get { return _Leading; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_Leading = value;
			}
		}

		public List<float> LineLengths
		{
			get { return _LineLengths; }
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
				Contract.Requires(!string.IsNullOrEmpty(value));

				_FullText = value;
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

			_Lines.Clear();
			_Glyphs.Clear();
			_Clusters.Clear();
			_LineLengths.Clear();

			_FullText = fullText;
		}
	}
}