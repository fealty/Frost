// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Frost.DirectX.Formatting
{
	internal struct Demerits : IEquatable<Demerits>
	{
		public static readonly Demerits FlaggedPenalty;
		public static readonly Demerits FitnessPenalty;

		public static readonly Demerits Infinity;

		public static readonly Demerits None;

		private readonly double _Value;

		static Demerits()
		{
			FlaggedPenalty = new Demerits(100.0);
			FitnessPenalty = new Demerits(3000.0);

			Infinity = new Demerits(10000.0);

			None = new Demerits(0.0);
		}

		public Demerits(double value)
		{
			Contract.Requires(Check.IsFinite(value));

			_Value = value;
		}

		public double Value
		{
			get { return _Value; }
		}

		public bool Equals(Demerits other)
		{
			return other._Value.Equals(_Value);
		}

		public static bool IsPositiveInfinity(Demerits value)
		{
			return value._Value >= Infinity._Value;
		}

		public static bool IsNegativeInfinity(Demerits value)
		{
			return value._Value <= -Infinity._Value;
		}

		public override string ToString()
		{
			return _Value.ToString(CultureInfo.InvariantCulture);
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
			return _Value.GetHashCode();
		}

		public static implicit operator Demerits(double value)
		{
			Contract.Requires(value >= double.MinValue && value <= double.MaxValue);

			return new Demerits(value);
		}

		public static bool operator >(Demerits a, Demerits b)
		{
			return a._Value > b._Value;
		}

		public static bool operator <(Demerits a, Demerits b)
		{
			return a._Value < b._Value;
		}

		public static bool operator >=(Demerits a, Demerits b)
		{
			return a._Value >= b._Value;
		}

		public static bool operator <=(Demerits a, Demerits b)
		{
			return a._Value <= b._Value;
		}

		public static Demerits operator +(Demerits a, Demerits b)
		{
			return new Demerits(a._Value + b._Value);
		}

		public static Demerits operator -(Demerits a, Demerits b)
		{
			return new Demerits(a._Value - b._Value);
		}

		public static Demerits operator *(Demerits a, Demerits b)
		{
			return new Demerits(a._Value * b._Value);
		}

		public static Demerits operator /(Demerits a, Demerits b)
		{
			return new Demerits(a._Value / b._Value);
		}

		public static Demerits operator ++(Demerits a)
		{
			return new Demerits(a._Value + 1.0);
		}

		public static Demerits operator --(Demerits a)
		{
			return new Demerits(a._Value - 1.0);
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