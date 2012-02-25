// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	public struct FontMetrics : IEquatable<FontMetrics>
	{
		private readonly float _Ascent;
		private readonly float _Descent;
		private readonly float _UnitsPerEm;

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(Check.IsPositive(_Ascent));
			Contract.Invariant(Check.IsPositive(_Descent));
			Contract.Invariant(Check.IsPositive(_UnitsPerEm));
		}

		public FontMetrics(float ascent, float descent, float unitsPerEm)
		{
			Contract.Requires(Check.IsPositive(ascent));
			Contract.Requires(Check.IsPositive(descent));
			Contract.Requires(Check.IsPositive(unitsPerEm));

			_Ascent = ascent;
			_Descent = descent;
			_UnitsPerEm = unitsPerEm;

			Contract.Assert(Ascent.Equals(ascent));
			Contract.Assert(Descent.Equals(descent));
			Contract.Assert(UnitsPerEm.Equals(unitsPerEm));
		}

		public float UnitsPerEm
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_UnitsPerEm));

				return _UnitsPerEm;
			}
		}

		public float Descent
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_Descent));

				return _Descent;
			}
		}

		public float Ascent
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_Ascent));

				return _Ascent;
			}
		}

		public bool Equals(FontMetrics other)
		{
			return other._Ascent.Equals(_Ascent) && other._Descent.Equals(_Descent) &&
			       other._UnitsPerEm.Equals(_UnitsPerEm);
		}

		public float MeasureAscent(float pointSize)
		{
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return ToPixels(_Ascent, pointSize);
		}

		public float MeasureDescent(float pointSize)
		{
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return ToPixels(_Descent, pointSize);
		}

		public float MeasureEm(float pointSize)
		{
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return ToPixels(_Ascent + _Descent, pointSize);
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
				int result = _Ascent.GetHashCode();
				result = (result * 397) ^ _Descent.GetHashCode();
				result = (result * 397) ^ _UnitsPerEm.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Ascent: {0}, Descent: {1}, UnitsPerEm: {2}", _Ascent, _Descent, _UnitsPerEm);
		}

		private float ToPixels(float value, float pointSize)
		{
			Contract.Requires(Check.IsPositive(value));
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return (((value / _UnitsPerEm) * pointSize) / 72.0f) * 96.0f;
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
			Assert.Equal(0, new FontMetrics(0, 1, 2).Ascent);
			Assert.Equal(1, new FontMetrics(0, 1, 2).Descent);
			Assert.Equal(2, new FontMetrics(0, 1, 2).UnitsPerEm);

			Assert.Equal(12, new FontMetrics(5, 10, 5).MeasureAscent(9));
			Assert.Equal(24, new FontMetrics(5, 10, 5).MeasureDescent(9));
			Assert.Equal(36, new FontMetrics(5, 10, 5).MeasureEm(9));

			Assert.TestObject(new FontMetrics(0, 1, 2), new FontMetrics(2, 1, 0));
		}
#endif
	}
}