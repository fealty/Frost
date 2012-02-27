// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using SharpDX.DirectWrite;

using FontStretch = Frost.Formatting.FontStretch;
using FontStyle = Frost.Formatting.FontStyle;
using FontWeight = Frost.Formatting.FontWeight;

namespace Frost.DirectX.Formatting
{
	public sealed class FontDevice : IDisposable
	{
		public const int CacheLimit = 20;

		private readonly Dictionary<FontHandle, FontHandle.FontInfo> _Cache;
		private readonly Factory _FontDevice;
		private readonly Dictionary<FontKey, FontHandle> _HandleCache;
		private readonly FontCollection _SystemCollection;

		public FontDevice()
		{
			_FontDevice = new Factory(FactoryType.Isolated);

			_SystemCollection = _FontDevice.GetSystemFontCollection(false);

			_Cache = new Dictionary<FontHandle, FontHandle.FontInfo>();
			_HandleCache = new Dictionary<FontKey, FontHandle>();
		}

		public Factory Factory
		{
			get { return _FontDevice; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public FontHandle FindFont(string family, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			Contract.Requires(!String.IsNullOrEmpty(family));

			FontKey key;

			key.Family = family;
			key.Style = style;
			key.Weight = weight;
			key.Stretch = stretch;

			FontHandle handle;

			if(_HandleCache.TryGetValue(key, out handle))
			{
				return handle;
			}

			handle = new FontHandle(ResolveHandle, family, style, weight, stretch);

			if(_HandleCache.Count == CacheLimit)
			{
				var item = _HandleCache.First();

				_HandleCache.Remove(item.Key);
			}

			_HandleCache.Add(key, handle);

			return handle;
		}

		private FontHandle.FontInfo ResolveHandle(FontHandle handle)
		{
			Contract.Requires(handle != null);

			FontHandle.FontInfo info;

			if(_Cache.TryGetValue(handle, out info))
			{
				return info;
			}

			int familyIndex;

			_SystemCollection.FindFamilyName(handle.Family, out familyIndex);

			if(familyIndex == int.MaxValue)
			{
				familyIndex = 0;
			}

			using(var family = _SystemCollection.GetFontFamily(familyIndex))
			{
				Font font = family.GetFirstMatchingFont(
					handle.Weight.ToDirectWrite(), handle.Stretch.ToDirectWrite(), handle.Style.ToDirectWrite());

				info = new FontHandle.FontInfo(font);
			}

			if(_Cache.Count == CacheLimit)
			{
				var firstItem = _Cache.First();

				_Cache.Remove(firstItem.Key);

				firstItem.Value.Dispose();
			}

			_Cache.Add(handle, info);

			return info;
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				foreach(var item in _Cache)
				{
					item.Value.Dispose();
				}

				_Cache.Clear();

				_SystemCollection.Dispose();
				_FontDevice.Dispose();
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
	}
}