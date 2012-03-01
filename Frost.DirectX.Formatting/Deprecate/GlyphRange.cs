// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal struct GlyphRange : IEquatable<GlyphRange>
	{
		public readonly int End;
		public readonly int Length;
		public readonly int Start;

		public GlyphRange(int start, int length)
		{
			Contract.Requires(start >= 0);
			Contract.Requires(length >= 0);

			Start = start;
			Length = length;

			End = (Start + Length) - 1;
		}

		public bool Equals(GlyphRange other)
		{
			return other.Start == Start && other.Length == Length && other.End == End;
		}

		public override string ToString()
		{
			return string.Format("Start: {0}, Length: {1}, End: {2}", Start, Length, End);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is GlyphRange && Equals((GlyphRange)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Start;
				result = (result * 397) ^ Length;
				result = (result * 397) ^ End;
				return result;
			}
		}

		public static bool operator ==(GlyphRange left, GlyphRange right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GlyphRange left, GlyphRange right)
		{
			return !left.Equals(right);
		}
	}
}