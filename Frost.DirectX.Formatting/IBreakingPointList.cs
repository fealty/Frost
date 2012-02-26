using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting
{
	[ContractClass(typeof(Contracts.IBreakingPointListContract))]
	public interface IBreakingPointList : IEnumerable<BreakIndex>
	{
		int Count { get; }

		BreakIndex this[int index] { get; }

		new List<BreakIndex>.Enumerator GetEnumerator();
	}
}