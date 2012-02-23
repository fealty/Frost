// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Shaping;

using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;
using Geometry = Frost.Shaping.Geometry;

namespace Frost.DirectX.Common
{
	internal sealed class GeometryBuilder : IGeometrySink, IDisposable
	{
		private readonly Factory _Factory2D;

		private bool _IsSinkInFigure;

		private PathGeometry _Path;
		private GeometrySink _PathSink;

		public GeometryBuilder(Factory factory2D)
		{
			Contract.Requires(factory2D != null);

			this._Factory2D = factory2D;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		void IGeometrySink.Begin()
		{
			Contract.Assert(this._Path == null);

			this._Path = new PathGeometry(this._Factory2D);

			this._IsSinkInFigure = false;

			Contract.Assert(this._PathSink == null);

			this._PathSink = this._Path.Open();
		}

		void IGeometrySink.End()
		{
			Contract.Assert(this._PathSink != null);

			if(this._IsSinkInFigure)
			{
				this._PathSink.EndFigure(FigureEnd.Open);
			}

			this._PathSink.Close();
		}

		void IGeometrySink.Close()
		{
			Contract.Assert(this._PathSink != null);

			Contract.Assert(this._IsSinkInFigure);

			this._PathSink.EndFigure(FigureEnd.Closed);

			this._IsSinkInFigure = false;
		}

		void IGeometrySink.MoveTo(Point point)
		{
			Contract.Assert(this._PathSink != null);

			if(this._IsSinkInFigure)
			{
				this._PathSink.EndFigure(FigureEnd.Open);

				this._IsSinkInFigure = false;
			}

			this._PathSink.BeginFigure(point.ToPointF(), FigureBegin.Filled);

			this._IsSinkInFigure = true;
		}

		void IGeometrySink.LineTo(Point endPoint)
		{
			Contract.Assert(this._PathSink != null);

			this._PathSink.AddLine(endPoint.ToPointF());
		}

		void IGeometrySink.QuadraticCurveTo(
			Point controlPoint, Point endPoint)
		{
			Contract.Assert(this._PathSink != null);

			QuadraticBezierSegment segment;

			segment.Point1 = controlPoint.ToPointF();
			segment.Point2 = endPoint.ToPointF();

			this._PathSink.AddQuadraticBezier(segment);
		}

		void IGeometrySink.BezierCurveTo(
			Point controlPoint1, Point controlPoint2, Point endPoint)
		{
			Contract.Assert(this._PathSink != null);

			BezierSegment segment;

			segment.Point1 = controlPoint1.ToPointF();
			segment.Point2 = controlPoint2.ToPointF();
			segment.Point3 = endPoint.ToPointF();

			this._PathSink.AddBezier(segment);
		}

		void IGeometrySink.ArcTo(
			Point tangentStart, Point tangentEnd, Size radius)
		{
			Contract.Assert(this._PathSink != null);

			this._PathSink.AddLine(tangentStart.ToPointF());

			ArcSegment segment;

			segment.Point = tangentEnd.ToPointF();
			segment.Size = radius.ToSizeF();
			segment.SweepDirection = SweepDirection.Clockwise;
			segment.RotationAngle = 0.0f;
			segment.ArcSize = ArcSize.Small;

			this._PathSink.AddArc(segment);
		}

		public void Build(Geometry geometry, out DxGeometry result)
		{
			Contract.Requires(geometry != null);
			Contract.Ensures(Contract.ValueAtReturn(out result) != null);

			IGeometrySink @this = this;

			try
			{
				@this.Begin();

				geometry.Extract(this);
			}
			finally
			{
				@this.End();

				result = this._Path;

				this._PathSink = null;
				this._Path = null;
			}
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				this._Path.SafeDispose();
				this._PathSink.SafeDispose();
			}
		}
	}
}