// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

namespace Frost
{
	/// <summary>
	///   indicates how a thing is aligned relative to a containing thing
	/// </summary>
	public enum Alignment
	{
		/// <summary>
		///   the thing will span the thing
		/// </summary>
		Stretch,

		/// <summary>
		///   the thing will align to the trailing edge of the containing thing in the current direction of layout
		/// </summary>
		Trailing,

		/// <summary>
		///   the thing will be centered within the containing thing
		/// </summary>
		Center,

		/// <summary>
		///   the thing will align to the leading edge of the containing thing in the current direction of layout
		/// </summary>
		Leading
	}

	/// <summary>
	///   indicates the 2D coordinate space axis for an operation
	/// </summary>
	public enum Axis
	{
		/// <summary>
		///   the operation applies to both the horizontal and vertical axes
		/// </summary>
		Both,

		/// <summary>
		///   the operation applies only to the horizontal axis
		/// </summary>
		Horizontal,

		/// <summary>
		///   the operation applies only to the vertical axis
		/// </summary>
		Vertical
	}

	/// <summary>
	///   indicates the direction of layout
	/// </summary>
	public enum LayoutDirection
	{
		/// <summary>
		///   items are set in item order from left to right
		/// </summary>
		LeftToRight,

		/// <summary>
		///   items are set in item order from right to left
		/// </summary>
		RightToLeft
	}

	/// <summary>
	///   indicates how to apply a transformation matrix to an existing transformation
	/// </summary>
	public enum TransformMode
	{
		/// <summary>
		///   multiply the tranformation matrices together
		/// </summary>
		Multiply,

		/// <summary>
		///   replace the existing transformation with the new transformation
		/// </summary>
		Replace
	}

	/// <summary>
	///   indicates whether a visual operation should be antialiased
	/// </summary>
	public enum Antialiasing
	{
		/// <summary>
		///   the visual operation will be antialiased
		/// </summary>
		Default,

		/// <summary>
		///   the visual operation will be aliased
		/// </summary>
		Aliased
	}

	/// <summary>
	///   indicates how an operation will handle existing data
	/// </summary>
	public enum Retention
	{
		/// <summary>
		///   clears the existing data
		/// </summary>
		ClearData,

		/// <summary>
		///   retains the existing data
		/// </summary>
		RetainData
	}
}