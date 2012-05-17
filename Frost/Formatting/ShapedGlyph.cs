// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	public struct ShapedGlyph : IEquatable<ShapedGlyph>
	{
		private readonly float _Advance;
		private readonly short _Index;
		private readonly Size _Offset;

		public ShapedGlyph(float advance, short index, Size offset)
		{
			Contract.Requires(Check.IsFinite(advance));
			Contract.Requires(Check.IsPositive(index));

			_Advance = advance;
			_Index = index;
			_Offset = offset;
		}

		public bool Equals(ShapedGlyph other)
		{
			return other._Advance.Equals(_Advance) &&
				other._Index == _Index &&
					other._Offset.Equals(_Offset);
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
				int result = _Advance.GetHashCode();
				result = (result * 397) ^ _Index.GetHashCode();
				result = (result * 397) ^ _Offset.GetHashCode();
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

		public override string ToString()
		{
			return string.Format(
				"Advance: {0}, Index: {1}, Offset: {2}", _Advance, _Index, _Offset);
		}
	}
}