// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;

namespace Frost.Formatting
{
	public abstract class TextShaper
	{
		private string _Paragraph;

		public string Paragraph
		{
			get { return _Paragraph; }
		}

		public void Begin(IShapedText outputSink, string paragraph)
		{
			Contract.Requires(Paragraph == null);
			Contract.Requires(outputSink != null);
			Contract.Requires(paragraph != null);

			_Paragraph = paragraph;

			OnBegin(outputSink);
		}

		public void SetBreakpoint(IndexedRange textRange, LineBreakpoint breakpoint)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));

			OnSetBreakpoints(textRange, breakpoint);
		}

		public void SetBidiLevel(IndexedRange textRange, byte resolvedLevel)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));

			OnSetBidiLevel(textRange, resolvedLevel);
		}

		public void SetCulture(IndexedRange textRange, CultureInfo culture)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));

			OnSetCulture(textRange, culture);
		}

		public void SetFamily(IndexedRange textRange, string family)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));

			OnSetFamily(textRange, family);
		}

		public void SetFeatures(
			IndexedRange textRange, FontFeatureCollection features)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));

			OnSetFeatures(textRange, features);
		}

		public void SetPointSize(IndexedRange textRange, float pointSize)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));
			Contract.Requires(Check.IsPositive(pointSize));

			OnSetPointSize(textRange, pointSize);
		}

		public void SetStretch(IndexedRange textRange, FontStretch stretch)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));

			OnSetStretch(textRange, stretch);
		}

		public void SetStyle(IndexedRange textRange, FontStyle style)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));

			OnSetStyle(textRange, style);
		}

		public void SetWeight(IndexedRange textRange, FontWeight weight)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));

			OnSetWeight(textRange, weight);
		}

		public void SetInline(IndexedRange textRange, object inlineObject)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph));
			Contract.Requires(textRange.Length == 1);

			OnSetInline(textRange, inlineObject);
		}

		public void End()
		{
			Contract.Requires(Paragraph != null);

			OnEnd();

			_Paragraph = null;
		}

		protected abstract void OnBegin(IShapedText outputSink);

		protected abstract void OnSetInline(
			IndexedRange textRange, object inlineObject);

		protected abstract void OnSetWeight(
			IndexedRange textRange, FontWeight weight);

		protected abstract void OnSetStyle(
			IndexedRange textRange, FontStyle style);

		protected abstract void OnSetStretch(
			IndexedRange textRange, FontStretch stretch);

		protected abstract void OnSetPointSize(
			IndexedRange textRange, float pointSize);

		protected abstract void OnSetBreakpoints(
			IndexedRange textRange, LineBreakpoint breakpoint);

		protected abstract void OnSetBidiLevel(
			IndexedRange textRange, byte resolvedLevel);

		protected abstract void OnSetCulture(
			IndexedRange textRange, CultureInfo culture);

		protected abstract void OnSetFamily(
			IndexedRange textRange, string family);

		protected abstract void OnSetFeatures(
			IndexedRange textRange, FontFeatureCollection features);

		protected abstract void OnEnd();

		public struct Cluster : IEquatable<Cluster>
		{
			private readonly float _Advance;
			private readonly LineBreakpoint _Breakpoint;

			private readonly IndexedRange _Text;
			private readonly IndexedRange _Glyphs;

			public Cluster(
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

			public bool Equals(Cluster other)
			{
				return other._Advance.Equals(_Advance) &&
					other._Breakpoint.Equals(_Breakpoint) && other._Text.Equals(_Text) &&
						other._Glyphs.Equals(_Glyphs);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is Cluster && Equals((Cluster)obj);
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

			public static bool operator ==(Cluster left, Cluster right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Cluster left, Cluster right)
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

		public struct Glyph : IEquatable<Glyph>
		{
			private readonly float _Advance;
			private readonly short _Index;
			private readonly Size _Offset;

			public Glyph(float advance, short index, Size offset)
			{
				Contract.Requires(Check.IsFinite(advance));
				Contract.Requires(Check.IsPositive(index));

				_Advance = advance;
				_Index = index;
				_Offset = offset;
			}

			public bool Equals(Glyph other)
			{
				return other._Advance.Equals(_Advance) &&
					other._Index == _Index &&
						other._Offset.Equals(_Offset);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is Glyph && Equals((Glyph)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int result = _Advance.GetHashCode();
					result = (result * 397) ^ _Index.GetHashCode();
					result = (result * 397) ^ _Offset.GetHashCode();
					return result;
				}
			}

			public static bool operator ==(Glyph left, Glyph right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Glyph left, Glyph right)
			{
				return !left.Equals(right);
			}

			public override string ToString()
			{
				return string.Format(
					"Advance: {0}, Index: {1}, Offset: {2}", _Advance, _Index, _Offset);
			}
		}

		public struct Span : IEquatable<Span>
		{
			private readonly FontIdentifier _FontId;

			private readonly float _PointSize;
			private readonly byte _BidiLevel;

			private readonly IndexedRange _Text;
			private readonly IndexedRange _Clusters;

			private readonly object _Inline;

			public Span(
				IndexedRange text,
				IndexedRange clusters,
				float pointSize,
				FontIdentifier fontId,
				byte bidiLevel,
				object inline = null)
			{
				_Text = text;
				_Clusters = clusters;
				_PointSize = pointSize;
				_FontId = fontId;
				_BidiLevel = bidiLevel;
				_Inline = inline;
			}

			public object Inline
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

			public bool Equals(Span other)
			{
				return other._FontId.Equals(_FontId) &&
					other._PointSize.Equals(_PointSize) &&
						other._BidiLevel == _BidiLevel &&
							other._Text.Equals(_Text) &&
								other._Clusters.Equals(_Clusters) &&
									Equals(other._Inline, _Inline);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is Span && Equals((Span)obj);
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
					result = (result * 397) ^ (_Inline != null ? _Inline.GetHashCode() : 0);
					return result;
				}
			}

			public static bool operator ==(Span left, Span right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Span left, Span right)
			{
				return !left.Equals(right);
			}

			public override string ToString()
			{
				return
					string.Format(
						"Text: {0}, Clusters: {1}, PointSize: {2}, FontId: {3}, Inline: {4}, BidiLevel: {5}",
						_Text,
						_Clusters,
						_PointSize,
						_FontId,
						_Inline,
						_BidiLevel);
			}
		}
	}
}