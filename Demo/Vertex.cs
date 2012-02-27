// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

namespace Yuki.Drawing
{
	/// <summary>
	///   This structure stores logical position and texture coordinates for a vertex.
	/// </summary>
	public struct Vertex : IEquatable<Vertex>
	{
		/// <summary>
		///   This property holds the U component of the vertex texture coordinate.
		/// </summary>
		public float U;

		/// <summary>
		///   This property holds the V component of the vertex texture coordinate.
		/// </summary>
		public float V;

		/// <summary>
		///   This property holds the X component of the vertex position.
		/// </summary>
		public float X;

		/// <summary>
		///   This property holds the Y component of the vertex position.
		/// </summary>
		public float Y;

		/// <summary>
		///   Initializes a new instance of the struct.
		/// </summary>
		/// <param name="x"> The value to set the X component to. </param>
		/// <param name="y"> The value to set the Y component to. </param>
		public Vertex(float x, float y) : this()
		{
			X = x;
			Y = y;
		}

		/// <summary>
		///   Initializes a new instance of the struct.
		/// </summary>
		/// <param name="x"> The value to set the X component to. </param>
		/// <param name="y"> The value to set the Y component to. </param>
		/// <param name="u"> The value to set the U component to. </param>
		/// <param name="v"> The value to set the V component to. </param>
		public Vertex(float x, float y, float u, float v) : this()
		{
			X = x;
			Y = y;
			U = u;
			V = v;
		}

		/// <inheritdoc />
		public bool Equals(Vertex other)
		{
			if(other.X != X)
			{
				return false;
			}
			if(other.Y != Y)
			{
				return false;
			}
			if(other.U != U)
			{
				return false;
			}
			if(other.V != V)
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			if(obj.GetType() != typeof(Vertex))
			{
				return false;
			}

			return Equals((Vertex)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				int result = X.GetHashCode();

				result = (result * 397) ^ Y.GetHashCode();
				result = (result * 397) ^ U.GetHashCode();
				result = (result * 397) ^ V.GetHashCode();

				return result;
			}
		}

		/// <inheritdoc />
		public static bool operator ==(Vertex left, Vertex right)
		{
			return left.Equals(right);
		}

		/// <inheritdoc />
		public static bool operator !=(Vertex left, Vertex right)
		{
			return !left.Equals(right);
		}
	}
}