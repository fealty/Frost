// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	public struct BoxBlurSettings : IEffectSettings, IEquatable<BoxBlurSettings>
	{
		private readonly Size _Amount;
		private readonly int _PassCount;

		public BoxBlurSettings(Size amount, int passCount)
		{
			Contract.Requires(Check.IsPositive(amount.Width));
			Contract.Requires(Check.IsPositive(amount.Height));
			Contract.Requires(Check.IsPositive(passCount));

			_Amount = amount;
			_PassCount = passCount;

			Contract.Assert(Amount.Equals(amount));
			Contract.Assert(PassCount.Equals(passCount));
		}

		public int PassCount
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);
				Contract.Ensures(Contract.Result<int>().Equals(_PassCount));

				return _PassCount;
			}
		}

		public Size Amount
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<Size>().Width));
				Contract.Ensures(Check.IsPositive(Contract.Result<Size>().Height));
				Contract.Ensures(Contract.Result<Size>().Equals(_Amount));

				return _Amount;
			}
		}

		public bool Equals(BoxBlurSettings other)
		{
			return other._Amount.Equals(_Amount) && other._PassCount == _PassCount;
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
				return (_Amount.GetHashCode() * 397) ^ _PassCount;
			}
		}

		public static bool operator ==(BoxBlurSettings left, BoxBlurSettings right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(BoxBlurSettings left, BoxBlurSettings right)
		{
			return !left.Equals(right);
		}
	}
}