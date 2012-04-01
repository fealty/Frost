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

	/// <summary>
	///   represents the expansion or contraction of one or more sides of a <see cref="Rectangle" />
	/// </summary>

	#region <<struct declaration>>
	public struct Thickness : IEquatable<Thickness>
	#endregion
	{
		//  The Solution
		// ==============
		// We need an immutable value type that represents each of the four
		// sides of a `Rectangle` that can change, so we need one read-only
		// variable for each side:

		# region <<declare instance members>>
		private readonly float _Bottom;
		private readonly float _Left;
		private readonly float _Right;
		private readonly float _Top;
		# endregion

		// We use floating point numbers to make using fractional portions
		// simple, but floating point numbers pose a few problems: 
		// 
		// * A `float` may indicate an infinite value.
		// * A `float` may indicate an invalid number (NaN).
		// * A `float` may indicate a negative number.
		// 
		// If you're using a display of infinite resolution, I apologize, but
		// for the rest of us mortals, infinity only begets infinity, which
		// we cannot reconcile into a finite representation; thus, infinite
		// values pose a problem.
		//
		// Neither can we reconcile an invalid number to a valid numerical
		// representation.
		//
		// As for negative values, how can we have something that is negative
		// five units thick? If something has less than zero thickness, it 
		// simply does not have presence, which is equivalent to zero thickness.
		// 
		// How can we stop the user from filling a `Thickness` with these bad
		// values? We use invariants to guarantee values are always positive
		// finite numbers:

		[ContractInvariantMethod] private void Invariant()
		{
			#region <<declare invariants>>
			CContract.Invariant(Check.IsPositive(_Left));
			CContract.Invariant(Check.IsPositive(_Top));
			CContract.Invariant(Check.IsPositive(_Right));
			CContract.Invariant(Check.IsPositive(_Bottom));
			#endregion
		}

		//  The Interface
		// ===============
		// Since `Thickness` is a value type, we want to expose information about
		// the type that a user may expect of value types. These include the
		// minimum value representable by the type, the maximum value representable
		// by the type, and the default or empty value.

		#region <<static type members>>
		private static readonly Thickness _MinValue;
		private static readonly Thickness _MaxValue;
		private static readonly Thickness _Empty;
		#endregion

		// The minimum representable value must equal of exceed zero to meet the
		// previously mentioned invariants. The maximum representable value may
		// not exceed the max finite storeable in a `float`. These limits are
		// conveniently exposed to the user:

		/// <summary>
		/// exposes the minimum value a <see cref="Thickness"/> can represent
		/// </summary>

		#region <<minimum representable>>
		public static Thickness MinValue
		#endregion
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_MinValue));

				return _MinValue;
			}
		}

		/// <summary>
		/// exposes the maximum value a <see cref="Thickness"/> can represent
		/// </summary>

		#region <<maximum representable>>
		public static Thickness MaxValue
		#endregion
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		/// <summary>
		/// exposes the default value of a <see cref="Thickness"/>
		/// </summary>

		#region <<empty value>>
		public static Thickness Empty
		#endregion
		{
			get
			{
				CContract.Ensures(CContract.Result<Thickness>().Equals(_Empty));

				return _Empty;
			}
		}

		// Why? The user may wish to start with an empty or max representable
		// value then perform an operation to change the value. We will address
		// these operations later. For now, let us examine how the user can
		// create a `Thickness` with custom values.

		/// <summary>
		/// constructs a <see cref="Thickness"/> from the thicknesses of four sides
		/// </summary>
		/// <param name="left">the thickness of the left side</param>
		/// <param name="top">the thickness of the top side</param>
		/// <param name="right">the thickness of the right side</param>
		/// <param name="bottom">the thickness of the bottom side</param>

		#region <<all sides constructor>>
		public Thickness(float left, float top, float right, float bottom)
		#endregion
		{
			#region <<constructor requires>>=
			CContract.Requires(Check.IsPositive(left));
			CContract.Requires(Check.IsPositive(top));
			CContract.Requires(Check.IsPositive(right));
			CContract.Requires(Check.IsPositive(bottom));
			#endregion

			_Left = left;
			_Top = top;
			_Right = right;
			_Bottom = bottom;

			CContract.Assert(Left.Equals(left));
			CContract.Assert(Top.Equals(top));
			CContract.Assert(Right.Equals(right));
			CContract.Assert(Bottom.Equals(bottom));
		}

		// The user may specify the value of each side. What happens if the user
		// gives an invalid value? In debug, the code asserts. In release, the
		// code behaves in an undefined manner. Why? Our friendly contract 
		// invariants do not protect us in release builds. To solve this problem,
		// we restrict the signature of the constructor with further contracts:
		//
		// <<constructor requires>>
		//
		// `Contract.Requires()` will verify parameters in release builds. Why
		// don't we give default values to the parameters?

		/// <summary>
		/// constructs a <see cref="Thickness"/> from two dual sides
		/// </summary>
		/// <param name="leftRight">the thickness of the left and right sides</param>
		/// <param name="topBottom">the thickness of the top and bottom sides</param>

		#region <<left-right constructor>>
		public Thickness(float leftRight, float topBottom)
		#endregion
			: this(leftRight, topBottom, leftRight, topBottom)
		{
			CContract.Requires(Check.IsPositive(leftRight));
			CContract.Requires(Check.IsPositive(topBottom));
		}

		// We believe users will find the ability to easily assign the same
		// value to multiple sides more useful than optional parameters. This
 		// idea culminates in the final constructor:

		/// <summary>
		/// constructs a <see cref="Thickness"/> from a thickness value
		/// </summary>
		/// <param name="leftRightTopBottom">the thickness of the left, top, right, and bottom sides</param>

		#region <<left-right-top-bottom constructor>>
		public Thickness(float leftRightTopBottom)
		#endregion
			: this(leftRightTopBottom, leftRightTopBottom, leftRightTopBottom, leftRightTopBottom)
		{
			CContract.Requires(Check.IsPositive(leftRightTopBottom));
		}

		// Users may access the individual thicknesses of the sides through the 
		// following properties:

		/// <summary>
		/// gets the thickness of the left side
		/// </summary>

		#region <<left property>>
		public float Left
		#endregion
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Left));

				return _Left;
			}
		}

		/// <summary>
		/// gets the thickness of the top side
		/// </summary>
		
		#region <<top property>>
		public float Top
		#endregion
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Top));

				return _Top;
			}
		}

		/// <summary>
		/// gets the thickness of the right side
		/// </summary>

		#region <<right property>>
		public float Right
		#endregion
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Right));

				return _Right;
			}
		}
	
		/// <summary>
		/// gets the thickness of the bottom side
		/// </summary>

		#region <<bottom property>>
		public float Bottom
		#endregion
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));
				CContract.Ensures(CContract.Result<float>().Equals(_Bottom));

				return _Bottom;
			}
		}

		// Imagine we have zero-sized rectangle. What happens if we attempt to
		// expand it by five units on all sides? `Thicknesses` have a width and
 		// height from their spatial nature. If `Left = 5 units` and `Right = 5
		// units`, we can say the `Width` of the `Thickness` is ten. The same
		// applies to the `Top` and `Bottom` sides, which constitute `Height`.
		// Users may access these values via the following properties:

		/// <summary>
		/// gets the width (<see cref="Left"/> + <see cref="Right"/>) of the <see cref="Thickness"/>
		/// </summary>

		#region <<width property>>
		public float Width
		#endregion
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));

				return _Left + _Right;
			}
		}

		/// <summary>
		/// gets the height (<see cref="Top"/> + <see cref="Bottom"/>) of the <see cref="Thickness"/>
		/// </summary>

		#region <<height property>>
		public float Height
		#endregion
		{
			get
			{
				CContract.Ensures(Check.IsPositive(CContract.Result<float>()));

				return _Top + _Bottom;
			}
		}
		
		// Now we get to the meat -- the operations we can perform. Users may
		// perform two operations on a `Thickness`: 

		/// <summary>
		/// produces a <see cref="Thickness"/> by contracting an existing <see cref="Thickness"/> on all sides
		/// </summary>
		/// <param name="left">the amount to contract the left side</param>
		/// <param name="top">the amount to contract the top side</param>
		/// <param name="right">the amount to contract the right side</param>
		/// <param name="bottom">the amount to contract the bottom side</param>
		/// <returns>the <see cref="Thickness"/> contracted by the specified amounts</returns>

		#region <<contract all sides>>
		public Thickness Contract(float left, float top, float right, float bottom)
		#endregion
		{
			#region <<contract-expand contracts>>=
			CContract.Requires(Check.IsFinite(left));
			CContract.Requires(Check.IsFinite(top));
			CContract.Requires(Check.IsFinite(right));
			CContract.Requires(Check.IsFinite(bottom));
			#endregion

			return new Thickness(_Left - left, _Top - top, _Right - right, _Bottom - bottom);
		}

		/// <summary>
		/// produces a <see cref="Thickness"/> by contracting an existing <see cref="Thickness"/> on two dual sides
		/// </summary>
		/// <param name="leftRight">the amount to contract the left and right sides</param>
		/// <param name="topBottom">the amount to contract the top and bottom sides</param>
		/// <returns>the <see cref="Thickness"/> contracted by the specified amounts</returns>

		#region <<contract left-right, top-bottom>>
		public Thickness Contract(float leftRight, float topBottom)
		#endregion
		{
			CContract.Requires(Check.IsFinite(leftRight));
			CContract.Requires(Check.IsFinite(topBottom));

			return Contract(leftRight, topBottom, leftRight, topBottom);
		}

		/// <summary>
		/// produces a <see cref="Thickness"/> by contracting an existing <see cref="Thickness"/> on all sides
		/// </summary>
		/// <param name="leftRightTopBottom">the amount to contract the left, top, right, and bottom sides</param>
		/// <returns>the <see cref="Thickness"/> contracted by the specified amount</returns>

		#region <<contract left-right-top-bottom>>
		public Thickness Contract(float leftRightTopBottom)
		#endregion
		{
			CContract.Requires(Check.IsFinite(leftRightTopBottom));

			return Contract(leftRightTopBottom, leftRightTopBottom, leftRightTopBottom, leftRightTopBottom);
		}

		/// <summary>
		/// produces a <see cref="Thickness"/> by contracting an existing <see cref="Thickness"/> by a <see cref="Thickness"/>
		/// </summary>
		/// <param name="left">the <see cref="Thickness"/> to contract</param>
		/// <param name="right">the <see cref="Thickness"/> to contract by</param>
		/// <returns>the result of <paramref name="left"/> contracted by <see cref="right"/></returns>

		#region <<contraction operator>>
		public static Thickness operator -(Thickness left, Thickness right)
		#endregion
		{
			return left.Contract(right.Left, right.Top, right.Right, right.Bottom);
		}

		// The `Contract` operation produces a `Thickness` by subtracting the
		// values of the sides of an existing `Thickness` by either specified
		// amounts or an existing `Thickness`.

		/// <summary>
		/// produces a <see cref="Thickness"/> by expanding an existing <see cref="Thickness"/> on all sides
		/// </summary>
		/// <param name="left">the amount to expand the left side</param>
		/// <param name="top">the amount to expand the top side</param>
		/// <param name="right">the amount to expand the right side</param>
		/// <param name="bottom">the amount to expand the bottom side</param>
		/// <returns>the <see cref="Thickness"/> expanded by the specified amounts</returns>

		#region <<expand all sides>>
		public Thickness Expand(float left, float top, float right, float bottom)
		#endregion
		{
			CContract.Requires(Check.IsFinite(left));
			CContract.Requires(Check.IsFinite(top));
			CContract.Requires(Check.IsFinite(right));
			CContract.Requires(Check.IsFinite(bottom));

			return new Thickness(_Left + left, _Top + top, _Right + right, _Bottom + bottom);
		}

		/// <summary>
		/// produces a <see cref="Thickness"/> by expanding an existing <see cref="Thickness"/> on two dual sides
		/// </summary>
		/// <param name="leftRight">the amount to expand the left and right sides</param>
		/// <param name="topBottom">the amount to expand the top and bottom sides</param>
		/// <returns>the <see cref="Thickness"/> expanded by the specified amounts</returns>

		#region <<expand left-right, top-bottom>>
		public Thickness Expand(float leftRight, float topBottom)
		#endregion
		{
			CContract.Requires(Check.IsFinite(leftRight));
			CContract.Requires(Check.IsFinite(topBottom));

			return Expand(leftRight, topBottom, leftRight, topBottom);
		}

		/// <summary>
		/// produces a <see cref="Thickness"/> by expanding an existing <see cref="Thickness"/> on all sides
		/// </summary>
		/// <param name="leftRightTopBottom">the amount to expand the left, top, right, and bottom sides</param>
		/// <returns>the <see cref="Thickness"/> expanded by the specified amount</returns>

		#region <<expand left-top-right-bottom>>
		public Thickness Expand(float leftRightTopBottom)
		#endregion
		{
			CContract.Requires(Check.IsFinite(leftRightTopBottom));

			return Expand(leftRightTopBottom, leftRightTopBottom, leftRightTopBottom, leftRightTopBottom);
		}

		/// <summary>
		/// produces a <see cref="Thickness"/> by expanding an existing <see cref="Thickness"/> by a <see cref="Thickness"/>
		/// </summary>
		/// <param name="left">the <see cref="Thickness"/> to expand</param>
		/// <param name="right">the <see cref="Thickness"/> to expand by</param>
		/// <returns>the result of <paramref name="left"/> expanded by <see cref="right"/></returns>

		#region <<expansion operator>>
		public static Thickness operator +(Thickness left, Thickness right)
		#endregion
		{
			return left.Expand(right.Left, right.Top, right.Right, right.Bottom);
		}

		// The `Expand` operation produces a `Thickness` by adding the values
		// of the sides of an existing `Thickness` by either specified amounts
		// or an existing `Thickness`.
		//
		// Unlike the constructors, both the `Contract` and `Expand` operations
  		// allow negative values:
		//
		// <<contract-expand contracts>>
		//
		// We permit negative values because adding or subtracting negative values
		// will not always produce an invalid `Thickness`. For example, if we
		// have a `Thickness(0)` and `Contract(-1)`, we end up with a `Thickness(1)`.
		//
		//  Misc.
		// =======
		// Users may determine the equality and inequality of two `Thicknesses`
		// by using the following operators:

		/// <summary>
		/// determines whether two <see cref="Thickness"/>es are equal
		/// </summary>
		/// <param name="left">the left operand</param>
		/// <param name="right">the right operand</param>
		/// <returns><c>true</c> if <paramref name="left"/> equals <paramref name="right"/>; otherwise, <c>false</c></returns>

		#region <<equals operator>>
		public static bool operator ==(Thickness left, Thickness right)
		#endregion
		{
			return left.Equals(right);
		}

		/// <summary>
		/// determines whether two <see cref="Thickness"/>es are not equal
		/// </summary>
		/// <param name="left">the left operand</param>
		/// <param name="right">the right operand</param>
		/// <returns><c>true</c> if <paramref name="left"/> does not equal <paramref name="right"/>; otherwise, <c>false</c></returns>

		#region <<notequal operator>>
		public static bool operator !=(Thickness left, Thickness right)
		#endregion
		{
			return !left.Equals(right);
		}

		// `Thickness` also implements `IEquatable<T>`.
		
		/// <inheritdoc/>
		
		#region <<equals iequatable>>
		public bool Equals(Thickness other)
		#endregion
		{
			return other._Bottom.Equals(_Bottom) &&
			       other._Left.Equals(_Left) &&
			       other._Right.Equals(_Right) &&
			       other._Top.Equals(_Top);
		}

		// `Thickness` also overrides `object` members.

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

		static Thickness()
		{
			_MinValue = new Thickness(0.0f);
			_MaxValue = new Thickness(float.MaxValue);

			_Empty = new Thickness(0.0f);
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