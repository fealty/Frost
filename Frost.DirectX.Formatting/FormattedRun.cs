// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost.Collections;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct contains data for runs that have been formatted.
	/// </summary>
	internal struct FormattedRun : IEquatable<FormattedRun>
	{
		public byte BidiLevel;
		public IndexedRange Clusters;
		public float EmSize;
		public FontHandle Font;
		public int LineNumber;
		public float PointSize;
		public IndexedRange TextRange;

		public bool Equals(FormattedRun other)
		{
			return other.LineNumber == LineNumber && other.BidiLevel == BidiLevel &&
			       other.Clusters.Equals(Clusters) && other.EmSize.Equals(EmSize) && Equals(other.Font, Font) &&
			       other.TextRange.Equals(TextRange) && other.PointSize.Equals(PointSize);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is FormattedRun && Equals((FormattedRun)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = LineNumber;
				result = (result * 397) ^ BidiLevel;
				result = (result * 397) ^ Clusters.GetHashCode();
				result = (result * 397) ^ EmSize.GetHashCode();
				result = (result * 397) ^ (Font != null ? Font.GetHashCode() : 0);
				result = (result * 397) ^ TextRange.GetHashCode();
				result = (result * 397) ^ PointSize.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(FormattedRun left, FormattedRun right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FormattedRun left, FormattedRun right)
		{
			return !left.Equals(right);
		}
	}
}