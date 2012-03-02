// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides storage for the formatted results from a <see cref="Formatter" /> .
	/// </summary>
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

		/// <summary>
		///   This property exposes the formatted runs.
		/// </summary>
		public List<FormattedRun> Runs
		{
			get
			{
				Contract.Ensures(Contract.Result<List<FormattedRun>>() != null);

				return _Runs;
			}
		}

		/// <summary>
		///   This property exposes the formatted glyphs.
		/// </summary>
		public List<FormattedGlyph> Glyphs
		{
			get
			{
				Contract.Ensures(Contract.Result<List<FormattedGlyph>>() != null);

				return _Glyphs;
			}
		}

		/// <summary>
		///   This property exposes the formatted clusters.
		/// </summary>
		public List<FormattedCluster> Clusters
		{
			get
			{
				Contract.Ensures(Contract.Result<List<FormattedCluster>>() != null);

				return _Clusters;
			}
		}

		/// <summary>
		///   This property indicates the paragraph layout region.
		/// </summary>
		public Rectangle LayoutRegion
		{
			get { return _LayoutRegion; }
			set { _LayoutRegion = value; }
		}

		/// <summary>
		///   This property indicates the common line height.
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
		///   This property indicates the common leading.
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
				Contract.Requires(!string.IsNullOrEmpty(value));

				_FullText = value;
			}
		}

		/// <summary>
		///   This property indicates the offset from the origin to the baseline.
		/// </summary>
		public float BaselineOffset
		{
			get { return _BaselineOffset; }
			set
			{
				Contract.Requires(Check.IsFinite(value));

				_BaselineOffset = value;
			}
		}

		/// <summary>
		///   This property indicates the offset from the baseline to the strikethrough line.
		/// </summary>
		public float StrikethroughOffset
		{
			get { return _StrikethroughOffset; }
			set
			{
				Contract.Requires(Check.IsFinite(value));

				_StrikethroughOffset = value;
			}
		}

		/// <summary>
		///   This property indicates the offset from the baseline to the underline line.
		/// </summary>
		public float UnderlineOffset
		{
			get { return _UnderlineOffset; }
			set
			{
				Contract.Requires(Check.IsFinite(value));

				_UnderlineOffset = value;
			}
		}

		/// <summary>
		///   This property indicates the recommended thickness of the strikethrough line.
		/// </summary>
		public float StrikethroughThickness
		{
			get { return _StrikethroughThickness; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_StrikethroughThickness = value;
			}
		}

		/// <summary>
		///   This property indicates the recommended thickness of the underline line.
		/// </summary>
		public float UnderlineThickness
		{
			get { return _UnderlineThickness; }
			set
			{
				Contract.Requires(Check.IsPositive(value));

				_UnderlineThickness = value;
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
		///   This method prepares the sink for formatting output.
		/// </summary>
		/// <param name="fullText"> This parameter references the text to be formatted. </param>
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