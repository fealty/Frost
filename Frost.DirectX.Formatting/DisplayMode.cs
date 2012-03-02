// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This enum indicates how a cluster should be displayed.
	/// </summary>
	public enum DisplayMode
	{
		/// <summary>
		///   This value indicates that the cluster is visible.
		/// </summary>
		Visible,

		/// <summary>
		///   This value indicates that the visibility of the cluster depends on the cluster's region.
		/// </summary>
		Neutral,

		/// <summary>
		///   This value indicates that the cluster is never visible.
		/// </summary>
		Suppressed
	}
}