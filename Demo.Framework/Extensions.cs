// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using SharpDX;
using SharpDX.DXGI;

namespace Demo.Framework
{
	public static class Extensions
	{
		public static void SafeDispose(this IDisposable disposable)
		{
			if(disposable != null)
			{
				disposable.Dispose();
			}
		}

		/// <summary>
		///   Acquires the keyed mutex.
		/// </summary>
		/// <remarks>
		///   This method only acquires key zero.
		/// </remarks>
		/// <param name="mutex"> The mutex to acquire. </param>
		/// <returns> Returns the result of the operation. </returns>
		public static Result AcquireSync(this KeyedMutex mutex)
		{
			if(mutex != null)
			{
				return mutex.Acquire(0, -1);
			}

			return Result.NoInterface;
		}

		/// <summary>
		///   Releases the keyed mutex.
		/// </summary>
		/// <remarks>
		///   This method only releases key zero.
		/// </remarks>
		/// <param name="mutex"> The mutex to release. </param>
		/// <returns> Returns the result of the operation. </returns>
		public static Result ReleaseSync(this KeyedMutex mutex)
		{
			if(mutex != null)
			{
				return mutex.Release(0);
			}

			return Result.NoInterface;
		}
	}
}