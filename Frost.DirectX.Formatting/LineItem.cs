// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct stores data as a box, glue, or penalty item.
	/// </summary>
	internal struct LineItem
	{
		public static readonly double Infinity = 10000.0;

		public readonly Demerits Flagged;

		public readonly Demerits Penalty;
		public readonly int Position;

		public readonly double Shrink;
		public readonly double Stretch;

		public readonly double Width;

		private readonly ItemType _Type;

		/// <summary>
		///   This constructor creates a new box item.
		/// </summary>
		/// <param name="width"> This parameter indicates the width of the box. </param>
		/// <param name="position"> This parameter indicates the user-defined index position. </param>
		private LineItem(double width, int position) : this()
		{
			Contract.Requires(Check.IsPositive(width));

			_Type = ItemType.Box;

			Width = width;
			Position = position;
		}

		/// <summary>
		///   This constructor creates a new glue item.
		/// </summary>
		/// <param name="width"> This parameter indicates the width of the glue. </param>
		/// <param name="position"> This parameter indicates the user-defined index position. </param>
		/// <param name="stretch"> This parameter indicates the number of units the glue may stretch. </param>
		/// <param name="shrink"> This parameter indicates the number of units the glue may shrink. </param>
		private LineItem(double width, int position, double stretch, double shrink) : this()
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(stretch));
			Contract.Requires(Check.IsFinite(shrink));

			_Type = ItemType.Glue;

			Width = width;
			Position = position;
			Stretch = stretch;
			Shrink = shrink;
		}

		/// <summary>
		///   This constructor creates a new penalty item.
		/// </summary>
		/// <param name="width"> This parameter indicates the width of the penalty item. </param>
		/// <param name="position"> This parameter indicates the user-defined index position. </param>
		/// <param name="penalty"> This parameter indicates the penalty incurred when the item is chosen. </param>
		/// <param name="flagged"> This parameter indicates the flagged penalty. </param>
		private LineItem(double width, int position, Demerits penalty, Demerits flagged) : this()
		{
			Contract.Requires(Check.IsPositive(width));

			_Type = ItemType.Penalty;

			Width = width;
			Position = position;
			Penalty = penalty;
			Flagged = flagged;
		}

		/// <summary>
		///   This property indicates whether the item is glue.
		/// </summary>
		public bool IsGlue
		{
			get { return _Type == ItemType.Glue; }
		}

		/// <summary>
		///   This property indicates whether the item is a box.
		/// </summary>
		public bool IsBox
		{
			get { return _Type == ItemType.Box; }
		}

		/// <summary>
		///   This property indicates whether the item is a penalty.
		/// </summary>
		public bool IsPenalty
		{
			get { return _Type == ItemType.Penalty; }
		}

		/// <summary>
		///   This property indicates whether the item is a forced break.
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
		///   This method creates a new box item.
		/// </summary>
		/// <param name="width"> This parameter indicates the width of the box. </param>
		/// <param name="position"> This parameter indicates the user-defined index position. </param>
		/// <returns> This method returns a new box item. </returns>
		public static LineItem CreateBox(double width, int position)
		{
			Contract.Requires(Check.IsPositive(width));

			return new LineItem(width, position);
		}

		/// <summary>
		///   This method creates a new glue item.
		/// </summary>
		/// <param name="width"> This parameter indicates the width of the glue. </param>
		/// <param name="position"> This parameter indicates the user-defined index position. </param>
		/// <param name="stretch"> This parameter indicates the number of units the glue may stretch. </param>
		/// <param name="shrink"> This parameter indicates the number of units the glue may shrink. </param>
		/// <returns> This method returns a new glue item. </returns>
		public static LineItem CreateGlue(double width, int position, double shrink, double stretch)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(shrink));
			Contract.Requires(Check.IsFinite(stretch));

			return new LineItem(width, position, stretch, shrink);
		}

		/// <summary>
		///   This method creates a new penalty item.
		/// </summary>
		/// <param name="width"> This parameter indicates the width of the penalty item. </param>
		/// <param name="position"> This parameter indicates the user-defined index position. </param>
		/// <param name="penalty"> This parameter indicates the penalty incurred when the item is chosen. </param>
		/// <param name="flagged"> This parameter indicates the flagged penalty. </param>
		/// <returns> This method returns a new penalty item. </returns>
		public static LineItem CreatePenalty(
			double width, int position, Demerits penalty, Demerits flagged)
		{
			Contract.Requires(Check.IsPositive(width));

			return new LineItem(width, position, penalty, flagged);
		}

		/// <summary>
		///   This method computes the width of the item.
		/// </summary>
		/// <param name="lineRatio"> This parameter indicates the current line ratio. </param>
		/// <returns> The method returns the computed item width. </returns>
		public double ComputeWidth(double lineRatio)
		{
			Contract.Requires(Check.IsFinite(lineRatio));

			return ComputeWidth(lineRatio, false);
		}

		/// <summary>
		///   This method computes the width of the item.
		/// </summary>
		/// <param name="lineRatio"> This parameter indicates the current line ratio. </param>
		/// <param name="isPenaltyActive"> This parameter indicates whether a penalty is active. </param>
		/// <returns> The method returns the computed item width. </returns>
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
		///   This method computes the demerits of the item.
		/// </summary>
		/// <param name="lineRatio"> This parameter indicates the current line ratio. </param>
		/// <returns> This method returns the computed demerits for the item. </returns>
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
		///   This method combines flagged demerits with another item.
		/// </summary>
		/// <param name="other"> This parameter contains the other item to combine. </param>
		/// <param name="demerits"> This parameter indicates the current demerits. </param>
		/// <returns> This method returns the combined demerits. </returns>
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
						"Item: {0}, Width: {1}, Stretch: {2}, Shrink: {3}", _Type, Width, Stretch, Shrink);
				case ItemType.Penalty:
					return string.Format(
						"Item: {0}, Width: {1}, Penalty: {2}, Flagged: {3}", _Type, Width, Penalty, Flagged);
			}

			return "Item: Untyped";
		}

		private enum ItemType
		{
			Box,
			Glue,
			Penalty
		}
	}
}