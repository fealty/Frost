// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

namespace Frost.Shaping
{
	public interface IShapedGlyphs
	{
		List<Shaper.Glyph> Glyphs { get; }

		List<Shaper.Cluster> Clusters { get; }

		List<Shaper.Span> Spans { get; }
	}
}