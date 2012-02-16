// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class LineCollection : ImmutableBase<IndexedRange>
	{
		public LineCollection(IndexedRange[] items) : base(items)
		{
		}

		public LineCollection(List<IndexedRange> items) : base(items)
		{
		}

		public LineCollection(IEnumerable<IndexedRange> items) : base(items)
		{
		}
	}
}