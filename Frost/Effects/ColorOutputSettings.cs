// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	public struct ColorOutputSettings
		: IEffectSettings, IEquatable<ColorOutputSettings>
	{
		private readonly Color _Color;
		private readonly ColorOperation _Operation;

		public ColorOutputSettings(Color color, ColorOperation operation)
		{
			this._Color = color;
			this._Operation = operation;

			Contract.Assert(Color.Equals(color));
			Contract.Assert(Operation.Equals(operation));
		}

		public ColorOperation Operation
		{
			get
			{
				Contract.Ensures(
					Contract.Result<ColorOperation>().Equals(this._Operation));

				return this._Operation;
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

		public bool Equals(ColorOutputSettings other)
		{
			return other._Color.Equals(this._Color) &&
			       other._Operation == this._Operation;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is ColorOutputSettings &&
			       Equals((ColorOutputSettings)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (this._Color.GetHashCode() * 397) ^
				       ((int)this._Operation).GetHashCode();
			}
		}

		public static bool operator ==(
			ColorOutputSettings left, ColorOutputSettings right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(
			ColorOutputSettings left, ColorOutputSettings right)
		{
			return !left.Equals(right);
		}
	}
}