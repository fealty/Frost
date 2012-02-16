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
	public class ImmutableBase<T> : IEnumerable<T>
	{
		private readonly T[] _Items;

		public ImmutableBase(T[] items)
		{
			Trace.Assert(items != null);

			_Items = (T[])items.Clone();
		}

		public ImmutableBase(List<T> items)
		{
			Trace.Assert(items != null);

			_Items = items.ToArray();
		}

		public ImmutableBase(IEnumerable<T> items)
		{
			Trace.Assert(items != null);

			_Items = items.ToArray();
		}

		public T this[int index]
		{
			get { return _Items[index]; }
		}

		public int Count
		{
			get { return _Items.Length; }
		}

		public Slice TakeSlice(int startIndex, int length)
		{
			Trace.Assert(startIndex >= 0);
			Trace.Assert(length >= 0);
			Trace.Assert(startIndex + length <= this._Items.Length);

			return new Slice(startIndex, length, this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
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

		public struct Slice : IEnumerable<T>
		{
			private readonly ImmutableBase<T> _Collection;

			private readonly int _Length;
			private readonly int _StartIndex;

			internal Slice(int startIndex, int length, ImmutableBase<T> collection)
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
				get { return this._Collection[this._StartIndex + index]; }
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public Slice TakeSlice(int startIndex, int length)
			{
				Trace.Assert(startIndex >= 0);
				Trace.Assert(length >= 0);
				Trace.Assert(startIndex + length <= this._Length);

				int adjustedIndex = this._StartIndex + startIndex;

				return new Slice(adjustedIndex, length, this._Collection);
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(this);
			}

			public struct Enumerator : IEnumerator<T>
			{
				private readonly Slice _Slice;

				private int _Index;

				internal Enumerator(Slice slice)
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
	}
}