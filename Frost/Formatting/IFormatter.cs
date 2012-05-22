// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using Frost.Collections;

namespace Frost.Formatting
{
	public interface IFormatter
	{
		GlyphShaper GlyphShaper { get; }

		GlyphOutline GetOutline(
			IndexedRange glyphRange,
			bool isVertical,
			bool isRightToLeft,
			FontHandle fontHandle,
			params GlyphShaper.Glyph[] glyphs);

		FontMetrics GetMetrics(FontHandle fontHandle);
	}
}