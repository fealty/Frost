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
	/// <summary>
	///   This class provides a lazy handle to a font resource.
	/// </summary>
	public sealed class FontHandle : IEquatable<FontHandle>
	{
		private readonly string _Family;
		private readonly Func<FontHandle, FontInfo> _Resolver;
		private readonly FontStretch _Stretch;
		private readonly FontStyle _Style;
		private readonly FontWeight _Weight;

		/// <summary>
		///   This constructor links a new instance of this class to a <see cref="FontDevice" /> or other resolver.
		/// </summary>
		/// <param name="resolver"> This parameter contains the delegate to the resolution method. </param>
		/// <param name="family"> This parameter indicates the font family. </param>
		/// <param name="style"> This parameter indicates the font style. </param>
		/// <param name="weight"> This parameter indicates the font weight. </param>
		/// <param name="stretch"> This parameter indicates the font stretch. </param>
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

		/// <summary>
		///   This property references the font family.
		/// </summary>
		public string Family
		{
			get { return _Family; }
		}

		/// <summary>
		///   This property indicates the font stretch.
		/// </summary>
		public FontStretch Stretch
		{
			get { return _Stretch; }
		}

		/// <summary>
		///   This property indicates the font style.
		/// </summary>
		public FontStyle Style
		{
			get { return _Style; }
		}

		/// <summary>
		///   This property indicates the font weight.
		/// </summary>
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

		/// <summary>
		///   This method resolves the DirectWrite font for the handle.
		/// </summary>
		/// <returns> This method returns the resolved DirectWrite font. </returns>
		public Font ResolveFont()
		{
			Contract.Ensures(Contract.Result<Font>() != null);

			FontInfo fontInfo = _Resolver(this);

			return fontInfo.Font;
		}

		/// <summary>
		///   This method resolves the DirectWrite font face for the handle.
		/// </summary>
		/// <returns> This method returns the resolved DirectWrite font face for the handle. </returns>
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

		/// <summary>
		///   This class provides storage for DirectWrite font resources.
		/// </summary>
		internal sealed class FontInfo : IDisposable
		{
			private FontFace _Face;
			private Font _Font;

			public FontInfo(Font font)
			{
				Contract.Requires(font != null);

				_Font = font;
				_Face = new FontFace(font);
			}

			/// <summary>
			///   This property exposes the DirectWrite font.
			/// </summary>
			public Font Font
			{
				get
				{
					Contract.Ensures(Contract.Result<Font>() != null);
					Contract.Ensures(Contract.Result<Font>().Equals(_Font));

					return _Font;
				}
			}

			/// <summary>
			///   This property exposes the DirectWrite font face.
			/// </summary>
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

					_Face = null;
					_Font = null;
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