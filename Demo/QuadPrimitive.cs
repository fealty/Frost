// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

namespace Yuki.Drawing
{
	/// <summary>
	///   This class provides a quad implementation of <see cref="IPrimitive" /> .
	/// </summary>
	public sealed class QuadPrimitive : IPrimitive
	{
		private readonly Triangle[] mTriangles;

		/// <summary>
		///   Initializes a new instance of the class.
		/// </summary>
		public QuadPrimitive()
		{
			mTriangles = new Triangle[2];

			SetPositions(0, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f);
			SetCoordinates(0, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f);

			SetPositions(1, 1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f);
			SetCoordinates(1, 1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f);
		}

		/// <inheritdoc />
		public int VertexCount
		{
			get { return 6; }
		}

		/// <inheritdoc />
		public int TriangleCount
		{
			get { return 2; }
		}

		/// <inheritdoc />
		public Triangle this[int index]
		{
			get { return mTriangles[index]; }
		}

		private void SetPositions(int slot, float x1, float y1, float x2, float y2, float x3, float y3)
		{
			mTriangles[slot] = new Triangle(x1, y1, x2, y2, x3, y3);
		}

		private void SetCoordinates(int slot, float u1, float v1, float u2, float v2, float u3, float v3)
		{
			mTriangles[slot] = new Triangle(
				new Vertex(mTriangles[slot][0].X, mTriangles[slot][0].Y, u1, v1),
				new Vertex(mTriangles[slot][1].X, mTriangles[slot][1].Y, u2, v2),
				new Vertex(mTriangles[slot][2].X, mTriangles[slot][2].Y, u3, v3));
		}
	}
}