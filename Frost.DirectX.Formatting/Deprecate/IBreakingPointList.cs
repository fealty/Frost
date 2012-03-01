// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.DirectX.Formatting.Contracts;

namespace Frost.DirectX.Formatting
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IBreakingPointList))] internal abstract class IBreakingPointListContract
			: IBreakingPointList
		{
			public int Count
			{
				get
				{
					Contract.Ensures(Contract.Result<int>() >= 0);

					throw new NotSupportedException();
				}
			}

			public BreakIndex this[int index]
			{
				get
				{
					Contract.Requires(index >= 0);

					throw new NotSupportedException();
				}
			}

			List<BreakIndex>.Enumerator IBreakingPointList.GetEnumerator()
			{
				throw new NotSupportedException();
			}

			IEnumerator<BreakIndex> IEnumerable<BreakIndex>.GetEnumerator()
			{
				throw new NotSupportedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotSupportedException();
			}
		}
	}

	[ContractClass(typeof(IBreakingPointListContract))] internal interface IBreakingPointList
		: IEnumerable<BreakIndex>
	{
		int Count { get; }

		BreakIndex this[int index] { get; }

		new List<BreakIndex>.Enumerator GetEnumerator();
	}
}