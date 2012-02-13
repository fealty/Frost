// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost
{
	public struct RGBColor : IEquatable<RGBColor>
	{
		private readonly float _A;
		private readonly float _B;
		private readonly float _G;
		private readonly float _R;

		public RGBColor(float red, float green, float blue, float alpha = 100.0f)
		{
			Trace.Assert(Check.IsByte(red));
			Trace.Assert(Check.IsByte(green));
			Trace.Assert(Check.IsByte(blue));
			Trace.Assert(Check.IsPercentage(alpha));

			this._R = red;
			this._G = green;
			this._B = blue;
			this._A = alpha;
		}

		public float R
		{
			get
			{
				Contract.Ensures(Check.IsByte(Contract.Result<float>()));

				return this._R;
			}
		}

		public float G
		{
			get
			{
				Contract.Ensures(Check.IsByte(Contract.Result<float>()));

				return this._G;
			}
		}

		public float B
		{
			get
			{
				Contract.Ensures(Check.IsByte(Contract.Result<float>()));

				return this._B;
			}
		}

		public float A
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));

				return this._A;
			}
		}

		public bool Equals(RGBColor other)
		{
			return other._A.Equals(this._A) && other._B.Equals(this._B) &&
			       other._G.Equals(this._G) && other._R.Equals(this._R);
		}

		public override string ToString()
		{
			return string.Format(
				"R: {0}, G: {1}, B: {2}, A: {3}", this._R, this._G, this._B, this._A);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is RGBColor && Equals((RGBColor)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = this._A.GetHashCode();
				result = (result * 397) ^ this._B.GetHashCode();
				result = (result * 397) ^ this._G.GetHashCode();
				result = (result * 397) ^ this._R.GetHashCode();
				return result;
			}
		}

		internal Color ToColor()
		{
			return new Color(
				this._R / 255.0f, this._G / 255.0f, this._B / 255.0f, this._A / 100.0f);
		}

		public static implicit operator RGBColor(Color color)
		{
			return new RGBColor(
				color.R * 255.0f, color.G * 255.0f, color.B * 255.0f, color.A * 100.0f);
		}

		public static bool operator ==(RGBColor left, RGBColor right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(RGBColor left, RGBColor right)
		{
			return !left.Equals(right);
		}
	}
}