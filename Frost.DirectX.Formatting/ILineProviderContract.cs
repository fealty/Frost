using System;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting.Contracts
{
	[ContractClassFor(typeof(ILineProvider))]
	internal abstract class ILineProviderContract : ILineProvider
	{
		public double ProduceLine(int lineIndex)
		{
			Contract.Requires(lineIndex >= 0 && lineIndex <= int.MaxValue);
			Contract.Ensures(
				Contract.Result<double>() >= 0.0 &&
				Contract.Result<double>() <= double.MaxValue);

			throw new NotSupportedException();
		}
	}
}