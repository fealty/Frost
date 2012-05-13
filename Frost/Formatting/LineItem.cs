// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	/// <summary>
	///   represents a box, glue, or penalty item as required by the Knuth-Plass line breaking algorithm
	/// </summary>
	public struct LineItem : IEquatable<LineItem>
	{
		private const double _Infinity = 10000.0;

		private readonly Demerits _Flagged;
		private readonly Demerits _Penalty;

		private readonly int _Position;

		private readonly double _Shrink;
		private readonly double _Stretch;
		private readonly double _Width;

		private readonly ItemType _Type;

		/// <summary>
		///   constructs a box item
		/// </summary>
		/// <param name="width"> the width of the box </param>
		/// <param name="position"> the user-defined index position </param>
		private LineItem(double width, int position) : this()
		{
			Contract.Requires(Check.IsPositive(width));

			_Type = ItemType.Box;

			_Width = width;
			_Position = position;
		}

		/// <summary>
		///   constructs a glue item
		/// </summary>
		/// <param name="width"> the width of the glue </param>
		/// <param name="position"> the user-defined index position </param>
		/// <param name="stretch"> the number of units the glue may stretch </param>
		/// <param name="shrink"> the number of units the glue may shrink </param>
		private LineItem(double width, int position, double stretch, double shrink)
			: this()
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(stretch));
			Contract.Requires(Check.IsFinite(shrink));

			_Type = ItemType.Glue;

			_Width = width;
			_Position = position;
			_Stretch = stretch;
			_Shrink = shrink;
		}

		/// <summary>
		///   constructs a penalty item
		/// </summary>
		/// <param name="width"> the width of the penalty item </param>
		/// <param name="position"> the user-defined index position </param>
		/// <param name="penalty"> the penalty incurred when the item is chosen </param>
		/// <param name="flagged"> the additional penalty incurred when flagged </param>
		private LineItem(
			double width, int position, Demerits penalty, Demerits flagged) : this()
		{
			Contract.Requires(Check.IsPositive(width));

			_Type = ItemType.Penalty;

			_Width = width;
			_Position = position;
			_Penalty = penalty;
			_Flagged = flagged;
		}

		/// <summary>
		///   the width of the item
		/// </summary>
		public double Width
		{
			get { return _Width; }
		}

		/// <summary>
		///   the stretch set on the item
		/// </summary>
		public double Stretch
		{
			get { return _Stretch; }
		}

		/// <summary>
		///   the shrink set on the item
		/// </summary>
		public double Shrink
		{
			get { return _Shrink; }
		}

		/// <summary>
		///   the user-defined index position
		/// </summary>
		public int Position
		{
			get { return _Position; }
		}

		/// <summary>
		///   the demerits given for penalty items
		/// </summary>
		public Demerits Penalty
		{
			get { return _Penalty; }
		}

		/// <summary>
		///   the demerits given for flagged items
		/// </summary>
		public Demerits Flagged
		{
			get { return _Flagged; }
		}

		/// <summary>
		///   the infinite value within the line breaking algorithm
		/// </summary>
		public static double Infinity
		{
			get { return _Infinity; }
		}

		/// <summary>
		///   gets a value indicating whether the item is glue
		/// </summary>
		public bool IsGlue
		{
			get { return _Type == ItemType.Glue; }
		}

		/// <summary>
		///   gets a value indicating whether the item is a box
		/// </summary>
		public bool IsBox
		{
			get { return _Type == ItemType.Box; }
		}

		/// <summary>
		///   gets a value indicating whether the item is a penalty
		/// </summary>
		public bool IsPenalty
		{
			get { return _Type == ItemType.Penalty; }
		}

		/// <summary>
		///   gets a value indicating whether the item is a forced break
		/// </summary>
		public bool IsForcedBreak
		{
			get
			{
				if(IsPenalty)
				{
					return Demerits.IsNegativeInfinity(Penalty);
				}

				return false;
			}
		}

		/// <summary>
		///   creates a new box item
		/// </summary>
		/// <param name="width"> the width of the box </param>
		/// <param name="position"> the user-defined index position </param>
		/// <returns> the new box item having the given width and position index </returns>
		public static LineItem CreateBox(double width, int position)
		{
			Contract.Requires(Check.IsPositive(width));

			return new LineItem(width, position);
		}

		/// <summary>
		///   creates a new glue item
		/// </summary>
		/// <param name="width"> the width of the glue </param>
		/// <param name="position"> the user-defined index position </param>
		/// <param name="stretch"> the number of units the glue may stretch </param>
		/// <param name="shrink"> the number of units the glue may shrink </param>
		/// <returns> the new glue item having the given width, index, stretch, and shrink </returns>
		public static LineItem CreateGlue(
			double width, int position, double shrink, double stretch)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(shrink));
			Contract.Requires(Check.IsFinite(stretch));

			return new LineItem(width, position, stretch, shrink);
		}

		/// <summary>
		///   creates a new penalty item
		/// </summary>
		/// <param name="width"> the width of the penalty item </param>
		/// <param name="position"> the user-defined index position </param>
		/// <param name="penalty"> the penalty incurred when the item is chosen </param>
		/// <param name="flagged"> the additional penalty incurred when flagged </param>
		/// <returns> the new penalty item having the given width, index, penalty, and flagged penalty </returns>
		public static LineItem CreatePenalty(
			double width, int position, Demerits penalty, Demerits flagged)
		{
			Contract.Requires(Check.IsPositive(width));

			return new LineItem(width, position, penalty, flagged);
		}

		/// <summary>
		///   computes the width of the item
		/// </summary>
		/// <param name="lineRatio"> the current line ratio </param>
		/// <returns> the width of the item given <paramref name="lineRatio" /> </returns>
		public double ComputeWidth(double lineRatio)
		{
			Contract.Requires(Check.IsFinite(lineRatio));

			return ComputeWidth(lineRatio, false);
		}

		/// <summary>
		///   computes the width of the item
		/// </summary>
		/// <param name="lineRatio"> the current line ratio </param>
		/// <param name="isPenaltyActive"> indicates whether penalties are active </param>
		/// <returns> the width of the item given <paramref name="lineRatio" /> </returns>
		public double ComputeWidth(double lineRatio, bool isPenaltyActive)
		{
			Contract.Requires(Check.IsFinite(lineRatio));

			if(IsGlue)
			{
				if(lineRatio < 0.0)
				{
					return Width + lineRatio * Shrink;
				}

				return Width + lineRatio * Stretch;
			}

			if(IsPenalty)
			{
				return isPenaltyActive ? Width : 0.0;
			}

			return Width;
		}

		/// <summary>
		///   compute the demerits for the item
		/// </summary>
		/// <param name="lineRatio"> the current line ratio </param>
		/// <returns> the demerits for the item given <paramref name="lineRatio" /> </returns>
		public Demerits ComputeDemerits(double lineRatio)
		{
			Contract.Requires(Check.IsFinite(lineRatio));

			double badness = 100.0 * Math.Pow(Math.Abs(lineRatio), 3.0);

			if(IsPenalty && Penalty >= 0.0)
			{
				return Math.Pow(10 + badness + Penalty.Value, 2.0);
			}

			if(IsPenalty && !IsForcedBreak)
			{
				return Math.Pow(10 + badness, 2.0) - Math.Pow(Penalty.Value, 2.0);
			}

			return Math.Pow(10 + badness, 2.0);
		}

		/// <summary>
		///   combines flagged demerits with the given item
		/// </summary>
		/// <param name="other"> the other item to combine </param>
		/// <param name="demerits"> the current demerits </param>
		/// <returns> the combined demerits </returns>
		public Demerits CombineFlaggedDemerits(LineItem other, Demerits demerits)
		{
			if(IsPenalty && other.IsPenalty)
			{
				return demerits + Demerits.FlaggedPenalty * Flagged * other.Flagged;
			}

			return demerits;
		}

		public override string ToString()
		{
			switch(_Type)
			{
				case ItemType.Box:
					return string.Format("Item: {0}, Width: {1}", _Type, Width);
				case ItemType.Glue:
					return string.Format(
						"Item: {0}, Width: {1}, Stretch: {2}, Shrink: {3}",
						_Type,
						Width,
						Stretch,
						Shrink);
				case ItemType.Penalty:
					return string.Format(
						"Item: {0}, Width: {1}, Penalty: {2}, Flagged: {3}",
						_Type,
						Width,
						Penalty,
						Flagged);
			}

			return "Item: Untyped";
		}

		private enum ItemType
		{
			Box,
			Glue,
			Penalty
		}

		public bool Equals(LineItem other)
		{
			return other.Flagged.Equals(Flagged) && other.Penalty.Equals(Penalty) &&
				other.Position == Position && other.Shrink.Equals(Shrink) &&
					other.Stretch.Equals(Stretch) && other.Width.Equals(Width) &&
						Equals(other._Type, _Type);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is LineItem && Equals((LineItem)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Flagged.GetHashCode();
				result = (result * 397) ^ Penalty.GetHashCode();
				result = (result * 397) ^ Position;
				result = (result * 397) ^ Shrink.GetHashCode();
				result = (result * 397) ^ Stretch.GetHashCode();
				result = (result * 397) ^ Width.GetHashCode();
				result = (result * 397) ^ _Type.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(LineItem left, LineItem right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LineItem left, LineItem right)
		{
			return !left.Equals(right);
		}
	}
}