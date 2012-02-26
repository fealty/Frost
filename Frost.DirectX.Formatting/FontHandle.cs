using System;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	public sealed class FontHandle : IEquatable<FontHandle>
	{
		private readonly string mFamily;
		private readonly Func<FontHandle, FontInfo> mResolver;
		private readonly FontStretch mStretch;
		private readonly FontStyle mStyle;
		private readonly FontWeight mWeight;

		internal FontHandle(
			Func<FontHandle, FontInfo> resolver,
			string family,
			FontStyle style,
			FontWeight weight,
			FontStretch stretch)
		{
			mResolver = resolver;
			mFamily = family;
			mStyle = style;
			mWeight = weight;
			mStretch = stretch;
		}

		public string Family
		{
			get { return mFamily; }
		}

		public FontStretch Stretch
		{
			get { return mStretch; }
		}

		public FontStyle Style
		{
			get { return mStyle; }
		}

		public FontWeight Weight
		{
			get { return mWeight; }
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

			return Equals(other.mFamily, mFamily) && Equals(other.mStretch, mStretch) &&
			       Equals(other.mStyle, mStyle) && Equals(other.mWeight, mWeight);
		}

		public Font ResolveFont()
		{
			FontInfo fontInfo = mResolver(this);

			return fontInfo.Font;
		}

		public FontFace ResolveFace()
		{
			FontInfo fontInfo = mResolver(this);

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
				int result = (mFamily != null ? mFamily.GetHashCode() : 0);

				int stretch = (int)mStretch;
				
				result = (result * 397) ^ stretch.GetHashCode();

				int style = (int)mStyle;

				result = (result * 397) ^ style.GetHashCode();

				int weight = (int)mWeight;

				result = (result * 397) ^ weight.GetHashCode();
				
				return result;
			}
		}

		internal sealed class FontInfo : IDisposable
		{
			private readonly FontFace mFace;
			private readonly Font mFont;

			public FontInfo(Font font)
			{
				mFont = font;
				mFace = new FontFace(font);
			}

			public Font Font
			{
				get { return mFont; }
			}

			public FontFace FontFace
			{
				get { return mFace; }
			}

			public void Dispose()
			{
				Dispose(true);

				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing)
			{
				if(disposing)
				{
					mFace.Dispose();
					mFont.Dispose();
				}
			}

			~FontInfo()
			{
				Dispose(false);
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