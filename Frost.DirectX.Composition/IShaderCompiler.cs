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
		[ContractClassFor(typeof(IShaderCompiler))] internal abstract class IShaderCompilerContract
			: IShaderCompiler
		{
			public void Compile(string text, string function, ref ShaderHandle result)
			{
				Contract.Requires(!string.IsNullOrEmpty(text));
				Contract.Requires(!string.IsNullOrEmpty(function));
			}
		}
	}

	[ContractClass(typeof(IShaderCompilerContract))] public interface IShaderCompiler
	{
		void Compile(string text, string function, ref ShaderHandle result);
	}
}