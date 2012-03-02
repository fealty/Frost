// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This enum indicates how text at a given character can be split.
	/// </summary>
	internal enum BreakCondition
	{
		/// <summary>
		///   This value indicates that the text can be split.
		/// </summary>
		CanBreak,

		/// <summary>
		///   This value indicates that the text may not be split.
		/// </summary>
		MayNotBreak,

		/// <summary>
		///   This value indicates that the text must be split.
		/// </summary>
		MustBreak,

		/// <summary>
		///   This value indicates that the value depends on neighboring break conditions.
		/// </summary>
		Neutral
	}
}