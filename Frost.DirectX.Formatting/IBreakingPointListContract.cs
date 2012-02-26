using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting.Contracts
{
	[ContractClassFor(typeof(IBreakingPointList))]
	internal abstract class IBreakingPointListContract : IBreakingPointList
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