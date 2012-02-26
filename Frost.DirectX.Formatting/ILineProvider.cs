using System.Diagnostics.Contracts;

namespace Cabbage.Formatting
{
	[ContractClass(typeof(Contracts.ILineProviderContract))]
	public interface ILineProvider
	{
		double ProduceLine(int lineIndex);
	}
}