// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost.Collections;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct holds data for clusters that have been shaped.
	/// </summary>
	internal struct ShapedCluster : IEquatable<ShapedCluster>
	{
		public Size Advance;
		public byte BidiLevel;
		public LineBreakpoint Breakpoint;

		public IndexedRange Characters;
		public ContentType ContentType;
		public Rectangle Floater;
		public FontHandle Font;

		public IndexedRange Glyphs;
		public Alignment HAlignment;

		public float PointSize;

		public Alignment VAlignment;

		public bool Equals(ShapedCluster other)
		{
			return other.BidiLevel == BidiLevel && Equals(other.Font, Font) &&
			       other.Characters.Equals(Characters) && other.Glyphs.Equals(Glyphs) &&
			       other.Advance.Equals(Advance) && other.Breakpoint.Equals(Breakpoint) &&
			       other.ContentType == ContentType && other.PointSize.Equals(PointSize) &&
			       other.HAlignment == HAlignment && other.Floater.Equals(Floater) &&
			       other.VAlignment == VAlignment;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is ShapedCluster && Equals((ShapedCluster)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = BidiLevel.GetHashCode();
				result = (result * 397) ^ (Font != null ? Font.GetHashCode() : 0);
				result = (result * 397) ^ Characters.GetHashCode();
				result = (result * 397) ^ Glyphs.GetHashCode();
				result = (result * 397) ^ Advance.GetHashCode();
				result = (result * 397) ^ Breakpoint.GetHashCode();

				int contentType = (int)ContentType;

				result = (result * 397) ^ contentType.GetHashCode();
				result = (result * 397) ^ PointSize.GetHashCode();

				int hAlignment = (int)HAlignment;

				result = (result * 397) ^ hAlignment.GetHashCode();
				result = (result * 397) ^ Floater.GetHashCode();

				int vAlignment = (int)VAlignment;

				result = (result * 397) ^ vAlignment.GetHashCode();

				return result;
			}
		}

		public static bool operator ==(ShapedCluster left, ShapedCluster right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ShapedCluster left, ShapedCluster right)
		{
			return !left.Equals(right);
		}
	}
}