// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;

namespace Frost.Formatting
{
	public struct TextRun : IEquatable<TextRun>
	{
		private readonly CultureInfo _Culture;
		private readonly string _Family;

		private readonly FontFeatureCollection _Features;

		private readonly Alignment _HAlignment;

		private readonly Size _Inline;
		private readonly float _PointSize;

		private readonly FontStretch _Stretch;
		private readonly FontStyle _Style;
		private readonly IndexedRange _TextRange;
		private readonly Alignment _VAlignment;
		private readonly FontWeight _Weight;

		public TextRun(
			IndexedRange textRange,
			CultureInfo culture,
			string family,
			FontStretch stretch,
			FontStyle style,
			FontWeight weight,
			float pointSize,
			Alignment hAlignment,
			Alignment vAlignment,
			Size inline,
			FontFeatureCollection features)
		{
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Requires(Check.IsPositive(inline.Width));
			Contract.Requires(Check.IsPositive(inline.Height));

			_TextRange = textRange;
			_Culture = culture;
			_Family = family;
			_Stretch = stretch;
			_Style = style;
			_Weight = weight;
			_PointSize = pointSize;
			_HAlignment = hAlignment;
			_VAlignment = vAlignment;
			_Inline = inline;
			_Features = features;

			Contract.Assert(TextRange.Equals(textRange));
			Contract.Assert(Culture == culture);
			Contract.Assert(Family == family);
			Contract.Assert(Stretch == stretch);
			Contract.Assert(Style == style);
			Contract.Assert(Weight == weight);
			Contract.Assert(PointSize.Equals(pointSize));
			Contract.Assert(HAlignment == hAlignment);
			Contract.Assert(VAlignment == vAlignment);
			Contract.Assert(Inline.Equals(inline));
			Contract.Assert(Equals(Features, features));
		}

		public FontFeatureCollection Features
		{
			get
			{
				Contract.Ensures(Contract.Result<FontFeatureCollection>() == _Features);

				return _Features;
			}
		}

		public FontWeight Weight
		{
			get
			{
				Contract.Ensures(Contract.Result<FontWeight>() == _Weight);

				return _Weight;
			}
		}

		public FontStyle Style
		{
			get
			{
				Contract.Ensures(Contract.Result<FontStyle>() == _Style);

				return _Style;
			}
		}

		public FontStretch Stretch
		{
			get
			{
				Contract.Ensures(Contract.Result<FontStretch>() == _Stretch);

				return _Stretch;
			}
		}

		public IndexedRange TextRange
		{
			get { return _TextRange; }
		}

		public Size Inline
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<Size>().Width));
				Contract.Ensures(Check.IsPositive(Contract.Result<Size>().Height));
				Contract.Ensures(Contract.Result<Size>().Equals(_Inline));

				return _Inline;
			}
		}

		public Alignment VAlignment
		{
			get
			{
				Contract.Ensures(Contract.Result<Alignment>() == _VAlignment);

				return _VAlignment;
			}
		}

		public Alignment HAlignment
		{
			get
			{
				Contract.Ensures(Contract.Result<Alignment>() == _HAlignment);

				return _HAlignment;
			}
		}

		public float PointSize
		{
			get
			{
				Contract.Ensures(Contract.Result<float>().Equals(_PointSize));

				return _PointSize;
			}
		}

		public string Family
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() == _Family);

				return _Family;
			}
		}

		public CultureInfo Culture
		{
			get
			{
				Contract.Ensures(Contract.Result<CultureInfo>() == _Culture);

				return _Culture;
			}
		}

		public bool Equals(TextRun other)
		{
			return Equals(other._Culture, _Culture) && Equals(other._Family, _Family) &&
			       Equals(other._Features, _Features) && other._HAlignment == _HAlignment &&
			       other._Inline.Equals(_Inline) && other._PointSize.Equals(_PointSize) &&
			       other._Stretch == _Stretch && other._Style == _Style &&
			       other._TextRange.Equals(_TextRange) && other._VAlignment == _VAlignment &&
			       other._Weight == _Weight;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is TextRun && Equals((TextRun)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (_Culture != null ? _Culture.GetHashCode() : 0);
				result = (result * 397) ^ (_Family != null ? _Family.GetHashCode() : 0);
				result = (result * 397) ^ (_Features != null ? _Features.GetHashCode() : 0);
				result = (result * 397) ^ _HAlignment.GetHashCode();
				result = (result * 397) ^ _Inline.GetHashCode();
				result = (result * 397) ^ _PointSize.GetHashCode();
				result = (result * 397) ^ _Stretch.GetHashCode();
				result = (result * 397) ^ _Style.GetHashCode();
				result = (result * 397) ^ _TextRange.GetHashCode();
				result = (result * 397) ^ _VAlignment.GetHashCode();
				result = (result * 397) ^ _Weight.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return
				string.Format(
					"TextRange: {0}, Culture: {1}, Family: {2}, Stretch: {3}, Style: {4}, Weight: {5}, PointSize: {6}, HAlignment: {7}, VAlignment: {8}, Inline: {9}, Features: {10}",
					_TextRange,
					_Culture,
					_Family,
					_Stretch,
					_Style,
					_Weight,
					_PointSize,
					_HAlignment,
					_VAlignment,
					_Inline,
					_Features);
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(Check.IsPositive(_PointSize));
			Contract.Invariant(Check.IsPositive(_Inline.Width));
			Contract.Invariant(Check.IsPositive(_Inline.Height));
		}

		public static bool operator ==(TextRun left, TextRun right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TextRun left, TextRun right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			TextRun run1 = new TextRun(
				IndexedRange.Empty,
				null,
				null,
				FontStretch.Regular,
				FontStyle.Regular,
				FontWeight.Regular,
				0.0f,
				Alignment.Stretch,
				Alignment.Stretch,
				Size.Empty,
				null);

			TextRun run2 = new TextRun(
				IndexedRange.Empty,
				null,
				null,
				FontStretch.Condensed,
				FontStyle.Italic,
				FontWeight.Bold,
				0.0f,
				Alignment.Stretch,
				Alignment.Stretch,
				Size.Empty,
				null);

			Assert.Equal(run1.Features, run2.Features);

			Assert.TestObject(run1, run2);
		}
#endif
	}
}