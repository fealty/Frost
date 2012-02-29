// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;

using Frost.DirectX.Common;

namespace Frost.DirectX
{
	public interface ISurfaceAtlas : IDisposable
	{
		bool InUse { get; }

		IEnumerable<Rectangle> FreeRegions { get; }
		IEnumerable<Rectangle> UsedRegions { get; }

		void Purge(bool isForced, SafeList<Canvas> invalidatedResources);

		void Forget(Canvas.ResolvedContext context);

		Surface2D Surface2D { get; }

		Canvas.ResolvedContext AcquireRegion(Size dimensions, Canvas target);
	}
}