// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost.Collections;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct contains data for clusters that have been typeset.
	/// </summary>
	internal struct TypesetCluster : IEquatable<TypesetCluster>
	{
		public Size Advance;
		public byte BidiLevel;

		public IndexedRange Characters;
		public ContentType ContentType;
		public DisplayMode Display;
		public Rectangle Floater;
		public FontHandle Font;

		public IndexedRange Glyphs;

		public int LineNumber;
		public float PointSize;

		public bool Equals(TypesetCluster other)
		{
			return other.ContentType == ContentType && other.Advance.Equals(Advance) &&
			       other.BidiLevel == BidiLevel && other.Characters.Equals(Characters) &&
			       Equals(other.Font, Font) && other.Glyphs.Equals(Glyphs) && other.LineNumber == LineNumber &&
			       other.Display == Display && other.PointSize.Equals(PointSize) &&
			       other.Floater.Equals(Floater);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is TypesetCluster && Equals((TypesetCluster)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = ContentType.GetHashCode();

				result = (result * 397) ^ Advance.GetHashCode();
				result = (result * 397) ^ BidiLevel;
				result = (result * 397) ^ Characters.GetHashCode();
				result = (result * 397) ^ (Font != null ? Font.GetHashCode() : 0);
				result = (result * 397) ^ Glyphs.GetHashCode();
				result = (result * 397) ^ LineNumber;

				int display = (int)Display;

				result = (result * 397) ^ display.GetHashCode();
				result = (result * 397) ^ PointSize.GetHashCode();
				result = (result * 397) ^ Floater.GetHashCode();

				return result;
			}
		}

		public static bool operator ==(TypesetCluster left, TypesetCluster right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TypesetCluster left, TypesetCluster right)
		{
			return !left.Equals(right);
		}
	}
}