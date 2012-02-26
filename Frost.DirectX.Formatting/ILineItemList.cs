using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting
{
	[ContractClass(typeof(Contracts.ILineItemListContract))]
	public interface ILineItemList : IEnumerable<LineItem>
	{
		int Count { get; }

		LineItem this[int index] { get; }

		new List<LineItem>.Enumerator GetEnumerator();
	}
}