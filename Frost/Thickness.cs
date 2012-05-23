// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using CContract = System.Diagnostics.Contracts.Contract;

namespace Frost
{
	/// <summary>
	///   represents the expansion or contraction of one or more sides of a <see cref="Rectangle" />
	/// </summary>
	public struct Thickness : IEquatable<Thickness>
	{
		private static readonly Thickness _MinValue;
		private static readonly Thickness _MaxValue;
		private static readonly Thickness _Empty;
		private readonly float _Bottom;
		private readonly float _Left;
		private readonly float _Right;
		private readonly float _Top;

		static Thickness()
		{
			_MinValue = new Thickness(0.0f);
			_MaxValue = new Thickness(float.MaxValue);

			_Empty = new Thickness(0.0f);
		}

		/// <summary>
		///   constructs a <see cref="Thickness" /> from the thicknesses of four sides
		/// </summary>
		/// <param name="left"> the thickness of the left side </param>
		/// <param name="top"> the thickness of the top side </param>
		/// <param name="right"> the thickness of the right side </param>
		/// <param name="bottom"> the thickness of the bottom side </param>
		public Thickness(float left, float top, float right, float bottom)
		{
			CContract.Requires(Check.IsPositive(left));
			CContract.Requires(Check.IsPositive(top));
			CContract.Requires(Check.IsPositive(right));
			CContract.Requires(Check.IsPositive(bottom));

			_Left = left;
			_Top = top;
			_Right = right;
			_Bottom = bottom;

			CContract.Assert(Left.Equals(left));
			CContract.Assert(Top.Equals(top));
			CContract.Assert(Right.Equals(right));
			CContract.Assert(Bottom.Equals(bottom));
		}

		/// <summary>
		///   constructs a <see cref="Thickness" /> from two dual sides
		/// </summary>
		/// <param name="leftRight"> the thickness of the left and right sides </param>
		/// <param name="topBottom"> the thickness of the top and bottom sides </param>
		public Thickness(float leftRight, float topBottom)
			: this(leftRight, topBottom, leftRight, topBottom)
		{
			CContract.Requires(Check.IsPositive(leftRight));
			CContract.Requires(Check.IsPositive(topBottom));
		}

		/// <summary>
		///   constructs a <see cref="Thickness" /> from a thickness value
		/// </summary>
		/// <param name="leftRightTopBottom"> the thickness of the left, top, right, and bottom sides </param>
		public Thickness(float leftRightTopBottom)
			: this(
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom)
		{
			CContract.Requires(Check.IsPositive(leftRightTopBottom));
		}

		/// <summary>
		///   gets the minimum value a <see cref="Thickness" /> can represent
		/// </summary>
		public static Thickness MinValue
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_MinValue));

				return _MinValue;
			}
		}

		/// <summary>
		///   gets the maximum value a <see cref="Thickness" /> can represent
		/// </summary>
		public static Thickness MaxValue
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		/// <summary>
		///   gets the default value of a <see cref="Thickness" />
		/// </summary>
		public static Thickness Empty
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_Empty));

				return _Empty;
			}
		}

		/// <summary>
		///   gets the thickness of the left side
		/// </summary>
		public float Left
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Left));

				return _Left;
			}
		}

		/// <summary>
		///   gets the thickness of the top side
		/// </summary>
		public float Top
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Top));

				return _Top;
			}
		}

		/// <summary>
		///   gets the thickness of the right side
		/// </summary>
		public float Right
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Right));

				return _Right;
			}
		}

		/// <summary>
		///   gets the thickness of the bottom side
		/// </summary>
		public float Bottom
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Bottom));

				return _Bottom;
			}
		}

		/// <summary>
		///   gets the width ( <see cref="Left" /> + <see cref="Right" /> ) of the <see cref="Thickness" />
		/// </summary>
		public float Width
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));

				return _Left + _Right;
			}
		}

		/// <summary>
		///   gets the height ( <see cref="Top" /> + <see cref="Bottom" /> ) of the <see cref="Thickness" />
		/// </summary>
		public float Height
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));

				return _Top + _Bottom;
			}
		}

		public bool Equals(Thickness other)
		{
			return other._Bottom.Equals(_Bottom) &&
				other._Left.Equals(_Left) &&
					other._Right.Equals(_Right) &&
						other._Top.Equals(_Top);
		}

		/// <summary>
		///   produces a <see cref="Thickness" /> by contracting an existing <see cref="Thickness" /> on all sides
		/// </summary>
		/// <param name="left"> the amount to contract the left side </param>
		/// <param name="top"> the amount to contract the top side </param>
		/// <param name="right"> the amount to contract the right side </param>
		/// <param name="bottom"> the amount to contract the bottom side </param>
		/// <returns> the <see cref="Thickness" /> contracted by the specified amounts </returns>
		public Thickness Contract(float left, float top, float right, float bottom)
		{
			CContract.Requires(Check.IsFinite(left));
			CContract.Requires(Check.IsFinite(top));
			CContract.Requires(Check.IsFinite(right));
			CContract.Requires(Check.IsFinite(bottom));

			return new Thickness(
				_Left - left, _Top - top, _Right - right, _Bottom - bottom);
		}

		/// <summary>
		///   produces a <see cref="Thickness" /> by contracting an existing <see cref="Thickness" /> on two dual sides
		/// </summary>
		/// <param name="leftRight"> the amount to contract the left and right sides </param>
		/// <param name="topBottom"> the amount to contract the top and bottom sides </param>
		/// <returns> the <see cref="Thickness" /> contracted by the specified amounts </returns>
		public Thickness Contract(float leftRight, float topBottom)
		{
			CContract.Requires(Check.IsFinite(leftRight));
			CContract.Requires(Check.IsFinite(topBottom));

			return Contract(leftRight, topBottom, leftRight, topBottom);
		}

		/// <summary>
		///   produces a <see cref="Thickness" /> by contracting an existing <see cref="Thickness" /> on all sides
		/// </summary>
		/// <param name="leftRightTopBottom"> the amount to contract the left, top, right, and bottom sides </param>
		/// <returns> the <see cref="Thickness" /> contracted by the specified amount </returns>
		public Thickness Contract(float leftRightTopBottom)
		{
			CContract.Requires(Check.IsFinite(leftRightTopBottom));

			return Contract(
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom);
		}

		/// <summary>
		///   produces a <see cref="Thickness" /> by expanding an existing <see cref="Thickness" /> on all sides
		/// </summary>
		/// <param name="left"> the amount to expand the left side </param>
		/// <param name="top"> the amount to expand the top side </param>
		/// <param name="right"> the amount to expand the right side </param>
		/// <param name="bottom"> the amount to expand the bottom side </param>
		/// <returns> the <see cref="Thickness" /> expanded by the specified amounts </returns>
		public Thickness Expand(float left, float top, float right, float bottom)
		{
			CContract.Requires(Check.IsFinite(left));
			CContract.Requires(Check.IsFinite(top));
			CContract.Requires(Check.IsFinite(right));
			CContract.Requires(Check.IsFinite(bottom));

			return new Thickness(
				_Left + left, _Top + top, _Right + right, _Bottom + bottom);
		}

		/// <summary>
		///   produces a <see cref="Thickness" /> by expanding an existing <see cref="Thickness" /> on two dual sides
		/// </summary>
		/// <param name="leftRight"> the amount to expand the left and right sides </param>
		/// <param name="topBottom"> the amount to expand the top and bottom sides </param>
		/// <returns> the <see cref="Thickness" /> expanded by the specified amounts </returns>
		public Thickness Expand(float leftRight, float topBottom)
		{
			CContract.Requires(Check.IsFinite(leftRight));
			CContract.Requires(Check.IsFinite(topBottom));

			return Expand(leftRight, topBottom, leftRight, topBottom);
		}

		/// <summary>
		///   produces a <see cref="Thickness" /> by expanding an existing <see cref="Thickness" /> on all sides
		/// </summary>
		/// <param name="leftRightTopBottom"> the amount to expand the left, top, right, and bottom sides </param>
		/// <returns> the <see cref="Thickness" /> expanded by the specified amount </returns>
		public Thickness Expand(float leftRightTopBottom)
		{
			CContract.Requires(Check.IsFinite(leftRightTopBottom));

			return Expand(
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Thickness && Equals((Thickness)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _Bottom.GetHashCode();

				result = (result * 397) ^ _Left.GetHashCode();
				result = (result * 397) ^ _Right.GetHashCode();
				result = (result * 397) ^ _Top.GetHashCode();

				return result;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Left: {0}, Top: {1}, Right: {2}, Bottom: {3}",
				_Left,
				_Top,
				_Right,
				_Bottom);
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			CContract.Invariant(Check.IsPositive(_Left));
			CContract.Invariant(Check.IsPositive(_Top));
			CContract.Invariant(Check.IsPositive(_Right));
			CContract.Invariant(Check.IsPositive(_Bottom));
		}

		/// <summary>
		///   produces a <see cref="Thickness" /> by contracting an existing <see cref="Thickness" /> by a <see cref="Thickness" />
		/// </summary>
		/// <param name="left"> the <see cref="Thickness" /> to contract </param>
		/// <param name="right"> the <see cref="Thickness" /> to contract by </param>
		/// <returns> the result of <paramref name="left" /> contracted by <see cref="right" /> </returns>
		public static Thickness operator -(Thickness left, Thickness right)
		{
			return left.Contract(right.Left, right.Top, right.Right, right.Bottom);
		}

		/// <summary>
		///   produces a <see cref="Thickness" /> by expanding an existing <see cref="Thickness" /> by a <see cref="Thickness" />
		/// </summary>
		/// <param name="left"> the <see cref="Thickness" /> to expand </param>
		/// <param name="right"> the <see cref="Thickness" /> to expand by </param>
		/// <returns> the result of <paramref name="left" /> expanded by <see cref="right" /> </returns>
		public static Thickness operator +(Thickness left, Thickness right)
		{
			return left.Expand(right.Left, right.Top, right.Right, right.Bottom);
		}

		/// <summary>
		///   determines whether two instances of <see cref="Thickness" /> are equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> equals <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator ==(Thickness left, Thickness right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   determines whether two instances of <see cref="Thickness" /> are not equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> does not equal <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator !=(Thickness left, Thickness right)
		{
			return !left.Equals(right);
		}
	}
}