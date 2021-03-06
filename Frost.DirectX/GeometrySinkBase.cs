﻿// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost.Shaping;

using SharpDX;
using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;

namespace Frost.DirectX
{
	internal abstract class GeometrySinkBase : CallbackBase, GeometrySink
	{
		protected Shape.Builder _Builder;

		void SimplifiedGeometrySink.SetFillMode(FillMode fillMode)
		{
		}

		void SimplifiedGeometrySink.SetSegmentFlags(PathSegment vertexFlags)
		{
		}

		void SimplifiedGeometrySink.BeginFigure(DrawingPointF startPoint, FigureBegin figureBegin)
		{
			_Builder.MoveTo(startPoint.X, startPoint.Y);
		}

		void SimplifiedGeometrySink.AddLines(DrawingPointF[] ointsRef)
		{
			foreach(DrawingPointF point in ointsRef)
			{
				_Builder.LineTo(point.X, point.Y);
			}
		}

		void SimplifiedGeometrySink.AddBeziers(BezierSegment[] beziers)
		{
			foreach(BezierSegment bezier in beziers)
			{
				_Builder.BezierCurveTo(
					bezier.Point1.X,
					bezier.Point1.Y,
					bezier.Point2.X,
					bezier.Point2.Y,
					bezier.Point3.X,
					bezier.Point3.Y);
			}
		}

		void SimplifiedGeometrySink.EndFigure(FigureEnd figureEnd)
		{
			if(figureEnd == FigureEnd.Closed)
			{
				_Builder.Close();
			}
		}

		void SimplifiedGeometrySink.Close()
		{
		}

		void GeometrySink.AddLine(DrawingPointF point)
		{
			throw new NotSupportedException();
		}

		void GeometrySink.AddBezier(BezierSegment bezier)
		{
			throw new NotSupportedException();
		}

		void GeometrySink.AddQuadraticBezier(QuadraticBezierSegment bezier)
		{
			throw new NotSupportedException();
		}

		void GeometrySink.AddQuadraticBeziers(QuadraticBezierSegment[] beziers)
		{
			throw new NotSupportedException();
		}

		void GeometrySink.AddArc(ArcSegment arc)
		{
			throw new NotSupportedException();
		}
	}
}