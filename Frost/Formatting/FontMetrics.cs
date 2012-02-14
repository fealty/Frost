// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	public struct FontMetrics : IEquatable<FontMetrics>
	{
		private readonly float _Ascent;
		private readonly float _Descent;
		private readonly float _UnitsPerEm;

		public FontMetrics(float ascent, float descent, float unitsPerEm)
		{
			Trace.Assert(Check.IsPositive(ascent));
			Trace.Assert(Check.IsPositive(descent));
			Trace.Assert(Check.IsPositive(unitsPerEm));

			this._Ascent = ascent;
			this._Descent = descent;
			this._UnitsPerEm = unitsPerEm;

			Contract.Assert(Ascent.Equals(ascent));
			Contract.Assert(Descent.Equals(descent));
			Contract.Assert(UnitsPerEm.Equals(unitsPerEm));
		}

		public float UnitsPerEm
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._UnitsPerEm));

				return this._UnitsPerEm;
			}
		}

		public float Descent
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._Descent));

				return this._Descent;
			}
		}

		public float Ascent
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._Ascent));

				return this._Ascent;
			}
		}

		public bool Equals(FontMetrics other)
		{
			return other._Ascent.Equals(this._Ascent) &&
			       other._Descent.Equals(this._Descent) &&
			       other._UnitsPerEm.Equals(this._UnitsPerEm);
		}

		public float MeasureAscent(float pointSize)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return ToPixels(this._Ascent, pointSize);
		}

		public float MeasureDescent(float pointSize)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return ToPixels(this._Descent, pointSize);
		}

		public float MeasureEm(float pointSize)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return ToPixels(this._Ascent + this._Descent, pointSize);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is FontMetrics && Equals((FontMetrics)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = this._Ascent.GetHashCode();
				result = (result * 397) ^ this._Descent.GetHashCode();
				result = (result * 397) ^ this._UnitsPerEm.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Ascent: {0}, Descent: {1}, UnitsPerEm: {2}",
				this._Ascent,
				this._Descent,
				this._UnitsPerEm);
		}

		private float ToPixels(float value, float pointSize)
		{
			Contract.Requires(Check.IsPositive(value));
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return (((value / this._UnitsPerEm) * pointSize) / 72.0f) * 96.0f;
		}

		public static bool operator ==(FontMetrics left, FontMetrics right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FontMetrics left, FontMetrics right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			
		}
#endif
	}
}