// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class FontFeatureCollection : ImmutableBase<FontFeature>
	{
		public FontFeatureCollection(FontFeature[] items) : base(items)
		{
		}

		public FontFeatureCollection(List<FontFeature> items) : base(items)
		{
		}

		public FontFeatureCollection(IEnumerable<FontFeature> items) : base(items)
		{
		}
	}
}