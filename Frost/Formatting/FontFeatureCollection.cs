// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class FontFeatureCollection : ImmutableBase<FontFeature>
	{
		public FontFeatureCollection(FontFeature[] items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public FontFeatureCollection(List<FontFeature> items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public FontFeatureCollection(IEnumerable<FontFeature> items) : base(items)
		{
			Contract.Requires(items != null);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			List<FontFeature> list = new List<FontFeature>();

			for(int i = 0; i < 10; ++i)
			{
				list.Add(new FontFeature(i.ToString(CultureInfo.InvariantCulture)));
			}

			TestDerived(new FontFeatureCollection(list.ToArray()));
			TestDerived(new FontFeatureCollection(list));
			TestDerived(new FontFeatureCollection((IEnumerable<FontFeature>)list));
		}
#endif
	}
}