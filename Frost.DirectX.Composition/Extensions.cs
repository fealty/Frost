// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

namespace Frost.DirectX.Composition
{
	public static class Extensions
	{
		public static void WriteConstants<T>(
			this Frost.Composition.Compositor compositor, ConstantRegister register, T value)
			where T : struct, IConstantBufferData
		{
			Contract.Requires(compositor != null);

			Compositor compositionContext = compositor as Compositor;

			if(compositionContext != null)
			{
				compositionContext.SetConstants(register, ref value);
			}
		}

		public static void SetShader(this Frost.Composition.Compositor compositor, ShaderHandle? shader)
		{
			Contract.Requires(compositor != null);

			Compositor compositionContext = compositor as Compositor;

			if(compositionContext != null)
			{
				compositionContext.Shader = shader;
			}
		}
	}
}