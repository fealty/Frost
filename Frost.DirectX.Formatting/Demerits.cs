// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct provides storage for text formatting penalty values.
	/// </summary>
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

		/// <summary>
		///   This constructor initializes a new instance of this struct.
		/// </summary>
		/// <param name="value"> This parameter contains the penalty value. </param>
		public Demerits(double value)
		{
			Contract.Requires(Check.IsFinite(value));

			_Value = value;
		}

		/// <summary>
		///   This property contains the penalty value.
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
		///   This method determines whether a penalty value is positive infinity.
		/// </summary>
		/// <param name="value"> This parameter contains the value to test. </param>
		/// <returns> This method returns <c>true</c> if the value is positive infinity; otherwise, this method returns <c>false</c> . </returns>
		public static bool IsPositiveInfinity(Demerits value)
		{
			return value._Value >= Infinity._Value;
		}

		/// <summary>
		///   This method determines whether a penalty value is negative infinity.
		/// </summary>
		/// <param name="value"> This parameter contains the value to test. </param>
		/// <returns> This method returns <c>true</c> if the value is negative infinity; otherwise, this method returns <c>false</c> . </returns>
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