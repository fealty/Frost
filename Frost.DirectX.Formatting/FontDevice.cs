using System;
using System.Collections.Generic;
using System.Linq;

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	public sealed class FontDevice : IDisposable
	{
		public const int CacheLimit = 20;

		private readonly Dictionary<FontHandle, FontHandle.FontInfo> mCache;
		private readonly Factory mFontDevice;
		private readonly Dictionary<FontKey, FontHandle> mHandleCache;
		private readonly FontCollection mSystemCollection;

		public FontDevice()
		{
			mFontDevice = new Factory(FactoryType.Isolated);

			mSystemCollection = mFontDevice.GetSystemFontCollection(false);

			mCache = new Dictionary<FontHandle, FontHandle.FontInfo>();
			mHandleCache = new Dictionary<FontKey, FontHandle>();
		}

		public Factory Factory
		{
			get { return mFontDevice; }
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		public FontHandle FindFont(
			string family, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			FontKey key;

			key.Family = family;
			key.Style = style;
			key.Weight = weight;
			key.Stretch = stretch;

			FontHandle handle;

			if(mHandleCache.TryGetValue(key, out handle))
			{
				return handle;
			}

			handle = new FontHandle(
				ResolveHandle, family, style, weight, stretch);

			if(mHandleCache.Count == CacheLimit)
			{
				var item = mHandleCache.First();

				mHandleCache.Remove(item.Key);
			}

			mHandleCache.Add(key, handle);

			return handle;
		}

		private FontHandle.FontInfo ResolveHandle(FontHandle handle)
		{
			FontHandle.FontInfo info;

			if(mCache.TryGetValue(handle, out info))
			{
				return info;
			}

			int familyIndex;

			mSystemCollection.FindFamilyName(handle.Family, out familyIndex);

			if(familyIndex == int.MaxValue)
			{
				familyIndex = 0;
			}

			using(var family = mSystemCollection.GetFontFamily(familyIndex))
			{
				Font font = family.GetFirstMatchingFont(
					handle.Weight.ToDirectWrite(),
					handle.Stretch.ToDirectWrite(),
					handle.Style.ToDirectWrite());

				info = new FontHandle.FontInfo(font);
			}

			if(mCache.Count == CacheLimit)
			{
				var firstItem = mCache.First();

				mCache.Remove(firstItem.Key);

				firstItem.Value.Dispose();
			}

			mCache.Add(handle, info);

			return info;
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				foreach(var item in mCache)
				{
					item.Value.Dispose();
				}

				mCache.Clear();

				mSystemCollection.Dispose();
				mFontDevice.Dispose();
			}
		}

		private struct FontKey : IEquatable<FontKey>
		{
			public string Family;
			public FontStretch Stretch;
			public FontStyle Style;
			public FontWeight Weight;

			public bool Equals(FontKey other)
			{
				return Equals(other.Family, Family) && Equals(other.Style, Style) &&
				       Equals(other.Weight, Weight) && Equals(other.Stretch, Stretch);
			}

			public override bool Equals(object obj)
			{
				if(ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is FontKey && Equals((FontKey)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int result = (Family != null ? Family.GetHashCode() : 0);
					result = (result * 397) ^ Style.GetHashCode();
					result = (result * 397) ^ Weight.GetHashCode();
					result = (result * 397) ^ Stretch.GetHashCode();
					return result;
				}
			}

			public static bool operator ==(FontKey left, FontKey right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(FontKey left, FontKey right)
			{
				return !left.Equals(right);
			}
		}

		~FontDevice()
		{
			Dispose(false);
		}
	}
}