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

	/// <summary>
	///   This interface provides the length of each line to the <see cref="LineBreaker" /> .
	/// </summary>
	[ContractClass(typeof(ILineProviderContract))] internal interface ILineProvider
	{
		/// <summary>
		///   This method provides the length of the line at the given index.
		/// </summary>
		/// <param name="lineIndex"> This parameter indicates the line index or line number. </param>
		/// <returns> This method returns the length of the line at the given index. </returns>
		double ProduceLine(int lineIndex);
	}
}