// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Composition;

namespace Frost.Effects
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IEffect))] internal abstract class
			IEffectContract : IEffect
		{
			public void Apply<T>(
				T batchedItems,
				IEffectContext effectContext,
				Compositor compositionContext)
				where T : class, IEnumerable<BatchedItem>
			{
				Contract.Requires(batchedItems != null);
				Contract.Requires(effectContext != null);
				Contract.Requires(compositionContext != null);
			}
		}
	}

	[ContractClass(typeof(Contracts.IEffectContract))] internal interface
		IEffect
	{
		void Apply<T>(
			T batchedItems,
			IEffectContext effectContext,
			Compositor compositionContext)
			where T : class, IEnumerable<BatchedItem>;
	}
}