// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost.Formatting;

using DxBreakCondition = SharpDX.DirectWrite.BreakCondition;
using DxLineBreakpoint = SharpDX.DirectWrite.LineBreakpoint;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct stores break conditions for each character.
	/// </summary>
	internal struct LineBreakpoint : IEquatable<LineBreakpoint>
	{
		public readonly BreakCondition BreakConditionAfter;
		public readonly BreakCondition BreakConditionBefore;

		private readonly BreakType _BreakType;

		/// <summary>
		///   This constructor creates a new instance of this struct from a DirectWrite representation.
		/// </summary>
		/// <param name="breakpoint"> This parameter contains the DirectWrite representation to convert. </param>
		public LineBreakpoint(DxLineBreakpoint breakpoint) : this()
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
				_BreakType = BreakType.SoftHyphen;
			}
			else if(breakpoint.IsWhitespace)
			{
				_BreakType = BreakType.Whitespace;
			}
			else
			{
				_BreakType = BreakType.None;
			}
		}

		/// <summary>
		///   This property indicates whether the break is whitespace.
		/// </summary>
		public bool IsWhitespace
		{
			get { return _BreakType == BreakType.Whitespace; }
		}

		/// <summary>
		///   This property indicates whether the break is a soft hyphen.
		/// </summary>
		public bool IsSoftHyphen
		{
			get { return _BreakType == BreakType.SoftHyphen; }
		}

		public bool Equals(LineBreakpoint other)
		{
			return other.BreakConditionAfter == BreakConditionAfter &&
			       other.BreakConditionBefore == BreakConditionBefore && Equals(other._BreakType, _BreakType);
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