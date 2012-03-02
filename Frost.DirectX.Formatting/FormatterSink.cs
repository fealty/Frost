// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal sealed class FormatterSink
	{
		private readonly List<FormattedCluster> _Clusters;
		private readonly List<FormattedGlyph> _Glyphs;
		private readonly List<FormattedRun> _Runs;

		private float _BaselineOffset;
		private string _FullText;
		private Rectangle _LayoutRegion;
		private float _Leading;
		private float _LineHeight;
		private float _StrikethroughOffset;
		private float _StrikethroughThickness;
		private float _UnderlineOffset;
		private float _UnderlineThickness;

		public FormatterSink()
		{
			_Runs = new List<FormattedRun>();

			_Glyphs = new List<FormattedGlyph>();

			_Clusters = new List<FormattedCluster>();
		}

		public List<FormattedRun> Runs
		{
			get
			{
				Contract.Ensures(Contract.Result<List<FormattedRun>>() != null);
				
				return _Runs;
			}
		}

		public List<FormattedGlyph> Glyphs
		{
			get
			{
				Contract.Ensures(Contract.Result<List<FormattedGlyph>>() != null);
				
				return _Glyphs;
			}
		}

		public List<FormattedCluster> Clusters
		{
			get
			{
				Contract.Ensures(Contract.Result<List<FormattedCluster>>() != null);
				
				return _Clusters;
			}
		}

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

		public float Leading
		{
			get { return _Leading; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_Leading = value;
			}
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

		public float BaselineOffset
		{
			get { return _BaselineOffset; }
			set
			{
				Contract.Requires(Check.IsFinite(value));

				_BaselineOffset = value;
			}
		}

		public float StrikethroughOffset
		{
			get { return _StrikethroughOffset; }
			set
			{
				Contract.Requires(Check.IsFinite(value));

				_StrikethroughOffset = value;
			}
		}

		public float UnderlineOffset
		{
			get { return _UnderlineOffset; }
			set
			{
				Contract.Requires(Check.IsFinite(value));

				_UnderlineOffset = value;
			}
		}

		public float StrikethroughThickness
		{
			get { return _StrikethroughThickness; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_StrikethroughThickness = value;
			}
		}

		public float UnderlineThickness
		{
			get { return _UnderlineThickness; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_UnderlineThickness = value;
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

			_Runs.Clear();
			_Glyphs.Clear();
			_Clusters.Clear();

			_FullText = fullText;
		}
	}
}