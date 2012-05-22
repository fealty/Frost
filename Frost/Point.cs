// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	/// <summary>
	///   represents a finite point in 2D space
	/// </summary>
	public struct Point : IEquatable<Point>
	{
		private static readonly Point _MaxValue;
		private static readonly Point _MinValue;

		private static readonly Point _Empty;

		private readonly float _X;
		private readonly float _Y;

		static Point()
		{
			_MinValue = new Point(Single.MinValue);
			_MaxValue = new Point(Single.MaxValue);

			_Empty = new Point(0.0f);
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsFinite(_X));
			Contract.Invariant(Check.IsFinite(_Y));
		}

		/// <summary>
		///   constructs a new <see cref="Point" /> from the given coordinates
		/// </summary>
		/// <param name="x"> the finite X coordinate </param>
		/// <param name="y"> the finite Y coordinate </param>
		public Point(float x, float y)
		{
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));

			_X = x;
			_Y = y;

			Contract.Assert(X.Equals(x));
			Contract.Assert(Y.Equals(y));
		}

		/// <summary>
		///   constructs a new <see cref="Point" /> from the given coordinates
		/// </summary>
		/// <param name="xy"> the finite X and Y coordinates </param>
		public Point(float xy) : this(xy, xy)
		{
			Contract.Requires(Check.IsFinite(xy));
		}

		/// <summary>
		///   implicitly converts a <see cref="Size" /> to a <see cref="Point" />
		/// </summary>
		/// <param name="size"> the <see cref="Size" /> to convert </param>
		/// <returns> the <see cref="Point" /> as represented by a <see cref="Size" /> </returns>
		public static implicit operator Point(Size size)
		{
			return new Point(size.Width, size.Height);
		}

		/// <summary>
		///   gets the Y coordinate of the <see cref="Point" />
		/// </summary>
		public float Y
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_Y));

				return _Y;
			}
		}

		/// <summary>
		///   gets the X coordinate of the <see cref="Point" />
		/// </summary>
		public float X
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_X));

				return _X;
			}
		}

		/// <summary>
		///   gets the default value for <see cref="Point" />
		/// </summary>
		public static Point Empty
		{
			get
			{
				Contract.Ensures(Contract.Result<Point>().Equals(_Empty));

				return _Empty;
			}
		}

		/// <summary>
		///   gets the minimum value a <see cref="Point" /> can represent
		/// </summary>
		public static Point MinValue
		{
			get
			{
				Contract.Ensures(Contract.Result<Point>().Equals(_MinValue));

				return _MinValue;
			}
		}

		/// <summary>
		///   gets the maximum value a <see cref="Point" /> can represent
		/// </summary>
		public static Point MaxValue
		{
			get
			{
				Contract.Ensures(Contract.Result<Point>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		public bool Equals(Point other)
		{
			return other._X.Equals(_X) && other._Y.Equals(_Y);
		}

		/// <summary>
		///   computes the distance from the <see cref="Point" /> to a point
		/// </summary>
		/// <param name="point"> the point to measure the distance to </param>
		/// <returns> the distance from the <see cref="Point" /> to <paramref name="point" /> </returns>
		public float DistanceTo(Point point)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			double dx = point.X - _X;
			double dy = point.Y - _Y;

			return Convert.ToSingle(Math.Sqrt((dx * dx) + (dy * dy)));
		}

		/// <summary>
		///   computes the distanced from the <see cref="Point" /> to a line
		/// </summary>
		/// <param name="lineStart"> the start location of the line </param>
		/// <param name="lineEnd"> the end location of the line </param>
		/// <returns> the distance from the <see cref="Point" /> to the line formed by <paramref name="lineStart" /> and <see
		///    cref="lineEnd" /> </returns>
		public float DistanceTo(Point lineStart, Point lineEnd)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			Point intersection = FindIntersectionWith(lineStart, lineEnd);

			return DistanceTo(intersection);
		}

		/// <summary>
		///   computes the distanced from the <see cref="Point" /> to a line
		/// </summary>
		/// <param name="lineStart"> the start location of the line </param>
		/// <param name="lineEnd"> the end location of the line </param>
		/// <param name="intersection"> the output location of intersection along the line </param>
		/// <returns> the distance from the <see cref="Point" /> to the line formed by <paramref name="lineStart" /> and <see
		///    cref="lineEnd" /> </returns>
		public float DistanceTo(
			Point lineStart, Point lineEnd, out Point intersection)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			intersection = FindIntersectionWith(lineStart, lineEnd);

			return DistanceTo(intersection);
		}

		/// <summary>
		///   computes the squared distance from the <see cref="Point" /> to a point
		/// </summary>
		/// <param name="point"> the point to measure the squared distance to </param>
		/// <returns> the squared distance from the <see cref="Point" /> to <paramref name="point" /> </returns>
		public float SquaredDistanceTo(Point point)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			double dx = point.X - _X;
			double dy = point.Y - _Y;

			return Convert.ToSingle((dx * dx) + (dy * dy));
		}

		/// <summary>
		///   computes the squared distanced from the <see cref="Point" /> to a line
		/// </summary>
		/// <param name="lineStart"> the start location of the line </param>
		/// <param name="lineEnd"> the end location of the line </param>
		/// <returns> the squared distance from the <see cref="Point" /> to the line formed by <paramref name="lineStart" /> and <see
		///    cref="lineEnd" /> </returns>
		public float SquaredDistanceTo(Point lineStart, Point lineEnd)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			Point intersection = FindIntersectionWith(lineStart, lineEnd);

			return SquaredDistanceTo(intersection);
		}

		/// <summary>
		///   computes the squared distanced from the <see cref="Point" /> to a line
		/// </summary>
		/// <param name="lineStart"> the start location of the line </param>
		/// <param name="lineEnd"> the end location of the line </param>
		/// <param name="intersection"> the output location of intersection along the line </param>
		/// <returns> the squared distance from the <see cref="Point" /> to the line formed by <paramref name="lineStart" /> and <see
		///    cref="lineEnd" /> </returns>
		public float SquaredDistanceTo(
			Point lineStart, Point lineEnd, out Point intersection)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			intersection = FindIntersectionWith(lineStart, lineEnd);

			return SquaredDistanceTo(intersection);
		}

		/// <summary>
		///   finds the nearest point of intersection with a line
		/// </summary>
		/// <param name="lineStart"> the start location of the line </param>
		/// <param name="lineEnd"> the end location of the line </param>
		/// <returns> the location of the intersection along the line formed by <paramref name="lineStart" /> and <paramref
		///    name="lineEnd" /> </returns>
		public Point FindIntersectionWith(Point lineStart, Point lineEnd)
		{
			Point v = new Point(lineEnd.X - lineStart.X, lineEnd.Y - lineStart.Y);
			Point w = new Point(_X - lineStart.X, _Y - lineStart.Y);

			float c1 = w.X * v.X + w.Y * v.Y;

			if(c1 <= 0.0)
			{
				return lineStart;
			}

			float c2 = v.X * v.X + v.Y * v.Y;

			if(c2 <= c1)
			{
				return lineEnd;
			}

			float d = c1 / c2;

			return new Point(lineStart.X + (d * v.X), lineStart.Y + (d * v.Y));
		}

		/// <summary>
		///   transforms the <see cref="Point" /> by the given transformation matrix
		/// </summary>
		/// <param name="transformation"> the transformation matrix </param>
		/// <returns> the <see cref="Point" /> transformed by <paramref name="transformation" /> </returns>
		public Point Transform(ref Matrix3X2 transformation)
		{
			return new Point(
				(_X * transformation.M11) + (_Y * transformation.M21) + transformation.M31,
				(_X * transformation.M12) + (_Y * transformation.M22) + transformation.M32);
		}

		/// <summary>
		///   translates the <see cref="Point" /> by the given amount
		/// </summary>
		/// <param name="amount"> the amount to translate on both horizontal and vertical axes </param>
		/// <returns> the <see cref="Point" /> translated by <paramref name="amount" /> </returns>
		public Point Translate(Size amount)
		{
			return new Point(_X + amount.Width, _Y + amount.Height);
		}

		/// <summary>
		///   translates the <see cref="Point" /> by the given amounts
		/// </summary>
		/// <param name="width"> the amount to translate on the horizontal axis </param>
		/// <param name="height"> the amount to translate on the vertical axis </param>
		/// <returns> the <see cref="Point" /> translated by <paramref name="width" /> and <paramref name="height" /> </returns>
		public Point Translate(float width, float height)
		{
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));

			return new Point(_X + width, _Y + height);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Point && Equals((Point)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_X.GetHashCode() * 397) ^ _Y.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format("X: {0}, Y: {1}", _X, _Y);
		}

		/// <summary>
		///   determines whether two instances of <see cref="Point" /> are equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> equals <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator ==(Point left, Point right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   determines whether two instances of <see cref="Point" /> are not equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> does not equal <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator !=(Point left, Point right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact]
		internal static void Test0()
		{
			Assert.Equal(0, new Point(0, 1).X);
			Assert.Equal(1, new Point(0, 1).Y);

			Assert.Equal(0, Empty.X);
			Assert.Equal(0, Empty.Y);

			Assert.Equal(1.41421, Math.Round(Empty.DistanceTo(new Point(1, 1)), 5));
			Assert.Equal(0.00000, Math.Round(Empty.DistanceTo(new Point(0, 0)), 5));

			Assert.Equal(2, Empty.SquaredDistanceTo(new Point(1, 1)));
			Assert.Equal(0, Empty.SquaredDistanceTo(new Point(0, 0)));

			Assert.Equal(
				new Point(0, 0),
				Empty.FindIntersectionWith(new Point(-5, 5), new Point(0, +0)));
			Assert.Equal(
				new Point(0, 0),
				Empty.FindIntersectionWith(new Point(+0, 0), new Point(5, -5)));
			Assert.Equal(
				new Point(0, 0),
				Empty.FindIntersectionWith(new Point(+0, 0), new Point(0, +0)));
			Assert.Equal(
				new Point(1, 0),
				Empty.FindIntersectionWith(new Point(+1, 5), new Point(1, -5)));
			Assert.Equal(
				new Point(5, 0),
				Empty.FindIntersectionWith(new Point(+5, 5), new Point(5, -5)));

			Assert.Equal(
				Empty.DistanceTo(new Point(0, 0)),
				Empty.DistanceTo(new Point(-5, 5), new Point(0, +0)));
			Assert.Equal(
				Empty.DistanceTo(new Point(0, 0)),
				Empty.DistanceTo(new Point(+0, 0), new Point(5, -5)));
			Assert.Equal(
				Empty.DistanceTo(new Point(0, 0)),
				Empty.DistanceTo(new Point(+0, 0), new Point(0, +0)));
			Assert.Equal(
				Empty.DistanceTo(new Point(1, 0)),
				Empty.DistanceTo(new Point(+1, 5), new Point(1, -5)));
			Assert.Equal(
				Empty.DistanceTo(new Point(5, 0)),
				Empty.DistanceTo(new Point(+5, 5), new Point(5, -5)));

			Point intersection;

			Assert.Equal(
				Empty.DistanceTo(new Point(0, 0)),
				Empty.DistanceTo(new Point(-5, 5), new Point(0, +0), out intersection));
			Assert.Equal(
				Empty.DistanceTo(new Point(0, 0)),
				Empty.DistanceTo(new Point(+0, 0), new Point(5, -5), out intersection));
			Assert.Equal(
				Empty.DistanceTo(new Point(0, 0)),
				Empty.DistanceTo(new Point(+0, 0), new Point(0, +0), out intersection));
			Assert.Equal(
				Empty.DistanceTo(new Point(1, 0)),
				Empty.DistanceTo(new Point(+1, 5), new Point(1, -5), out intersection));
			Assert.Equal(
				Empty.DistanceTo(new Point(5, 0)),
				Empty.DistanceTo(new Point(+5, 5), new Point(5, -5), out intersection));

			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(0, 0)),
				Empty.SquaredDistanceTo(new Point(-5, 5), new Point(0, +0)));
			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(0, 0)),
				Empty.SquaredDistanceTo(new Point(+0, 0), new Point(5, -5)));
			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(0, 0)),
				Empty.SquaredDistanceTo(new Point(+0, 0), new Point(0, +0)));
			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(1, 0)),
				Empty.SquaredDistanceTo(new Point(+1, 5), new Point(1, -5)));
			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(5, 0)),
				Empty.SquaredDistanceTo(new Point(+5, 5), new Point(5, -5)));

			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(0, 0)),
				Empty.SquaredDistanceTo(
					new Point(-5, 5), new Point(0, +0), out intersection));
			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(0, 0)),
				Empty.SquaredDistanceTo(
					new Point(+0, 0), new Point(5, -5), out intersection));
			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(0, 0)),
				Empty.SquaredDistanceTo(
					new Point(+0, 0), new Point(0, +0), out intersection));
			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(1, 0)),
				Empty.SquaredDistanceTo(
					new Point(+1, 5), new Point(1, -5), out intersection));
			Assert.Equal(
				Empty.SquaredDistanceTo(new Point(5, 0)),
				Empty.SquaredDistanceTo(
					new Point(+5, 5), new Point(5, -5), out intersection));

			Assert.Equal(new Point(5, 5), Empty.Translate(new Size(5)));

			Assert.Equal<Point>(new Point(5, 5), new Size(5));

			Matrix3X2 identity = Matrix3X2.Identity;

			Assert.Equal(Empty, Empty.Transform(ref identity));

			Assert.TestObject(MinValue, MaxValue);
		}
#endif
	}
}