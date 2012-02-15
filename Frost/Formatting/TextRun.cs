// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Frost.Formatting
{
	public struct TextRun : IEquatable<TextRun>
	{
		private readonly CultureInfo _Culture;
		private readonly string _Family;
		private readonly FontFeature[] _Features;

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
			FontFeature[] features)
		{
			Trace.Assert(Check.IsPositive(pointSize));
			Trace.Assert(Check.IsPositive(inline.Width));
			Trace.Assert(Check.IsPositive(inline.Height));

			this._TextRange = textRange;
			this._Culture = culture;
			this._Family = family;
			this._Stretch = stretch;
			this._Style = style;
			this._Weight = weight;
			this._PointSize = pointSize;
			this._HAlignment = hAlignment;
			this._VAlignment = vAlignment;
			this._Inline = inline;
			this._Features = features;

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
			Contract.Assert(Features == features);
		}

		public FontFeature[] Features
		{
			get
			{
				Contract.Ensures(Contract.Result<FontFeature[]>() == this._Features);

				return this._Features;
			}
		}

		public FontWeight Weight
		{
			get
			{
				Contract.Ensures(Contract.Result<FontWeight>() == this._Weight);

				return this._Weight;
			}
		}

		public FontStyle Style
		{
			get
			{
				Contract.Ensures(Contract.Result<FontStyle>() == this._Style);

				return this._Style;
			}
		}

		public FontStretch Stretch
		{
			get
			{
				Contract.Ensures(Contract.Result<FontStretch>() == this._Stretch);

				return this._Stretch;
			}
		}

		public IndexedRange TextRange
		{
			get { return this._TextRange; }
		}

		public Size Inline
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<Size>().Width));
				Contract.Ensures(Check.IsPositive(Contract.Result<Size>().Height));
				Contract.Ensures(Contract.Result<Size>().Equals(this._Inline));

				return this._Inline;
			}
		}

		public Alignment VAlignment
		{
			get
			{
				Contract.Ensures(Contract.Result<Alignment>() == this._VAlignment);

				return this._VAlignment;
			}
		}

		public Alignment HAlignment
		{
			get
			{
				Contract.Ensures(Contract.Result<Alignment>() == this._HAlignment);

				return this._HAlignment;
			}
		}

		public float PointSize
		{
			get
			{
				Contract.Ensures(Contract.Result<float>().Equals(this._PointSize));

				return this._PointSize;
			}
		}

		public string Family
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() == this._Family);

				return this._Family;
			}
		}

		public CultureInfo Culture
		{
			get
			{
				Contract.Ensures(Contract.Result<CultureInfo>() == this._Culture);

				return this._Culture;
			}
		}

		public bool Equals(TextRun other)
		{
			return Equals(other._Culture, this._Culture) &&
			       Equals(other._Family, this._Family) &&
			       Equals(other._Features, this._Features) &&
			       other._HAlignment == this._HAlignment &&
			       other._Inline.Equals(this._Inline) &&
			       other._PointSize.Equals(this._PointSize) &&
			       other._Stretch == this._Stretch && other._Style == this._Style &&
			       other._TextRange.Equals(this._TextRange) &&
			       other._VAlignment == this._VAlignment && other._Weight == this._Weight;
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
				int result = (this._Culture != null ? this._Culture.GetHashCode() : 0);
				result = (result * 397) ^
				         (this._Family != null ? this._Family.GetHashCode() : 0);
				result = (result * 397) ^
				         (this._Features != null ? this._Features.GetHashCode() : 0);
				result = (result * 397) ^ this._HAlignment.GetHashCode();
				result = (result * 397) ^ this._Inline.GetHashCode();
				result = (result * 397) ^ this._PointSize.GetHashCode();
				result = (result * 397) ^ this._Stretch.GetHashCode();
				result = (result * 397) ^ this._Style.GetHashCode();
				result = (result * 397) ^ this._TextRange.GetHashCode();
				result = (result * 397) ^ this._VAlignment.GetHashCode();
				result = (result * 397) ^ this._Weight.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return
				string.Format(
					"TextRange: {0}, Culture: {1}, Family: {2}, Stretch: {3}, Style: {4}, Weight: {5}, PointSize: {6}, HAlignment: {7}, VAlignment: {8}, Inline: {9}, Features: {10}",
					this._TextRange,
					this._Culture,
					this._Family,
					this._Stretch,
					this._Style,
					this._Weight,
					this._PointSize,
					this._HAlignment,
					this._VAlignment,
					this._Inline,
					this._Features);
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(Check.IsPositive(this._PointSize));
			Contract.Invariant(Check.IsPositive(this._Inline.Width));
			Contract.Invariant(Check.IsPositive(this._Inline.Height));
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

			Assert.TestObject(run1, run2);
		}
#endif
	}
}