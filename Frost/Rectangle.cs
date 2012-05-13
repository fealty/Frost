// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Contracts = System.Diagnostics.Contracts.Contract;

namespace Frost
{
	/// <summary>
	/// represents a two-dimensional region having a finite non-negative area
	/// </summary>
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

		[ContractInvariantMethod] private void Invariant()
		{
			Contracts.Invariant(Check.IsFinite(_X));
			Contracts.Invariant(Check.IsFinite(_Y));
			Contracts.Invariant(Check.IsPositive(_Width));
			Contracts.Invariant(Check.IsPositive(_Height));
		}

		/// <summary>
		/// creates a new <see cref="Rectangle"/> from the given left, top, right, and bottom sides
		/// </summary>
		/// <param name="left">the left side of the new <see cref="Rectangle"/></param>
		/// <param name="top">the top side of the new <see cref="Rectangle"/></param>
		/// <param name="right">the right side of the new <see cref="Rectangle"/></param>
		/// <param name="bottom">the bottom side of the new <see cref="Rectangle"/></param>
		/// <returns>the <see cref="Rectangle"/> formed by the <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, and <paramref name="bottom"/> sides</returns>
		public static Rectangle FromEdges(float left, float top, float right, float bottom)
		{
			Contracts.Requires(Check.IsFinite(left));
			Contracts.Requires(Check.IsFinite(top));
			Contracts.Requires(Check.IsFinite(right));
			Contracts.Requires(Check.IsFinite(bottom));
			Contracts.Requires(Check.IsPositive(right - left));
			Contracts.Requires(Check.IsPositive(bottom - top));

			return new Rectangle(left, top, right - left, bottom - top);
		}

		/// <summary>
		/// constructs a new <see cref="Rectangle"/> from the given X and Y coordinates and width and height
		/// </summary>
		/// <param name="x">the X coordinate of the new <see cref="Rectangle"/></param>
		/// <param name="y">the Y coordinate of the new <see cref="Rectangle"/></param>
		/// <param name="width">the width of the new <see cref="Rectangle"/></param>
		/// <param name="height">the height of the new <see cref="Rectangle"/></param>
		public Rectangle(float x, float y, float width, float height)
		{
			Contracts.Requires(Check.IsFinite(x));
			Contracts.Requires(Check.IsFinite(y));
			Contracts.Requires(Check.IsPositive(width));
			Contracts.Requires(Check.IsPositive(height));

			_X = x;
			_Y = y;
			_Width = width;
			_Height = height;

			Contracts.Assert(X.Equals(x));
			Contracts.Assert(Y.Equals(y));
			Contracts.Assert(Width.Equals(_Width));
			Contracts.Assert(Height.Equals(_Height));
		}

		/// <summary>
		/// constructs a new <see cref="Rectangle"/> from the given X and Y coordinates and size
		/// </summary>
		/// <param name="x">the X coordinate of the new <see cref="Rectangle"/></param>
		/// <param name="y">the Y coordinate of the new <see cref="Rectangle"/></param>
		/// <param name="size">the size of the new <see cref="Rectangle"/></param>
		public Rectangle(float x, float y, Size size) : this(x, y, size.Width, size.Height)
		{
			Contracts.Requires(Check.IsFinite(x));
			Contracts.Requires(Check.IsFinite(y));
			Contracts.Requires(Check.IsPositive(size.Width));
			Contracts.Requires(Check.IsPositive(size.Height));
		}

		/// <summary>
		/// constructs a new <see cref="Rectangle"/> from the given location and width and height
		/// </summary>
		/// <param name="location">the location of the new <see cref="Rectangle"/></param>
		/// <param name="width">the width of the new <see cref="Rectangle"/></param>
		/// <param name="height">the height of the new <see cref="Rectangle"/></param>
		public Rectangle(Point location, float width, float height)
			: this(location.X, location.Y, width, height)
		{
			Contracts.Requires(Check.IsPositive(width));
			Contracts.Requires(Check.IsPositive(height));
		}

		/// <summary>
		/// constructs a new <see cref="Rectangle"/> from the given location and size
		/// </summary>
		/// <param name="location">the location of the new <see cref="Rectangle"/></param>
		/// <param name="size">the size of the new <see cref="Rectangle"/></param>
		public Rectangle(Point location, Size size)
			: this(location.X, location.Y, size.Width, size.Height)
		{
			Contracts.Requires(Check.IsPositive(size.Width));
			Contracts.Requires(Check.IsPositive(size.Height));
		}

		/// <summary>
		/// gets the height of the <see cref="Rectangle"/>
		/// </summary>
		public float Height
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_Height));

				return _Height;
			}
		}

		/// <summary>
		/// gets the width of the <see cref="Rectangle"/>
		/// </summary>
		public float Width
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_Width));

				return _Width;
			}
		}

		/// <summary>
		/// gets the Y coordinate of the <see cref="Rectangle"/>
		/// </summary>
		public float Y
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_Y));

				return _Y;
			}
		}

		/// <summary>
		/// gets the X coordinate of the <see cref="Rectangle"/>
		/// </summary>
		public float X
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_X));

				return _X;
			}
		}

		/// <summary>
		///   gets the default value for <see cref="Rectangle" />
		/// </summary>
		public static Rectangle Empty
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Rectangle>().Equals(_Empty));

				return _Empty;
			}
		}

		/// <summary>
		/// gets the maximum value a <see cref="Rectangle"/> can represent
		/// </summary>
		public static Rectangle MaxValue
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Rectangle>().Equals(_MaxValue));

				return _MaxValue;
			}
		}

		/// <summary>
		///   gets the minimum value a <see cref="Rectangle" /> can represent
		/// </summary>
		public static Rectangle MinValue
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Rectangle>().Equals(_MinValue));

				return _MinValue;
			}
		}

		/// <summary>
		/// gets the <see cref="Rectangle.X"/> and <see cref="Rectangle.Y"/> of the <see cref="Rectangle"/> as a <see cref="Point"/>
		/// </summary>
		public Point Location
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Point>().Equals(new Point(_X, _Y)));

				return new Point(_X, _Y);
			}
		}

		/// <summary>
		/// gets the <see cref="Rectangle.Width"/> and <see cref="Rectangle.Height"/> of the <see cref="Rectangle"/> as a <see cref="Size"/>
		/// </summary>
		public Size Size
		{
			get
			{
				Contracts.Ensures(Contracts.Result<Size>().Equals(new Size(_Width, _Height)));

				return new Size(_Width, _Height);
			}
		}

		/// <summary>
		/// gets the left side coordinate of the <see cref="Rectangle"/>
		/// </summary>
		public float Left
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_X));

				return _X;
			}
		}

		/// <summary>
		/// gets the top side coordinate of the <see cref="Rectangle"/>
		/// </summary>
		public float Top
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));
				Contracts.Ensures(Contracts.Result<float>().Equals(_Y));

				return _Y;
			}
		}

		/// <summary>
		/// gets the right side coordinate of the <see cref="Rectangle"/>
		/// </summary>
		public float Right
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));

				return _X + _Width;
			}
		}

		/// <summary>
		/// gets the bottom side coordinate of the <see cref="Rectangle"/>
		/// </summary>
		public float Bottom
		{
			get
			{
				Contracts.Ensures(Check.IsFinite(Contracts.Result<float>()));

				return _Y + _Height;
			}
		}

		/// <summary>
		/// gets the area of the <see cref="Rectangle"/>
		/// </summary>
		public float Area
		{
			get
			{
				Contracts.Ensures(Check.IsPositive(Contracts.Result<float>()));

				return _Width * _Height;
			}
		}

		/// <summary>
		/// gets a value indicating whether the <see cref="Rectangle"/> has an area of zero or less
		/// </summary>
		public bool IsEmpty
		{
			get { return _Width * _Height <= 0.0f; }
		}

		/// <summary>
		/// gets the <see cref="Point"/> located in the center of the <see cref="Rectangle"/>
		/// </summary>
		public Point Center
		{
			get { return new Point(_X + (_Width / 2.0f), _Y + (_Height / 2.0f)); }
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> scaled by the given amount
		/// </summary>
		/// <param name="amount">the amount to scale by</param>
		/// <returns>the <see cref="Rectangle"/> scaled by <paramref name="amount"/></returns>
		public Rectangle Scale(Size amount)
		{
			Contracts.Requires(Check.IsPositive(amount.Width));
			Contracts.Requires(Check.IsPositive(amount.Height));

			return new Rectangle(Location, _Width * amount.Width, _Height * amount.Height);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> scaled by the given amounts
		/// </summary>
		/// <param name="width">the positive amount to scale along the horizontal axis</param>
		/// <param name="height">the positive amount to scale along the vertical axis</param>
		/// <returns>the <see cref="Rectangle"/> scaled by <paramref name="width"/> and <paramref name="height"/></returns>
		public Rectangle Scale(float width, float height)
		{
			Contracts.Requires(Check.IsPositive(width));
			Contracts.Requires(Check.IsPositive(height));

			return new Rectangle(Location, _Width * width, _Height * height);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> resized to the given size
		/// </summary>
		/// <param name="newSize">the size to resize to</param>
		/// <returns>the <see cref="Rectangle"/> resized to <paramref name="newSize"/></returns>
		public Rectangle Resize(Size newSize)
		{
			Contracts.Requires(Check.IsPositive(newSize.Width));
			Contracts.Requires(Check.IsPositive(newSize.Height));

			return new Rectangle(Location, newSize);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> resized to the given width and height
		/// </summary>
		/// <param name="width">the positive width to resize to</param>
		/// <param name="height">the positive height to resize to</param>
		/// <returns>the <see cref="Rectangle"/> resized to <paramref name="width"/> and <paramref name="height"/></returns>
		public Rectangle Resize(float width, float height)
		{
			Contracts.Requires(Check.IsPositive(width));
			Contracts.Requires(Check.IsPositive(height));

			return new Rectangle(Location, width, height);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> with its <see cref="Rectangle.X"/> and <see cref="Rectangle.Y"/> components set to the given location
		/// </summary>
		/// <param name="newLocation">the location</param>
		/// <returns>the <see cref="Rectangle"/> relocated to <paramref name="newLocation"/></returns>
		public Rectangle Relocate(Point newLocation)
		{
			return new Rectangle(newLocation, Size);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> with its <see cref="Rectangle.X"/> and <see cref="Rectangle.Y"/> components set to the given coordinates
		/// </summary>
		/// <param name="x">the finite coordinate along the horizontal axis</param>
		/// <param name="y">the finite coordinate along the vertical axis</param>
		/// <returns>the <see cref="Rectangle"/> relocated to (<paramref name="x"/>, <paramref name="y"/>)</returns>
		public Rectangle Relocate(float x, float y)
		{
			Contracts.Requires(Check.IsFinite(x));
			Contracts.Requires(Check.IsFinite(y));

			return new Rectangle(x, y, Size);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> translated by the given amount
		/// </summary>
		/// <param name="amount">the amount to translate along both the horizontal and vertical axes</param>
		/// <returns>the <see cref="Rectangle"/> translated by <paramref name="amount"/></returns>
		public Rectangle Translate(Size amount)
		{
			return new Rectangle(_X + amount.Width, _Y + amount.Height, Size);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> translated by the given amounts
		/// </summary>
		/// <param name="width">the finite amount to translate along the horizontal axis</param>
		/// <param name="height">the finite amount to translate along the vertical axis</param>
		/// <returns>the <see cref="Rectangle"/> translated by <paramref name="width"/> and <paramref name="height"/></returns>
		public Rectangle Translate(float width, float height)
		{
			Contracts.Requires(Check.IsFinite(width));
			Contracts.Requires(Check.IsFinite(height));

			return new Rectangle(_X + width, _Y + height, Size);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> contracted by the given amount
		/// </summary>
		/// <param name="amount">the amount to contract the left, top, right, and bottom sides</param>
		/// <returns>the <see cref="Rectangle"/> contracted by <paramref name="amount"/></returns>
		public Rectangle Contract(Thickness amount)
		{
			return FromEdges(
				Left + amount.Left, Top + amount.Top, Right - amount.Right, Bottom - amount.Bottom);
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> contracted by the given amounts
		/// </summary>
		/// <param name="left">the positive amount to contract the left side</param>
		/// <param name="top">the positive amount to contract the top side</param>
		/// <param name="right">the positive amount to contract the right side</param>
		/// <param name="bottom">the positive amount to contract the bottom side</param>
		/// <returns>the <see cref="Rectangle"/> contracted by <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, and <paramref name="bottom"/></returns>
		public Rectangle Contract(float left, float top, float right, float bottom)
		{
			Contracts.Requires(Check.IsPositive(left));
			Contracts.Requires(Check.IsPositive(top));
			Contracts.Requires(Check.IsPositive(right));
			Contracts.Requires(Check.IsPositive(bottom));

			return Contract(new Thickness(left, top, right, bottom));
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> contracted by the given amounts
		/// </summary>
		/// <param name="leftRight">the positive amount to contract the left and right sides</param>
		/// <param name="topBottom">the positive amount to contract the top and bottom sides</param>
		/// <returns>the <see cref="Rectangle"/> contracted by <paramref name="leftRight"/> and <paramref name="topBottom"/></returns>
		public Rectangle Contract(float leftRight, float topBottom)
		{
			Contracts.Requires(Check.IsPositive(leftRight));
			Contracts.Requires(Check.IsPositive(topBottom));

			return Contract(new Thickness(leftRight, topBottom));
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> contracted by the given amount
		/// </summary>
		/// <param name="leftRightTopBottom">the positive amount to contract the left, top, right, and bottom sides</param>
		/// <returns>the <see cref="Rectangle"/> contracted by <paramref name="leftRightTopBottom"/></returns>
		public Rectangle Contract(float leftRightTopBottom)
		{
			Contracts.Requires(Check.IsPositive(leftRightTopBottom));

			return Contract(new Thickness(leftRightTopBottom));
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> expanded by the given amounts
		/// </summary>
		/// <param name="left">the positive amount to expand the left side</param>
		/// <param name="top">the positive amount to expand the top side</param>
		/// <param name="right">the positive amount to expand the right side</param>
		/// <param name="bottom">the positive amount to expand the bottom side</param>
		/// <returns>the <see cref="Rectangle"/> expanded by <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, and <paramref name="bottom"/></returns>
		public Rectangle Expand(float left, float top, float right, float bottom)
		{
			Contracts.Requires(Check.IsPositive(left));
			Contracts.Requires(Check.IsPositive(top));
			Contracts.Requires(Check.IsPositive(right));
			Contracts.Requires(Check.IsPositive(bottom));

			return Expand(new Thickness(left, top, right, bottom));
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> expanded by the given amounts
		/// </summary>
		/// <param name="leftRight">the positive amount to expand the left and right sides</param>
		/// <param name="topBottom">the positive amount to expand the top and bottom sides</param>
		/// <returns>the <see cref="Rectangle"/> expanded by <paramref name="leftRight"/> and <paramref name="topBottom"/></returns>
		public Rectangle Expand(float leftRight, float topBottom)
		{
			Contracts.Requires(Check.IsPositive(leftRight));
			Contracts.Requires(Check.IsPositive(topBottom));

			return Expand(new Thickness(leftRight, topBottom));
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> expanded by the given amount
		/// </summary>
		/// <param name="leftRightTopBottom">the positive amount to expand the left, top, right, and bottom sides</param>
		/// <returns>the <see cref="Rectangle"/> expanded by <paramref name="leftRightTopBottom"/></returns>
		public Rectangle Expand(float leftRightTopBottom)
		{
			Contracts.Requires(Check.IsPositive(leftRightTopBottom));

			return Expand(new Thickness(leftRightTopBottom));
		}

		/// <summary>
		/// produces the <see cref="Rectangle"/> expanded by the given amount
		/// </summary>
		/// <param name="amount">the amount to expand the left, top, right, and bottom sides</param>
		/// <returns>the <see cref="Rectangle"/> expanded by <paramref name="amount"/></returns>
		public Rectangle Expand(Thickness amount)
		{
			return FromEdges(
				Left - amount.Left, Top - amount.Top, Right + amount.Right, Bottom + amount.Bottom);
		}

		/// <summary>
		/// aligns the <see cref="Rectangle"/> relative to another <see cref="Rectangle"/>
		/// </summary>
		/// <param name="container">the <see cref="Rectangle"/> to align relative to</param>
		/// <param name="alignment">the alignment operation</param>
		/// <param name="alignmentAxis">the axis to align on</param>
		/// <param name="direction">the reading direction for the alignment</param>
		/// <returns>the <see cref="Rectangle"/> aligned relative to <paramref name="container"/> using the given alignment options</returns>
		public Rectangle AlignRelativeTo( 
			Rectangle container,
			Alignment alignment,
			Axis alignmentAxis,
			LayoutDirection direction = LayoutDirection.LeftToRight)
		{
			float x = _X;
			float y = _Y;
			float width = _Width;
			float height = _Height;

			if((alignmentAxis == Axis.Both) || (alignmentAxis == Axis.Horizontal))
			{
				if(direction == LayoutDirection.RightToLeft)
				{
					switch(alignment)
					{
						case Alignment.Stretch:
							width = container.Width;
							x = container.X;
							break;
						case Alignment.Trailing:
							x = container.Right - _Width;
							break;
						case Alignment.Center:
							x = container.Center.X - (_Width / 2.0f);
							break;
						case Alignment.Leading:
							x = container.Left;
							break;
					}
				}
				else
				{
					switch(alignment)
					{
						case Alignment.Stretch:
							width = container.Width;
							x = container.X;
							break;
						case Alignment.Center:
							x = container.Center.X - (_Width / 2.0f);
							break;
						case Alignment.Leading:
							x = container.Right - _Width;
							break;
						case Alignment.Trailing:
							x = container.Left;
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
						y = container.Y;
						break;
					case Alignment.Center:
						y = container.Center.Y - (_Height / 2.0f);
						break;
					case Alignment.Leading:
						y = container.Bottom - _Height;
						break;
					case Alignment.Trailing:
						y = container.Top;
						break;
				}
			}

			return new Rectangle(x, y, width, height);
		}

		/// <summary>
		/// determines whether the <see cref="Rectangle"/> contains the given <see cref="Point"/>
		/// </summary>
		/// <param name="point">the location to test</param>
		/// <returns><c>true</c> if the instance contains <paramref name="point"/>; otherwise, <c>false</c></returns>
		[Pure] public bool Contains(Point point)
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

		/// <summary>
		/// determines whether the <see cref="Rectangle"/> contains the given <see cref="Rectangle"/>
		/// </summary>
		/// <param name="region">the <see cref="Rectangle"/> to test</param>
		/// <returns><c>true</c> if the instance contains <paramref name="region"/>; otherwise, <c>false</c></returns>
		[Pure] public bool Contains(Rectangle region)
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
			return other._Height.Equals(_Height) && other._Width.Equals(_Width) && other._X.Equals(_X) &&
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

		/// <summary>
		///   determines whether two instances of <see cref="RGBColor" /> are equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> equals <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator ==(Rectangle left, Rectangle right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   determines whether two instances of <see cref="Rectangle" /> are not equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> does not equal <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator !=(Rectangle left, Rectangle right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("X: {0}, Y: {1}, Width: {2}, Height: {3}", _X, _Y, _Width, _Height);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.Equal(0, FromEdges(0, 1, 2, 3).X);
			Assert.Equal(1, FromEdges(0, 1, 2, 3).Y);
			Assert.Equal(0, FromEdges(0, 1, 2, 3).Left);
			Assert.Equal(1, FromEdges(0, 1, 2, 3).Top);
			Assert.Equal(2, FromEdges(0, 1, 2, 3).Right);
			Assert.Equal(3, FromEdges(0, 1, 2, 3).Bottom);
			Assert.Equal(10, FromEdges(0, 5, 10, 15).Width);
			Assert.Equal(10, FromEdges(0, 5, 10, 15).Height);
			Assert.Equal(new Point(0, 1), FromEdges(0, 1, 2, 3).Location);
			Assert.Equal(new Size(2, 2), FromEdges(0, 1, 2, 3).Size);

			Assert.Equal(FromEdges(1, 1, 1, 1), FromEdges(0, 0, 2, 2).Contract(new Thickness(1)));
			Assert.Equal(FromEdges(-1, -1, 3, 3), FromEdges(0, 0, 2, 2).Expand(new Thickness(1)));

			Assert.Equal(4, FromEdges(0, 0, 2, 2).Area);

			Assert.Equal(new Point(0.5f), FromEdges(0, 0, 1, 1).Center);

			Assert.True(FromEdges(0, 0, 1, 1).Contains(new Point(0.5f, 0.5f)));
			Assert.True(FromEdges(0, 0, 1, 1).Contains(new Point(0.0f, 0.0f)));
			Assert.True(FromEdges(0, 0, 1, 1).Contains(new Point(1.0f, 0.0f)));
			Assert.True(FromEdges(0, 0, 1, 1).Contains(new Point(1.0f, 1.0f)));
			Assert.True(FromEdges(0, 0, 1, 1).Contains(new Point(0.0f, 1.0f)));
			Assert.False(FromEdges(0, 0, 1, 1).Contains(new Point(1.5f, 0.5f)));

			Assert.True(FromEdges(0, 0, 1, 1).Contains(Empty));
			Assert.True(FromEdges(0, 0, 1, 1).Contains(FromEdges(0, 0, 1, 1)));
			Assert.True(FromEdges(0, 0, 1, 1).Contains(FromEdges(0.5f, 0, 0.5f, 1)));
			Assert.False(FromEdges(0, 0, 1, 1).Contains(FromEdges(-1, -1, 1, 1)));

			Assert.Equal(new Rectangle(2, 2, 05, 05), new Rectangle(0, 0, 5, 5).Translate(new Size(2)));
			Assert.Equal(new Rectangle(0, 0, 10, 10), new Rectangle(0, 0, 5, 5).Scale(new Size(2)));

			Assert.TestObject(MinValue, MaxValue);
		}

		[Fact] internal static void Test1()
		{
			Assert.Equal(
				FromEdges(50, 0, 100, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Leading, Axis.Horizontal));
			Assert.Equal(
				FromEdges(0, 0, 50, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Leading, Axis.Horizontal, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(0, 50, 50, 100),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Leading, Axis.Vertical));
			Assert.Equal(
				FromEdges(0, 50, 50, 100),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Leading, Axis.Vertical, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(50, 50, 100, 100),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(FromEdges(0, 0, 100, 100), Alignment.Leading, Axis.Both));
			Assert.Equal(
				FromEdges(0, 50, 50, 100),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Leading, Axis.Both, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(0, 0, 50, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Trailing, Axis.Horizontal));
			Assert.Equal(
				FromEdges(50, 0, 100, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Trailing, Axis.Horizontal, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(0, 0, 50, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Trailing, Axis.Vertical));
			Assert.Equal(
				FromEdges(0, 0, 50, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Trailing, Axis.Vertical, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(0, 0, 50, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Trailing, Axis.Both));
			Assert.Equal(
				FromEdges(50, 0, 100, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Trailing, Axis.Both, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(25, 0, 75, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Center, Axis.Horizontal));
			Assert.Equal(
				FromEdges(25, 0, 75, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Center, Axis.Horizontal, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(0, 25, 50, 75),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Center, Axis.Vertical));
			Assert.Equal(
				FromEdges(0, 25, 50, 75),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Center, Axis.Vertical, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(25, 25, 75, 75),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(FromEdges(0, 0, 100, 100), Alignment.Center, Axis.Both));
			Assert.Equal(
				FromEdges(25, 25, 75, 75),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Center, Axis.Both, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(0, 0, 100, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Stretch, Axis.Horizontal));
			Assert.Equal(
				FromEdges(0, 0, 100, 50),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Stretch, Axis.Horizontal, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(0, 0, 50, 100),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Stretch, Axis.Vertical));
			Assert.Equal(
				FromEdges(0, 0, 50, 100),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Stretch, Axis.Vertical, LayoutDirection.RightToLeft));
			Assert.Equal(
				FromEdges(0, 0, 100, 100),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(FromEdges(0, 0, 100, 100), Alignment.Stretch, Axis.Both));
			Assert.Equal(
				FromEdges(0, 0, 100, 100),
				FromEdges(0, 0, 50, 50).AlignRelativeTo(
					FromEdges(0, 0, 100, 100), Alignment.Stretch, Axis.Both, LayoutDirection.RightToLeft));
		}
#endif
	}
}