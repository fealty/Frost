// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.Construction
{
	public sealed class Figure : IEquatable<Figure>
	{
		[ThreadStatic]
		private static Builder _Builder;

		private static readonly Figure _Square;
		private static readonly Figure _Circle;

		private readonly GeometryCommand[] _Commands;
		private readonly Point[] _Points;

		private Matrix3X2 _Transform;

		static Figure()
		{
			_Square =
				Create().MoveTo(0.0f, 0.0f).LineTo(1.0f, 0.0f).LineTo(1.0f, 1.0f).LineTo(
					0.0f, 1.0f).LineTo(
						0.0f, 0.0f).Build();

			_Circle =
				Create().MoveTo(0.5f, 0.0f).ArcTo(0.5f, 0.0f, 0.5f, 1.0f, 0.5f, 0.5f).
					ArcTo(
						0.5f, 1.0f, 0.5f, 0.0f, 0.5f, 0.5f).Build();
		}

		private Figure(Point[] points, GeometryCommand[] commands)
		{
			Contract.Requires(points != null);
			Contract.Requires(commands != null);

			_Points = points;
			_Commands = commands;
			_Transform = Matrix3X2.Identity;
		}

		private Figure(
			Point[] points, GeometryCommand[] commands, ref Matrix3X2 transformation)
		{
			Contract.Requires(points != null);
			Contract.Requires(commands != null);

			_Points = points;
			_Commands = commands;
			_Transform = transformation;
		}

		public static Figure Circle
		{
			get { return _Circle; }
		}

		public static Figure Square
		{
			get { return _Square; }
		}

		public Matrix3X2 Transformation
		{
			get { return _Transform; }
		}

		public bool Equals(Figure other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other._Commands, _Commands) && Equals(other._Points, _Points) &&
				other._Transform.Equals(_Transform);
		}

		public static Builder Create()
		{
			Contract.Ensures(Contract.Result<Builder>() != null);

			_Builder = _Builder ?? new Builder();

			_Builder.Reset();

			return _Builder;
		}

		public Figure Scale(float width, float height)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));
			Contract.Ensures(Contract.Result<Figure>() != null);

			Matrix3X2 result;

			_Transform.Scale(width, height, out result);

			return new Figure(_Points, _Commands, ref result);
		}

		public Figure Scale(Size size)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));
			Contract.Ensures(Contract.Result<Figure>() != null);

			return Scale(size.Width, size.Height);
		}

		public Figure Scale(float width, float height, float originX, float originY)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));
			Contract.Ensures(Contract.Result<Figure>() != null);

			Matrix3X2 result;

			_Transform.Scale(width, height, originX, originY, out result);

			return new Figure(_Points, _Commands, ref result);
		}

		public Figure Skew(float angleX, float angleY)
		{
			Contract.Requires(Check.IsDegrees(angleX));
			Contract.Requires(Check.IsDegrees(angleY));
			Contract.Ensures(Contract.Result<Figure>() != null);

			Matrix3X2 result;

			_Transform.Skew(angleX, angleY, out result);

			return new Figure(_Points, _Commands, ref result);
		}

		public Figure Rotate(float angle)
		{
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Ensures(Contract.Result<Figure>() != null);

			Matrix3X2 result;

			_Transform.Rotate(angle, out result);

			return new Figure(_Points, _Commands, ref result);
		}

		public Figure Rotate(float angle, float originX, float originY)
		{
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));
			Contract.Ensures(Contract.Result<Figure>() != null);

			Matrix3X2 result;

			_Transform.Rotate(angle, originX, originY, out result);

			return new Figure(_Points, _Commands, ref result);
		}

		public Figure Rotate(float angle, Point origin)
		{
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Ensures(Contract.Result<Figure>() != null);

			return Rotate(angle, origin.X, origin.Y);
		}

		public Figure Translate(float width, float height)
		{
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));
			Contract.Ensures(Contract.Result<Figure>() != null);

			Matrix3X2 result;

			_Transform.Translate(width, height, out result);

			return new Figure(_Points, _Commands, ref result);
		}

		public Figure Translate(Size value)
		{
			Contract.Ensures(Contract.Result<Figure>() != null);

			return Translate(value.Width, value.Height);
		}

		public Figure Transform(
			ref Matrix3X2 transformation,
			TransformMode operation = TransformMode.Multiply)
		{
			Contract.Ensures(Contract.Result<Figure>() != null);

			if(operation == TransformMode.Replace)
			{
				return new Figure(_Points, _Commands, ref transformation);
			}

			Matrix3X2 result;

			transformation.Multiply(ref _Transform, out result);

			return new Figure(_Points, _Commands, ref result);
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
						sink.MoveTo(_Points[pointIndex + 0].Transform(ref _Transform));
						pointIndex++;
						continue;
					case GeometryCommand.LineTo:
						sink.LineTo(_Points[pointIndex + 0].Transform(ref _Transform));
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

			return obj is Figure && Equals((Figure)obj);
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

		[ContractInvariantMethod]
		private void Invariant()
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

			public Figure Build()
			{
				if(_Points.Count > 0 && _Commands.Count > 0)
				{
					return new Figure(_Points.ToArray(), _Commands.ToArray());
				}

				return null;
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
				float controlPointX, float controlPointY, float endPointX, float endPointY)
			{
				Contract.Requires(Check.IsFinite(controlPointX));
				Contract.Requires(Check.IsFinite(controlPointY));
				Contract.Requires(Check.IsFinite(endPointX));
				Contract.Requires(Check.IsFinite(endPointY));
				Contract.Ensures(Contract.Result<Builder>() != null);

				return QuadraticCurveTo(
					new Point(controlPointX, controlPointY), new Point(endPointX, endPointY));
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

			public Builder ArcTo(Point tangentStart, Point tangentEnd, Size radius)
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

				_Transform.Scale(width, height, originX, originY, out _Transform);

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

				_Transform.Rotate(angle, originX, originY, out _Transform);

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

		public static bool operator ==(Figure left, Figure right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Figure left, Figure right)
		{
			return !Equals(left, right);
		}
	}
}