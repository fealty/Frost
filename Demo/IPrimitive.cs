// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

namespace Yuki.Drawing
{
	/// <summary>
	///   This interface defines the data required of a rendering primitive.
	/// </summary>
	public interface IPrimitive
	{
		/// <summary>
		///   This property indicates how many vertices are in the primitive.
		/// </summary>
		int VertexCount { get; }

		/// <summary>
		///   This property indicates how many triangles are in the primitive.
		/// </summary>
		int TriangleCount { get; }

		/// <summary>
		///   Gets the triangle at <paramref name="index" /> .
		/// </summary>
		/// <param name="index"> The index of the triangle. </param>
		/// <returns> Returns the triangle at <paramref name="index" /> . </returns>
		Triangle this[int index] { get; }
	}
}