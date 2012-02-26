using System;
using System.Runtime.InteropServices;

namespace Frost.DirectX.Formatting
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct TypesetCluster : IEquatable<TypesetCluster>
	{
		[FieldOffset(0)]
		public ContentType ContentType;

		[FieldOffset(4)]
		public Size Advance;

		[FieldOffset(20)]
		public Rectangle Floater;

		[FieldOffset(52)]
		public DisplayMode Display;

		[FieldOffset(60)]
		public TextRange Characters;

		[FieldOffset(72)]
		public GlyphRange Glyphs;

		[FieldOffset(84)]
		public double PointSize;

		[FieldOffset(92)]
		public int LineNumber;

		[FieldOffset(96)]
		public byte BidiLevel;

		[FieldOffset(104)]
		public FontHandle Font;

		public bool Equals(TypesetCluster other)
		{
			return other.ContentType == ContentType && other.Advance.Equals(Advance) &&
			       other.BidiLevel == BidiLevel && other.Characters.Equals(Characters)
			       && Equals(other.Font, Font) &&
			       other.Glyphs.Equals(Glyphs) && other.LineNumber == LineNumber &&
			       other.Display == Display &&
			       other.PointSize.Equals(PointSize) && other.Floater.Equals(Floater);
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