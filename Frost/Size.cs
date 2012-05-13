#region Licensing

// # Licensing Information
// 
// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

#endregion

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	/// <summary>
	///   represents a finite width-height pair
	/// </summary>
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

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsFinite(_Width));
			Contract.Invariant(Check.IsFinite(_Height));
		}

		/// <summary>
		///   constructs a <see cref="Size" /> from a width and height
		/// </summary>
		/// <param name="width"> the finite width </param>
		/// <param name="height"> the finite height </param>
		public Size(float width, float height)
		{
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));

			_Width = width;
			_Height = height;

			Contract.Assert(Width.Equals(width));
			Contract.Assert(Height.Equals(height));
		}

		/// <summary>
		///   constructs a <see cref="Size" /> from a width-height pair
		/// </summary>
		/// <param name="widthHeight"> the finite width and height </param>
		public Size(float widthHeight) : this(widthHeight, widthHeight)
		{
			Contract.Requires(Check.IsFinite(widthHeight));
		}

		/// <summary>
		///   implicitly converts a given <see cref="Point" /> to a <see cref="Size" />
		/// </summary>
		/// <param name="location"> the location to convert </param>
		/// <returns> the <see cref="Size" /> representation of <paramref name="location" /> </returns>
		public static implicit operator Size(Point location)
		{
			return new Size(location.X, location.Y);
		}

		/// <summary>
		///   produces a <see cref="Size" /> by adding a <see cref="Size" /> to another <see cref="Size" />
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> the result of <paramref name="right" /> added to <paramref name="left" /> </returns>
		public static Size operator +(Size left, Size right)
		{
			return new Size(left.Width + right.Width, left.Height + right.Height);
		}

		/// <summary>
		///   produces a <see cref="Size" /> by subtracting a <see cref="Size" /> from another <see cref="Size" />
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> the result of <paramref name="left" /> subtracted by <paramref name="right" /> </returns>
		public static Size operator -(Size left, Size right)
		{
			return new Size(left.Width - right.Width, left.Height - right.Height);
		}

		/// <summary>
		///   produces a <see cref="Size" /> by multiplying a <see cref="Size" /> by another <see cref="Size" />
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> the result of <paramref name="left" /> multiplied by <paramref name="right" /> </returns>
		public static Size operator *(Size left, Size right)
		{
			return new Size(left.Width * right.Width, left.Height * right.Height);
		}

		/// <summary>
		///   produces a <see cref="Size" /> by dividing a <see cref="Size" /> by another <see cref="Size" />
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> the result of <paramref name="left" /> divided by <paramref name="right" /> </returns>
		public static Size operator /(Size left, Size right)
		{
			return new Size(left.Width / right.Width, left.Height / right.Height);
		}

		/// <summary>
		///   gets the height
		/// </summary>
		public float Height
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_Height));

				return _Height;
			}
		}

		/// <summary>
		///   gets the width
		/// </summary>
		public float Width
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_Width));

				return _Width;
			}
		}

		/// <summary>
		///   gets the default value for <see cref="Size" />
		/// </summary>
		public static Size Empty
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_Empty));

				return _Empty;
			}
		}

		/// <summary>
		///   gets the maximum value a <see cref="Size" /> can represent
		/// </summary>
		public static Size MaxValue
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		/// <summary>
		///   gets the minimum value a <see cref="Size" /> can represent
		/// </summary>
		public static Size MinValue
		{
			get
			{
				Contract.Ensures(Contract.Result<Size>().Equals(_MinValue));

				return _MinValue;
			}
		}

		/// <summary>
		///   gets the non-negative area
		/// </summary>
		public float Area
		{
			get
			{
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				return Math.Max(0.0f, _Width * _Height);
			}
		}

		/// <summary>
		///   computes a new <see cref="Size" /> scaled by the given amount
		/// </summary>
		/// <param name="amount"> the normalized amount to scale by </param>
		/// <returns> the <see cref="Size" /> scaled by <paramref name="amount" /> </returns>
		public Size Scale(Size amount)
		{
			Contract.Requires(Check.IsPositive(amount.Width));
			Contract.Requires(Check.IsPositive(amount.Height));

			return new Size(_Width * amount.Width, _Height * amount.Height);
		}

		/// <summary>
		///   computes a new <see cref="Size" /> transformed by a given transformation matrix
		/// </summary>
		/// <param name="transformation"> the transformation matrix </param>
		/// <returns> the <see cref="Size" /> transformed by <paramref name="transformation" /> </returns>
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

		/// <summary>
		///   determines whether two instances of <see cref="Size" /> are equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> equals <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator ==(Size left, Size right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   determines whether two instances of <see cref="Size" /> are not equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> does not equal <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator !=(Size left, Size right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("Width: {0}, Height: {1}", _Width, _Height);
		}

#if(UNIT_TESTING)
		[Fact]
		internal static void Test0()
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

			Assert.Equal(new Size(50), new Size(10, 10) * new Size(5));
			Assert.Equal(new Size(15), new Size(10, 10) + new Size(5));
			Assert.Equal(new Size(05), new Size(10, 10) - new Size(5));
			Assert.Equal(new Size(02), new Size(10, 10) / new Size(5));

			Assert.Equal(new Size(4), new Size(2).Scale(new Size(2)));

			Assert.Equal<Size>(new Size(3), new Point(3));

			Assert.TestObject(MinValue, MaxValue);
		}
#endif
	}
}