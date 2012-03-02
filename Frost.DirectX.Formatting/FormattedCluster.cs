// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost.Collections;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct contains data for clusters that have been formatted.
	/// </summary>
	internal struct FormattedCluster : IEquatable<FormattedCluster>
	{
		public IndexedRange Characters;
		public DisplayMode Display;
		public IndexedRange Glyphs;
		public Rectangle Region;

		public bool Equals(FormattedCluster other)
		{
			return other.Characters.Equals(Characters) && other.Region.Equals(Region) &&
			       other.Glyphs.Equals(Glyphs) && other.Display == Display;
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