// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Contracts = System.Diagnostics.Contracts.Contract;

namespace Frost
{
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

		[System.Diagnostics.Contracts.ContractInvariantMethod] private void
			Invariant()
		{
			Contracts.Invariant(Check.IsPositive(this._Left));
			Contracts.Invariant(Check.IsPositive(this._Top));
			Contracts.Invariant(Check.IsPositive(this._Right));
			Contracts.Invariant(Check.IsPositive(this._Bottom));
		}

		public Thickness(float left, float top, float right, float bottom)
		{
			Contracts.Requires(Check.IsPositive(left));
			Contracts.Requires(Check.IsPositive(top));
			Contracts.Requires(Check.IsPositive(right));
			Contracts.Requires(Check.IsPositive(bottom));

			this._Left = left;
			this._Top = top;
			this._Right = right;
			this._Bottom = bottom;

			Contracts.Assert(Left.Equals(left));
			Contracts.Assert(Top.Equals(top));
			Contracts.Assert(Right.Equals(right));
			Contracts.Assert(Bottom.Equals(bottom));
		}

		public Thickness(float leftRight, float topBottom)
			: this(leftRight, topBottom, leftRight, topBottom)
		{
			Contracts.Requires(Check.IsPositive(leftRight));
			Contracts.Requires(Check.IsPositive(topBottom));
		}

		public Thickness(float leftRightTopBottom)
			: this(
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom)
		{
			Contracts.Requires(Check.IsPositive(leftRightTopBottom));
		}

		public float Bottom
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(this._Bottom));

				return this._Bottom;
			}
		}

		public float Right
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(this._Right));

				return this._Right;
			}
		}

		public float Top
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(this._Top));

				return this._Top;
			}
		}

		public float Left
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(this._Left));

				return this._Left;
			}
		}

		public static Thickness Empty
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Thickness>().Equals(_Empty));

				return _Empty;
			}
		}

		public static Thickness MaxValue
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Thickness>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		public static Thickness MinValue
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Thickness>().Equals(_MinValue));

				return _MinValue;
			}
		}

		public float Width
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));

				return this._Left + this._Right;
			}
		}

		public float Height
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));

				return this._Top + this._Bottom;
			}
		}

		public bool Equals(Thickness other)
		{
			return other._Bottom.Equals(this._Bottom) &&
			       other._Left.Equals(this._Left) &&
			       other._Right.Equals(this._Right) &&
			       other._Top.Equals(this._Top);
		}

		public Thickness Contract(
			float left, float top, float right, float bottom)
		{
			Contracts.Requires(Check.IsFinite(left));
			Contracts.Requires(Check.IsFinite(top));
			Contracts.Requires(Check.IsFinite(right));
			Contracts.Requires(Check.IsFinite(bottom));

			return new Thickness(
				this._Left - left,
				this._Top - top,
				this._Right - right,
				this._Bottom - bottom);
		}

		public Thickness Contract(float leftRight, float topBottom)
		{
			Contracts.Requires(Check.IsFinite(leftRight));
			Contracts.Requires(Check.IsFinite(topBottom));

			return Contract(leftRight, topBottom, leftRight, topBottom);
		}

		public Thickness Contract(float leftRightTopBottom)
		{
			Contracts.Requires(Check.IsFinite(leftRightTopBottom));

			return Contract(
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom,
				leftRightTopBottom);
		}

		public Thickness Expand(
			float left, float top, float right, float bottom)
		{
			Contracts.Requires(Check.IsFinite(left));
			Contracts.Requires(Check.IsFinite(top));
			Contracts.Requires(Check.IsFinite(right));
			Contracts.Requires(Check.IsFinite(bottom));

			return new Thickness(
				this._Left + left,
				this._Top + top,
				this._Right + right,
				this._Bottom + bottom);
		}

		public Thickness Expand(float leftRight, float topBottom)
		{
			Contracts.Requires(Check.IsFinite(leftRight));
			Contracts.Requires(Check.IsFinite(topBottom));

			return Expand(leftRight, topBottom, leftRight, topBottom);
		}

		public Thickness Expand(float leftRightTopBottom)
		{
			Contracts.Requires(Check.IsFinite(leftRightTopBottom));

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
				int result = this._Bottom.GetHashCode();
				result = (result * 397) ^ this._Left.GetHashCode();
				result = (result * 397) ^ this._Right.GetHashCode();
				result = (result * 397) ^ this._Top.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Left: {0}, Top: {1}, Right: {2}, Bottom: {3}",
				this._Left,
				this._Top,
				this._Right,
				this._Bottom);
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
			Assert.Equal(
				new Thickness(6), new Thickness(5).Contract(-1, -1, -1, -1));

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