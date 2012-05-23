// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

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

		private readonly float _Height;
		private readonly float _Width;

		static Size()
		{
			_MinValue = new Size(float.MinValue);
			_MaxValue = new Size(float.MaxValue);

			_Empty = new Size(0.0f);
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

		public bool Equals(Size other)
		{
			return other._Width.Equals(_Width) && other._Height.Equals(_Height);
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

		public override string ToString()
		{
			return string.Format("Width: {0}, Height: {1}", _Width, _Height);
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsFinite(_Width));
			Contract.Invariant(Check.IsFinite(_Height));
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
	}
}