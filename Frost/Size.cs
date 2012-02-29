// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
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
			Contract.Invariant(Check.IsFinite(_Width));
			Contract.Invariant(Check.IsFinite(_Height));
		}

		public Size(float width, float height)
		{
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));

			_Width = width;
			_Height = height;

			Contract.Assert(Width.Equals(width));
			Contract.Assert(Height.Equals(height));
		}

		public Size(float widthHeight) : this(widthHeight, widthHeight)
		{
			Contract.Requires(Check.IsFinite(widthHeight));
		}

		public static implicit operator Size(Point location)
		{
			return new Size(location.X, location.Y);
		}

		public float Height
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_Height));

				return _Height;
			}
		}

		public float Width
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_Width));

				return _Width;
			}
		}

		public static Size Empty
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_Empty));

				return _Empty;
			}
		}

		public static Size MaxValue
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		public static Size MinValue
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_MinValue));

				return _MinValue;
			}
		}

		public float Area
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				return Math.Max(0.0f, _Width * _Height);
			}
		}

		public Size Scale(Size amount)
		{
			Contract.Requires(Check.IsPositive(amount.Width));
			Contract.Requires(Check.IsPositive(amount.Height));

			return new Size(_Width * amount.Width, _Height * amount.Height);
		}

		public Size Transform(ref Matrix3X2 transformation)
		{
			Point p1 = new Point(0.0f, 0.0f);
			Point p2 = new Point(_Width, 0.0f);
			Point p4 = new Point(0.0f, _Height);

			p1 = p1.Transform(ref transformation);
			p2 = p2.Transform(ref transformation);
			p4 = p4.Transform(ref transformation);

			return new Size(p1.DistanceTo(p2), p1.DistanceTo(p4));
		}

		public bool Equals(Size other)
		{
			return other._Width.Equals(_Width) && other._Height.Equals(_Height);
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
				return (_Width.GetHashCode() * 397) ^ _Height.GetHashCode();
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
			return string.Format("Width: {0}, Height: {1}", _Width, _Height);
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