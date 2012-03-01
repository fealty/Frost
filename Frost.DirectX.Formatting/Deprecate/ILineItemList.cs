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
		[ContractClassFor(typeof(ILineItemList))] internal abstract class ILineItemListContract
			: ILineItemList
		{
			public int Count
			{
				get
				{
					Contract.Ensures(Contract.Result<int>() >= 0);

					throw new NotSupportedException();
				}
			}

			public LineItem this[int index]
			{
				get
				{
					Contract.Requires(index >= 0);

					throw new NotSupportedException();
				}
			}

			List<LineItem>.Enumerator ILineItemList.GetEnumerator()
			{
				throw new NotSupportedException();
			}

			IEnumerator<LineItem> IEnumerable<LineItem>.GetEnumerator()
			{
				throw new NotSupportedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotSupportedException();
			}
		}
	}

	[ContractClass(typeof(ILineItemListContract))] internal interface ILineItemList
		: IEnumerable<LineItem>
	{
		int Count { get; }

		LineItem this[int index] { get; }

		new List<LineItem>.Enumerator GetEnumerator();
	}
}