// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class TextRunCollection : ImmutableBase<TextRun>
	{
		public TextRunCollection(TextRun[] items) : base(items)
		{
		}

		public TextRunCollection(List<TextRun> items) : base(items)
		{
		}

		public TextRunCollection(IEnumerable<TextRun> items) : base(items)
		{
		}
	}
}