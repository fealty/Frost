// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class LineCollection : ImmutableBase<IndexedRange>
	{
		public LineCollection(IndexedRange[] items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public LineCollection(List<IndexedRange> items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public LineCollection(IEnumerable<IndexedRange> items) : base(items)
		{
			Contract.Requires(items != null);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			List<IndexedRange> list = new List<IndexedRange>();

			for(int i = 0; i < 10; ++i)
			{
				list.Add(new IndexedRange(0, i));
			}

			TestDerived(new LineCollection(list.ToArray()));
			TestDerived(new LineCollection(list));
			TestDerived(new LineCollection((IEnumerable<IndexedRange>)list));
		}
#endif
	}
}