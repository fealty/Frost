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
	/// <summary>
	///   This class provides management of font resources.
	/// </summary>
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

		/// <summary>
		///   This property exposes the DirectWrite factory.
		/// </summary>
		public Factory Factory
		{
			get
			{
				Contract.Ensures(Contract.Result<Factory>() != null);

				return _FontDevice;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		///   This method finds the font best matching the given family, style, weight, and stretch.
		/// </summary>
		/// <param name="family"> This parameter references the font family to match. </param>
		/// <param name="style"> This parameter indicates the font style to match. </param>
		/// <param name="weight"> This parameter indicates the font weight to match. </param>
		/// <param name="stretch"> This parameter indicates the font stretch to match. </param>
		/// <returns> This methods returns the handle for the font best matching the given parameters. </returns>
		public FontHandle FindFont(string family, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			Contract.Requires(!String.IsNullOrEmpty(family));
			Contract.Ensures(Contract.Result<FontHandle>() != null);

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

		/// <summary>
		///   This method resolves a <see cref="FontHandle" /> to its lazy information.
		/// </summary>
		/// <param name="handle"> This parameter references the font handle to resolve. </param>
		/// <returns> This method returns the lazy information for the given font handle. </returns>
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

		/// <summary>
		///   This struct provides storage for variables identifying a unique font.
		/// </summary>
		private struct FontKey : IEquatable<FontKey>
		{
			public string Family;
			public FontStretch Stretch;
			public FontStyle Style;
			public FontWeight Weight;

			public bool Equals(FontKey other)
			{
				return Equals(other.Family, Family) && other.Style == Style && other.Weight == Weight &&
				       other.Stretch == Stretch;
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

					int style = (int)Style;

					result = (result * 397) ^ style.GetHashCode();

					int weight = (int)Weight;

					result = (result * 397) ^ weight.GetHashCode();

					int stretch = (int)Stretch;

					result = (result * 397) ^ stretch.GetHashCode();

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