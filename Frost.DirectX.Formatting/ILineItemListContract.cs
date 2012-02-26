using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting.Contracts
{
	[ContractClassFor(typeof(ILineItemList))]
	internal abstract class ILineItemListContract : ILineItemList
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