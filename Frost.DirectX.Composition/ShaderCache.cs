// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.DirectX.Common.Diagnostics;

using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;

namespace Frost.DirectX.Composition
{
	internal sealed class ShaderCache : IShaderCompiler, IDisposable
	{
		private readonly IntegerCounter _CachedShaderCount = new IntegerCounter(
			"Composition", "CachedShaders");

		private readonly Device _Device3D;

		private readonly List<PixelShader> _Shaders;

		public ShaderCache(Device device3D, Device2D device2D)
		{
			Contract.Requires(device3D != null);
			Contract.Requires(device2D != null);

			device2D.Diagnostics.Register(_CachedShaderCount);

			_Shaders = new List<PixelShader>();

			_Device3D = device3D;
		}

		public IntegerCounter CachedShaderCount
		{
			get { return _CachedShaderCount; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		void IShaderCompiler.CompileDefaultShader(string text, string function, ref ShaderHandle result)
		{
			if(!result.IsValid || result.Reference != this)
			{
				Compile(text, function, out result);
			}
		}

		public PixelShader Resolve(ShaderHandle shader)
		{
			if(shader.IsValid && shader.Reference == this)
			{
				return _Shaders[shader.Index];
			}

			return null;
		}

		private void Compile(string text, string function, out ShaderHandle result)
		{
			Contract.Requires(!String.IsNullOrEmpty(text));
			Contract.Requires(!String.IsNullOrEmpty(function));

			const ShaderFlags flags = ShaderFlags.OptimizationLevel3;

			text = text.Insert(0, PredefinedConstants.ShaderText);

			using(var byteCode = ShaderBytecode.Compile(text, function, "ps_4_0", flags))
			{
				PixelShader shader = new PixelShader(_Device3D, byteCode);

				try
				{
					_Shaders.Add(shader);
				}
				catch
				{
					shader.Dispose();

					throw;
				}

				_CachedShaderCount.Value = _Shaders.Count;
			}

			result = new ShaderHandle(_Shaders.Count - 1, this);
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_Shaders.ForEach(item => item.Dispose());

				_Shaders.Clear();
			}
		}
	}
}