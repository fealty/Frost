// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Effects;

namespace Frost.DirectX.Composition
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IShaderEffect))] internal abstract class IShaderEffectContract
			: IShaderEffect
		{
			public void Compile(IShaderCompiler compiler)
			{
				Contract.Requires(compiler != null);
			}

			public abstract void Apply<T>(
				T batchedItems, IEffectContext effectContext, Cabbage.Compositor compositionContext)
				where T : class, IEnumerable<BatchedItem>;
		}
	}

	[ContractClass(typeof(Contracts.IShaderEffectContract))] public interface IShaderEffect : IEffect
	{
		void Compile(IShaderCompiler compiler);
	}
}