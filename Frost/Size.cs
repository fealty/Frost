// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost
{
	public struct Size : IEquatable<Size>
	{
		private static readonly Size _MinValue;
		private static readonly Size _MaxValue;

		private static readonly Size _Empty;

		private readonly float _Width;
		private readonly float _Height;

		static Size()
		{
			_MinValue = new Size(float.MinValue);
			_MaxValue = new Size(float.MaxValue);

			_Empty = new Size(0.0f);
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(Check.IsFinite(this._Width));
			Contract.Invariant(Check.IsFinite(this._Height));
		}

		public Size(float width, float height)
		{
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));

			this._Width = width;
			this._Height = height;

			Contract.Assert(Width.Equals(width));
			Contract.Assert(Height.Equals(height));
		}

		public Size(float widthHeight) : this(widthHeight, widthHeight)
		{
		}

		public float Height
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._Height));

				return this._Height;
			}
		}

		public float Width
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._Width));

				return this._Width;
			}
		}

		public static Size Empty
		{
			get { return _Empty; }
		}

		public static Size MaxValue
		{
			get { return _MaxValue; }
		}

		public static Size MinValue
		{
			get { return _MinValue; }
		}

		public float Area
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				return Math.Max(0.0f, this._Width * this._Height);
			}
		}

		public Size Transform(ref Matrix3X2 transformation)
		{
			Point p1 = new Point(0.0f, 0.0f);
			Point p2 = new Point(this._Width, 0.0f);
			Point p4 = new Point(0.0f, this._Height);

			p1 = p1.Transform(ref transformation);
			p2 = p2.Transform(ref transformation);
			p4 = p4.Transform(ref transformation);

			return new Size(p1.DistanceTo(p2), p1.DistanceTo(p4));
		}

		public bool Equals(Size other)
		{
			return other._Width.Equals(this._Width) &&
			       other._Height.Equals(this._Height);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Size && Equals((Size)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (this._Width.GetHashCode() * 397) ^
				       this._Height.GetHashCode();
			}
		}

		public static bool operator ==(Size left, Size right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Size left, Size right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format(
				"Width: {0}, Height: {1}", this._Width, this._Height);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.Equal(0, new Size(0, 1).Width);
			Assert.Equal(1, new Size(0, 1).Height);

			Assert.Equal(0, Empty.Width);
			Assert.Equal(0, Empty.Height);

			Assert.Equal(6, new Size(3, 2).Area);
			Assert.Equal(25, new Size(5).Area);
			Assert.Equal(0, new Size(-5, 6).Area);

			Matrix3X2 scaling;
			Matrix3X2.Identity.Scale(2.0f, 2.0f, out scaling);

			Size scaled = new Size(5, 5).Transform(ref scaling);

			Assert.Equal(new Size(10, 10), scaled);

			Assert.TestObject(MinValue, MaxValue);
		}
#endif
	}
}