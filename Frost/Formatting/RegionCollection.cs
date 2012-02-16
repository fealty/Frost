// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class RegionCollection : ImmutableBase<Rectangle>
	{
		public RegionCollection(Rectangle[] items) : base(items)
		{
		}

		public RegionCollection(List<Rectangle> items) : base(items)
		{
		}

		public RegionCollection(IEnumerable<Rectangle> items) : base(items)
		{
		}
	}
}