using System;

namespace Frost
{
	internal static class Check
	{
		public static bool IsFinite(float value)
		{
			return !Single.IsNaN(value) && !Single.IsInfinity(value);
		}

		public static bool IsNormalized(float value)
		{
			return value >= 0.0 && value <= 1.0;
		}

		public static bool IsDegrees(float value)
		{
			return value >= 0.0 && value <= 360.0;
		}

		public static bool IsPositive(float value)
		{
			return value >= 0.0 && IsFinite(value);
		}
	}
}