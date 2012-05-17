using System.Collections.Generic;

namespace Frost.Formatting
{
	public interface IShapedText
	{
		Paragraph Paragraph { get; }

		List<ShapedGlyph> Glyphs { get; }

		List<ShapedCluster> Clusters { get; }

		List<ShapedSpan> Spans { get; }
	}
}