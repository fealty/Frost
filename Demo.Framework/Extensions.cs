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

		internal static Result AcquireSync(this KeyedMutex mutex)
		{
			return mutex != null ? mutex.Acquire(0, -1) : Result.NoInterface;
		}

		internal static Result ReleaseSync(this KeyedMutex mutex)
		{
			return mutex != null ? mutex.Release(0) : Result.NoInterface;
		}
	}
}