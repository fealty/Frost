using System.Collections.Generic;

namespace Frost.Formatting
{
	public interface IShapedText
	{
		List<ShapedGlyph> Glyphs { get; }

		List<ShapedCluster> Clusters { get; }

		List<ShapedSpan> Spans { get; }
	}
}