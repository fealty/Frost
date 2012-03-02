// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost.Collections;
using Frost.Formatting;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct provides association of feature collection with text ranges.
	/// </summary>
	internal struct FeatureRange : IEquatable<FeatureRange>
	{
		private readonly FontFeatureCollection _Features;
		private readonly IndexedRange _Range;

		/// <summary>
		///   This constructor associates a given feature collection with a range of text.
		/// </summary>
		/// <param name="range"> This parameter indicates the text range. </param>
		/// <param name="features"> This parameter indicates the feature collection. </param>
		public FeatureRange(IndexedRange range, FontFeatureCollection features)
		{
			_Range = range;
			_Features = features;
		}

		/// <summary>
		///   This property indicates the text range to which the feature collection applies.
		/// </summary>
		public IndexedRange Range
		{
			get { return _Range; }
		}

		/// <summary>
		///   This property references the feature collection associated with the text range.
		/// </summary>
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