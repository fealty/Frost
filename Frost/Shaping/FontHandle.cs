// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Shaping
{
	public struct FontHandle : IEquatable<FontHandle>
	{
		private readonly string _Family;
		private readonly FontStretch _Stretch;
		private readonly FontStyle _Style;
		private readonly FontWeight _Weight;

		public FontHandle(
			string family, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			Contract.Requires(!string.IsNullOrWhiteSpace(family));

			_Family = family;
			_Style = style;
			_Weight = weight;
			_Stretch = stretch;
		}

		public FontStretch Stretch
		{
			get { return _Stretch; }
		}

		public FontWeight Weight
		{
			get { return _Weight; }
		}

		public FontStyle Style
		{
			get { return _Style; }
		}

		public string Family
		{
			get { return _Family; }
		}

		public bool Equals(FontHandle other)
		{
			return Equals(other._Family, _Family) &&
				other._Style == _Style &&
					other._Weight == _Weight &&
						other._Stretch == _Stretch;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is FontHandle && Equals((FontHandle)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _Family.GetHashCode();
				result = (result * 397) ^ ((int)_Style).GetHashCode();
				result = (result * 397) ^ ((int)_Weight).GetHashCode();
				result = (result * 397) ^ ((int)_Stretch).GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Family: {0}, Style: {1}, Weight: {2}, Stretch: {3}",
				_Family,
				_Style,
				_Weight,
				_Stretch);
		}

		public static bool operator ==(FontHandle left, FontHandle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FontHandle left, FontHandle right)
		{
			return !left.Equals(right);
		}
	}
}