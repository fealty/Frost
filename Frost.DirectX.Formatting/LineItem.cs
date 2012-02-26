using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Cabbage.Formatting
{
	[StructLayout(LayoutKind.Explicit)]
	public struct LineItem
	{
		public static readonly double Infinity = 10000.0;

		[FieldOffset(0)]
		public readonly Demerits Flagged;

		[FieldOffset(8)]
		public readonly Demerits Penalty;

		[FieldOffset(0)]
		public readonly double Stretch;

		[FieldOffset(8)]
		public readonly double Shrink;

		[FieldOffset(16)]
		public readonly double Width;

		[FieldOffset(24)]
		public readonly int Position;

		[FieldOffset(28)]
		private readonly ItemType mType;

		private LineItem(double width, int position) : this()
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);

			mType = ItemType.Box;

			Width = width;
			Position = position;
		}

		private LineItem(
			double width, int position, double stretch, double shrink) : this()
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);
			Contract.Requires(stretch >= double.MinValue && stretch <= double.MaxValue);
			Contract.Requires(shrink >= double.MinValue && shrink <= double.MaxValue);

			mType = ItemType.Glue;

			Width = width;
			Position = position;
			Stretch = stretch;
			Shrink = shrink;
		}

		private LineItem(
			double width, int position, Demerits penalty, Demerits flagged) : this()
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);

			mType = ItemType.Penalty;

			Width = width;
			Position = position;
			Penalty = penalty;
			Flagged = flagged;
		}

		public bool IsGlue
		{
			get { return mType == ItemType.Glue; }
		}

		public bool IsBox
		{
			get { return mType == ItemType.Box; }
		}

		public bool IsPenalty
		{
			get { return mType == ItemType.Penalty; }
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
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);

			return new LineItem(width, position);
		}

		public static LineItem CreateGlue(
			double width, int position, double shrink, double stretch)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);
			Contract.Requires(shrink >= double.MinValue && shrink <= double.MaxValue);
			Contract.Requires(stretch >= double.MinValue && stretch <= double.MaxValue);

			return new LineItem(width, position, stretch, shrink);
		}

		public static LineItem CreatePenalty(
			double width, int position, Demerits penalty, Demerits flagged)
		{
			Contract.Requires(width >= 0.0 && width <= double.MaxValue);
			Contract.Requires(position >= int.MinValue && position <= int.MaxValue);

			return new LineItem(width, position, penalty, flagged);
		}

		public double ComputeWidth(double lineRatio)
		{
			Contract.Requires(
				lineRatio >= double.MinValue && lineRatio <= double.MaxValue);

			return ComputeWidth(lineRatio, false);
		}

		public double ComputeWidth(double lineRatio, bool isPenaltyActive)
		{
			Contract.Requires(
				lineRatio >= double.MinValue && lineRatio <= double.MaxValue);

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
			switch(mType)
			{
				case ItemType.Box:
					return string.Format("Item: {0}, Width: {1}", mType, Width);
				case ItemType.Glue:
					return string.Format(
						"Item: {0}, Width: {1}, Stretch: {2}, Shrink: {3}",
						mType,
						Width,
						Stretch,
						Shrink);
				case ItemType.Penalty:
					return string.Format(
						"Item: {0}, Width: {1}, Penalty: {2}, Flagged: {3}",
						mType,
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
	}
}