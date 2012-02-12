// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	public static class Check
	{
		[Pure]
		public static bool IsFinite(float value)
		{
			return !Single.IsNaN(value) && !Single.IsInfinity(value);
		}

		[Pure]
		public static bool IsNormalized(float value)
		{
			return value >= 0.0 && value <= 1.0;
		}

		[Pure]
		public static bool IsDegrees(float value)
		{
			return value >= 0.0 && value <= 360.0;
		}

		[Pure]
		public static bool IsPositive(float value)
		{
			return value >= 0.0 && IsFinite(value);
		}
	}
}