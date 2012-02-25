﻿// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.Shaping
{
	public sealed class Geometry : IEquatable<Geometry>
	{
		[ThreadStatic] private static Builder _Builder;

		private static readonly Geometry _Square;
		private static readonly Geometry _Circle;

		private readonly GeometryCommand[] _Commands;
		private readonly Point[] _Points;

		private Matrix3X2 _Transform;

		static Geometry()
		{
			_Square =
				Create().MoveTo(0.0f, 0.0f).LineTo(1.0f, 0.0f).LineTo(1.0f, 1.0f).
					LineTo(0.0f, 1.0f).LineTo(0.0f, 0.0f).Build();

			_Circle =
				Create().MoveTo(0.5f, 0.0f).ArcTo(
					0.5f, 0.0f, 0.5f, 1.0f, 0.5f, 0.5f).ArcTo(
						0.5f, 1.0f, 0.5f, 0.0f, 0.5f, 0.5f).Build();
		}

		private Geometry(Point[] points, GeometryCommand[] commands)
		{
			Contract.Requires(points != null);
			Contract.Requires(commands != null);

			_Points = points;
			_Commands = commands;
			_Transform = Matrix3X2.Identity;
		}

		private Geometry(
			Point[] points,
			GeometryCommand[] commands,
			ref Matrix3X2 transformation)
		{
			Contract.Requires(points != null);
			Contract.Requires(commands != null);

			_Points = points;
			_Commands = commands;
			_Transform = transformation;
		}

		public static Geometry Circle
		{
			get { return _Circle; }
		}

		public static Geometry Square
		{
			get { return _Square; }
		}

		public Matrix3X2 Transformation
		{
			get { return _Transform; }
		}

		public static Builder Create()
		{
			Contract.Ensures(Contract.Result<Builder>() != null);

			_Builder = _Builder ?? new Builder();

			_Builder.Reset();

			return _Builder;
		}

		public Geometry Scale(float width, float height)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			Matrix3X2 result;

			_Transform.Scale(width, height, out result);

			return new Geometry(_Points, _Commands, ref result);
		}

		public Geometry Scale(Size size)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return Scale(size.Width, size.Height);
		}

		public Geometry Scale(
			float width, float height, float originX, float originY)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			Matrix3X2 result;

			_Transform.Scale(width, height, originX, originY, out result);

			return new Geometry(_Points, _Commands, ref result);
		}

		public Geometry Skew(float angleX, float angleY)
		{
			Contract.Requires(Check.IsDegrees(angleX));
			Contract.Requires(Check.IsDegrees(angleY));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			Matrix3X2 result;

			_Transform.Skew(angleX, angleY, out result);

			return new Geometry(_Points, _Commands, ref result);
		}

		public Geometry Rotate(float angle)
		{
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			Matrix3X2 result;

			_Transform.Rotate(angle, out result);

			return new Geometry(_Points, _Commands, ref result);
		}

		public Geometry Rotate(float angle, float originX, float originY)
		{
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			Matrix3X2 result;

			_Transform.Rotate(angle, originX, originY, out result);

			return new Geometry(_Points, _Commands, ref result);
		}

		public Geometry Rotate(float angle, Point origin)
		{
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return Rotate(angle, origin.X, origin.Y);
		}

		public Geometry Translate(float width, float height)
		{
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			Matrix3X2 result;

			_Transform.Translate(width, height, out result);

			return new Geometry(_Points, _Commands, ref result);
		}

		public Geometry Translate(Size value)
		{
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return Translate(value.Width, value.Height);
		}

		public Geometry Transform(
			ref Matrix3X2 transformation,
			TransformMode operation = TransformMode.Multiply)
		{
			Contract.Ensures(Contract.Result<Geometry>() != null);

			if(operation == TransformMode.Replace)
			{
				return new Geometry(
					_Points, _Commands, ref transformation);
			}

			Matrix3X2 result;

			transformation.Multiply(ref _Transform, out result);

			return new Geometry(_Points, _Commands, ref result);
		}

		public void Extract(IGeometrySink sink)
		{
			Contract.Requires(sink != null);

			int pointIndex = 0;

			sink.Begin();

			foreach(GeometryCommand command in _Commands)
			{
				switch(command)
				{
					case GeometryCommand.Close:
						sink.Close();
						continue;
					case GeometryCommand.MoveTo:
						sink.MoveTo(
							_Points[pointIndex + 0].Transform(ref _Transform));
						pointIndex++;
						continue;
					case GeometryCommand.LineTo:
						sink.LineTo(
							_Points[pointIndex + 0].Transform(ref _Transform));
						pointIndex++;
						continue;
					case GeometryCommand.QuadraticCurveTo:
						sink.QuadraticCurveTo(
							_Points[pointIndex + 0].Transform(ref _Transform),
							_Points[pointIndex + 1].Transform(ref _Transform));
						pointIndex += 2;
						continue;
					case GeometryCommand.BezierCurveTo:
						sink.BezierCurveTo(
							_Points[pointIndex + 0].Transform(ref _Transform),
							_Points[pointIndex + 1].Transform(ref _Transform),
							_Points[pointIndex + 2].Transform(ref _Transform));
						pointIndex += 3;
						continue;
					case GeometryCommand.ArcTo:
						Size radius = new Size(
							_Points[pointIndex + 2].X, _Points[pointIndex + 2].Y);
						sink.ArcTo(
							_Points[pointIndex + 0].Transform(ref _Transform),
							_Points[pointIndex + 1].Transform(ref _Transform),
							radius.Transform(ref _Transform));
						pointIndex += 3;
						continue;
				}
			}

			Debug.Assert(pointIndex == _Points.Length);

			sink.End();
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(_Points != null);
			Contract.Invariant(_Commands != null);
		}

		public sealed class Builder
		{
			private readonly List<GeometryCommand> _Commands;
			private readonly List<Point> _Points;
			private readonly Stack<Matrix3X2> _States;

			private Matrix3X2 _Transform;

			internal Builder()
			{
				_Points = new List<Point>();
				_Commands = new List<GeometryCommand>();
				_States = new Stack<Matrix3X2>();

				_Transform = Matrix3X2.Identity;
			}

			public Geometry Build()
			{
				Contract.Ensures(Contract.Result<Geometry>() != null);

				Trace.Assert(_Points.Count > 0);
				Trace.Assert(_Commands.Count > 0);

				return new Geometry(
					_Points.ToArray(), _Commands.ToArray());
			}

			public Builder SaveState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_States.Push(_Transform);

				return this;
			}

			public Builder ResetState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Transform = Matrix3X2.Identity;

				return this;
			}

			public Builder RestoreState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Transform = _States.Pop();

				return this;
			}

			public Builder Close()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Commands.Add(GeometryCommand.Close);

				return this;
			}

			public Builder MoveTo(float x, float y)
			{
				Contract.Requires(Check.IsFinite(x));
				Contract.Requires(Check.IsFinite(y));
				Contract.Ensures(Contract.Result<Builder>() != null);

				return MoveTo(new Point(x, y));
			}

			public Builder MoveTo(Point point)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Commands.Add(GeometryCommand.MoveTo);

				try
				{
					_Points.Add(point.Transform(ref _Transform));
				}
				catch
				{
					// rollback changes to the command list on failure
					_Points.RemoveAt(_Points.Count - 1);

					// rethrow the exception
					throw;
				}

				return this;
			}

			public Builder LineTo(float x, float y)
			{
				Contract.Requires(Check.IsFinite(x));
				Contract.Requires(Check.IsFinite(y));
				Contract.Ensures(Contract.Result<Builder>() != null);

				return LineTo(new Point(x, y));
			}

			public Builder LineTo(Point point)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Commands.Add(GeometryCommand.LineTo);

				try
				{
					_Points.Add(point.Transform(ref _Transform));
				}
				catch
				{
					// rollback changes to the command list on failure
					_Commands.RemoveAt(_Commands.Count - 1);

					// rethrow the exception
					throw;
				}

				return this;
			}

			public Builder QuadraticCurveTo(
				float controlPointX,
				float controlPointY,
				float endPointX,
				float endPointY)
			{
				Contract.Requires(Check.IsFinite(controlPointX));
				Contract.Requires(Check.IsFinite(controlPointY));
				Contract.Requires(Check.IsFinite(endPointX));
				Contract.Requires(Check.IsFinite(endPointY));
				Contract.Ensures(Contract.Result<Builder>() != null);

				return QuadraticCurveTo(
					new Point(controlPointX, controlPointY),
					new Point(endPointX, endPointY));
			}

			public Builder QuadraticCurveTo(Point controlPoint, Point endPoint)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Commands.Add(GeometryCommand.QuadraticCurveTo);

				try
				{
					_Points.Add(controlPoint.Transform(ref _Transform));

					try
					{
						_Points.Add(endPoint.Transform(ref _Transform));
					}
					catch
					{
						// rollback changes to the points list on failure
						_Points.RemoveAt(_Points.Count - 1);

						// rethrow the exception
						throw;
					}
				}
				catch
				{
					// rollback changes to the command list on failure
					_Commands.RemoveAt(_Commands.Count - 1);

					// rethrow the exception
					throw;
				}

				return this;
			}

			public Builder BezierCurveTo(
				float controlPoint1X,
				float controlPoint1Y,
				float controlPoint2X,
				float controlPoint2Y,
				float endPointX,
				float endPointY)
			{
				Contract.Requires(Check.IsFinite(controlPoint1X));
				Contract.Requires(Check.IsFinite(controlPoint1Y));
				Contract.Requires(Check.IsFinite(controlPoint2X));
				Contract.Requires(Check.IsFinite(controlPoint2Y));
				Contract.Requires(Check.IsFinite(endPointX));
				Contract.Requires(Check.IsFinite(endPointY));
				Contract.Ensures(Contract.Result<Builder>() != null);

				return BezierCurveTo(
					new Point(controlPoint1X, controlPoint1Y),
					new Point(controlPoint2X, controlPoint2Y),
					new Point(endPointX, endPointY));
			}

			public Builder BezierCurveTo(
				Point controlPoint1, Point controlPoint2, Point endPoint)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Commands.Add(GeometryCommand.BezierCurveTo);

				try
				{
					_Points.Add(controlPoint1.Transform(ref _Transform));

					try
					{
						_Points.Add(controlPoint2.Transform(ref _Transform));

						try
						{
							_Points.Add(endPoint.Transform(ref _Transform));
						}
						catch
						{
							// rollback changes to the points list on failure
							_Points.RemoveAt(_Points.Count - 1);

							// rethrow the exception
							throw;
						}
					}
					catch
					{
						// rollback changes to the points list on failure
						_Points.RemoveAt(_Points.Count - 1);

						// rethrow the exception
						throw;
					}
				}
				catch
				{
					// rollback changes to the command list on failure
					_Commands.RemoveAt(_Commands.Count - 1);

					// rethrow the exception
					throw;
				}

				return this;
			}

			public Builder ArcTo(
				float tangentStartX,
				float tangentStartY,
				float tangentEndX,
				float tangentEndY,
				float radiusWidth,
				float radiusHeight)
			{
				Contract.Requires(Check.IsFinite(tangentStartX));
				Contract.Requires(Check.IsFinite(tangentStartY));
				Contract.Requires(Check.IsFinite(tangentEndX));
				Contract.Requires(Check.IsFinite(tangentEndY));
				Contract.Requires(Check.IsPositive(radiusWidth));
				Contract.Requires(Check.IsPositive(radiusHeight));
				Contract.Ensures(Contract.Result<Builder>() != null);

				return ArcTo(
					new Point(tangentStartX, tangentStartY),
					new Point(tangentEndX, tangentEndY),
					new Size(radiusWidth, radiusHeight));
			}

			public Builder ArcTo(
				Point tangentStart, Point tangentEnd, Size radius)
			{
				Contract.Requires(Check.IsPositive(radius.Width));
				Contract.Requires(Check.IsPositive(radius.Height));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Commands.Add(GeometryCommand.ArcTo);

				try
				{
					_Points.Add(tangentStart.Transform(ref _Transform));

					try
					{
						_Points.Add(tangentEnd.Transform(ref _Transform));

						try
						{
							// transform the radius by the active state
							Size tsize = radius.Transform(ref _Transform);

							// store the radius in a point
							_Points.Add(new Point(tsize.Width, tsize.Height));
						}
						catch
						{
							// rollback the change to the points list on failure
							_Points.RemoveAt(_Points.Count - 1);

							// rethrow the exception
							throw;
						}
					}
					catch
					{
						// rollback the change to the points list on failure
						_Points.RemoveAt(_Points.Count - 1);

						// rethrow the exception
						throw;
					}
				}
				catch
				{
					// rollback the changes to the command list on failure
					_Commands.RemoveAt(_Commands.Count - 1);

					// rethrow the exception
					throw;
				}

				return this;
			}

			public Builder Scale(float width, float height)
			{
				Contract.Requires(Check.IsPositive(width));
				Contract.Requires(Check.IsPositive(height));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Transform.Scale(width, height, out _Transform);

				return this;
			}

			public Builder Scale(Size size)
			{
				Contract.Requires(Check.IsPositive(size.Width));
				Contract.Requires(Check.IsPositive(size.Height));
				Contract.Ensures(Contract.Result<Builder>() != null);

				return Scale(size.Width, size.Height);
			}

			public Builder Scale(
				float width, float height, float originX, float originY)
			{
				Contract.Requires(Check.IsPositive(width));
				Contract.Requires(Check.IsPositive(height));
				Contract.Requires(Check.IsFinite(originX));
				Contract.Requires(Check.IsFinite(originY));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Transform.Scale(
					width, height, originX, originY, out _Transform);

				return this;
			}

			public Builder Skew(float angleX, float angleY)
			{
				Contract.Requires(Check.IsDegrees(angleX));
				Contract.Requires(Check.IsDegrees(angleY));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Transform.Skew(angleX, angleY, out _Transform);

				return this;
			}

			public Builder Rotate(float angle)
			{
				Contract.Requires(Check.IsDegrees(angle));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Transform.Rotate(angle, out _Transform);

				return this;
			}

			public Builder Rotate(float angle, float originX, float originY)
			{
				Contract.Requires(Check.IsDegrees(angle));
				Contract.Requires(Check.IsFinite(originX));
				Contract.Requires(Check.IsFinite(originY));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Transform.Rotate(
					angle, originX, originY, out _Transform);

				return this;
			}

			public Builder Rotate(float angle, Point origin)
			{
				Contract.Requires(Check.IsDegrees(angle));
				Contract.Ensures(Contract.Result<Builder>() != null);

				return Rotate(angle, origin.X, origin.Y);
			}

			public Builder Translate(float width, float height)
			{
				Contract.Requires(Check.IsFinite(width));
				Contract.Requires(Check.IsFinite(height));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Transform.Translate(width, height, out _Transform);

				return this;
			}

			public Builder Translate(Size value)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				return Translate(value.Width, value.Height);
			}

			public Builder Transform(
				ref Matrix3X2 transformation,
				TransformMode operation = TransformMode.Multiply)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				if(operation == TransformMode.Replace)
				{
					_Transform = transformation;

					return this;
				}

				transformation.Multiply(ref _Transform, out _Transform);

				return this;
			}

			internal void Reset()
			{
				_Points.Clear();
				_Commands.Clear();
				_States.Clear();

				_Transform = Matrix3X2.Identity;
			}
		}

		public bool Equals(Geometry other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other._Commands, _Commands) &&
			       Equals(other._Points, _Points) &&
			       other._Transform.Equals(_Transform);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			if(ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj is Geometry && this.Equals((Geometry)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _Commands.GetHashCode();
				result = (result * 397) ^ _Points.GetHashCode();
				result = (result * 397) ^ _Transform.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(Geometry left, Geometry right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Geometry left, Geometry right)
		{
			return !Equals(left, right);
		}

#if(UNIT_TESTING)
		private sealed class GeometrySink : IGeometrySink
		{
			public Geometry Geometry;
			private Builder _Builder;

			public void Begin()
			{
				_Builder = Create();
			}

			public void End()
			{
				Geometry = _Builder.Build();
			}

			public void Close()
			{
				_Builder.Close();
			}

			public void MoveTo(Point point)
			{
				_Builder.MoveTo(point);
			}

			public void LineTo(Point point)
			{
				_Builder.LineTo(point);
			}

			public void QuadraticCurveTo(Point controlPoint, Point endPoint)
			{
				_Builder.QuadraticCurveTo(controlPoint, endPoint);
			}

			public void BezierCurveTo(
				Point controlPoint1, Point controlPoint2, Point controlPoint3)
			{
				_Builder.BezierCurveTo(
					controlPoint1, controlPoint2, controlPoint3);
			}

			public void ArcTo(
				Point tangentStart, Point tangentEnd, Size radius)
			{
				_Builder.ArcTo(tangentStart, tangentEnd, radius);
			}
		}

		[Fact] internal static void Test0()
		{
			GeometrySink sink = new GeometrySink();

			Square.Scale(2.0f, 2.0f).Extract(sink);

			Assert.Equal(sink.Geometry._Points[0], new Point(0.0f, 0.0f));
			Assert.Equal(sink.Geometry._Points[1], new Point(2.0f, 0.0f));
			Assert.Equal(sink.Geometry._Points[2], new Point(2.0f, 2.0f));
			Assert.Equal(sink.Geometry._Points[3], new Point(0.0f, 2.0f));
			Assert.Equal(sink.Geometry._Points[4], new Point(0.0f, 0.0f));

			Square.Translate(2.0f, 2.0f).Extract(sink);

			Assert.Equal(sink.Geometry._Points[0], new Point(2.0f, 2.0f));
			Assert.Equal(sink.Geometry._Points[1], new Point(3.0f, 2.0f));
			Assert.Equal(sink.Geometry._Points[2], new Point(3.0f, 3.0f));
			Assert.Equal(sink.Geometry._Points[3], new Point(2.0f, 3.0f));
			Assert.Equal(sink.Geometry._Points[4], new Point(2.0f, 2.0f));

			Assert.TestObject(Square, Circle);
		}
#endif
	}
}