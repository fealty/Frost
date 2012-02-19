// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;

namespace Frost.Formatting
{
	public sealed class TextRunCollection : ImmutableBase<TextRun>
	{
		public TextRunCollection(TextRun[] items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public TextRunCollection(List<TextRun> items) : base(items)
		{
			Contract.Requires(items != null);
		}

		public TextRunCollection(IEnumerable<TextRun> items) : base(items)
		{
			Contract.Requires(items != null);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			List<TextRun> list = new List<TextRun>();

			for(int i = 0; i < 10; ++i)
			{
				list.Add(
					new TextRun(
						IndexedRange.Empty,
						null,
						null,
						FontStretch.Regular,
						FontStyle.Regular,
						FontWeight.Regular,
						i,
						Alignment.Stretch,
						Alignment.Stretch,
						Size.Empty,
						null));
			}

			TestDerived(new TextRunCollection(list.ToArray()));
			TestDerived(new TextRunCollection(list));
			TestDerived(new TextRunCollection((IEnumerable<TextRun>)list));
		}
#endif
	}
}