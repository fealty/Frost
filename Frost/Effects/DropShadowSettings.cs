// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	public struct DropShadowSettings : IEffectSettings, IEquatable<DropShadowSettings>
	{
		private readonly float _Amount;
		private readonly Color _Color;
		private readonly Size _Offset;
		private readonly Size _Scale;

		public DropShadowSettings(Color color, Size offset, float amount, Size scale)
		{
			Contract.Requires(Check.IsPositive(scale.Width));
			Contract.Requires(Check.IsPositive(scale.Height));

			_Color = color;
			_Offset = offset;
			_Amount = amount;
			_Scale = scale;

			Contract.Assert(Color.Equals(color));
			Contract.Assert(Offset.Equals(offset));
			Contract.Assert(Amount.Equals(amount));
			Contract.Assert(Scale.Equals(scale));
		}

		public Size Scale
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_Scale));

				return _Scale;
			}
		}

		public Size Offset
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_Offset));

				return _Offset;
			}
		}

		public Color Color
		{
			get
			{
				Contract.Ensures(Contract.Result<Color>().Equals(_Color));

				return _Color;
			}
		}

		public float Amount
		{
			get
			{
				Contract.Ensures(Contract.Result<float>().Equals(_Amount));

				return _Amount;
			}
		}

		public bool Equals(DropShadowSettings other)
		{
			return other._Amount.Equals(_Amount) && other._Color.Equals(_Color) &&
			       other._Offset.Equals(_Offset) && other._Scale.Equals(_Scale);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is DropShadowSettings && Equals((DropShadowSettings)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _Amount.GetHashCode();
				result = (result * 397) ^ _Color.GetHashCode();
				result = (result * 397) ^ _Offset.GetHashCode();
				result = (result * 397) ^ _Scale.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(DropShadowSettings left, DropShadowSettings right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DropShadowSettings left, DropShadowSettings right)
		{
			return !left.Equals(right);
		}
	}
}