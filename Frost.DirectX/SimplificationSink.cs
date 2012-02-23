// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Drawing;

using SharpDX;
using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;
using Geometry = Frost.Shaping.Geometry;

namespace Frost.DirectX
{
	internal sealed class SimplificationSink : CallbackBase, GeometrySink
	{
		private Geometry.Builder _Builder;

		void SimplifiedGeometrySink.SetFillMode(FillMode fillMode)
		{
		}

		void SimplifiedGeometrySink.SetSegmentFlags(PathSegment vertexFlags)
		{
		}

		void SimplifiedGeometrySink.BeginFigure(
			PointF startPoint, FigureBegin figureBegin)
		{
			this._Builder.MoveTo(startPoint.X, startPoint.Y);
		}

		void SimplifiedGeometrySink.AddLines(PointF[] ointsRef)
		{
			foreach(PointF point in ointsRef)
			{
				this._Builder.LineTo(point.X, point.Y);
			}
		}

		void SimplifiedGeometrySink.AddBeziers(BezierSegment[] beziers)
		{
		}

		void SimplifiedGeometrySink.EndFigure(FigureEnd figureEnd)
		{
		}

		void SimplifiedGeometrySink.Close()
		{
			this._Builder.Close();
		}

		void GeometrySink.AddLine(PointF point)
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

		void GeometrySink.AddQuadraticBeziers(
			QuadraticBezierSegment[] beziers)
		{
			throw new NotSupportedException();
		}

		void GeometrySink.AddArc(ArcSegment arc)
		{
			throw new NotSupportedException();
		}

		public Geometry CreateSimplification(
			DxGeometry resolvedSource, float tolerance)
		{
			Contract.Requires(resolvedSource != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			this._Builder = Geometry.Create();

			resolvedSource.Simplify(
				GeometrySimplificationOption.Lines, tolerance, this);

			Geometry result = this._Builder.Build();

			this._Builder = null;

			return result;
		}
	}
}