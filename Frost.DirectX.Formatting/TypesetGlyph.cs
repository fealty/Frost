// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct contains data for glyphs that have been typeset.
	/// </summary>
	internal struct TypesetGlyph : IEquatable<TypesetGlyph>
	{
		public float Advance;
		public short Index;
		public GlyphOffset Offset;

		public bool Equals(TypesetGlyph other)
		{
			return other.Advance.Equals(Advance) && other.Index == Index && other.Offset.Equals(Offset);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is TypesetGlyph && Equals((TypesetGlyph)obj);
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

		public static bool operator ==(TypesetGlyph left, TypesetGlyph right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TypesetGlyph left, TypesetGlyph right)
		{
			return !left.Equals(right);
		}
	}
}