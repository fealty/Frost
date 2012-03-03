// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct stores data required to identify a unique text geometry.
	/// </summary>
	internal struct TextGeometryKey : IEquatable<TextGeometryKey>
	{
		public float[] Advances;
		public short[] Indices;

		public GlyphOffset[] Offsets;

		public bool Equals(TextGeometryKey other)
		{
			if(!ReferenceEquals(Advances, other.Advances))
			{
				if(Advances != null)
				{
					if(Advances.Length == other.Advances.Length)
					{
						for(int i = 0; i < Advances.Length; ++i)
						{
							if(!Advances[i].Equals(other.Advances[i]))
							{
								return false;
							}
						}
					}
					else
					{
						return false;
					}
				}
			}

			if(!ReferenceEquals(Indices, other.Indices))
			{
				if(Indices != null)
				{
					if(Indices.Length == other.Indices.Length)
					{
						for(int i = 0; i < Indices.Length; ++i)
						{
							if(!Indices[i].Equals(other.Indices[i]))
							{
								return false;
							}
						}
					}
					else
					{
						return false;
					}
				}
			}

			if(!ReferenceEquals(Offsets, other.Offsets))
			{
				if(Offsets != null)
				{
					if(Offsets.Length == other.Offsets.Length)
					{
						for(int i = 0; i < Offsets.Length; ++i)
						{
							if(!Offsets[i].AdvanceOffset.Equals(other.Offsets[i].AdvanceOffset) ||
							   !Offsets[i].AscenderOffset.Equals(other.Offsets[i].AscenderOffset))
							{
								return false;
							}
						}
					}
					else
					{
						return false;
					}
				}
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is TextGeometryKey && Equals((TextGeometryKey)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = 0;

				if(Advances != null)
				{
					foreach(float item in Advances)
					{
						result = (result * 397) ^ item.GetHashCode();
					}
				}

				if(Indices != null)
				{
					foreach(short item in Indices)
					{
						result = (result * 397) ^ item.GetHashCode();
					}
				}

				if(Offsets != null)
				{
					foreach(GlyphOffset item in Offsets)
					{
						result = (result * 397) ^ item.AdvanceOffset.GetHashCode();
						result = (result * 397) ^ item.AscenderOffset.GetHashCode();
					}
				}

				return result;
			}
		}

		public static bool operator ==(TextGeometryKey left, TextGeometryKey right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TextGeometryKey left, TextGeometryKey right)
		{
			return !left.Equals(right);
		}
	}
}