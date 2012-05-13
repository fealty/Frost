// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Frost.Formatting
{
	/// <summary>
	///   represents zero or more demerits as required for the Knuth-Plass line breaking algorithm
	/// </summary>
	public struct Demerits : IEquatable<Demerits>
	{
		private static readonly Demerits _FlaggedPenalty;
		private static readonly Demerits _FitnessPenalty;
		private static readonly Demerits _Infinity;
		private static readonly Demerits _None;

		private readonly double _Value;

		static Demerits()
		{
			_FlaggedPenalty = new Demerits(100.0);
			_FitnessPenalty = new Demerits(3000.0);

			_Infinity = new Demerits(10000.0);

			_None = new Demerits(0.0);
		}

		/// <summary>
		///   constructs the <see cref="Demerits" />
		/// </summary>
		/// <param name="value"> the number of demerits </param>
		public Demerits(double value)
		{
			Contract.Requires(Check.IsFinite(value));

			_Value = value;
		}

		/// <summary>
		///   zero demerits
		/// </summary>
		public static Demerits None
		{
			get { return _None; }
		}

		/// <summary>
		///   infinite demerits
		/// </summary>
		public static Demerits Infinity
		{
			get { return _Infinity; }
		}

		/// <summary>
		///   the penalty incurred for fitness
		/// </summary>
		public static Demerits FitnessPenalty
		{
			get { return _FitnessPenalty; }
		}

		/// <summary>
		///   the penalty incurred for flagged
		/// </summary>
		public static Demerits FlaggedPenalty
		{
			get { return _FlaggedPenalty; }
		}

		/// <summary>
		///   gets the number of demerits
		/// </summary>
		public double Value
		{
			get { return _Value; }
		}

		public bool Equals(Demerits other)
		{
			return other._Value.Equals(_Value);
		}

		/// <summary>
		///   determines whether a penalty value is positive infinity
		/// </summary>
		/// <param name="value"> the demerit value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is positive infinity; otherwise, <c>false</c> </returns>
		public static bool IsPositiveInfinity(Demerits value)
		{
			return value._Value >= Infinity._Value;
		}

		/// <summary>
		///   determines whether a penalty value is negative infinity
		/// </summary>
		/// <param name="value"> the demerit value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is negative infinity; otherwise, <c>false</c> </returns>
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