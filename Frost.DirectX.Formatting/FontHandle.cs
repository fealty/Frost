// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using SharpDX.DirectWrite;

using FontStretch = Frost.Formatting.FontStretch;
using FontStyle = Frost.Formatting.FontStyle;
using FontWeight = Frost.Formatting.FontWeight;

namespace Frost.DirectX.Formatting
{
	public sealed class FontHandle : IEquatable<FontHandle>
	{
		private readonly string _Family;
		private readonly Func<FontHandle, FontInfo> _Resolver;
		private readonly FontStretch _Stretch;
		private readonly FontStyle _Style;
		private readonly FontWeight _Weight;

		internal FontHandle(
			Func<FontHandle, FontInfo> resolver,
			string family,
			FontStyle style,
			FontWeight weight,
			FontStretch stretch)
		{
			Contract.Requires(resolver != null);
			Contract.Requires(!String.IsNullOrEmpty(family));

			_Resolver = resolver;
			_Family = family;
			_Style = style;
			_Weight = weight;
			_Stretch = stretch;
		}

		public string Family
		{
			get { return _Family; }
		}

		public FontStretch Stretch
		{
			get { return _Stretch; }
		}

		public FontStyle Style
		{
			get { return _Style; }
		}

		public FontWeight Weight
		{
			get { return _Weight; }
		}

		public bool Equals(FontHandle other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other._Family, _Family) && other._Stretch == _Stretch && other._Style == _Style &&
			       other._Weight == _Weight;
		}

		public Font ResolveFont()
		{
			Contract.Ensures(Contract.Result<Font>() != null);

			FontInfo fontInfo = _Resolver(this);

			return fontInfo.Font;
		}

		public FontFace ResolveFace()
		{
			Contract.Ensures(Contract.Result<FontFace>() != null);

			FontInfo fontInfo = _Resolver(this);

			return fontInfo.FontFace;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			if(ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj is FontHandle && Equals((FontHandle)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (_Family != null ? _Family.GetHashCode() : 0);

				int stretch = (int)_Stretch;

				result = (result * 397) ^ stretch.GetHashCode();

				int style = (int)_Style;

				result = (result * 397) ^ style.GetHashCode();

				int weight = (int)_Weight;

				result = (result * 397) ^ weight.GetHashCode();

				return result;
			}
		}

		internal sealed class FontInfo : IDisposable
		{
			private readonly FontFace _Face;
			private readonly Font _Font;

			public FontInfo(Font font)
			{
				Contract.Requires(font != null);

				_Font = font;
				_Face = new FontFace(font);
			}

			public Font Font
			{
				get
				{
					Contract.Ensures(Contract.Result<Font>() != null);
					Contract.Ensures(Contract.Result<Font>().Equals(_Font));

					return _Font;
				}
			}

			public FontFace FontFace
			{
				get
				{
					Contract.Ensures(Contract.Result<FontFace>() != null);
					Contract.Ensures(Contract.Result<FontFace>().Equals(_Face));

					return _Face;
				}
			}

			public void Dispose()
			{
				Dispose(true);
			}

			private void Dispose(bool disposing)
			{
				if(disposing)
				{
					_Face.Dispose();
					_Font.Dispose();
				}
			}
		}

		public static bool operator ==(FontHandle left, FontHandle right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(FontHandle left, FontHandle right)
		{
			return !Equals(left, right);
		}
	}
}