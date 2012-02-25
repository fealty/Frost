// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Contracts = System.Diagnostics.Contracts.Contract;

namespace Frost
{
	public struct Rectangle : IEquatable<Rectangle>
	{
		private static readonly Rectangle _MinValue;
		private static readonly Rectangle _MaxValue;

		private static readonly Rectangle _Empty;

		private readonly float _Height;
		private readonly float _Width;

		private readonly float _X;
		private readonly float _Y;

		static Rectangle()
		{
			_MinValue = new Rectangle(Point.Empty, Size.Empty);
			_MaxValue = new Rectangle(Point.Empty, Size.MaxValue);

			_Empty = new Rectangle(Point.Empty, Size.Empty);
		}

		[System.Diagnostics.Contracts.ContractInvariantMethod] private void
			Invariant()
		{
			Contracts.Invariant(Check.IsFinite(_X));
			Contracts.Invariant(Check.IsFinite(_Y));
			Contracts.Invariant(Check.IsPositive(_Width));
			Contracts.Invariant(Check.IsPositive(_Height));
		}

		public Rectangle(float left, float top, float right, float bottom)
		{
			Contracts.Requires(Check.IsFinite(left));
			Contracts.Requires(Check.IsFinite(top));
			Contracts.Requires(Check.IsFinite(right));
			Contracts.Requires(Check.IsFinite(bottom));
			Contracts.Requires(Check.IsPositive(right - left));
			Contracts.Requires(Check.IsPositive(bottom - top));

			_X = left;
			_Y = top;
			_Width = right - left;
			_Height = bottom - top;

			Contracts.Assert(X.Equals(left));
			Contracts.Assert(Y.Equals(top));
			Contracts.Assert(Width.Equals(_Width));
			Contracts.Assert(Height.Equals(_Height));
		}

		public Rectangle(Point location, Size size)
			: this(
				location.X,
				location.Y,
				location.X + size.Width,
				location.Y + size.Height)
		{
			Contracts.Requires(Check.IsPositive(size.Width));
			Contracts.Requires(Check.IsPositive(size.Height));
		}

		public float Height
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_Height));

				return _Height;
			}
		}

		public float Width
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_Width));

				return _Width;
			}
		}

		public float Y
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_Y));

				return _Y;
			}
		}

		public float X
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_X));

				return _X;
			}
		}

		public static Rectangle Empty
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Rectangle>().Equals(_Empty));

				return _Empty;
			}
		}

		public static Rectangle MaxValue
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Rectangle>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		public static Rectangle MinValue
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Rectangle>().Equals(_MinValue));

				return _MinValue;
			}
		}

		public Point Location
		{
			get
			{
				Contracts.Ensures(
					Contracts.Result<Point>().Equals(new Point(_X, _Y)));

				return new Point(_X, _Y);
			}
		}

		public Size Size
		{
			get
			{
				Contracts.Ensures(
					Contracts.Result<Size>().Equals(
						new Size(_Width, _Height)));

				return new Size(_Width, _Height);
			}
		}

		public float Left
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_X));

				return _X;
			}
		}

		public float Top
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_Y));

				return _Y;
			}
		}

		public float Right
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));

				return _X + _Width;
			}
		}

		public float Bottom
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));

				return _Y + _Height;
			}
		}

		public float Area
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));

				return _Width * _Height;
			}
		}

		public Point Center
		{
			get
			{
				return new Point(
					_X + (_Width / 2.0f), _Y + (_Height / 2.0f));
			}
		}

		public Rectangle Contract(Thickness amount)
		{
			return new Rectangle(
				Left + amount.Left,
				Top + amount.Top,
				Right - amount.Right,
				Bottom - amount.Bottom);
		}

		public Rectangle Expand(Thickness amount)
		{
			return new Rectangle(
				Left - amount.Left,
				Top - amount.Top,
				Right + amount.Right,
				Bottom + amount.Bottom);
		}

		public Rectangle AlignWithin(
			Rectangle container,
			Alignment alignment,
			Axis alignmentAxis,
			LayoutDirection direction = LayoutDirection.LeftToRight)
		{
			float x = _X;
			float y = _Y;
			float width = _Width;
			float height = _Height;

			if((alignmentAxis == Axis.Both) ||
			   (alignmentAxis == Axis.Horizontal))
			{
				if(direction == LayoutDirection.RightToLeft)
				{
					switch(alignment)
					{
						case Alignment.Stretch:
							width = container.Width;
							break;
						case Alignment.Trailing:
							x = (container.Width - _Width) + _X;
							break;
						case Alignment.Center:
							x = ((container.Width / 2.0f) - (_Width / 2.0f)) + _X;
							break;
						case Alignment.Leading:
							break;
					}
				}
				else
				{
					switch(alignment)
					{
						case Alignment.Stretch:
							width = container.Width;
							break;
						case Alignment.Center:
							x = ((container.Width / 2.0f) - (_Width / 2.0f)) + _X;
							break;
						case Alignment.Leading:
							x = (container.Width - _Width) + _X;
							break;
						case Alignment.Trailing:
							break;
					}
				}
			}

			if((alignmentAxis == Axis.Both) || (alignmentAxis == Axis.Vertical))
			{
				switch(alignment)
				{
					case Alignment.Stretch:
						height = container.Height;
						break;
					case Alignment.Center:
						y = ((container.Height / 2.0f) - (_Height / 2.0f)) +
						    _Y;
						break;
					case Alignment.Leading:
						y = (container.Height - _Height) + _Y;
						break;
					case Alignment.Trailing:
						break;
				}
			}

			return new Rectangle(x, y, x + width, y + height);
		}

		public bool Contains(Point point)
		{
			if((point.X >= Left) && (point.X <= Right))
			{
				if((point.Y >= Top) && (point.Y <= Bottom))
				{
					return true;
				}
			}

			return false;
		}

		public bool Contains(Rectangle region)
		{
			if((region.Left >= Left) && (region.Right <= Right))
			{
				if((region.Top >= Top) && (region.Bottom <= Bottom))
				{
					return true;
				}
			}

			return false;
		}

		public bool Equals(Rectangle other)
		{
			return other._Height.Equals(_Height) &&
			       other._Width.Equals(_Width) && other._X.Equals(_X) &&
			       other._Y.Equals(_Y);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Rectangle && Equals((Rectangle)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _Height.GetHashCode();
				result = (result * 397) ^ _Width.GetHashCode();
				result = (result * 397) ^ _X.GetHashCode();
				result = (result * 397) ^ _Y.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(Rectangle left, Rectangle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Rectangle left, Rectangle right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format(
				"X: {0}, Y: {1}, Width: {2}, Height: {3}",
				_X,
				_Y,
				_Width,
				_Height);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.Equal(0, new Rectangle(0, 1, 2, 3).X);
			Assert.Equal(1, new Rectangle(0, 1, 2, 3).Y);
			Assert.Equal(0, new Rectangle(0, 1, 2, 3).Left);
			Assert.Equal(1, new Rectangle(0, 1, 2, 3).Top);
			Assert.Equal(2, new Rectangle(0, 1, 2, 3).Right);
			Assert.Equal(3, new Rectangle(0, 1, 2, 3).Bottom);
			Assert.Equal(10, new Rectangle(0, 5, 10, 15).Width);
			Assert.Equal(10, new Rectangle(0, 5, 10, 15).Height);
			Assert.Equal(new Point(0, 1), new Rectangle(0, 1, 2, 3).Location);
			Assert.Equal(new Size(2, 2), new Rectangle(0, 1, 2, 3).Size);

			Assert.Equal(
				new Rectangle(1, 1, 1, 1),
				new Rectangle(0, 0, 2, 2).Contract(new Thickness(1)));
			Assert.Equal(
				new Rectangle(-1, -1, 3, 3),
				new Rectangle(0, 0, 2, 2).Expand(new Thickness(1)));

			Assert.Equal(4, new Rectangle(0, 0, 2, 2).Area);

			Assert.Equal(new Point(0.5f), new Rectangle(0, 0, 1, 1).Center);

			Assert.True(
				new Rectangle(0, 0, 1, 1).Contains(new Point(0.5f, 0.5f)));
			Assert.True(
				new Rectangle(0, 0, 1, 1).Contains(new Point(0.0f, 0.0f)));
			Assert.True(
				new Rectangle(0, 0, 1, 1).Contains(new Point(1.0f, 0.0f)));
			Assert.True(
				new Rectangle(0, 0, 1, 1).Contains(new Point(1.0f, 1.0f)));
			Assert.True(
				new Rectangle(0, 0, 1, 1).Contains(new Point(0.0f, 1.0f)));
			Assert.False(
				new Rectangle(0, 0, 1, 1).Contains(new Point(1.5f, 0.5f)));

			Assert.True(new Rectangle(0, 0, 1, 1).Contains(Empty));
			Assert.True(
				new Rectangle(0, 0, 1, 1).Contains(new Rectangle(0, 0, 1, 1)));
			Assert.True(
				new Rectangle(0, 0, 1, 1).Contains(
					new Rectangle(0.5f, 0, 0.5f, 1)));
			Assert.False(
				new Rectangle(0, 0, 1, 1).Contains(new Rectangle(-1, -1, 1, 1)));

			Assert.TestObject(MinValue, MaxValue);
		}

		[Fact] internal static void Test1()
		{
			Assert.Equal(
				new Rectangle(50, 0, 100, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Leading, Axis.Horizontal));
			Assert.Equal(
				new Rectangle(0, 0, 50, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Leading,
					Axis.Horizontal,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(0, 50, 50, 100),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Leading, Axis.Vertical));
			Assert.Equal(
				new Rectangle(0, 50, 50, 100),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Leading,
					Axis.Vertical,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(50, 50, 100, 100),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Leading, Axis.Both));
			Assert.Equal(
				new Rectangle(0, 50, 50, 100),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Leading,
					Axis.Both,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(0, 0, 50, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Trailing,
					Axis.Horizontal));
			Assert.Equal(
				new Rectangle(50, 0, 100, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Trailing,
					Axis.Horizontal,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(0, 0, 50, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Trailing, Axis.Vertical));
			Assert.Equal(
				new Rectangle(0, 0, 50, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Trailing,
					Axis.Vertical,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(0, 0, 50, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Trailing, Axis.Both));
			Assert.Equal(
				new Rectangle(50, 0, 100, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Trailing,
					Axis.Both,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(25, 0, 75, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Center, Axis.Horizontal));
			Assert.Equal(
				new Rectangle(25, 0, 75, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Center,
					Axis.Horizontal,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(0, 25, 50, 75),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Center, Axis.Vertical));
			Assert.Equal(
				new Rectangle(0, 25, 50, 75),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Center,
					Axis.Vertical,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(25, 25, 75, 75),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Center, Axis.Both));
			Assert.Equal(
				new Rectangle(25, 25, 75, 75),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Center,
					Axis.Both,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(0, 0, 100, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Stretch, Axis.Horizontal));
			Assert.Equal(
				new Rectangle(0, 0, 100, 50),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Stretch,
					Axis.Horizontal,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(0, 0, 50, 100),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Stretch, Axis.Vertical));
			Assert.Equal(
				new Rectangle(0, 0, 50, 100),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Stretch,
					Axis.Vertical,
					LayoutDirection.RightToLeft));
			Assert.Equal(
				new Rectangle(0, 0, 100, 100),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100), Alignment.Stretch, Axis.Both));
			Assert.Equal(
				new Rectangle(0, 0, 100, 100),
				new Rectangle(0, 0, 50, 50).AlignWithin(
					new Rectangle(0, 0, 100, 100),
					Alignment.Stretch,
					Axis.Both,
					LayoutDirection.RightToLeft));
		}
#endif
	}
}