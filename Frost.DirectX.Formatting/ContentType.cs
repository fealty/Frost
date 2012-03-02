// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This enum indicates the content type of a cluster.
	/// </summary>
	internal enum ContentType
	{
		/// <summary>
		///   This value indicates that the content of the cluster is normal text.
		/// </summary>
		Normal,

		/// <summary>
		///   This value indicates that the content of the cluster is an inline object.
		/// </summary>
		Inline,

		/// <summary>
		///   This value indicates that the content of the cluster is a floating object.
		/// </summary>
		Floater,

		/// <summary>
		///   This value indicates that the content of the cluster is a formatting command.
		/// </summary>
		Format
	}
}