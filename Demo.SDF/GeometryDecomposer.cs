// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost;
using Frost.Shaping;

namespace Demo.SDF
{
	internal sealed class SimplifiedGeometry : IGeometrySink
	{
		private static readonly List<FullLine> mBuiltLines;

		private static LineBuilder mLineBuilder;

		private readonly Geometry mGeometry;
		private readonly FullLine[] mLines;
		private readonly Rectangle mRegion;

		static SimplifiedGeometry()
		{
			mBuiltLines = new List<FullLine>();

			mLineBuilder = new LineBuilder();
		}

		public SimplifiedGeometry(Geometry geometry, Device2D device2D)
		{
			mGeometry = geometry;

			mRegion = device2D.Geometry.MeasureRegion(geometry);

			Geometry simplified = device2D.Geometry.Simplify(
				geometry, 1.0f / DistanceField.ResolvedLength);

			simplified.Extract(this);

			mLines = mBuiltLines.ToArray();
		}

		public bool IsLine
		{
			get { return mLines.Length == 1; }
		}

		public Rectangle Region
		{
			get { return mRegion; }
		}

		public Geometry Geometry
		{
			get { return mGeometry; }
		}

		void IGeometrySink.Begin()
		{
			mBuiltLines.Clear();

			mLineBuilder = new LineBuilder();
		}

		void IGeometrySink.End()
		{
		}

		void IGeometrySink.Close()
		{
			throw new NotSupportedException();
		}

		void IGeometrySink.MoveTo(Point point)
		{
			mLineBuilder.Point1 = point;
		}

		void IGeometrySink.LineTo(Point endPoint)
		{
			if(mLineBuilder.Point1 == null)
			{
				mLineBuilder.Point1 = endPoint;
			}
			else if(mLineBuilder.Point2 == null)
			{
				mLineBuilder.Point2 = endPoint;

				FullLine newLine;

				newLine.Point1 = mLineBuilder.Point1.Value;
				newLine.Point2 = mLineBuilder.Point2.Value;

				mBuiltLines.Add(newLine);

				mLineBuilder = new LineBuilder();
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

		public void ComputeDistancesFrom(
			ref Point point, out double minimumResult, out double maximumResult)
		{
			minimumResult = double.MaxValue;
			maximumResult = double.MinValue;

			Point topLeft = new Point(mRegion.Left, mRegion.Top);
			Point topRight = new Point(mRegion.Right, mRegion.Top);
			Point bottomRight = new Point(mRegion.Right, mRegion.Bottom);
			Point bottomLeft = new Point(mRegion.Left, mRegion.Bottom);

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

		public void Query(ref Point point, out float distance, out Point intersection)
		{
			distance = float.MaxValue;

			FullLine selectedLine = mLines[0];

			if(mLines.Length > 1)
			{
				foreach(FullLine line in mLines)
				{
					Point point1 = line.Point1;
					Point point2 = line.Point2;

					float newDistance = point.SquaredDistanceTo(point1, point2);

					if(newDistance < distance)
					{
						distance = newDistance;

						selectedLine = line;
					}
				}
			}

			distance = point.DistanceTo(selectedLine.Point1, selectedLine.Point2, out intersection);
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

	internal sealed class GeometryDecomposer : IGeometrySink
	{
		private readonly LinkedList<Test4> mActiveNodes;
		private readonly List<SimplifiedGeometry> mComponents;
		private readonly List<Geometry> mGeometries;
		private readonly LinkedList<Test4> mInactiveNodes;
		private Point mFigureCurrent;

		private Point? mFigureStart;
		private Geometry mGeometry;
		private Rectangle mRegion;

		private Matrix3X2 mTransform;

		public GeometryDecomposer()
		{
			mGeometries = new List<Geometry>();
			mComponents = new List<SimplifiedGeometry>();

			mActiveNodes = new LinkedList<Test4>();
			mInactiveNodes = new LinkedList<Test4>();
		}

		public List<SimplifiedGeometry> Components
		{
			get { return mComponents; }
		}

		public Rectangle Region
		{
			get { return mRegion; }
		}

		void IGeometrySink.Begin()
		{
			mFigureStart = null;

			mFigureCurrent = Point.Empty;
		}

		void IGeometrySink.End()
		{
		}

		void IGeometrySink.Close()
		{
			Contract.Assert(mFigureStart != null);

			mGeometries.Add(
				Geometry.Create().Transform(ref mTransform, TransformMode.Replace).MoveTo(mFigureCurrent).LineTo
					(mFigureStart.Value.X, mFigureStart.Value.Y).Build());

			mFigureCurrent = Point.Empty;
		}

		void IGeometrySink.MoveTo(Point point)
		{
			mFigureStart = point;
			mFigureCurrent = point;
		}

		void IGeometrySink.LineTo(Point endPoint)
		{
			Contract.Assert(mFigureStart != null);

			mGeometries.Add(
				Geometry.Create().Transform(ref mTransform, TransformMode.Replace).MoveTo(mFigureCurrent).LineTo
					(endPoint).Build());

			mFigureCurrent = endPoint;
		}

		void IGeometrySink.QuadraticCurveTo(Point controlPoint, Point endPoint)
		{
			Contract.Assert(mFigureStart != null);

			mGeometries.Add(
				Geometry.Create().Transform(ref mTransform, TransformMode.Replace).MoveTo(mFigureCurrent).
					QuadraticCurveTo(controlPoint, endPoint).Build());

			mFigureCurrent = endPoint;
		}

		void IGeometrySink.BezierCurveTo(Point controlPoint1, Point controlPoint2, Point endPoint)
		{
			Contract.Assert(mFigureStart != null);

			mGeometries.Add(
				Geometry.Create().Transform(ref mTransform, TransformMode.Replace).MoveTo(mFigureCurrent).
					BezierCurveTo(controlPoint1, controlPoint2, endPoint).Build());

			mFigureCurrent = endPoint;
		}

		void IGeometrySink.ArcTo(Point tangentStart, Point tangentEnd, Size radius)
		{
			Contract.Assert(mFigureStart != null);

			mGeometries.Add(
				Geometry.Create().Transform(ref mTransform, TransformMode.Replace).MoveTo(mFigureCurrent).ArcTo(
					tangentStart, tangentEnd, radius).Build());

			mFigureCurrent = tangentEnd;
		}

		public void Decompose(Geometry geometry, ref Matrix3X2 transform, Device2D device2D)
		{
			Geometry tg = geometry.Transform(ref transform);

			mRegion = device2D.Geometry.MeasureRegion(tg);

			mGeometry = geometry;

			mTransform = transform;

			geometry.Extract(this);

			mComponents.Clear();

			foreach(Geometry item in mGeometries)
			{
				mComponents.Add(new SimplifiedGeometry(item, device2D));
			}

			foreach(SimplifiedGeometry geo in mComponents)
			{
				Test4 item;

				item.Geometry = geo;
				item.MinimumDistance = 0.0;
				item.MaximumDistance = 0.0;

				mInactiveNodes.AddLast(item);
			}

			mGeometries.Clear();
		}

		public void Query(ref Point point, Device2D device2D, out float distance, out Point intersection)
		{
			// compute the distances to each bounding region
			for(var node = mInactiveNodes.First; node != null; node = node.Next)
			{
				Test4 item = node.Value;

				double minimumDistanceA;
				double maximumDistanceA;

				item.Geometry.ComputeDistancesFrom(ref point, out minimumDistanceA, out maximumDistanceA);

				item.MinimumDistance = minimumDistanceA;
				item.MaximumDistance = maximumDistanceA;

				node.Value = item;
			}

			// transfer all inactive nodes to the active node list
			ExchangeItems(mInactiveNodes, mActiveNodes);

			// Select the smallest maximum distance
			double leastMaximumDistance = double.MaxValue;

			foreach(Test4 item in mActiveNodes)
			{
				if(item.MaximumDistance < leastMaximumDistance)
				{
					leastMaximumDistance = item.MaximumDistance;
				}
			}

			// remove nodes that cannot possibly the nearest
			LinkedListNode<Test4> activeNode = mActiveNodes.First;

			while(activeNode != null)
			{
				LinkedListNode<Test4> next = activeNode.Next;

				if(activeNode.Value.MinimumDistance > leastMaximumDistance)
				{
					if(!activeNode.Value.Geometry.Region.Contains(point))
					{
						mActiveNodes.Remove(activeNode);

						mInactiveNodes.AddLast(activeNode);
					}
				}

				activeNode = next;
			}

			distance = float.MaxValue;

			intersection = Point.Empty;

			foreach(Test4 item in mActiveNodes)
			{
				Point newDirection;

				float newDistance;

				item.Geometry.Query(ref point, out newDistance, out newDirection);

				if(Math.Abs(newDistance) < Math.Abs(distance))
				{
					distance = newDistance;
					intersection = newDirection;
				}
			}

			Geometry tg = mGeometry.Transform(ref mTransform);

			if(device2D.Geometry.Contains(tg, point, 1))
			{
				distance = -distance;
			}

			// transfer all active nodes to the inactive node list
			ExchangeItems(mActiveNodes, mInactiveNodes);
		}

		private static void ExchangeItems<T>(LinkedList<T> source, LinkedList<T> destination)
		{
			LinkedListNode<T> activeNode = source.First;

			while(activeNode != null)
			{
				LinkedListNode<T> next = activeNode.Next;

				source.Remove(activeNode);

				destination.AddLast(activeNode);

				activeNode = next;
			}
		}

		private struct Test4
		{
			public SimplifiedGeometry Geometry;
			public double MaximumDistance;
			public double MinimumDistance;
		}
	}
}