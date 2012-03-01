// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
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

		private LineItem(double width, int position) : this()
		{
			Contract.Requires(Check.IsPositive(width));

			_Type = ItemType.Box;

			Width = width;
			Position = position;
		}

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

		private LineItem(double width, int position, Demerits penalty, Demerits flagged) : this()
		{
			Contract.Requires(Check.IsPositive(width));

			_Type = ItemType.Penalty;

			Width = width;
			Position = position;
			Penalty = penalty;
			Flagged = flagged;
		}

		public bool IsGlue
		{
			get { return _Type == ItemType.Glue; }
		}

		public bool IsBox
		{
			get { return _Type == ItemType.Box; }
		}

		public bool IsPenalty
		{
			get { return _Type == ItemType.Penalty; }
		}

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

		public static LineItem CreateBox(double width, int position)
		{
			Contract.Requires(Check.IsPositive(width));

			return new LineItem(width, position);
		}

		public static LineItem CreateGlue(double width, int position, double shrink, double stretch)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsFinite(shrink));
			Contract.Requires(Check.IsFinite(stretch));

			return new LineItem(width, position, stretch, shrink);
		}

		public static LineItem CreatePenalty(
			double width, int position, Demerits penalty, Demerits flagged)
		{
			Contract.Requires(Check.IsPositive(width));

			return new LineItem(width, position, penalty, flagged);
		}

		public double ComputeWidth(double lineRatio)
		{
			Contract.Requires(Check.IsFinite(lineRatio));

			return ComputeWidth(lineRatio, false);
		}

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