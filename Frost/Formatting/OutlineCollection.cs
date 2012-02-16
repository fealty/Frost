// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class OutlineCollection : ImmutableBase<Outline>
	{
		public OutlineCollection(Outline[] items) : base(items)
		{
		}

		public OutlineCollection(List<Outline> items) : base(items)
		{
		}

		public OutlineCollection(IEnumerable<Outline> items) : base(items)
		{
		}
	}
}