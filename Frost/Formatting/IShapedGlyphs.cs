// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

namespace Frost.Formatting
{
	public interface IShapedGlyphs
	{
		List<TextShaper.Glyph> Glyphs { get; }

		List<TextShaper.Cluster> Clusters { get; }

		List<TextShaper.Span> Spans { get; }
	}
}