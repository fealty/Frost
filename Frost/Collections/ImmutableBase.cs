// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frost.Collections
{
	public abstract class ImmutableBase<T> : IEnumerable<T>
	{
		private readonly T[] _Items;

		protected ImmutableBase(T[] items)
		{
			Trace.Assert(items != null);

			_Items = (T[])items.Clone();
		}

		protected ImmutableBase(List<T> items)
		{
			Trace.Assert(items != null);

			_Items = items.ToArray();
		}

		protected ImmutableBase(IEnumerable<T> items)
		{
			Trace.Assert(items != null);

			_Items = items.ToArray();
		}

		public T this[int index]
		{
			get
			{
				Contract.Requires(index >= 0 && index < Count);

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
			Trace.Assert(startIndex >= 0);
			Trace.Assert(length >= 0);
			Trace.Assert(startIndex + length <= this._Items.Length);

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

				this._StartIndex = startIndex;
				this._Length = length;
				this._Collection = collection;
			}

			public int Count
			{
				get
				{
					Contract.Ensures(Contract.Result<int>() >= 0);

					return this._Length;
				}
			}

			public T this[int index]
			{
				get
				{
					Contract.Requires(index >= 0 && index < Count);

					return this._Collection[this._StartIndex + index];
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
				Trace.Assert(startIndex >= 0);
				Trace.Assert(length >= 0);
				Trace.Assert(startIndex + length <= this._Length);

				int adjustedIndex = this._StartIndex + startIndex;

				return new CollectionSlice(adjustedIndex, length, this._Collection);
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
					this._Slice = slice;

					this._Index = -1;
				}

				public void Dispose()
				{
				}

				public bool MoveNext()
				{
					return ++this._Index < this._Slice.Count;
				}

				public void Reset()
				{
					this._Index = -1;
				}

				public T Current
				{
					get { return this._Slice[this._Index]; }
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
				this._Collection = collection;

				this._Index = -1;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return ++this._Index < this._Collection._Items.Length;
			}

			public void Reset()
			{
				this._Index = -1;
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public T Current
			{
				get { return this._Collection._Items[this._Index]; }
			}
		}

#if(UNIT_TESTING)
		protected static void TestDerived(ImmutableBase<T> @this)
		{
			Assert.Equal(10, @this.Count);
			Assert.Equal(@this._Items, @this);

			foreach(T item in @this)
			{
				Assert.Equal(item, item);
			}

			new Enumerator().Reset();

			var slice = @this.TakeSlice(1, 8);

			foreach(T item in slice)
			{
				Assert.Equal(item, item);
			}

			new CollectionSlice.Enumerator().Reset();

			T[] expected = new[]
			{
				@this._Items[1], @this._Items[2], @this._Items[3], @this._Items[4],
				@this._Items[5], @this._Items[6], @this._Items[7], @this._Items[8]
			};

			Assert.Equal(expected, slice);

			expected = new[] {slice[1], slice[2], slice[3], slice[4], slice[5], slice[6]};

			slice = slice.TakeSlice(1, 6);

			Assert.Equal(expected, slice);
		}
#endif
	}
}