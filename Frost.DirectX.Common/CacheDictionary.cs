// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Common
{
	internal sealed class CacheDictionary<TKey, TValue> : IDisposable
	{
		private readonly int _ItemLimit;

		private readonly LinkedList<Item> _Items;
		private readonly Dictionary<TKey, LinkedListNode<Item>> _Lookup;

		public CacheDictionary(int itemLimit)
		{
			Contract.Requires(itemLimit > 0);

			_Lookup = new Dictionary<TKey, LinkedListNode<Item>>();

			_Items = new LinkedList<Item>();

			_ItemLimit = itemLimit;
		}

		public void Dispose()
		{
			Clear();
		}

		public void Add(TKey key, TValue value)
		{
			if(_Lookup.Count + 1 > _ItemLimit)
			{
				EvictLeastTouchedItem();
			}

			Item item;

			item.Key = key;
			item.Value = value;

			_Lookup[key] = _Items.AddFirst(item);
		}

		public void Remove(TKey key)
		{
			LinkedListNode<Item> item;

			if(_Lookup.TryGetValue(key, out item))
			{
				_Items.Remove(item);
				_Lookup.Remove(key);

				IDisposable disposable = item.Value.Value as IDisposable;

				if(disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			LinkedListNode<Item> existingItem;

			if(_Lookup.TryGetValue(key, out existingItem))
			{
				_Items.Remove(existingItem);

				value = existingItem.Value.Value;

				_Items.AddFirst(existingItem);

				return true;
			}

			value = default(TValue);

			return false;
		}

		public void Clear()
		{
			_Lookup.Clear();

			foreach(var item in _Items)
			{
				IDisposable disposable = item.Value as IDisposable;

				if(disposable != null)
				{
					disposable.Dispose();
				}
			}

			_Items.Clear();
		}

		private void EvictLeastTouchedItem()
		{
			LinkedListNode<Item> leastUsedItem = _Items.Last;

			_Items.Remove(leastUsedItem);

			Item item = leastUsedItem.Value;

			_Lookup.Remove(item.Key);

			IDisposable disposable = item.Value as IDisposable;

			if(disposable != null)
			{
				disposable.Dispose();
			}
		}

		private struct Item
		{
			public TKey Key;
			public TValue Value;
		}
	}
}