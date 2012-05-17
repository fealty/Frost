// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Collections;

namespace Frost.Formatting
{
	public struct ShapedCluster : IEquatable<ShapedCluster>
	{
		private readonly float _Advance;
		private readonly LineBreakpoint _Breakpoint;

		private readonly IndexedRange _Text;
		private readonly IndexedRange _Glyphs;

		public ShapedCluster(
			float advance,
			LineBreakpoint breakpoint,
			IndexedRange glyphs,
			IndexedRange text)
		{
			Contract.Requires(Check.IsFinite(advance));

			_Advance = advance;
			_Breakpoint = breakpoint;
			_Glyphs = glyphs;
			_Text = text;
		}

		public IndexedRange Glyphs
		{
			get { return _Glyphs; }
		}

		public IndexedRange Text
		{
			get { return _Text; }
		}

		public LineBreakpoint Breakpoint
		{
			get { return _Breakpoint; }
		}

		public float Advance
		{
			get { return _Advance; }
		}

		public bool Equals(ShapedCluster other)
		{
			return other._Advance.Equals(_Advance) &&
				other._Breakpoint.Equals(_Breakpoint) && other._Text.Equals(_Text) &&
					other._Glyphs.Equals(_Glyphs);
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
				int result = _Advance.GetHashCode();
				result = (result * 397) ^ _Breakpoint.GetHashCode();
				result = (result * 397) ^ _Text.GetHashCode();
				result = (result * 397) ^ _Glyphs.GetHashCode();
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

		public override string ToString()
		{
			return
				string.Format(
					"Advance: {0}, Breakpoint: {1}, Glyphs: {2}, Text: {3}",
					_Advance,
					_Breakpoint,
					_Glyphs,
					_Text);
		}
	}
}