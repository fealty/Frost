// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	// ReSharper disable InconsistentNaming
	public struct px : IEquatable<px>
	// ReSharper restore InconsistentNaming
	{
		private readonly float _Value;

		private static readonly px _Empty;
		private static readonly px _MaxValue;
		private static readonly px _MinValue;

		static px()
		{
			_Empty = new px(0.0);

			_MinValue = new px(float.MinValue);
			_MaxValue = new px(float.MaxValue);
		}

		public static implicit operator px(float value)
		{
			return new px(value);
		}

		public static implicit operator px(double value)
		{
			return new px(value);
		}

		public px(float value)
		{
			Contract.Requires(Check.IsFinite(value));

			_Value = value;
		}

		public px(double value)
		{
			Contract.Requires(Check.IsFinite(value));

			_Value = Convert.ToSingle(value);
		}

		public static px operator +(px left, px right)
		{
			return new px(left._Value + right._Value);
		}

		public static px operator -(px left, px right)
		{
			return new px(left._Value - right._Value);
		}

		public static px operator *(px left, px right)
		{
			return new px(left._Value * right._Value);
		}

		public static px operator /(px left, px right)
		{
			return new px(left._Value / right._Value);
		}

		public static px operator ++(px instance)
		{
			return new px(instance._Value + 1.0f);
		}

		public static px operator --(px instance)
		{
			return new px(instance._Value - 1.0f);
		}

		public static bool operator >(px left, px right)
		{
			return left._Value > right._Value;
		}

		public static bool operator <(px left, px right)
		{
			return left._Value < right._Value;
		}

		public static bool operator <=(px left, px right)
		{
			return left._Value <= right._Value;
		}

		public static bool operator >=(px left, px right)
		{
			return left._Value >= right._Value;
		}

		public static px operator %(px left, px right)
		{
			return new px(left._Value % right._Value);
		}

		public static px MinValue
		{
			get { return _MinValue; }
		}

		public static px MaxValue
		{
			get { return _MaxValue; }
		}

		public static px Empty
		{
			get { return _Empty; }
		}

		public bool Equals(px other)
		{
			return other._Value.Equals(_Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is px && Equals((px)obj);
		}

		public override int GetHashCode()
		{
			return _Value.GetHashCode();
		}

		public static bool operator ==(px left, px right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(px left, px right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("{0}px", _Value);
		}
	}
}