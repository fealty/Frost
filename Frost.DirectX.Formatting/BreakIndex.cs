using System;

namespace Cabbage.Formatting
{
	public struct BreakIndex : IEquatable<BreakIndex>
	{
		public readonly int Index;
		public readonly double Ratio;

		public BreakIndex(int index, double ratio)
		{
			Index = index;
			Ratio = ratio;
		}

		public bool Equals(BreakIndex other)
		{
			return other.Index == Index && other.Ratio.Equals(Ratio);
		}

		public override string ToString()
		{
			return string.Format("Index: {0}, Ratio: {1}", Index, Ratio);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is BreakIndex && Equals((BreakIndex)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Index * 397) ^ Ratio.GetHashCode();
			}
		}

		public static bool operator ==(BreakIndex left, BreakIndex right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(BreakIndex left, BreakIndex right)
		{
			return !left.Equals(right);
		}
	}
}