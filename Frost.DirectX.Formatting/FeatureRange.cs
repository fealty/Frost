using System;

namespace Frost.DirectX.Formatting
{
	internal struct FeatureRange : IEquatable<FeatureRange>
	{
		public TextFeature[] Features;
		public TextRange Range;

		public bool Equals(FeatureRange other)
		{
			return other.Range.Equals(Range) && Equals(other.Features, Features);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is FeatureRange && Equals((FeatureRange)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Range.GetHashCode() * 397) ^
				       (Features != null ? Features.GetHashCode() : 0);
			}
		}

		public static bool operator ==(FeatureRange left, FeatureRange right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FeatureRange left, FeatureRange right)
		{
			return !left.Equals(right);
		}
	}
}