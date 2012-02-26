// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

using Frost.DirectX.Composition.Contracts;

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
		}
	}

	[ContractClass(typeof(IShaderEffectContract))] public interface IShaderEffect
	{
		void Compile(IShaderCompiler compiler);
	}
}