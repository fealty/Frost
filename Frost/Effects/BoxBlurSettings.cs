// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	public struct BoxBlurSettings
		: IEffectSettings, IEquatable<BoxBlurSettings>
	{
		private readonly Size _Amount;
		private readonly int _PassCount;

		public BoxBlurSettings(Size amount, int passCount)
		{
			Contract.Requires(Check.IsPositive(amount.Width));
			Contract.Requires(Check.IsPositive(amount.Height));
			Contract.Requires(Check.IsPositive(passCount));

			this._Amount = amount;
			this._PassCount = passCount;

			Contract.Assert(Amount.Equals(amount));
			Contract.Assert(PassCount.Equals(passCount));
		}

		public int PassCount
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);
				Contract.Ensures(Contract.Result<int>().Equals(this._PassCount));

				return this._PassCount;
			}
		}

		public Size Amount
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<Size>().Width));
				Contract.Ensures(Check.IsPositive(Contract.Result<Size>().Height));
				Contract.Ensures(Contract.Result<Size>().Equals(this._Amount));

				return this._Amount;
			}
		}

		public bool Equals(BoxBlurSettings other)
		{
			return other._Amount.Equals(this._Amount) &&
			       other._PassCount == this._PassCount;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is BoxBlurSettings && Equals((BoxBlurSettings)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (this._Amount.GetHashCode() * 397) ^ this._PassCount;
			}
		}

		public static bool operator ==(
			BoxBlurSettings left, BoxBlurSettings right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(
			BoxBlurSettings left, BoxBlurSettings right)
		{
			return !left.Equals(right);
		}
	}
}