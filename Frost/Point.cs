// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost
{
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

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(Check.IsFinite(this._X));
			Contract.Invariant(Check.IsFinite(this._Y));
		}

		public Point(float x, float y)
		{
			Contract.Requires(Check.IsFinite(x));
			Contract.Requires(Check.IsFinite(y));

			this._X = x;
			this._Y = y;

			Contract.Assert(X.Equals(x));
			Contract.Assert(Y.Equals(y));
		}

		public Point(float xy) : this(xy, xy)
		{
		}

		public float Y
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._Y));

				return this._Y;
			}
		}

		public float X
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._X));

				return this._X;
			}
		}

		public static Point Empty
		{
			get { return _Empty; }
		}

		public static Point MinValue
		{
			get { return _MinValue; }
		}

		public static Point MaxValue
		{
			get { return _MaxValue; }
		}

		public bool Equals(Point other)
		{
			return other._X.Equals(this._X) && other._Y.Equals(this._Y);
		}

		public float DistanceTo(Point point)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			double dx = point.X - this._X;
			double dy = point.Y - this._Y;

			return Convert.ToSingle(Math.Sqrt((dx * dx) + (dy * dy)));
		}

		public float SquaredDistanceTo(Point point)
		{
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			double dx = point.X - this._X;
			double dy = point.Y - this._Y;

			return Convert.ToSingle((dx * dx) + (dy * dy));
		}

		public Point FindIntersectionWith(Point lineStart, Point lineEnd)
		{
			Point v = new Point(
				lineEnd.X - lineStart.X, lineEnd.Y - lineStart.Y);
			Point w = new Point(this._X - lineStart.X, this._Y - lineStart.Y);

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

		public Point Transform(ref Matrix3X2 transformation)
		{
			return
				new Point(
					(this._X * transformation.M11) + (this._Y * transformation.M21) +
					transformation.M31,
					(this._X * transformation.M12) + (this._Y * transformation.M22) +
					transformation.M32);
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
				return (this._X.GetHashCode() * 397) ^ this._Y.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format("X: {0}, Y: {1}", this._X, this._Y);
		}

		public static bool operator ==(Point left, Point right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Point left, Point right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.Equal(0, new Point(0, 1).X);
			Assert.Equal(1, new Point(0, 1).Y);

			Assert.Equal(0, Empty.X);
			Assert.Equal(0, Empty.Y);

			Assert.Equal(
				1.41421, Math.Round(Empty.DistanceTo(new Point(1, 1)), 5));
			Assert.Equal(
				0.00000, Math.Round(Empty.DistanceTo(new Point(0, 0)), 5));

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

			Matrix3X2 identity = Matrix3X2.Identity;

			Assert.Equal(Empty, Empty.Transform(ref identity));

			Assert.TestObject(MinValue, MaxValue);
		}
#endif
	}
}