using System;
using System.Runtime.InteropServices;

namespace Frost.DirectX.Formatting
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct ShapedCluster : IEquatable<ShapedCluster>
	{
		[FieldOffset(0)]
		public ContentType ContentType;

		[FieldOffset(4)]
		public Size Advance;

		[FieldOffset(20)]
		public Rectangle Floater;

		[FieldOffset(52)]
		public Alignment HAlignment;

		[FieldOffset(60)]
		public TextRange Characters;

		[FieldOffset(72)]
		public GlyphRange Glyphs;

		[FieldOffset(84)]
		public LineBreakpoint Breakpoint;

		[FieldOffset(96)]
		public double PointSize;

		[FieldOffset(104)]
		public byte BidiLevel;

		[FieldOffset(108)]
		public Alignment VAlignment;

		[FieldOffset(112)]
		public FontHandle Font;

		public bool Equals(ShapedCluster other)
		{
			return other.BidiLevel == BidiLevel &&
			       Equals(other.Font, Font) &&
			       other.Characters.Equals(Characters) &&
			       other.Glyphs.Equals(Glyphs) && other.Advance.Equals(Advance) &&
			       other.Breakpoint.Equals(Breakpoint) &&
			       other.ContentType == ContentType &&
			       other.PointSize.Equals(PointSize) && other.HAlignment == HAlignment &&
			       other.Floater.Equals(Floater) && other.VAlignment == VAlignment;
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