// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	/// <summary>
	///   contains methods for verifying the values of in-built types
	/// </summary>
	public static class Check
	{
		/// <summary>
		///   determines whether the given value is not NaN and not infinity
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is not NaN and not infinity; otherwise, <c>false</c> </returns>
		[Pure]
		public static bool IsFinite(double value)
		{
			return !Double.IsNaN(value) && !Double.IsInfinity(value);
		}

		/// <summary>
		///   determines whether the given value is a finite value of zero or greater
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is a finite value of zero or greater; otherwise, <c>false</c> </returns>
		[Pure]
		public static bool IsPositive(double value)
		{
			return value >= 0.0 && IsFinite(value);
		}

		/// <summary>
		///   determines whether the given value is zero or greater and less than or equal to one
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is zero or greater and less than or equal to one; otherwise, <c>false</c> </returns>
		[Pure]
		public static bool IsNormalized(double value)
		{
			return value >= 0.0 && value <= 1.0;
		}

		/// <summary>
		///   determines whether the given value is not NaN and not infinity
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is not NaN and not infinity; otherwise, <c>false</c> </returns>
		[Pure]
		public static bool IsFinite(float value)
		{
			return !Single.IsNaN(value) && !Single.IsInfinity(value);
		}

		/// <summary>
		///   determines whether the given value is zero or greater and less than or equal to one
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is zero or greater and less than or equal to one; otherwise, <c>false</c> </returns>
		[Pure]
		public static bool IsNormalized(float value)
		{
			return value >= 0.0 && value <= 1.0;
		}

		/// <summary>
		///   determines whether the given value is zero or greater and less than or equal to three hundred and sixty
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is zero or greater and less than or equal to three hundred and sixty; otherwise, <c>false</c> </returns>
		[Pure]
		public static bool IsDegrees(float value)
		{
			return value >= 0.0 && value <= 360.0;
		}

		/// <summary>
		///   determines whether the given value is finite and zero or greater
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is zero or greater and finite; otherwise, <c>false</c> </returns>
		[Pure]
		public static bool IsPositive(float value)
		{
			return value >= 0.0 && IsFinite(value);
		}

		/// <summary>
		///   determines whether the given value is zero or greater and less than or equal to one hundred
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is zero or greater and less than or equal to one hundred </returns>
		[Pure]
		public static bool IsPercentage(float value)
		{
			return value >= 0.0 && value <= 100.0f;
		}

		/// <summary>
		///   determines whether the given value is zero or greater and less than or equal to two hundred and fifty-five
		/// </summary>
		/// <param name="value"> the value to test </param>
		/// <returns> <c>true</c> if <paramref name="value" /> is zero or greater and less than or equal to two hundred and fifty-five </returns>
		[Pure]
		public static bool IsByte(float value)
		{
			return value >= 0.0 && value <= 255.0f;
		}
	}
}