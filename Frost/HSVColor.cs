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

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(Check.IsDegrees(this._H));
			Contract.Invariant(Check.IsPercentage(this._S));
			Contract.Invariant(Check.IsPercentage(this._V));
			Contract.Invariant(Check.IsPercentage(this._A));
		}

		public HSVColor(
			float hue, float saturation, float value, float alpha = 100.0f)
		{
			Trace.Assert(Check.IsDegrees(hue));
			Trace.Assert(Check.IsPercentage(saturation));
			Trace.Assert(Check.IsPercentage(value));
			Trace.Assert(Check.IsPercentage(alpha));

			hue = Convert.ToSingle(Math.Round(hue, 4));
			value = Convert.ToSingle(Math.Round(value, 4));
			alpha = Convert.ToSingle(Math.Round(alpha, 4));
			saturation = Convert.ToSingle(Math.Round(saturation, 4));

			this._H = hue;
			this._S = saturation;
			this._V = value;
			this._A = alpha;

			Contract.Assert(H.Equals(hue));
			Contract.Assert(S.Equals(saturation));
			Contract.Assert(V.Equals(value));
			Contract.Assert(A.Equals(alpha));
		}

		public float V
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._V));

				return this._V;
			}
		}

		public float S
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._S));

				return this._S;
			}
		}

		public float H
		{
			get
			{
				Contract.Ensures(Check.IsDegrees(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._H));

				return this._H;
			}
		}

		public float A
		{
			get
			{
				Contract.Ensures(Check.IsPercentage(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._A));

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
				"H: {0}, S: {1}, V: {2}, A: {3}",
				this._H,
				this._S,
				this._V,
				this._A);
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
			float r;
			float g;
			float b;

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
			Assert.Equal(1, new HSVColor(1, 2, 3, 4).H);
			Assert.Equal(2, new HSVColor(1, 2, 3, 4).S);
			Assert.Equal(3, new HSVColor(1, 2, 3, 4).V);
			Assert.Equal(4, new HSVColor(1, 2, 3, 4).A);

			Assert.Equal<HSVColor>(
				new HSVColor(000, 100, 100), new Color(1, 0, 0));
			Assert.Equal<HSVColor>(
				new HSVColor(060, 100, 100), new Color(1, 1, 0));
			Assert.Equal<HSVColor>(
				new HSVColor(120, 100, 100), new Color(0, 1, 0));
			Assert.Equal<HSVColor>(
				new HSVColor(180, 100, 100), new Color(0, 1, 1));
			Assert.Equal<HSVColor>(
				new HSVColor(240, 100, 100), new Color(0, 0, 1));
			Assert.Equal<HSVColor>(
				new HSVColor(300, 100, 100), new Color(1, 0, 1));
			Assert.Equal<HSVColor>(
				new HSVColor(000, 000, 100), new Color(1, 1, 1));

			Assert.Equal<Color>(
				new Color(1, 0, 0), new HSVColor(000, 100, 100));
			Assert.Equal<Color>(
				new Color(1, 1, 0), new HSVColor(060, 100, 100));
			Assert.Equal<Color>(
				new Color(0, 1, 0), new HSVColor(120, 100, 100));
			Assert.Equal<Color>(
				new Color(0, 1, 1), new HSVColor(180, 100, 100));
			Assert.Equal<Color>(
				new Color(0, 0, 1), new HSVColor(240, 100, 100));
			Assert.Equal<Color>(
				new Color(1, 0, 1), new HSVColor(360, 100, 100));
			Assert.Equal<Color>(
				new Color(1, 1, 1), new HSVColor(180, 000, 100));

			Assert.TestObject<HSVColor>(Color.Red, Color.Blue);
		}
#endif
	}
}