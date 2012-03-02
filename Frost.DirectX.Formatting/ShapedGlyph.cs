// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct holds data for glyphs that have been shaped.
	/// </summary>
	internal struct ShapedGlyph : IEquatable<ShapedGlyph>
	{
		public float Advance;
		public ShapingGlyphProperties GlyphProperties;
		public short Index;
		public GlyphOffset Offset;

		public bool Equals(ShapedGlyph other)
		{
			return other.Advance.Equals(Advance) && other.GlyphProperties.Equals(GlyphProperties) &&
			       other.Index == Index && other.Offset.Equals(Offset);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is ShapedGlyph && Equals((ShapedGlyph)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Advance.GetHashCode();
				result = (result * 397) ^ GlyphProperties.GetHashCode();
				result = (result * 397) ^ Index.GetHashCode();
				result = (result * 397) ^ Offset.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(ShapedGlyph left, ShapedGlyph right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ShapedGlyph left, ShapedGlyph right)
		{
			return !left.Equals(right);
		}
	}
}