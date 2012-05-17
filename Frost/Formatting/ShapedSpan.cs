// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Collections;

namespace Frost.Formatting
{
	public struct ShapedSpan : IEquatable<ShapedSpan>
	{
		private readonly FontIdentifier _FontId;

		private readonly float _PointSize;
		private readonly byte _BidiLevel;

		private readonly IndexedRange _Text;
		private readonly IndexedRange _Clusters;

		private readonly Size _Inline;
		private readonly Alignment _HAlignment;
		private readonly Alignment _VAlignment;

		public ShapedSpan(
			IndexedRange text,
			IndexedRange clusters,
			float pointSize,
			FontIdentifier fontId,
			byte bidiLevel) : this()
		{
			Contract.Requires(Check.IsPositive(pointSize));

			_Text = text;
			_Clusters = clusters;
			_PointSize = pointSize;
			_FontId = fontId;
			_BidiLevel = bidiLevel;
		}

		public ShapedSpan(
			IndexedRange text,
			Size inline,
			Alignment hAlignment,
			Alignment vAlignment,
			byte bidiLevel)
			: this()
		{
			_Text = text;
			_Inline = inline;
			_HAlignment = hAlignment;
			_VAlignment = vAlignment;
			_BidiLevel = bidiLevel;
		}

		public Alignment VAlignment
		{
			get { return _VAlignment; }
		}

		public Alignment HAlignment
		{
			get { return _HAlignment; }
		}

		public Size Inline
		{
			get { return _Inline; }
		}

		public IndexedRange Clusters
		{
			get { return _Clusters; }
		}

		public IndexedRange Text
		{
			get { return _Text; }
		}

		public byte BidiLevel
		{
			get { return _BidiLevel; }
		}

		public float PointSize
		{
			get { return _PointSize; }
		}

		public FontIdentifier FontId
		{
			get { return _FontId; }
		}

		public bool IsInline
		{
			get
			{
				return !_Inline.Area.Equals(0.0f) &&
					(_HAlignment == Alignment.Stretch && _VAlignment == Alignment.Stretch);
			}
		}

		public bool IsFloater
		{
			get
			{
				return !_Inline.Area.Equals(0.0f) &&
					(_HAlignment != Alignment.Stretch || _VAlignment != Alignment.Stretch);
			}
		}

		public bool Equals(ShapedSpan other)
		{
			return other._FontId.Equals(_FontId) &&
				other._PointSize.Equals(_PointSize) &&
					other._BidiLevel == _BidiLevel &&
						other._Text.Equals(_Text) &&
							other._Clusters.Equals(_Clusters) &&
								other._Inline.Equals(_Inline) &&
									other._HAlignment == _HAlignment &&
										other._VAlignment == _VAlignment;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is ShapedSpan && Equals((ShapedSpan)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _FontId.GetHashCode();
				result = (result * 397) ^ _PointSize.GetHashCode();
				result = (result * 397) ^ _BidiLevel.GetHashCode();
				result = (result * 397) ^ _Text.GetHashCode();
				result = (result * 397) ^ _Clusters.GetHashCode();
				result = (result * 397) ^ _Inline.GetHashCode();
				result = (result * 397) ^ ((int)_HAlignment).GetHashCode();
				result = (result * 397) ^ ((int)_VAlignment).GetHashCode();
				return result;
			}
		}

		public static bool operator ==(ShapedSpan left, ShapedSpan right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ShapedSpan left, ShapedSpan right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return
				string.Format(
					"Text: {0}, Clusters: {1}, PointSize: {2}, FontId: {3}, Inline: {4}, HAlignment: {5}, VAlignment: {6}, BidiLevel: {7}",
					_Text,
					_Clusters,
					_PointSize,
					_FontId,
					_Inline,
					_HAlignment,
					_VAlignment,
					_BidiLevel);
		}
	}
}