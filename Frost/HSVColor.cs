// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost
{
	public struct HSVColor : IEquatable<HSVColor>
	{
		private readonly float _A;
		private readonly float _H;
		private readonly float _S;
		private readonly float _V;

		public HSVColor(
			float hue, float saturation, float value, float alpha = 100.0f)
		{
			Trace.Assert(Check.IsDegrees(hue));
			Trace.Assert(Check.IsPercentage(saturation));
			Trace.Assert(Check.IsPercentage(value));
			Trace.Assert(Check.IsPercentage(alpha));

			this._H = hue;
			this._S = saturation;
			this._V = value;
			this._A = alpha;
		}

		public float V
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));

				return this._V;
			}
		}

		public float S
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));

				return this._S;
			}
		}

		public float H
		{
			get
			{
				Contract.Ensures(Check.IsDegrees(Contract.Result<float>()));

				return this._H;
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

		public bool Equals(HSVColor other)
		{
			return other._A.Equals(this._A) && other._H.Equals(this._H) &&
			       other._S.Equals(this._S) && other._V.Equals(this._V);
		}

		public override string ToString()
		{
			return string.Format(
				"H: {0}, S: {1}, V: {2}, A: {3}", this._H, this._S, this._V, this._A);
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
				int result = this._A.GetHashCode();
				result = (result * 397) ^ this._H.GetHashCode();
				result = (result * 397) ^ this._S.GetHashCode();
				result = (result * 397) ^ this._V.GetHashCode();
				return result;
			}
		}

		internal Color ToColor()
		{
			float r = 0.0f;
			float g = 0.0f;
			float b = 0.0f;

			float h = this._H;
			float s = this._S / 100.0f;
			float v = this._V / 100.0f;

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

			return new Color(r, g, b, this._A / 100.0f);
		}

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

			if(max.Equals(0.0f) || delta.Equals(0.0f))
			{
				// undefined behavior, just use zero to prevent problems
				s = 0.0f;
				h = 0.0f;
			}
			else
			{
				s = delta / max;

				if(r.Equals(max))
				{
					// between yellow and magenta
					h = (g - b) / delta;
				}
				else if(g.Equals(max))
				{
					// between cyan and yellow
					h = 2.0f + (b - r) / delta;
				}
				else // b.Equals(max)
				{
					// between magenta and cyan
					h = 4.0f + (r - g) / delta;
				}
			}

			// scale to 0 ... 360
			h *= 60.0f;

			if(h < 0.0f)
			{
				h += 360.0f;
			}

			return new HSVColor(h, s * 100.0f, v * 100.0f, a * 100.0f);
		}

		public static bool operator ==(HSVColor left, HSVColor right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HSVColor left, HSVColor right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.Equal(0, 1);
		}

		[Fact] internal static void Test1()
		{
			Assert.True(true);
		}
#endif
	}
}