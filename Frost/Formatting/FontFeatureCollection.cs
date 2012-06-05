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
	}
}