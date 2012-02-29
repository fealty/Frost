﻿// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;

namespace Frost.DirectX
{
	public interface ISurfaceAtlas : IDisposable
	{
		bool InUse { get; }

		IEnumerable<Rectangle> FreeRegions { get; }
		IEnumerable<Rectangle> UsedRegions { get; }

		Canvas.ResolvedContext AcquireRegion(Size dimensions, Canvas target);
	}
}