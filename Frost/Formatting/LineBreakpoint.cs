// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;

namespace Frost.Formatting
{
	/// <summary>
	///   represents the break conditions for an item
	/// </summary>
	public struct LineBreakpoint : IEquatable<LineBreakpoint>
	{
		private readonly LineBreakCondition _BreakConditionAfter;
		private readonly LineBreakCondition _BreakConditionBefore;

		private readonly BreakType _BreakType;

		/// <summary>
		///   constructs the <see cref="LineBreakpoint" /> from the given options
		/// </summary>
		/// <param name="before"> the break condition before the item </param>
		/// <param name="after"> the break condition after the item </param>
		/// <param name="isWhitespace"> indicates whether the breakpoint is on whitespace </param>
		/// <param name="isSoftHyphen"> indicates whether the breakpoint is on a soft hyphen </param>
		public LineBreakpoint(
			LineBreakCondition before,
			LineBreakCondition after,
			bool isWhitespace = false,
			bool isSoftHyphen = false) : this()
		{
			_BreakConditionBefore = before;
			_BreakConditionAfter = after;

			if(isSoftHyphen)
			{
				_BreakType = BreakType.SoftHyphen;
			}
			else if(isWhitespace)
			{
				_BreakType = BreakType.Whitespace;
			}
			else
			{
				_BreakType = BreakType.None;
			}
		}

		/// <summary>
		///   indicates the break condition after the item
		/// </summary>
		public LineBreakCondition BreakConditionAfter
		{
			get { return _BreakConditionAfter; }
		}

		/// <summary>
		///   indicates the break condition prior to the item
		/// </summary>
		public LineBreakCondition BreakConditionBefore
		{
			get { return _BreakConditionBefore; }
		}

		/// <summary>
		///   indicates whether the break is whitespace
		/// </summary>
		public bool IsWhitespace
		{
			get { return _BreakType == BreakType.Whitespace; }
		}

		/// <summary>
		///   indicates whether the break is a soft hyphen
		/// </summary>
		public bool IsSoftHyphen
		{
			get { return _BreakType == BreakType.SoftHyphen; }
		}

		public bool Equals(LineBreakpoint other)
		{
			return other.BreakConditionAfter == BreakConditionAfter &&
				other.BreakConditionBefore == BreakConditionBefore &&
					Equals(other._BreakType, _BreakType);
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
				result = (result * 397) ^ _BreakType.GetHashCode();
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