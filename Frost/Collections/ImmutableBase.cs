// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frost.Collections
{
	public abstract class ImmutableBase<T> : IEnumerable<T>
	{
		private readonly T[] _Items;

		protected ImmutableBase(T[] items)
		{
			Contract.Requires(items != null);

			_Items = (T[])items.Clone();
		}

		protected ImmutableBase(List<T> items)
		{
			Contract.Requires(items != null);

			_Items = items.ToArray();
		}

		protected ImmutableBase(IEnumerable<T> items)
		{
			Contract.Requires(items != null);

			_Items = items.ToArray();
		}

		public T this[int index]
		{
			get
			{
				Contract.Assert(index >= 0 && index < Count);

				return _Items[index];
			}
		}

		public int Count
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return _Items.Length;
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			IEnumerable<T> @this = this;

			return @this.GetEnumerator();
		}

		public CollectionSlice TakeSlice(int startIndex, int length)
		{
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Requires(startIndex + length <= Count);

			return new CollectionSlice(startIndex, length, this);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public struct CollectionSlice : IEnumerable<T>
		{
			private readonly ImmutableBase<T> _Collection;

			private readonly int _Length;
			private readonly int _StartIndex;

			internal CollectionSlice(
				int startIndex, int length, ImmutableBase<T> collection)
			{
				Contract.Requires(startIndex >= 0);
				Contract.Requires(length >= 0);
				Contract.Requires(collection != null);
				Contract.Requires(startIndex + length <= collection.Count);

				_StartIndex = startIndex;
				_Length = length;
				_Collection = collection;
			}

			public int Count
			{
				get
				{
					Contract.Ensures(Contract.Result<int>() >= 0);

					return _Length;
				}
			}

			public T this[int index]
			{
				get
				{
					Contract.Requires(index >= 0 && index < Count);

					return _Collection[_StartIndex + index];
				}
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				IEnumerable<T> @this = this;

				return @this.GetEnumerator();
			}

			public CollectionSlice TakeSlice(int startIndex, int length)
			{
				Contract.Requires(startIndex >= 0);
				Contract.Requires(length >= 0);
				Contract.Requires(startIndex + length <= Count);

				int adjustedIndex = _StartIndex + startIndex;

				return new CollectionSlice(adjustedIndex, length, _Collection);
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(this);
			}

			public struct Enumerator : IEnumerator<T>
			{
				private readonly CollectionSlice _Slice;

				private int _Index;

				internal Enumerator(CollectionSlice slice)
				{
					_Slice = slice;

					_Index = -1;
				}

				public void Dispose()
				{
				}

				public bool MoveNext()
				{
					return ++_Index < _Slice.Count;
				}

				public void Reset()
				{
					_Index = -1;
				}

				public T Current
				{
					get { return _Slice[_Index]; }
				}

				object IEnumerator.Current
				{
					get { return Current; }
				}
			}
		}

		public struct Enumerator : IEnumerator<T>
		{
			private readonly ImmutableBase<T> _Collection;

			private int _Index;

			internal Enumerator(ImmutableBase<T> collection)
			{
				Contract.Requires(collection != null);

				_Collection = collection;

				_Index = -1;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return ++_Index < _Collection._Items.Length;
			}

			public void Reset()
			{
				_Index = -1;
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public T Current
			{
				get { return _Collection._Items[_Index]; }
			}
		}
	}
}