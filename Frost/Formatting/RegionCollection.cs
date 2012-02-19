// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class RegionCollection : ImmutableBase<Rectangle>
	{
		public RegionCollection(Rectangle[] items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public RegionCollection(List<Rectangle> items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public RegionCollection(IEnumerable<Rectangle> items) : base(items)
		{
			Contract.Requires(items != null);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			List<Rectangle> list = new List<Rectangle>();

			for(int i = 0; i < 10; ++i)
			{
				list.Add(new Rectangle(i, i, i, i));
			}

			TestDerived(new RegionCollection(list.ToArray()));
			TestDerived(new RegionCollection(list));
			TestDerived(new RegionCollection((IEnumerable<Rectangle>)list));
		}
#endif
	}
}