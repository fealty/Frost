using System;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting
{
	public struct Demerits : IEquatable<Demerits>
	{
		public static readonly Demerits FlaggedPenalty;
		public static readonly Demerits FitnessPenalty;

		public static readonly Demerits Infinity;

		public static readonly Demerits None;

		public readonly double Value;

		static Demerits()
		{
			FlaggedPenalty = new Demerits(100.0);
			FitnessPenalty = new Demerits(3000.0);

			Infinity = new Demerits(10000.0);

			None = new Demerits(0.0);
		}

		public Demerits(double value)
		{
			Contract.Requires(value >= double.MinValue && value <= double.MaxValue);

			Value = value;
		}

		public bool Equals(Demerits other)
		{
			return other.Value.Equals(Value);
		}

		public static bool IsPositiveInfinity(Demerits value)
		{
			return value.Value >= Infinity.Value;
		}

		public static bool IsNegativeInfinity(Demerits value)
		{
			return value.Value <= -Infinity.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Demerits && Equals((Demerits)obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public static implicit operator Demerits(double value)
		{
			Contract.Requires(value >= double.MinValue && value <= double.MaxValue);

			return new Demerits(value);
		}

		public static bool operator >(Demerits a, Demerits b)
		{
			return a.Value > b.Value;
		}

		public static bool operator <(Demerits a, Demerits b)
		{
			return a.Value < b.Value;
		}

		public static bool operator >=(Demerits a, Demerits b)
		{
			return a.Value >= b.Value;
		}

		public static bool operator <=(Demerits a, Demerits b)
		{
			return a.Value <= b.Value;
		}

		public static Demerits operator +(Demerits a, Demerits b)
		{
			return new Demerits(a.Value + b.Value);
		}

		public static Demerits operator -(Demerits a, Demerits b)
		{
			return new Demerits(a.Value - b.Value);
		}

		public static Demerits operator *(Demerits a, Demerits b)
		{
			return new Demerits(a.Value * b.Value);
		}

		public static Demerits operator /(Demerits a, Demerits b)
		{
			return new Demerits(a.Value / b.Value);
		}

		public static Demerits operator ++(Demerits a)
		{
			return new Demerits(a.Value + 1.0);
		}

		public static Demerits operator --(Demerits a)
		{
			return new Demerits(a.Value - 1.0);
		}

		public static bool operator ==(Demerits left, Demerits right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Demerits left, Demerits right)
		{
			return !left.Equals(right);
		}
	}
}