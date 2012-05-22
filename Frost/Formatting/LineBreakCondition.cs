// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

namespace Frost.Formatting
{
	/// <summary>
	///   indicates how text at a given character can be split
	/// </summary>
	public enum LineBreakCondition
	{
		/// <summary>
		///   indicates that the text can be split
		/// </summary>
		CanBreak,

		/// <summary>
		///   indicates that the text may not be split
		/// </summary>
		MayNotBreak,

		/// <summary>
		///  indicates that the text must be split
		/// </summary>
		MustBreak,

		/// <summary>
		///  indicates that the value depends on neighboring break conditions
		/// </summary>
		Neutral
	}
}