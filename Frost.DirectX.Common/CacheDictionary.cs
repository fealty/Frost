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

			this._Lookup = new Dictionary<TKey, LinkedListNode<Item>>();

			this._Items = new LinkedList<Item>();

			this._ItemLimit = itemLimit;
		}

		public void Dispose()
		{
			Clear();
		}

		public void Add(TKey key, TValue value)
		{
			if(this._Lookup.Count + 1 > this._ItemLimit)
			{
				this.EvictLeastTouchedItem();
			}

			Item item;

			item.Key = key;
			item.Value = value;

			this._Lookup[key] = this._Items.AddFirst(item);
		}

		public void Remove(TKey key)
		{
			LinkedListNode<Item> item;

			if(this._Lookup.TryGetValue(key, out item))
			{
				this._Items.Remove(item);
				this._Lookup.Remove(key);

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

			if(this._Lookup.TryGetValue(key, out existingItem))
			{
				this._Items.Remove(existingItem);

				value = existingItem.Value.Value;

				this._Items.AddFirst(existingItem);

				return true;
			}

			value = default(TValue);

			return false;
		}

		public void Clear()
		{
			this._Lookup.Clear();

			foreach(var item in this._Items)
			{
				IDisposable disposable = item.Value as IDisposable;

				if(disposable != null)
				{
					disposable.Dispose();
				}
			}

			this._Items.Clear();
		}

		private void EvictLeastTouchedItem()
		{
			LinkedListNode<Item> leastUsedItem = this._Items.Last;

			this._Items.Remove(leastUsedItem);

			Item item = leastUsedItem.Value;

			this._Lookup.Remove(item.Key);

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