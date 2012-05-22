using System.Collections.Generic;

namespace Frost.Formatting
{
	public interface IShapedText
	{
		List<TextShaper.Glyph> Glyphs { get; }

		List<TextShaper.Cluster> Clusters { get; }

		List<TextShaper.Span> Spans { get; }
	}
}