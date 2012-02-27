using System;
using System.Collections.Generic;

namespace Frost.DirectX
{
	public interface ISurfaceAtlas : Atlasing.ISurfaceAtlas, IDisposable
	{
		bool InUse { get; }

		IEnumerable<Rectangle> FreeRegions { get; }
		IEnumerable<Rectangle> UsedRegions { get; }
	}
}