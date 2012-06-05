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
		private readonly FontId _FontId;

		public FontMetrics(float ascent, float descent, float unitsPerEm, FontId fontId)
		{
			Contract.Requires(Check.IsPositive(ascent));
			Contract.Requires(Check.IsPositive(descent));
			Contract.Requires(Check.IsPositive(unitsPerEm));

			_Ascent = ascent;
			_Descent = descent;
			_UnitsPerEm = unitsPerEm;
			_FontId = fontId;

			Contract.Assert(Ascent.Equals(ascent));
			Contract.Assert(Descent.Equals(descent));
			Contract.Assert(UnitsPerEm.Equals(unitsPerEm));
		}

		public FontId FontId
		{
			get
			{
				Contract.Ensures(Contract.Result<FontId>().Equals(_FontId));

				return _FontId;
			}
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
				other._UnitsPerEm.Equals(_UnitsPerEm) && other._FontId.Equals(_FontId);
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

		public float Measure(float value, float pointSize)
		{
			Contract.Requires(Check.IsFinite(value));
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Ensures(Check.IsFinite(Contract.Result<float>()));

			return ToPixels(value, pointSize);
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
				result = (result * 397) ^ _FontId.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Ascent: {0}, Descent: {1}, UnitsPerEm: {2} FontId: {3}",
				_Ascent,
				_Descent,
				_UnitsPerEm,
				_FontId);
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsPositive(_Ascent));
			Contract.Invariant(Check.IsPositive(_Descent));
			Contract.Invariant(Check.IsPositive(_UnitsPerEm));
		}

		private float ToPixels(float value, float pointSize)
		{
			Contract.Requires(Check.IsFinite(value));
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Ensures(Check.IsFinite(Contract.Result<float>()));

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
	}
}