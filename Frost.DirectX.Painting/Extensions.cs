// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using Frost.Painting;

using SharpDX.Direct2D1;

using LineJoin = SharpDX.Direct2D1.LineJoin;

namespace Frost.DirectX.Painting
{
	internal static class Extensions
	{
		internal static DashStyle ToDirectWrite(this LineStyle value)
		{
			switch(value)
			{
				case LineStyle.Solid:
					return DashStyle.Solid;
				case LineStyle.Dot:
					return DashStyle.Dot;
				case LineStyle.Dash:
					return DashStyle.Dash;
				default:
					return DashStyle.Solid;
			}
		}

		internal static CapStyle ToDirectWrite(this LineCap value)
		{
			switch(value)
			{
				case LineCap.Butt:
					return CapStyle.Flat;
				case LineCap.Round:
					return CapStyle.Round;
				case LineCap.Square:
					return CapStyle.Square;
				default:
					return CapStyle.Flat;
			}
		}

		internal static LineJoin ToDirectWrite(this Frost.Painting.LineJoin value)
		{
			switch(value)
			{
				case Frost.Painting.LineJoin.Miter:
					return LineJoin.Miter;
				case Frost.Painting.LineJoin.Bevel:
					return LineJoin.Bevel;
				case Frost.Painting.LineJoin.Round:
					return LineJoin.Round;
				default:
					return LineJoin.Miter;
			}
		}
	}
}