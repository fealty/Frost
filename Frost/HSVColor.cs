// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	/// <summary>
	///   represents a color defined by its hue, saturation, value, and alpha components
	/// </summary>
	public struct HSVColor : IEquatable<HSVColor>
	{
		private readonly float _A;
		private readonly float _H;
		private readonly float _S;
		private readonly float _V;

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsDegrees(_H));
			Contract.Invariant(Check.IsPercentage(_S));
			Contract.Invariant(Check.IsPercentage(_V));
			Contract.Invariant(Check.IsPercentage(_A));
		}

		/// <summary>
		///   constructs a <see cref="HSVColor" /> from hue, saturation, value, and optionally alpha values
		/// </summary>
		/// <param name="hue"> the hue in degrees ranging from zero to three hundred and sixty </param>
		/// <param name="saturation"> the saturation in percentage ranging from zero to one hundred </param>
		/// <param name="value"> the value in percentage ranging from zero to one hundred </param>
		/// <param name="alpha"> the alpha in percentage ranging from zero to one hundred </param>
		public HSVColor(
			float hue, float saturation, float value, float alpha = 100.0f)
		{
			Contract.Requires(Check.IsDegrees(hue));
			Contract.Requires(Check.IsPercentage(saturation));
			Contract.Requires(Check.IsPercentage(value));
			Contract.Requires(Check.IsPercentage(alpha));

			hue = Convert.ToSingle(Math.Round(hue, 4));
			value = Convert.ToSingle(Math.Round(value, 4));
			alpha = Convert.ToSingle(Math.Round(alpha, 4));
			saturation = Convert.ToSingle(Math.Round(saturation, 4));

			_H = hue;
			_S = saturation;
			_V = value;
			_A = alpha;

			Contract.Assert(H.Equals(hue));
			Contract.Assert(S.Equals(saturation));
			Contract.Assert(V.Equals(value));
			Contract.Assert(A.Equals(alpha));
		}

		/// <summary>
		///   gets the value in percentage ranging from zero to one hundred
		/// </summary>
		public float V
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_V));

				return _V;
			}
		}

		/// <summary>
		///   gets the saturation value in percentage ranging from zero to one hundred
		/// </summary>
		public float S
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_S));

				return _S;
			}
		}

		/// <summary>
		///   gets the hue value in degrees ranging from zero to three hundred and sixty
		/// </summary>
		public float H
		{
			get
			{
				Contract.Ensures(Check.IsDegrees(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_H));

				return _H;
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

		public bool Equals(HSVColor other)
		{
			return other._A.Equals(_A) && other._H.Equals(_H) && other._S.Equals(_S) &&
				other._V.Equals(_V);
		}

		public override string ToString()
		{
			return string.Format("H: {0}, S: {1}, V: {2}, A: {3}", _H, _S, _V, _A);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is HSVColor && Equals((HSVColor)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _A.GetHashCode();
				result = (result * 397) ^ _H.GetHashCode();
				result = (result * 397) ^ _S.GetHashCode();
				result = (result * 397) ^ _V.GetHashCode();
				return result;
			}
		}

		internal Color ToColor()
		{
			float r;
			float g;
			float b;

			float h = _H;
			float s = _S / 100.0f;
			float v = _V / 100.0f;

			if(s.Equals(0.0f))
			{
				r = v;
				g = v;
				b = v;
			}
			else
			{
				float sectorPosition = h / 60.0f;

				// determine the sector within the color wheel
				int sectorId = (int)Math.Floor(sectorPosition);

				// degrees into the sector
				float degrees = sectorPosition - sectorId;

				// three axes of the color
				float p = v * (1.0f - s);
				float q = v * (1.0f - (s * degrees));
				float t = v * (1.0f - (s * (1.0f - degrees)));

				// assign color by sector
				switch(sectorId)
				{
					case 0:
						r = v;
						g = t;
						b = p;
						break;

					case 1:
						r = q;
						g = v;
						b = p;
						break;

					case 2:
						r = p;
						g = v;
						b = t;
						break;

					case 3:
						r = p;
						g = q;
						b = v;
						break;

					case 4:
						r = t;
						g = p;
						b = v;
						break;

					default:
						r = v;
						g = p;
						b = q;
						break;
				}
			}

			return new Color(r, g, b, _A / 100.0f);
		}

		/// <summary>
		///   implicitly converts a <see cref="Color" /> to a <see cref="HSVColor" />
		/// </summary>
		/// <param name="color"> the <see cref="Color" /> to convert </param>
		/// <returns> the <paramref name="color" /> converted to a <see cref="HSVColor" /> </returns>
		public static implicit operator HSVColor(Color color)
		{
			float r = color.R;
			float g = color.G;
			float b = color.B;
			float a = color.A;

			float h;
			float s;

			float min = Math.Min(r, Math.Min(g, b));
			float max = Math.Max(r, Math.Max(g, b));

			float v = max;

			float delta = max - min;

			if(delta.Equals(0.0f))
			{
				h = 0.0f;
				s = 0.0f;
			}
			else
			{
				s = delta / max;

				float delR = (((max - r) / 6.0f) + (delta * 0.5f)) / delta;
				float delG = (((max - g) / 6.0f) + (delta * 0.5f)) / delta;
				float delB = (((max - b) / 6.0f) + (delta * 0.5f)) / delta;

				h = 0.0f;

				if(r.Equals(max))
				{
					h = delB - delG;
				}
				else if(g.Equals(max))
				{
					h = (1.0f / 3.0f) + delR - delB;
				}
				else if(b.Equals(max))
				{
					h = (2.0f / 3.0f) + delG - delR;
				}

				h += h < 0.0f ? 1.0f : 0.0f;
				h -= h > 1.0f ? 1.0f : 0.0f;
			}

			return new HSVColor(h * 360.0f, s * 100.0f, v * 100.0f, a * 100.0f);
		}

		/// <summary>
		///   determines whether two instances of <see cref="HSVColor" /> are equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> equals <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator ==(HSVColor left, HSVColor right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   determines whether two instances of <see cref="HSVColor" /> are not equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> does not equal <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator !=(HSVColor left, HSVColor right)
		{
			return !left.Equals(right);
		}
	}
}