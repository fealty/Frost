using System;

using DxBreakCondition = SharpDX.DirectWrite.BreakCondition;
using DxLineBreakpoint = SharpDX.DirectWrite.LineBreakpoint;

namespace Frost.DirectX.Formatting
{
	internal struct LineBreakpoint : IEquatable<LineBreakpoint>
	{
		public readonly BreakCondition BreakConditionAfter;
		public readonly BreakCondition BreakConditionBefore;

		private readonly BreakType mBreakType;

		public LineBreakpoint(DxLineBreakpoint breakpoint)
			: this()
		{
			switch(breakpoint.BreakConditionBefore)
			{
				case DxBreakCondition.CanBreak:
					BreakConditionBefore = BreakCondition.CanBreak;
					break;
				case DxBreakCondition.MayNotBreak:
					BreakConditionBefore = BreakCondition.MayNotBreak;
					break;
				case DxBreakCondition.MustBreak:
					BreakConditionBefore = BreakCondition.MustBreak;
					break;
				case DxBreakCondition.Neutral:
					BreakConditionBefore = BreakCondition.Neutral;
					break;
			}

			switch(breakpoint.BreakConditionAfter)
			{
				case DxBreakCondition.CanBreak:
					BreakConditionAfter = BreakCondition.CanBreak;
					break;
				case DxBreakCondition.MayNotBreak:
					BreakConditionAfter = BreakCondition.MayNotBreak;
					break;
				case DxBreakCondition.MustBreak:
					BreakConditionAfter = BreakCondition.MustBreak;
					break;
				case DxBreakCondition.Neutral:
					BreakConditionAfter = BreakCondition.Neutral;
					break;
			}

			if(breakpoint.IsSoftHyphen)
			{
				mBreakType = BreakType.SoftHyphen;
			}
			else if(breakpoint.IsWhitespace)
			{
				mBreakType = BreakType.Whitespace;
			}
			else
			{
				mBreakType = BreakType.None;
			}
		}

		public bool IsWhitespace
		{
			get { return mBreakType == BreakType.Whitespace; }
		}

		public bool IsSoftHyphen
		{
			get { return mBreakType == BreakType.SoftHyphen; }
		}

		public bool Equals(LineBreakpoint other)
		{
			return other.BreakConditionAfter == BreakConditionAfter &&
			       other.BreakConditionBefore == BreakConditionBefore &&
			       Equals(other.mBreakType, mBreakType);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is LineBreakpoint && Equals((LineBreakpoint)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = BreakConditionAfter.GetHashCode();
				result = (result * 397) ^ BreakConditionBefore.GetHashCode();
				result = (result * 397) ^ mBreakType.GetHashCode();
				return result;
			}
		}

		private enum BreakType
		{
			None,
			Whitespace,
			SoftHyphen
		}

		public static bool operator ==(LineBreakpoint left, LineBreakpoint right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LineBreakpoint left, LineBreakpoint right)
		{
			return !left.Equals(right);
		}
	}
}