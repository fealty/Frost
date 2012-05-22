// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Construction;

using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;

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

			_Factory2D = factory2D;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		void IGeometrySink.Begin()
		{
			Contract.Assert(_Path == null);

			_Path = new PathGeometry(_Factory2D);

			_IsSinkInFigure = false;

			Contract.Assert(_PathSink == null);

			_PathSink = _Path.Open();
		}

		void IGeometrySink.End()
		{
			Contract.Assert(_PathSink != null);

			if(_IsSinkInFigure)
			{
				_PathSink.EndFigure(FigureEnd.Open);
			}

			_PathSink.Close();
		}

		void IGeometrySink.Close()
		{
			Contract.Assert(_PathSink != null);

			Contract.Assert(_IsSinkInFigure);

			_PathSink.EndFigure(FigureEnd.Closed);

			_IsSinkInFigure = false;
		}

		void IGeometrySink.MoveTo(Point point)
		{
			Contract.Assert(_PathSink != null);

			if(_IsSinkInFigure)
			{
				_PathSink.EndFigure(FigureEnd.Open);

				_IsSinkInFigure = false;
			}

			_PathSink.BeginFigure(point.ToPointF(), FigureBegin.Filled);

			_IsSinkInFigure = true;
		}

		void IGeometrySink.LineTo(Point endPoint)
		{
			Contract.Assert(_PathSink != null);

			_PathSink.AddLine(endPoint.ToPointF());
		}

		void IGeometrySink.QuadraticCurveTo(Point controlPoint, Point endPoint)
		{
			Contract.Assert(_PathSink != null);

			QuadraticBezierSegment segment;

			segment.Point1 = controlPoint.ToPointF();
			segment.Point2 = endPoint.ToPointF();

			_PathSink.AddQuadraticBezier(segment);
		}

		void IGeometrySink.BezierCurveTo(Point controlPoint1, Point controlPoint2, Point endPoint)
		{
			Contract.Assert(_PathSink != null);

			BezierSegment segment;

			segment.Point1 = controlPoint1.ToPointF();
			segment.Point2 = controlPoint2.ToPointF();
			segment.Point3 = endPoint.ToPointF();

			_PathSink.AddBezier(segment);
		}

		void IGeometrySink.ArcTo(Point tangentStart, Point tangentEnd, Size radius)
		{
			Contract.Assert(_PathSink != null);

			_PathSink.AddLine(tangentStart.ToPointF());

			ArcSegment segment;

			segment.Point = tangentEnd.ToPointF();
			segment.Size = radius.ToSizeF();
			segment.SweepDirection = SweepDirection.Clockwise;
			segment.RotationAngle = 0.0f;
			segment.ArcSize = ArcSize.Small;

			_PathSink.AddArc(segment);
		}

		public void Build(Figure figure, out DxGeometry result)
		{
			Contract.Requires(figure != null);
			Contract.Ensures(Contract.ValueAtReturn(out result) != null);

			try
			{
				figure.Extract(this);
			}
			finally
			{
				result = _Path;

				_PathSink = null;
				_Path = null;
			}
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_Path.SafeDispose();
				_PathSink.SafeDispose();
			}
		}
	}
}