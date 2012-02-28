// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost;

namespace Demo.Framework
{
	public interface IDemoContext : IDisposable
	{
		string Name { get; }

		void Reset(Canvas target, Device2D device2D);
	}
}