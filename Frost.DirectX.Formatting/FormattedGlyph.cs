// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct contains data for glyphs that have been formatted.
	/// </summary>
	internal struct FormattedGlyph : IEquatable<FormattedGlyph>
	{
		public float Advance;
		public short Index;
		public GlyphOffset Offset;

		public bool Equals(FormattedGlyph other)
		{
			return other.Advance.Equals(Advance) && other.Index == Index && other.Offset.Equals(Offset);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is FormattedGlyph && Equals((FormattedGlyph)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Advance.GetHashCode();
				result = (result * 397) ^ Index.GetHashCode();
				result = (result * 397) ^ Offset.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(FormattedGlyph left, FormattedGlyph right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FormattedGlyph left, FormattedGlyph right)
		{
			return !left.Equals(right);
		}
	}
}