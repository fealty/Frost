using System;

namespace Frost.DirectX.Formatting
{
	public struct FormattedCluster : IEquatable<FormattedCluster>
	{
		public TextRange Characters;
		public DisplayMode Display;
		public GlyphRange Glyphs;
		public Rectangle Region;

		public bool Equals(FormattedCluster other)
		{
			return other.Characters.Equals(Characters) && other.Region.Equals(Region) &&
			       other.Glyphs.Equals(Glyphs) &&
			       other.Display == Display;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is FormattedCluster && Equals((FormattedCluster)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Characters.GetHashCode();

				result = (result * 397) ^ Region.GetHashCode();
				result = (result * 397) ^ Glyphs.GetHashCode();

				int display = (int)Display;

				result = (result * 397) ^ display.GetHashCode();

				return result;
			}
		}

		public static bool operator ==(FormattedCluster left, FormattedCluster right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FormattedCluster left, FormattedCluster right)
		{
			return !left.Equals(right);
		}
	}
}