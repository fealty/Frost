// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	public struct DropShadowSettings
		: IEffectSettings, IEquatable<DropShadowSettings>
	{
		private readonly float _Amount;
		private readonly Color _Color;
		private readonly Size _Offset;
		private readonly Size _Scale;

		public DropShadowSettings(
			Color color, Size offset, float amount, Size scale)
		{
			Contract.Requires(Check.IsPositive(scale.Width));
			Contract.Requires(Check.IsPositive(scale.Height));

			this._Color = color;
			this._Offset = offset;
			this._Amount = amount;
			this._Scale = scale;

			Contract.Assert(Color.Equals(color));
			Contract.Assert(Offset.Equals(offset));
			Contract.Assert(Amount.Equals(amount));
			Contract.Assert(Scale.Equals(scale));
		}

		public Size Scale
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(this._Scale));

				return this._Scale;
			}
		}

		public Size Offset
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(this._Offset));

				return this._Offset;
			}
		}

		public Color Color
		{
			get
			{
				Contract.Ensures(Contract.Result<Color>().Equals(this._Color));

				return this._Color;
			}
		}

		public float Amount
		{
			get
			{
				Contract.Ensures(Contract.Result<float>().Equals(this._Amount));

				return this._Amount;
			}
		}

		public bool Equals(DropShadowSettings other)
		{
			return other._Amount.Equals(this._Amount) &&
			       other._Color.Equals(this._Color) &&
			       other._Offset.Equals(this._Offset) &&
			       other._Scale.Equals(this._Scale);
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
				int result = this._Amount.GetHashCode();
				result = (result * 397) ^ this._Color.GetHashCode();
				result = (result * 397) ^ this._Offset.GetHashCode();
				result = (result * 397) ^ this._Scale.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(
			DropShadowSettings left, DropShadowSettings right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(
			DropShadowSettings left, DropShadowSettings right)
		{
			return !left.Equals(right);
		}
	}
}