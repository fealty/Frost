// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Runtime.InteropServices;

using Frost.DirectX.Common;
using Frost.Effects;

using SharpDX;

using SDX = SharpDX;

namespace Frost.DirectX.Composition
{
	internal sealed class ColorOutputEffect : Effect<ColorOutputSettings>, IShaderEffect
	{
		private const string _ShaderText =
			@"
			Texture2D mTexture : register(t0);
			SamplerState mSampler : register(s0);

			cbuffer PSConstants : register(b0)
			{
				float4 mUserColor : packoffset(c0);
			};

			struct PS_IN
			{
				float4 pos : SV_POSITION;
				float2 tex : TEXCOORD;
			};

			float4 ReplaceColor(PS_IN input) : SV_Target
			{
				float4 color = mUserColor * mUserColor.w;

				color.w = mUserColor.w;

				return color;
			}

			float4 ModulateColor(PS_IN input) : SV_Target
			{
				float4 color = mTexture.Sample(mSampler, input.tex);
				
				float alpha = color.w * mUserColor.w;

				color.xyz = mUserColor.xyz * alpha;
	
				color.w = alpha;
				
				return color;
			}
			";

		private ShaderHandle _ModulateShader;
		private ShaderHandle _ReplaceShader;

		public void Compile(IShaderCompiler compiler)
		{
			compiler.Compile(_ShaderText, "ReplaceColor", ref _ReplaceShader);
			compiler.Compile(_ShaderText, "ModulateColor", ref _ModulateShader);
		}

		public override void Apply<TEnum>(
			TEnum batchedItems,
			EffectContext<ColorOutputSettings> effectContext,
			Frost.Composition.Compositor compositionContext)
		{
			Constants constants;

			constants.Color = effectContext.Options.Color.ToColor4();

			compositionContext.WriteConstants(ConstantRegister.One, constants);

			compositionContext.SetShader(
				effectContext.Options.Operation == ColorOperation.Replace ? _ReplaceShader : _ModulateShader);

			foreach(BatchedItem item in batchedItems)
			{
				compositionContext.SaveState();

				try
				{
					compositionContext.Blend = item.Blend;

					Matrix3X2 transform = item.Transformation;

					compositionContext.Transform(ref transform);

					Rectangle srcRegion = item.SourceRegion;
					Rectangle dstRegion = item.DestinationRegion;

					compositionContext.Composite(item.Canvas, srcRegion, dstRegion);
				}
				finally
				{
					compositionContext.RestoreState();
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)] private struct Constants : IConstantBufferData
		{
			public Color4 Color;

			public int ByteSize
			{
				get { return GPUData.SizeOf<Constants>(); }
			}

			public void Serialize(DataStream stream)
			{
				stream.Write(Color);
			}
		}
	}
}