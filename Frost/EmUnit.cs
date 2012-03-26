// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	// ReSharper disable InconsistentNaming
	public struct em : IEquatable<em>
	// ReSharper restore InconsistentNaming
	{
		private readonly float _Value;

		private static readonly em _Empty;
		private static readonly em _MaxValue;
		private static readonly em _MinValue;

		static em()
		{
			_Empty = new em(0.0);

			_MinValue = new em(float.MinValue);
			_MaxValue = new em(float.MaxValue);
		}

		public em(float value)
		{
			Contract.Requires(Check.IsFinite(value));

			_Value = value;
		}

		public em(double value)
		{
			Contract.Requires(Check.IsFinite(value));

			_Value = Convert.ToSingle(value);
		}

		public static implicit operator em(float value)
		{
			return new em(value);
		}

		public static implicit operator em(double value)
		{
			return new em(value);
		}

		public static em operator+(em left, em right)
		{
			return new em(left._Value + right._Value);
		}

		public static em operator-(em left, em right)
		{
			return new em(left._Value - right._Value);
		}

		public static em operator*(em left, em right)
		{
			return new em(left._Value * right._Value);
		}

		public static em operator/(em left, em right)
		{
			return new em(left._Value / right._Value);
		}

		public static em operator++(em instance)
		{
			return new em(instance._Value + 1.0f);
		}

		public static em operator--(em instance)
		{
			return new em(instance._Value - 1.0f);
		}

		public static bool operator>(em left, em right)
		{
			return left._Value > right._Value;
		}

		public static bool operator<(em left, em right)
		{
			return left._Value < right._Value;
		}

		public static bool operator<=(em left, em right)
		{
			return left._Value <= right._Value;
		}

		public static bool operator>=(em left, em right)
		{
			return left._Value >= right._Value;
		}

		public static em operator%(em left, em right)
		{
			return new em(left._Value % right._Value);
		}

		public static em MinValue
		{
			get { return _MinValue; }
		}

		public static em MaxValue
		{
			get { return _MaxValue; }
		}

		public static em Empty
		{
			get { return _Empty; }
		}

		public bool Equals(em other)
		{
			return other._Value.Equals(_Value);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is em && Equals((em)obj);
		}

		public override int GetHashCode()
		{
			return _Value.GetHashCode();
		}

		public static bool operator ==(em left, em right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(em left, em right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("{0}em", _Value);
		}
	}
}