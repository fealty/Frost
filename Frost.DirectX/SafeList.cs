// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Frost.DirectX
{
	/// <summary>
	///   This class provides a thread-safe wrapper of <see cref="List{T}" /> .
	/// </summary>
	/// <typeparam name="T"> The type of the items stored. </typeparam>
	public class SafeList<T> : IEnumerable<T>
	{
		private readonly List<T> _InternalList;

		public SafeList() : this(0)
		{
		}

		/// <summary>
		///   Initializes a new instance of the class.
		/// </summary>
		/// <param name="capacity"> The initial capacity of the collection. </param>
		public SafeList(int capacity)
		{
			Contract.Requires(capacity >= 0);

			_InternalList = new List<T>(capacity);
		}

		/// <summary>
		///   Initializes a new instance of the class.
		/// </summary>
		/// <param name="items"> The items to populate the list with. </param>
		public SafeList(IEnumerable<T> items)
		{
			Contract.Requires(items != null);

			_InternalList = new List<T>(items);
		}

		/// <summary>
		///   This property indicates the capacity of the collection.
		/// </summary>
		public int Capacity
		{
			get
			{
				bool isLockTaken = false;

				try
				{
					EnterReadLock(out isLockTaken);

					return _InternalList.Capacity;
				}
				finally
				{
					if(isLockTaken)
					{
						ExitReadLock();
					}
				}
			}
			set
			{
				bool isLockTaken = false;

				try
				{
					EnterWriteLock(out isLockTaken);

					_InternalList.Capacity = value;
				}
				finally
				{
					if(isLockTaken)
					{
						ExitWriteLock();
					}
				}
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new SafeEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new SafeEnumerator(this);
		}

		/// <summary>
		///   Removes all items from the collection.
		/// </summary>
		public void Clear()
		{
			bool isLockTaken = false;

			try
			{
				EnterWriteLock(out isLockTaken);

				T[] items = _InternalList.ToArray();

				_InternalList.Clear();

				foreach(T item in items)
				{
					OnItemRemoved(item);
				}
			}
			finally
			{
				if(isLockTaken)
				{
					ExitWriteLock();
				}
			}
		}

		/// <summary>
		///   Adds an item to the collection.
		/// </summary>
		/// <param name="item"> The item to add. </param>
		public void Add(T item)
		{
			bool isLockTaken = false;

			try
			{
				EnterWriteLock(out isLockTaken);

				_InternalList.Add(item);

				OnItemAdded(item);
			}
			finally
			{
				if(isLockTaken)
				{
					ExitWriteLock();
				}
			}
		}

		public void AddRange<TEnum>(TEnum enumerable) where TEnum : IEnumerable<T>
		{
			bool isLockTaken = false;

			try
			{
				EnterWriteLock(out isLockTaken);

				foreach(T item in enumerable)
				{
					_InternalList.Add(item);

					OnItemAdded(item);
				}
			}
			finally
			{
				if(isLockTaken)
				{
					ExitWriteLock();
				}
			}
		}

		/// <summary>
		///   Removes an item from the collection.
		/// </summary>
		/// <param name="item"> The item to remove. </param>
		/// <returns> Returns <c>true</c> if the item was removed; otherwise, <c>false</c> . </returns>
		public bool Remove(T item)
		{
			bool isLockTaken = false;

			try
			{
				EnterWriteLock(out isLockTaken);

				if(_InternalList.Remove(item))
				{
					OnItemRemoved(item);

					return true;
				}
			}
			finally
			{
				if(isLockTaken)
				{
					ExitWriteLock();
				}
			}

			return false;
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int startIndex)
		{
			bool isLockTaken = false;

			try
			{
				EnterReadLock(out isLockTaken);

				_InternalList.CopyTo(array, startIndex);
			}
			finally
			{
				if(isLockTaken)
				{
					ExitReadLock();
				}
			}
		}

		/// <summary>
		///   Acquires the unsafe writable context for the thread-safe collection.
		/// </summary>
		/// <returns> Returns a new unsafe collection context. </returns>
		public UnsafeContext AcquireLock()
		{
			return new UnsafeContext(this);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			bool isLockTaken = false;

			try
			{
				EnterReadLock(out isLockTaken);

				return _InternalList.ToString();
			}
			finally
			{
				if(isLockTaken)
				{
					ExitReadLock();
				}
			}
		}

		public SafeEnumerator GetEnumerator()
		{
			return new SafeEnumerator(this);
		}

		/// <summary>
		///   Internal event: Occurs when an item has been removed.
		/// </summary>
		/// <param name="item"> The item that was removed. </param>
		protected virtual void OnItemRemoved(T item)
		{
		}

		/// <summary>
		///   Internal event: Occurs when an item has been inserted.
		/// </summary>
		/// <param name="item"> The item that was inserted. </param>
		protected virtual void OnItemAdded(T item)
		{
		}

		/// <summary>
		///   Internal event: Occurs when an item is replaced with another item.
		/// </summary>
		/// <param name="oldItem"> The old item. </param>
		/// <param name="newItem"> The new item. </param>
		protected virtual void OnItemReplaced(T oldItem, T newItem)
		{
		}

		/// <summary>
		///   Enters the read lock.
		/// </summary>
		/// <param name="isLockTaken"> Output value indicating whether the lock was taken. </param>
		private void EnterReadLock(out bool isLockTaken)
		{
			isLockTaken = false;

			Monitor.Enter(_InternalList, ref isLockTaken);
		}

		/// <summary>
		///   Enters the write lock.
		/// </summary>
		/// <param name="isLockTaken"> Output value indicating whether the lock was taken. </param>
		private void EnterWriteLock(out bool isLockTaken)
		{
			isLockTaken = false;

			Monitor.Enter(_InternalList, ref isLockTaken);
		}

		/// <summary>
		///   Exits the read lock.
		/// </summary>
		private void ExitReadLock()
		{
			Monitor.Exit(_InternalList);
		}

		/// <summary>
		///   Exits the write lock.
		/// </summary>
		private void ExitWriteLock()
		{
			Monitor.Exit(_InternalList);
		}

		/// <summary>
		///   This class provides a read-only wrapper of the thread-safe list.
		/// </summary>
		public class ReadOnly : IEnumerable<T>
		{
			private readonly SafeList<T> _WrappedList;

			/// <summary>
			///   Initializes a new instance of the class.
			/// </summary>
			/// <param name="wrappedList"> The wrapped list. </param>
			public ReadOnly(SafeList<T> wrappedList)
			{
				Contract.Requires(wrappedList != null);

				_WrappedList = wrappedList;
			}

			/// <inheritdoc />
			public int Capacity
			{
				get { return _WrappedList.Capacity; }
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			/// <inheritdoc />
			public void CopyTo(T[] array, int startIndex)
			{
				_WrappedList.CopyTo(array, startIndex);
			}

			/// <summary>
			///   Acquires the unsafe context for the read-only thread-safe collection.
			/// </summary>
			/// <returns> Returns a new unsafe collection context. </returns>
			public UnsafeReadOnlyContext AcquireLock()
			{
				return new UnsafeReadOnlyContext(_WrappedList);
			}

			public SafeEnumerator GetEnumerator()
			{
				return new SafeEnumerator(_WrappedList);
			}
		}

		public struct SafeEnumerator : IEnumerator<T>
		{
			private readonly bool _IsLocked;
			private readonly SafeList<T> _List;
			private List<T>.Enumerator _Enum;

			internal SafeEnumerator(SafeList<T> list)
			{
				_List = list;

				_IsLocked = false;

				_List.EnterReadLock(out _IsLocked);

				_Enum = _List._InternalList.GetEnumerator();
			}

			public bool MoveNext()
			{
				return _Enum.MoveNext();
			}

			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			public T Current
			{
				get { return _Enum.Current; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			void IDisposable.Dispose()
			{
				if(_IsLocked)
				{
					_List.ExitReadLock();
				}
			}
		}

		/// <summary>
		///   This struct provides an unsafe context for the list collection.
		/// </summary>
		public struct UnsafeContext : IDisposable, IEnumerable<T>
		{
			private readonly SafeList<T> _BoundList;
			private readonly bool _IsLocked;

			/// <summary>
			///   Initializes a new instance of the struct.
			/// </summary>
			/// <param name="boundList"> The bound list. </param>
			internal UnsafeContext(SafeList<T> boundList) : this()
			{
				Contract.Requires(boundList != null);

				_BoundList = boundList;

				_IsLocked = false;

				_BoundList.EnterWriteLock(out _IsLocked);
			}

			/// <inheritdoc />
			public int Count
			{
				get
				{
					Debug.Assert(_BoundList != null);

					return _BoundList._InternalList.Count;
				}
			}

			/// <inheritdoc />
			void IDisposable.Dispose()
			{
				if(_IsLocked)
				{
					_BoundList.ExitWriteLock();
				}
			}

			/// <inheritdoc />
			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return GetEnumerator();
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			/// <inheritdoc />
			public bool Contains(T item)
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList.Contains(item);
			}

			/// <inheritdoc />
			public int IndexOf(T item)
			{
				Debug.Assert(_BoundList != null);

				return IndexOf(item, 0, Count);
			}

			/// <inheritdoc />
			public int IndexOf(T item, int startIndex)
			{
				Debug.Assert(_BoundList != null);

				return IndexOf(item, startIndex, Count);
			}

			/// <inheritdoc />
			public int IndexOf(T item, int startIndex, int rangeCount)
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList.IndexOf(item, startIndex, rangeCount);
			}

			/// <inheritdoc />
			public int LastIndexOf(T item)
			{
				Debug.Assert(_BoundList != null);

				return LastIndexOf(item, Count - 1, Count);
			}

			/// <inheritdoc />
			public int LastIndexOf(T item, int startIndex)
			{
				Debug.Assert(_BoundList != null);

				return LastIndexOf(item, startIndex, Count);
			}

			/// <inheritdoc />
			public int LastIndexOf(T item, int startIndex, int rangeCount)
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList.LastIndexOf(item, startIndex, rangeCount);
			}

			/// <inheritdoc />
			public int RemoveAt(int index)
			{
				Debug.Assert(_BoundList != null);

				T item = GetItem(index);

				_BoundList._InternalList.RemoveAt(index);

				_BoundList.OnItemRemoved(item);

				return index > 0 ? index - 1 : index;
			}

			/// <inheritdoc />
			public int RemoveLast()
			{
				Debug.Assert(_BoundList != null);

				return RemoveAt(Count - 1);
			}

			/// <inheritdoc />
			public int RemoveFirst()
			{
				Debug.Assert(_BoundList != null);

				return RemoveAt(0);
			}

			/// <inheritdoc />
			public void Insert(int index, T item)
			{
				Debug.Assert(_BoundList != null);

				_BoundList._InternalList.Insert(index, item);

				_BoundList.OnItemAdded(item);
			}

			/// <inheritdoc />
			public T GetItem(int index)
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList[index];
			}

			public T GetItemOrDefault(int index)
			{
				Debug.Assert(_BoundList != null);

				if(index >= 0 && index < _BoundList._InternalList.Count)
				{
					return _BoundList._InternalList[index];
				}

				return default(T);
			}

			/// <inheritdoc />
			public void SetItem(int index, T newValue)
			{
				Debug.Assert(_BoundList != null);

				T oldItem = GetItem(index);

				_BoundList._InternalList[index] = newValue;

				_BoundList.OnItemReplaced(oldItem, newValue);
			}

			/// <inheritdoc />
			public void Sort()
			{
				Debug.Assert(_BoundList != null);

				_BoundList._InternalList.Sort();
			}

			/// <inheritdoc />
			public void Sort(Comparison<T> comparison)
			{
				Debug.Assert(_BoundList != null);

				_BoundList._InternalList.Sort(comparison);
			}

			/// <inheritdoc />
			public void Sort(IComparer<T> comparer)
			{
				Debug.Assert(_BoundList != null);

				_BoundList._InternalList.Sort(comparer);
			}

			/// <inheritdoc />
			public List<T>.Enumerator GetEnumerator()
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList.GetEnumerator();
			}
		}

		/// <summary>
		///   This class provides a read-only context for the thread-safe list.
		/// </summary>
		public struct UnsafeReadOnlyContext : IDisposable, IEnumerable<T>
		{
			private readonly SafeList<T> _BoundList;
			private readonly bool _IsLocked;

			/// <summary>
			///   Initializes a new instance of the struct.
			/// </summary>
			/// <param name="boundList"> The bound list. </param>
			public UnsafeReadOnlyContext(SafeList<T> boundList)
			{
				Contract.Requires(boundList != null);

				_BoundList = boundList;

				_IsLocked = false;

				_BoundList.EnterReadLock(out _IsLocked);
			}

			/// <inheritdoc />
			public int Count
			{
				get
				{
					Debug.Assert(_BoundList != null);

					return _BoundList._InternalList.Count;
				}
			}

			/// <inheritdoc />
			void IDisposable.Dispose()
			{
				if(_IsLocked)
				{
					_BoundList.ExitReadLock();
				}
			}

			/// <inheritdoc />
			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return GetEnumerator();
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			/// <inheritdoc />
			public bool Contains(T item)
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList.Contains(item);
			}

			/// <inheritdoc />
			public int IndexOf(T item)
			{
				Debug.Assert(_BoundList != null);

				return IndexOf(item, 0, Count);
			}

			/// <inheritdoc />
			public int IndexOf(T item, int startIndex)
			{
				Debug.Assert(_BoundList != null);

				return IndexOf(item, startIndex, Count);
			}

			/// <inheritdoc />
			public int IndexOf(T item, int startIndex, int rangeCount)
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList.IndexOf(item, startIndex, rangeCount);
			}

			/// <inheritdoc />
			public int LastIndexOf(T item)
			{
				Debug.Assert(_BoundList != null);

				return LastIndexOf(item, Count - 1, Count);
			}

			/// <inheritdoc />
			public int LastIndexOf(T item, int startIndex)
			{
				Debug.Assert(_BoundList != null);

				return LastIndexOf(item, startIndex, Count);
			}

			/// <inheritdoc />
			public int LastIndexOf(T item, int startIndex, int rangeCount)
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList.LastIndexOf(item, startIndex, rangeCount);
			}

			/// <inheritdoc />
			public T GetItem(int index)
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList[index];
			}

			public T GetItemOrDefault(int index)
			{
				Debug.Assert(_BoundList != null);

				if(index >= 0 && index < _BoundList._InternalList.Count)
				{
					return _BoundList._InternalList[index];
				}

				return default(T);
			}

			/// <inheritdoc />
			public List<T>.Enumerator GetEnumerator()
			{
				Debug.Assert(_BoundList != null);

				return _BoundList._InternalList.GetEnumerator();
			}
		}
	}
}