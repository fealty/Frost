using System.Collections.Generic;

namespace Frost.Formatting
{
	public interface IShapedGlyphs
	{
		List<GlyphShaper.Glyph> Glyphs { get; }

		List<GlyphShaper.Cluster> Clusters { get; }

		List<GlyphShaper.Span> Spans { get; }
	}
}