// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	/// <summary>
	///   represents a color having red, green, blue, and alpha components
	/// </summary>
	public struct RGBColor : IEquatable<RGBColor>
	{
		private readonly float _A;
		private readonly float _B;
		private readonly float _G;
		private readonly float _R;

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsByte(_R));
			Contract.Invariant(Check.IsByte(_G));
			Contract.Invariant(Check.IsByte(_B));
			Contract.Invariant(Check.IsPercentage(_A));
		}

		/// <summary>
		///   constructs a <see cref="	RGBColor" /> from red, green, blue, and optionally alpha components
		/// </summary>
		/// <param name="red"> the red value ranging from zero to two hundred fifty-five </param>
		/// <param name="green"> the green value ranging from zero to two hundred fifty-five </param>
		/// <param name="blue"> the blue value ranging from zero to two hundred fifty-five </param>
		/// <param name="alpha"> the alpha value ranging from zero to one hundred </param>
		public RGBColor(float red, float green, float blue, float alpha = 100.0f)
		{
			Contract.Requires(Check.IsByte(red));
			Contract.Requires(Check.IsByte(green));
			Contract.Requires(Check.IsByte(blue));
			Contract.Requires(Check.IsPercentage(alpha));

			red = Convert.ToSingle(Math.Round(red, 4));
			green = Convert.ToSingle(Math.Round(green, 4));
			blue = Convert.ToSingle(Math.Round(blue, 4));
			alpha = Convert.ToSingle(Math.Round(alpha, 4));

			_R = red;
			_G = green;
			_B = blue;
			_A = alpha;

			Contract.Assert(R.Equals(red));
			Contract.Assert(G.Equals(green));
			Contract.Assert(B.Equals(blue));
			Contract.Assert(A.Equals(alpha));
		}

		/// <summary>
		///   gets the red value ranging from zero to two hundred fifty-five
		/// </summary>
		public float R
		{
			get
			{
				Contract.Ensures(Check.IsByte(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_R));

				return _R;
			}
		}

		/// <summary>
		///   gets the green value ranging from zero to two hundred fifty-five
		/// </summary>
		public float G
		{
			get
			{
				Contract.Ensures(Check.IsByte(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_G));

				return _G;
			}
		}

		/// <summary>
		///   gets the blue value ranging from zero to two hundred fifty-five
		/// </summary>
		public float B
		{
			get
			{
				Contract.Ensures(Check.IsByte(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_B));

				return _B;
			}
		}

		/// <summary>
		///   gets the alpha value ranging from zero to one hundred
		/// </summary>
		public float A
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_A));

				return _A;
			}
		}

		public bool Equals(RGBColor other)
		{
			return other._A.Equals(_A) && other._B.Equals(_B) && other._G.Equals(_G) &&
				other._R.Equals(_R);
		}

		public override string ToString()
		{
			return string.Format("R: {0}, G: {1}, B: {2}, A: {3}", _R, _G, _B, _A);
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
				int result = _A.GetHashCode();
				result = (result * 397) ^ _B.GetHashCode();
				result = (result * 397) ^ _G.GetHashCode();
				result = (result * 397) ^ _R.GetHashCode();
				return result;
			}
		}

		internal Color ToColor()
		{
			return new Color(_R / 255.0f, _G / 255.0f, _B / 255.0f, _A / 100.0f);
		}

		/// <summary>
		///   implicitly converts a <see cref="Color" /> to a <see cref="RGBColor" />
		/// </summary>
		/// <param name="color"> the <see cref="Color" /> to convert </param>
		/// <returns> the <paramref name="color" /> converted to a <see cref="RGBColor" /> </returns>
		public static implicit operator RGBColor(Color color)
		{
			return new RGBColor(
				color.R * 255.0f, color.G * 255.0f, color.B * 255.0f, color.A * 100.0f);
		}

		/// <summary>
		///   determines whether two instances of <see cref="RGBColor" /> are equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> equals <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator ==(RGBColor left, RGBColor right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   determines whether two instances of <see cref="RGBColor" /> are not equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> does not equal <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator !=(RGBColor left, RGBColor right)
		{
			return !left.Equals(right);
		}
	}
}