//  Licensing Information
// =======================
// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using CContract = System.Diagnostics.Contracts.Contract;

namespace Frost
{
	//  The Problem
	// =============
	// We need a way to represent the expansion and compaction of a `Rectangle`,
	// which the concept of margins and other delta changes require. The primitive
	// may change by four methods:
	//
	// * The left side of a rectangle may either contract or expand by 
	//	 modifying the `X` component of the rectangle.
	// * The top side of a rectangle may either contract or expand by
	//	 modifying the `Y` component of the rectangle.
	// * The right side of a rectangle may either contract or expand by
	//	 modifying the `Width` component of the rectangle.
	// * The bottom side of a rectangle may either contract or expand by
	//	 modifying the `Height` component of the rectangle.
	// 
	// The resulting `Rectangle` from any of the above operations loses the
	// amount of change, whether contraction or expansion, applied to each side.
	// This loss of information necessitates the need for a type that stores the
	// changes applied to each side:

	#region <<struct declaration>>
	public struct Thickness : IEquatable<Thickness>
	#endregion
	{
		//  The Solution
		// ==============
		// 

		private static readonly Thickness _MinValue;
		private static readonly Thickness _MaxValue;

		private static readonly Thickness _Empty;

		# region <<declare instance members>>=
		private readonly float _Bottom;
		private readonly float _Left;
		private readonly float _Right;
		private readonly float _Top;
		# endregion

		static Thickness()
		{
			_MinValue = new Thickness(0.0f);
			_MaxValue = new Thickness(float.MaxValue);

			_Empty = new Thickness(0.0f);
		}

		[ContractInvariantMethod] private void Invariant()
		{
			CContract.Invariant(Check.IsPositive(_Left));
			CContract.Invariant(Check.IsPositive(_Top));
			CContract.Invariant(Check.IsPositive(_Right));
			CContract.Invariant(Check.IsPositive(_Bottom));
		}

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

		public Thickness(float leftRight, float topBottom)
			: this(leftRight, topBottom, leftRight, topBottom)
		{
			CContract.Requires(Check.IsPositive(leftRight));
			CContract.Requires(Check.IsPositive(topBottom));
		}

		public Thickness(float leftRightTopBottom)
			: this(leftRightTopBottom, leftRightTopBottom, leftRightTopBottom, leftRightTopBottom)
		{
			CContract.Requires(Check.IsPositive(leftRightTopBottom));
		}

		public float Bottom
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Bottom));

				return _Bottom;
			}
		}

		public float Right
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Right));

				return _Right;
			}
		}

		public float Top
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Top));

				return _Top;
			}
		}

		public float Left
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Left));

				return _Left;
			}
		}

		public static Thickness Empty
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_Empty));

				return _Empty;
			}
		}

		public static Thickness MaxValue
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		public static Thickness MinValue
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_MinValue));

				return _MinValue;
			}
		}

		public float Width
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));

				return _Left + _Right;
			}
		}

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
			return other._Bottom.Equals(_Bottom) && other._Left.Equals(_Left) && other._Right.Equals(_Right) &&
			       other._Top.Equals(_Top);
		}

		public Thickness Contract(float left, float top, float right, float bottom)
		{
			CContract.Requires(Check.IsFinite(left));
			CContract.Requires(Check.IsFinite(top));
			CContract.Requires(Check.IsFinite(right));
			CContract.Requires(Check.IsFinite(bottom));

			return new Thickness(_Left - left, _Top - top, _Right - right, _Bottom - bottom);
		}

		public Thickness Contract(float leftRight, float topBottom)
		{
			CContract.Requires(Check.IsFinite(leftRight));
			CContract.Requires(Check.IsFinite(topBottom));

			return Contract(leftRight, topBottom, leftRight, topBottom);
		}

		public Thickness Contract(float leftRightTopBottom)
		{
			CContract.Requires(Check.IsFinite(leftRightTopBottom));

			return Contract(leftRightTopBottom, leftRightTopBottom, leftRightTopBottom, leftRightTopBottom);
		}

		public Thickness Expand(float left, float top, float right, float bottom)
		{
			CContract.Requires(Check.IsFinite(left));
			CContract.Requires(Check.IsFinite(top));
			CContract.Requires(Check.IsFinite(right));
			CContract.Requires(Check.IsFinite(bottom));

			return new Thickness(_Left + left, _Top + top, _Right + right, _Bottom + bottom);
		}

		public Thickness Expand(float leftRight, float topBottom)
		{
			CContract.Requires(Check.IsFinite(leftRight));
			CContract.Requires(Check.IsFinite(topBottom));

			return Expand(leftRight, topBottom, leftRight, topBottom);
		}

		public Thickness Expand(float leftRightTopBottom)
		{
			CContract.Requires(Check.IsFinite(leftRightTopBottom));

			return Expand(leftRightTopBottom, leftRightTopBottom, leftRightTopBottom, leftRightTopBottom);
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

		public static bool operator ==(Thickness left, Thickness right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Thickness left, Thickness right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.Equal(0, new Thickness(0, 1, 2, 3).Left);
			Assert.Equal(1, new Thickness(0, 1, 2, 3).Top);
			Assert.Equal(2, new Thickness(0, 1, 2, 3).Right);
			Assert.Equal(3, new Thickness(0, 1, 2, 3).Bottom);

			Assert.Equal(0, new Thickness(0, 1).Left);
			Assert.Equal(1, new Thickness(0, 1).Top);
			Assert.Equal(0, new Thickness(0, 1).Right);
			Assert.Equal(1, new Thickness(0, 1).Bottom);

			Assert.Equal(1, new Thickness(1).Left);
			Assert.Equal(1, new Thickness(1).Top);
			Assert.Equal(1, new Thickness(1).Right);
			Assert.Equal(1, new Thickness(1).Bottom);

			Assert.Equal(8, new Thickness(3, 2, 5, 3).Width);
			Assert.Equal(5, new Thickness(3, 2, 5, 3).Height);

			Assert.Equal(Empty, new Thickness(5).Contract(5, 5, 5, 5));
			Assert.Equal(new Thickness(6), new Thickness(5).Contract(-1, -1, -1, -1));

			Assert.Equal(Empty, new Thickness(5).Contract(5, 5));
			Assert.Equal(new Thickness(6), new Thickness(5).Contract(-1, -1));

			Assert.Equal(Empty, new Thickness(5).Contract(5));
			Assert.Equal(new Thickness(6), new Thickness(5).Contract(-1));

			Assert.Equal(Empty, new Thickness(5).Expand(-5, -5, -5, -5));
			Assert.Equal(new Thickness(6), new Thickness(5).Expand(1, 1, 1, 1));

			Assert.Equal(Empty, new Thickness(5).Expand(-5, -5));
			Assert.Equal(new Thickness(6), new Thickness(5).Expand(1, 1));

			Assert.Equal(Empty, new Thickness(5).Expand(-5));
			Assert.Equal(new Thickness(6), new Thickness(5).Expand(1));

			Assert.TestObject(MinValue, MaxValue);
		}
#endif
	}
}