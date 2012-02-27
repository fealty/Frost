// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost;
using Frost.Shaping;

namespace Demo
{
	internal sealed class GeometryNode : ClusterNode, IGeometrySink
	{
		private static readonly List<FullLine> _BuiltLines;

		private static LineBuilder _LineBuilder;

		private static Point _FigureStart;

		private readonly Geometry _Geometry;
		private readonly FullLine[] _Lines;

		static GeometryNode()
		{
			_BuiltLines = new List<FullLine>();

			_LineBuilder = new LineBuilder();
		}

		public GeometryNode(Geometry geometry, float resolution, Device2D device2D)
		{
			Contract.Requires(geometry != null);
			Contract.Requires(resolution >= double.MinValue && resolution <= double.MaxValue);
			Contract.Requires(device2D != null);

			Rectangle region = device2D.ComputeRegion(geometry);

			SetRegion(ref region);

			_Geometry = geometry;

			Geometry simplified = device2D.Simplify(geometry, 1.0f / resolution);

			simplified.Extract(this);

			_Lines = _BuiltLines.ToArray();
		}

		public override bool IsGroup
		{
			get { return false; }
		}

		public override Geometry Geometry
		{
			get { return _Geometry; }
		}

		void IGeometrySink.Begin()
		{
			_BuiltLines.Clear();

			_LineBuilder = new LineBuilder();
		}

		void IGeometrySink.End()
		{
		}

		void IGeometrySink.Close()
		{
			if(_LineBuilder.Point1 == null)
			{
				throw new InvalidOperationException();
			}

			_LineBuilder.Point2 = _FigureStart;

			FullLine newLine;

			newLine.Point1 = _LineBuilder.Point1.Value;
			newLine.Point2 = _LineBuilder.Point2.Value;

			_BuiltLines.Add(newLine);

			_LineBuilder = new LineBuilder();
		}

		void IGeometrySink.MoveTo(Point point)
		{
			_FigureStart = point;

			_LineBuilder.Point1 = point;
		}

		void IGeometrySink.LineTo(Point endPoint)
		{
			if(_LineBuilder.Point1 == null)
			{
				_LineBuilder.Point1 = endPoint;
			}
			else if(_LineBuilder.Point2 == null)
			{
				_LineBuilder.Point2 = endPoint;

				FullLine newLine;

				newLine.Point1 = _LineBuilder.Point1.Value;
				newLine.Point2 = _LineBuilder.Point2.Value;

				_BuiltLines.Add(newLine);

				_LineBuilder = new LineBuilder();
			}
		}

		void IGeometrySink.QuadraticCurveTo(Point controlPoint, Point endPoint)
		{
			throw new NotSupportedException();
		}

		void IGeometrySink.BezierCurveTo(Point controlPoint1, Point controlPoint2, Point endPoint)
		{
			throw new NotSupportedException();
		}

		void IGeometrySink.ArcTo(Point tangentStart, Point tangentEnd, Size radius)
		{
			throw new NotSupportedException();
		}

		public override void ComputeDistancesFrom(
			ref Point point, out double minimumResult, out double maximumResult)
		{
			minimumResult = double.MaxValue;
			maximumResult = double.MinValue;

			Rectangle region = Region;

			Point topLeft = new Point(region.Left, region.Top);
			Point topRight = new Point(region.Right, region.Top);
			Point bottomRight = new Point(region.Right, region.Bottom);
			Point bottomLeft = new Point(region.Left, region.Bottom);

			double leftEdge = point.SquaredDistanceTo(topLeft, bottomLeft);
			double topEdge = point.SquaredDistanceTo(topLeft, topRight);
			double rightEdge = point.SquaredDistanceTo(topRight, bottomRight);
			double bottomEdge = point.SquaredDistanceTo(bottomLeft, bottomRight);

			minimumResult = Math.Min(minimumResult, leftEdge);
			minimumResult = Math.Min(minimumResult, topEdge);
			minimumResult = Math.Min(minimumResult, rightEdge);
			minimumResult = Math.Min(minimumResult, bottomEdge);

			maximumResult = Math.Max(maximumResult, leftEdge);
			maximumResult = Math.Max(maximumResult, topEdge);
			maximumResult = Math.Max(maximumResult, rightEdge);
			maximumResult = Math.Max(maximumResult, bottomEdge);
		}

		public override void Query(ref Point point, out Sample.Location sample)
		{
			sample.Distance = float.MaxValue;

			FullLine selectedLine = _Lines[0];

			if(_Lines.Length > 1)
			{
				foreach(FullLine line in _Lines)
				{
					Point point1 = line.Point1;
					Point point2 = line.Point2;

					float newDistance = point.SquaredDistanceTo(point1, point2);

					if(newDistance < sample.Distance)
					{
						sample.Distance = newDistance;

						selectedLine = line;
					}
				}
			}

			sample.Distance = point.DistanceTo(
				selectedLine.Point1, selectedLine.Point2, out sample.Intersection);
		}

		private struct FullLine
		{
			public Point Point1;
			public Point Point2;
		}

		private struct LineBuilder
		{
			public Point? Point1;
			public Point? Point2;
		}
	}
}