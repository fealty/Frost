// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

using Frost.Shaping;

using SharpDX;
using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;

namespace Frost.DirectX
{
	internal sealed class TessellationSink : CallbackBase, SharpDX.Direct2D1.TessellationSink
	{
		private ITessellationSink _Sink;

		void SharpDX.Direct2D1.TessellationSink.AddTriangles(Triangle[] triangles)
		{
			foreach(Triangle triangle in triangles)
			{
				Point point1 = new Point(triangle.Point1.X, triangle.Point1.Y);
				Point point2 = new Point(triangle.Point2.X, triangle.Point2.Y);
				Point point3 = new Point(triangle.Point3.X, triangle.Point3.Y);

				_Sink.AddTriangle(point1, point2, point3);
			}
		}

		void SharpDX.Direct2D1.TessellationSink.Close()
		{
			_Sink.End();
		}

		public void Tessellate(DxGeometry resolvedSource, ITessellationSink sink, float tolerance)
		{
			Contract.Requires(resolvedSource != null);
			Contract.Requires(sink != null);
			Contract.Requires(Check.IsPositive(tolerance));

			_Sink = sink;

			_Sink.Begin();

			resolvedSource.Tessellate(tolerance, this);

			_Sink = null;
		}
	}
}