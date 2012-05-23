// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

using Frost.Collections;

namespace Frost.Shaping
{
	//TODO: implementation must respect forced breaks
	public abstract class Shaper
	{
		private readonly Thread _BoundThread;
		private readonly Device2D _Device2D;

		private string _Text;

		protected Shaper(Device2D device2D)
		{
			Contract.Requires(device2D != null);

			_BoundThread = Thread.CurrentThread;
			_Device2D = device2D;

			Contract.Assert(Device2D.Equals(device2D));
		}

		public Device2D Device2D
		{
			get { return _Device2D; }
		}

		public Thread BoundThread
		{
			get { return _BoundThread; }
		}

		public string Text
		{
			get
			{
				Contract.Requires(Thread.CurrentThread == BoundThread);

				return _Text;
			}
		}

		public void Begin(IShapedGlyphs outputSink, string paragraph)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text == null);
			Contract.Requires(outputSink != null);
			Contract.Requires(paragraph != null);

			_Text = paragraph;

			OnBegin(outputSink);
		}

		public void SetBreakpoint(IndexedRange textRange, LineBreakpoint breakpoint)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));

			OnSetBreakpoint(textRange, breakpoint);
		}

		public void SetBidiLevel(IndexedRange textRange, byte resolvedLevel)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));

			OnSetBidiLevel(textRange, resolvedLevel);
		}

		public void SetCulture(IndexedRange textRange, CultureInfo culture)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));

			OnSetCulture(textRange, culture);
		}

		public void SetFamily(IndexedRange textRange, string family)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));

			OnSetFamily(textRange, family);
		}

		public void SetFeatures(
			IndexedRange textRange, FontFeatureCollection features)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));

			OnSetFeatures(textRange, features);
		}

		public void SetPointSize(IndexedRange textRange, float pointSize)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));
			Contract.Requires(Check.IsPositive(pointSize));

			OnSetPointSize(textRange, pointSize);
		}

		public void SetStretch(IndexedRange textRange, FontStretch stretch)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));

			OnSetStretch(textRange, stretch);
		}

		public void SetStyle(IndexedRange textRange, FontStyle style)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));

			OnSetStyle(textRange, style);
		}

		public void SetWeight(IndexedRange textRange, FontWeight weight)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));

			OnSetWeight(textRange, weight);
		}

		public void SetInline(IndexedRange textRange, object inlineObject)
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);
			Contract.Requires(textRange.IsWithin(Text));
			Contract.Requires(textRange.Length == 1);

			OnSetInline(textRange, inlineObject);
		}

		public void AnalyzeScripts()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);

			OnAnalyzeScripts();
		}

		public void End()
		{
			Contract.Requires(Thread.CurrentThread == BoundThread);
			Contract.Requires(Text != null);

			OnEnd();

			_Text = null;
		}

		protected abstract void OnBegin(IShapedGlyphs outputSink);

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

		protected abstract void OnSetBreakpoint(
			IndexedRange textRange, LineBreakpoint breakpoint);

		protected abstract void OnSetBidiLevel(
			IndexedRange textRange, byte resolvedLevel);

		protected abstract void OnSetCulture(
			IndexedRange textRange, CultureInfo culture);

		protected abstract void OnSetFamily(
			IndexedRange textRange, string family);

		protected abstract void OnSetFeatures(
			IndexedRange textRange, FontFeatureCollection features);

		protected abstract void OnAnalyzeScripts();

		protected abstract void OnEnd();

		public struct Cluster : IEquatable<Cluster>
		{
			private readonly float _Advance;
			private readonly LineBreakpoint _Breakpoint;

			private readonly IndexedRange _Glyphs;
			private readonly IndexedRange _Text;

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
				if(ReferenceEquals(null, obj))
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

			public static bool operator ==(Cluster left, Cluster right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Cluster left, Cluster right)
			{
				return !left.Equals(right);
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

			public Size Offset
			{
				get { return _Offset; }
			}

			public short Index
			{
				get { return _Index; }
			}

			public float Advance
			{
				get { return _Advance; }
			}

			public bool Equals(Glyph other)
			{
				return other._Advance.Equals(_Advance) &&
					other._Index == _Index &&
						other._Offset.Equals(_Offset);
			}

			public override bool Equals(object obj)
			{
				if(ReferenceEquals(null, obj))
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

			public override string ToString()
			{
				return string.Format(
					"Advance: {0}, Index: {1}, Offset: {2}", _Advance, _Index, _Offset);
			}

			public static bool operator ==(Glyph left, Glyph right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Glyph left, Glyph right)
			{
				return !left.Equals(right);
			}
		}

		public struct Span : IEquatable<Span>
		{
			private readonly byte _BidiLevel;

			private readonly IndexedRange _Clusters;
			private readonly FontHandle _FontId;

			private readonly object _Inline;
			private readonly float _PointSize;
			private readonly IndexedRange _Text;

			public Span(
				IndexedRange text,
				IndexedRange clusters,
				float pointSize,
				FontHandle fontId,
				byte bidiLevel,
				object inline = null)
			{
				Contract.Requires(Check.IsPositive(pointSize));

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

			public FontHandle FontId
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
				if(ReferenceEquals(null, obj))
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

			public static bool operator ==(Span left, Span right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Span left, Span right)
			{
				return !left.Equals(right);
			}
		}
	}
}