// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;
using Frost.Shaping;

namespace Frost.Formatting
{
	public sealed class OutlineCollection : ImmutableBase<Outline>
	{
		public OutlineCollection(Outline[] items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public OutlineCollection(List<Outline> items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public OutlineCollection(IEnumerable<Outline> items) : base(items)
		{
			Contract.Requires(items != null);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			List<Outline> list = new List<Outline>();

			for(int i = 0; i < 10; ++i)
			{
				list.Add(new Outline(Geometry.Square, i, i));
			}

			TestDerived(new OutlineCollection(list.ToArray()));
			TestDerived(new OutlineCollection(list));
			TestDerived(new OutlineCollection((IEnumerable<Outline>)list));
		}
#endif
	}
}