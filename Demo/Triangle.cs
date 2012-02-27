// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

namespace Yuki.Drawing
{
	/// <summary>
	///   This structure stores three vertices that make up a triangle.
	/// </summary>
	public struct Triangle : IEquatable<Triangle>
	{
		public const int VertexCount = 3;

		public Vertex Vertex1;
		public Vertex Vertex2;
		public Vertex Vertex3;

		/// <summary>
		///   Initializes a new instance of the struct.
		/// </summary>
		/// <param name="x1"> The X component of the first vertex. </param>
		/// <param name="y1"> The Y component of the first vertex. </param>
		/// <param name="x2"> The X component of the second vertex. </param>
		/// <param name="y2"> The Y component of the second vertex. </param>
		/// <param name="x3"> The X component of the third vertex. </param>
		/// <param name="y3"> The Y component of the third vertex. </param>
		public Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
		{
			Vertex1 = new Vertex(x1, y1);
			Vertex2 = new Vertex(x2, y2);
			Vertex3 = new Vertex(x3, y3);
		}

		/// <summary>
		///   Initializes a new instance of the struct.
		/// </summary>
		/// <param name="v1"> The first vertex. </param>
		/// <param name="v2"> The second vertex. </param>
		/// <param name="v3"> The third vertex. </param>
		public Triangle(Vertex v1, Vertex v2, Vertex v3)
		{
			Vertex1 = v1;
			Vertex2 = v2;
			Vertex3 = v3;
		}

		/// <summary>
		///   Read-only indexer for the type.
		/// </summary>
		/// <param name="index"> The index to get. </param>
		/// <returns> Returns the vertex at index. </returns>
		public Vertex this[int index]
		{
			get
			{
				switch(index)
				{
					case 0:
						return Vertex1;
					case 1:
						return Vertex2;
					case 2:
						return Vertex3;
				}

				throw new IndexOutOfRangeException();
			}
			set
			{
				switch(index)
				{
					case 0:
						Vertex1 = value;
						break;
					case 1:
						Vertex2 = value;
						break;
					case 2:
						Vertex3 = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}

		public bool Equals(Triangle other)
		{
			return other.Vertex1.Equals(Vertex1) && other.Vertex2.Equals(Vertex2) &&
			       other.Vertex3.Equals(Vertex3);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			if(obj.GetType() != typeof(Triangle))
			{
				return false;
			}

			return Equals((Triangle)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				int result = Vertex1.GetHashCode();
				result = (result * 397) ^ Vertex2.GetHashCode();
				result = (result * 397) ^ Vertex3.GetHashCode();
				return result;
			}
		}

		/// <inheritdoc />
		public static bool operator ==(Triangle left, Triangle right)
		{
			return left.Equals(right);
		}

		/// <inheritdoc />
		public static bool operator !=(Triangle left, Triangle right)
		{
			return !left.Equals(right);
		}
	}
}