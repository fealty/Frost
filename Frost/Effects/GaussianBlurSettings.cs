// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	public struct GaussianBlurSettings
		: IEffectSettings, IEquatable<GaussianBlurSettings>
	{
		private readonly Size _Amount;

		public GaussianBlurSettings(Size amount)
		{
			Contract.Requires(Check.IsPositive(amount.Width));
			Contract.Requires(Check.IsPositive(amount.Height));

			_Amount = amount;

			Contract.Assert(Amount.Equals(amount));
		}

		public Size Amount
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_Amount));

				return _Amount;
			}
		}

		public bool Equals(GaussianBlurSettings other)
		{
			return other._Amount.Equals(_Amount);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is GaussianBlurSettings && Equals((GaussianBlurSettings)obj);
		}

		public override int GetHashCode()
		{
			return _Amount.GetHashCode();
		}

		public static bool operator ==(
			GaussianBlurSettings left, GaussianBlurSettings right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(
			GaussianBlurSettings left, GaussianBlurSettings right)
		{
			return !left.Equals(right);
		}
	}
}