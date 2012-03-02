// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Globalization;

using Frost.Formatting;

using SharpDX.DirectWrite;

using FontStretch = Frost.Formatting.FontStretch;
using FontStyle = Frost.Formatting.FontStyle;
using FontWeight = Frost.Formatting.FontWeight;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct provides storage for all available character formatting properties.
	/// </summary>
	internal struct CharacterFormat : IEquatable<CharacterFormat>
	{
		public byte BidiLevel;
		public LineBreakpoint Breakpoint;
		public CultureInfo Culture;
		public string Family;
		public FontFeatureCollection Features;
		public Alignment HAlignment;
		public Size Inline;
		public NumberSubstitution NumberSubstitution;
		public float PointSize;
		public ScriptAnalysis ScriptAnalysis;
		public FontStretch Stretch;
		public FontStyle Style;
		public Alignment VAlignment;
		public FontWeight Weight;

		public bool Equals(CharacterFormat other)
		{
			return other.BidiLevel == BidiLevel && Equals(other.Culture, Culture) &&
			       Equals(other.Family, Family) && Equals(other.Features, Features) &&
			       other.Inline.Equals(Inline) && other.Breakpoint.Equals(Breakpoint) &&
			       Equals(other.NumberSubstitution, NumberSubstitution) && other.PointSize.Equals(PointSize) &&
			       other.ScriptAnalysis.Equals(ScriptAnalysis) && other.Stretch == Stretch &&
			       other.Style == Style && other.Weight == Weight && other.HAlignment == HAlignment &&
			       other.VAlignment == VAlignment;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is CharacterFormat && Equals((CharacterFormat)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = BidiLevel.GetHashCode();

				result = (result * 397) ^ (Culture != null ? Culture.GetHashCode() : 0);
				result = (result * 397) ^ (Family != null ? Family.GetHashCode() : 0);
				result = (result * 397) ^ (Features != null ? Features.GetHashCode() : 0);
				result = (result * 397) ^ Inline.GetHashCode();
				result = (result * 397) ^ Breakpoint.GetHashCode();
				result = (result * 397) ^ (NumberSubstitution != null ? NumberSubstitution.GetHashCode() : 0);
				result = (result * 397) ^ PointSize.GetHashCode();
				result = (result * 397) ^ ScriptAnalysis.GetHashCode();

				int stretch = (int)Stretch;

				result = (result * 397) ^ stretch.GetHashCode();

				int style = (int)Style;

				result = (result * 397) ^ style.GetHashCode();

				int weight = (int)Weight;

				result = (result * 397) ^ weight.GetHashCode();

				int hAlignment = (int)HAlignment;

				result = (result * 397) ^ hAlignment.GetHashCode();

				int vAlignment = (int)VAlignment;

				result = (result * 397) ^ vAlignment.GetHashCode();

				return result;
			}
		}

		public static bool operator ==(CharacterFormat left, CharacterFormat right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(CharacterFormat left, CharacterFormat right)
		{
			return !left.Equals(right);
		}
	}
}