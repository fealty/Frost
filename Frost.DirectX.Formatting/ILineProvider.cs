// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.DirectX.Formatting.Contracts;

namespace Frost.DirectX.Formatting
{
	namespace Contracts
	{
		[ContractClassFor(typeof(ILineProvider))] internal abstract class ILineProviderContract
			: ILineProvider
		{
			public double ProduceLine(int lineIndex)
			{
				Contract.Requires(lineIndex >= 0);
				Contract.Ensures(Check.IsPositive(Contract.Result<double>()));

				throw new NotSupportedException();
			}
		}
	}

	[ContractClass(typeof(ILineProviderContract))] internal interface ILineProvider
	{
		double ProduceLine(int lineIndex);
	}
}