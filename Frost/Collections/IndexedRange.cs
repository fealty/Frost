// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.Collections
{
	public struct IndexedRange : IEquatable<IndexedRange>, IEnumerable<int>
	{
		public static implicit operator IndexedRange(string str)
		{
			return str != null ? new IndexedRange(0, str.Length) : Empty;
		}

		private static readonly IndexedRange _Empty;

		private readonly int _Length;
		private readonly int _StartIndex;

		static IndexedRange()
		{
			_Empty = new IndexedRange(0, 0);
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Length >= 0);
			Contract.Invariant(_StartIndex >= 0);
		}

		public IndexedRange(int startIndex, int length)
		{
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

			_StartIndex = startIndex;
			_Length = length;

			Contract.Assert(StartIndex.Equals(startIndex));
			Contract.Assert(Length.Equals(length));
		}

		public IndexedRange Slice(int startIndex)
		{
			return Slice(0, _Length);
		}

		public IndexedRange Extend(int length)
		{
			Contract.Requires(length >= 0);

			return new IndexedRange(StartIndex, Length + length);
		}

		public IndexedRange Shrink(int length)
		{
			Contract.Requires(length >= 0);

			return new IndexedRange(StartIndex, Length - length);
		}

		public bool Contains(int index)
		{
			return index >= StartIndex && index <= LastIndex;
		}

		public IndexedRange Slice(int startIndex, int length)
		{
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Assert(length <= _Length);
			Contract.Assert(startIndex + length <= _StartIndex + _Length);

			return new IndexedRange(startIndex, length);
		}

		public int LastIndex
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return (_StartIndex + _Length) - 1;
			}
		}

		public bool IsWithin(IndexedRange range)
		{
			return StartIndex >= range.StartIndex && LastIndex <= range.LastIndex;
		}

		public int Length
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return _Length;
			}
		}

		public int StartIndex
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return _StartIndex;
			}
		}

		public static IndexedRange Empty
		{
			get { return _Empty; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<int> IEnumerable<int>.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Equals(IndexedRange other)
		{
			return other._StartIndex == _StartIndex && other._Length == _Length;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is IndexedRange && Equals((IndexedRange)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_StartIndex * 397) ^ _Length;
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public override string ToString()
		{
			return string.Format("StartIndex: {0}, Length: {1}", _StartIndex, _Length);
		}

		public struct Enumerator : IEnumerator<int>
		{
			private readonly int _Length;
			private readonly int _StartIndex;

			private int _Index;

			internal Enumerator(IndexedRange range)
			{
				_StartIndex = range.StartIndex;
				_Length = range.Length;

				_Index = -1;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return ++_Index < _Length;
			}

			public void Reset()
			{
				_Index = -1;
			}

			public int Current
			{
				get { return _StartIndex + _Index; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}
		}

		public static bool operator ==(IndexedRange left, IndexedRange right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(IndexedRange left, IndexedRange right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact]
		internal static void Test0()
		{
			Assert.Equal(1, new IndexedRange(1, 5).StartIndex);
			Assert.Equal(5, new IndexedRange(1, 5).LastIndex);
			Assert.Equal(5, new IndexedRange(1, 5).Length);

			int[] expectedValues = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

			Assert.Equal(expectedValues, new IndexedRange(0, 10));

			IndexedRange testSlice = new IndexedRange(0, 10).Slice(0);

			Assert.Equal(expectedValues, testSlice);

			int[] expectedSlice = {2, 3, 4, 5};

			testSlice = testSlice.Slice(2, 4);

			Assert.Equal(expectedSlice, testSlice);

			foreach(int index in ((IEnumerable<int>)testSlice))
			{
				Assert.Equal(index, index);

				new Enumerator().Reset();
			}

			Assert.TestObject(Empty, new IndexedRange(2, 5));
		}
#endif
	}
}