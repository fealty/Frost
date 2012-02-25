using Frost.Painting;

namespace Frost.DirectX.Painting
{
	internal static class Extensions
	{
		internal static SharpDX.Direct2D1.DashStyle ToDirectWrite(
			this LineStyle value)
		{
			switch (value)
			{
				case LineStyle.Solid:
					return SharpDX.Direct2D1.DashStyle.Solid;
				case LineStyle.Dot:
					return SharpDX.Direct2D1.DashStyle.Dot;
				case LineStyle.Dash:
					return SharpDX.Direct2D1.DashStyle.Dash;
				default:
					return SharpDX.Direct2D1.DashStyle.Solid;
			}
		}

		internal static SharpDX.Direct2D1.CapStyle ToDirectWrite(this LineCap value)
		{
			switch (value)
			{
				case LineCap.Butt:
					return SharpDX.Direct2D1.CapStyle.Flat;
				case LineCap.Round:
					return SharpDX.Direct2D1.CapStyle.Round;
				case LineCap.Square:
					return SharpDX.Direct2D1.CapStyle.Square;
				default:
					return SharpDX.Direct2D1.CapStyle.Flat;
			}
		}

		internal static SharpDX.Direct2D1.LineJoin ToDirectWrite(this LineJoin value)
		{
			switch (value)
			{
				case LineJoin.Miter:
					return SharpDX.Direct2D1.LineJoin.Miter;
				case LineJoin.Bevel:
					return SharpDX.Direct2D1.LineJoin.Bevel;
				case LineJoin.Round:
					return SharpDX.Direct2D1.LineJoin.Round;
				default:
					return SharpDX.Direct2D1.LineJoin.Miter;
			}
		}
	}
}