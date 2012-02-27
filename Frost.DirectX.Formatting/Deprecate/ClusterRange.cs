// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	public struct ClusterRange : IEquatable<ClusterRange>
	{
		public static readonly ClusterRange Empty;

		public readonly int End;
		public readonly int Length;
		public readonly int Start;

		static ClusterRange()
		{
			Empty = new ClusterRange(0, 0);
		}

		public ClusterRange(int start, int length)
		{
			Contract.Requires(start >= 0);
			Contract.Requires(length >= 0);

			Start = start;
			Length = length;

			End = (Start + Length) - 1;
		}

		public bool Equals(ClusterRange other)
		{
			return other.End == End && other.Length == Length && other.Start == Start;
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

			return obj is ClusterRange && Equals((ClusterRange)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = End;
				result = (result * 397) ^ Length;
				result = (result * 397) ^ Start;
				return result;
			}
		}

		public static bool operator ==(ClusterRange left, ClusterRange right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ClusterRange left, ClusterRange right)
		{
			return !left.Equals(right);
		}
	}
}