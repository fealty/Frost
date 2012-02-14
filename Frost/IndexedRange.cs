// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	public struct IndexedRange : IEquatable<IndexedRange>, IEnumerable<int>
	{
		private static readonly IndexedRange _Empty;

		private readonly int _Length;
		private readonly int _StartIndex;

		static IndexedRange()
		{
			_Empty = new IndexedRange(0, 0);
		}

		public IndexedRange(int startIndex, int length)
		{
			Trace.Assert(startIndex >= 0);
			Trace.Assert(length >= 0);

			this._StartIndex = startIndex;
			this._Length = length;

			Contract.Assert(StartIndex.Equals(startIndex));
			Contract.Assert(Length.Equals(length));
		}

		public IndexedRange Slice(int startIndex)
		{
			return Slice(0, this._Length);
		}

		public IndexedRange Slice(int startIndex, int length)
		{
			Trace.Assert(startIndex >= 0);
			Trace.Assert(length >= 0);
			Trace.Assert(length <= this._Length);
			Trace.Assert(startIndex + length <= this._StartIndex + this._Length);

			return new IndexedRange(startIndex, length);
		}

		public int LastIndex
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return (this._StartIndex + this._Length) - 1;
			}
		}

		public int Length
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return this._Length;
			}
		}

		public int StartIndex
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return this._StartIndex;
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
			return other._StartIndex == this._StartIndex && other._Length == this._Length;
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
				return (this._StartIndex * 397) ^ this._Length;
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public override string ToString()
		{
			return string.Format(
				"StartIndex: {0}, Length: {1}", this._StartIndex, this._Length);
		}

		public struct Enumerator : IEnumerator<int>
		{
			private readonly int _Length;
			private readonly int _StartIndex;

			private int _Index;

			internal Enumerator(IndexedRange range)
			{
				this._StartIndex = range.StartIndex;
				this._Length = range.Length;

				this._Index = -1;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return ++this._Index < this._Length;
			}

			public void Reset()
			{
				this._Index = -1;
			}

			public int Current
			{
				get { return this._StartIndex + this._Index; }
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
		[Fact] internal static void Test0()
		{
			Assert.Equal(1, new IndexedRange(1, 5).StartIndex);
			Assert.Equal(5, new IndexedRange(1, 5).LastIndex);
			Assert.Equal(5, new IndexedRange(1, 5).Length);

			int[] expectedValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			Assert.Equal(expectedValues, new IndexedRange(0, 10));

			IndexedRange testSlice = new IndexedRange(0, 10).Slice(0);

			Assert.Equal(expectedValues, testSlice);

			int[] expectedSlice = { 2, 3, 4, 5 };

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