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
		[Pure] public static bool IsFinite(float value)
		{
			return !Single.IsNaN(value) && !Single.IsInfinity(value);
		}

		[Pure] public static bool IsNormalized(float value)
		{
			return value >= 0.0 && value <= 1.0;
		}

		[Pure] public static bool IsDegrees(float value)
		{
			return value >= 0.0 && value <= 360.0;
		}

		[Pure] public static bool IsPositive(float value)
		{
			return value >= 0.0 && IsFinite(value);
		}

		[Pure] public static bool IsPercentage(float value)
		{
			return value >= 0.0 && value <= 100.0f;
		}

		[Pure] public static bool IsByte(float value)
		{
			return value >= 0.0 && value <= 255.0f;
		}

		[Pure] public static bool IsValid(Canvas canvas, Device2D device2D)
		{
			return canvas != null && canvas.IsValid &&
			       canvas.Device2D == device2D;
		}

		[Pure] public static bool IsValid(Canvas canvas)
		{
			return canvas != null && canvas.IsValid;
		}
	}
}