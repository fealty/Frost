// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost.Collections;
using Frost.Formatting;

namespace Frost.DirectX.Formatting
{
	internal struct FeatureRange : IEquatable<FeatureRange>
	{
		private readonly FontFeatureCollection _Features;
		private readonly IndexedRange _Range;

		public FeatureRange(IndexedRange range, FontFeatureCollection features)
		{
			_Range = range;
			_Features = features;
		}

		public IndexedRange Range
		{
			get { return _Range; }
		}

		public FontFeatureCollection Features
		{
			get { return _Features; }
		}

		public bool Equals(FeatureRange other)
		{
			return other._Range.Equals(_Range) && Equals(other._Features, _Features);
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
				return (_Range.GetHashCode() * 397) ^ (_Features != null ? _Features.GetHashCode() : 0);
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