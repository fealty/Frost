// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.Shaping
{
	// Add traditional shapes as normalized, static geometries?
	public sealed class Geometry
	{
		[ThreadStatic] private static Builder _Builder;

		private readonly GeometryCommand[] _Commands;
		private readonly Point[] _Points;

		private Geometry(Point[] points, GeometryCommand[] commands)
		{
			Contract.Requires(points != null);
			Contract.Requires(commands != null);

			this._Points = points;
			this._Commands = commands;
		}

		public static Builder Create()
		{
			Contract.Ensures(Contract.Result<Builder>() != null);

			_Builder = _Builder ?? new Builder();

			_Builder.Reset();

			return _Builder;
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

				return new Geometry(
					this._Points.ToArray(), this._Commands.ToArray());
			}

			public Builder SaveState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				this._States.Push(this._Transform);

				return this;
			}

			public Builder ResetState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				this._Transform = Matrix3X2.Identity;

				return this;
			}

			public Builder RestoreState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				this._Transform = this._States.Pop();

				return this;
			}

			public Builder Close()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				this._Commands.Add(GeometryCommand.Close);

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

				this._Commands.Add(GeometryCommand.MoveTo);

				try
				{
					this._Points.Add(point.Transform(ref this._Transform));
				}
				catch
				{
					// rollback changes to the command list on failure
					this._Points.RemoveAt(this._Points.Count - 1);

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

				this._Commands.Add(GeometryCommand.LineTo);

				try
				{
					this._Points.Add(point.Transform(ref this._Transform));
				}
				catch
				{
					// rollback changes to the command list on failure
					this._Commands.RemoveAt(this._Commands.Count - 1);

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

				this._Commands.Add(GeometryCommand.QuadraticCurveTo);

				try
				{
					this._Points.Add(controlPoint.Transform(ref this._Transform));

					try
					{
						this._Points.Add(endPoint.Transform(ref this._Transform));
					}
					catch
					{
						// rollback changes to the points list on failure
						this._Points.RemoveAt(this._Points.Count - 1);

						// rethrow the exception
						throw;
					}
				}
				catch
				{
					// rollback changes to the command list on failure
					this._Commands.RemoveAt(this._Commands.Count - 1);

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

				this._Commands.Add(GeometryCommand.BezierCurveTo);

				try
				{
					this._Points.Add(controlPoint1.Transform(ref this._Transform));

					try
					{
						this._Points.Add(controlPoint2.Transform(ref this._Transform));

						try
						{
							this._Points.Add(endPoint.Transform(ref this._Transform));
						}
						catch
						{
							// rollback changes to the points list on failure
							this._Points.RemoveAt(this._Points.Count - 1);

							// rethrow the exception
							throw;
						}
					}
					catch
					{
						// rollback changes to the points list on failure
						this._Points.RemoveAt(this._Points.Count - 1);

						// rethrow the exception
						throw;
					}
				}
				catch
				{
					// rollback changes to the command list on failure
					this._Commands.RemoveAt(this._Commands.Count - 1);

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
				Contract.Ensures(Contract.Result<Builder>() != null);

				Trace.Assert(Check.IsPositive(radius.Width));
				Trace.Assert(Check.IsPositive(radius.Height));

				this._Commands.Add(GeometryCommand.ArcTo);

				try
				{
					this._Points.Add(tangentStart.Transform(ref this._Transform));

					try
					{
						this._Points.Add(tangentEnd.Transform(ref this._Transform));

						try
						{
							// transform the radius by the active state
							Size tsize = radius.Transform(ref this._Transform);

							// store the radius in a point
							this._Points.Add(new Point(tsize.Width, tsize.Height));
						}
						catch
						{
							// rollback the change to the points list on failure
							this._Points.RemoveAt(this._Points.Count - 1);

							// rethrow the exception
							throw;
						}
					}
					catch
					{
						// rollback the change to the points list on failure
						this._Points.RemoveAt(this._Points.Count - 1);

						// rethrow the exception
						throw;
					}
				}
				catch
				{
					// rollback the changes to the command list on failure
					this._Commands.RemoveAt(this._Commands.Count - 1);

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

				this._Transform.Scale(width, height, out this._Transform);

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

				this._Transform.Scale(
					width, height, originX, originY, out this._Transform);

				return this;
			}

			public Builder Skew(float angleX, float angleY)
			{
				Contract.Requires(Check.IsDegrees(angleX));
				Contract.Requires(Check.IsDegrees(angleY));
				Contract.Ensures(Contract.Result<Builder>() != null);

				this._Transform.Skew(angleX, angleY, out this._Transform);

				return this;
			}

			public Builder Rotate(float angle)
			{
				Contract.Requires(Check.IsDegrees(angle));
				Contract.Ensures(Contract.Result<Builder>() != null);

				this._Transform.Rotate(angle, out this._Transform);

				return this;
			}

			public Builder Rotate(float angle, float originX, float originY)
			{
				Contract.Requires(Check.IsDegrees(angle));
				Contract.Requires(Check.IsFinite(originX));
				Contract.Requires(Check.IsFinite(originY));
				Contract.Ensures(Contract.Result<Builder>() != null);

				this._Transform.Rotate(
					angle, originX, originY, out this._Transform);

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
				
				this._Transform.Translate(width, height, out this._Transform);

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
					this._Transform = transformation;

					return this;
				}

				transformation.Multiply(ref this._Transform, out this._Transform);

				return this;
			}

			internal void Reset()
			{
				this._Points.Clear();
				this._Commands.Clear();
				this._States.Clear();

				this._Transform = Matrix3X2.Identity;
			}
		}
	}
}