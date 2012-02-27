// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Runtime.InteropServices;

using Frost.Collections;

namespace Frost.DirectX.Formatting
{
	[StructLayout(LayoutKind.Explicit)] internal struct TypesetCluster : IEquatable<TypesetCluster>
	{
		public ContentType ContentType;

		public Size Advance;

		public Rectangle Floater;

		public DisplayMode Display;

		public IndexedRange Characters;

		public GlyphRange Glyphs;

		public float PointSize;

		public int LineNumber;

		public byte BidiLevel;

		public FontHandle Font;

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