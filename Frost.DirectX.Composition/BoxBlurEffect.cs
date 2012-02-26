// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Runtime.InteropServices;

using Frost.Composition;
using Frost.Effects;

using SharpDX;

using SDX = SharpDX;

namespace Frost.DirectX.Composition
{
	internal sealed class BoxBlurEffect : Effect<BoxBlurSettings>, IShaderEffect
	{
		private const string _ShaderText =
			@"
			Texture2D mTexture : register(t0);
			SamplerState mSampler : register(s0);

			cbuffer PSConstants : register(b0)
			{
				float2 mBlurWeight : packoffset(c0);
				float2 mBlurRange : packoffset(c0.z);
			};

			struct PS_IN
			{
				float4 pos : SV_POSITION;
				float2 tex : TEXCOORD;
			};

			float4 BoxHorizontalBlur(PS_IN input) : SV_Target
			{
				float4 sum = float4(0.0f, 0.0f, 0.0f, 0.0f);
				
				for(float i = -mBlurRange.x; i < mBlurRange.x; i += 1.0f)
				{
					sum += mTexture.Sample(mSampler, float2(
						input.tex.x + (ceil(i) * cx_TargetTexelSize.x), input.tex.y)) * mBlurWeight.x;
				}

				return sum;
			}

			float4 BoxVerticalBlur(PS_IN input) : SV_Target
			{
				float4 sum = float4(0.0f, 0.0f, 0.0f, 0.0f);

				for(float i = -mBlurRange.y; i < mBlurRange.y; i += 1.0f)
				{
					sum += mTexture.Sample(mSampler, float2(
						input.tex.x, input.tex.y + (ceil(i) * cx_TargetTexelSize.y))) * mBlurWeight.y;
				}

				return sum;
			}
			";

		private ShaderHandle _HorizontalBlur;
		private ShaderHandle _VerticalBlur;

		void IShaderEffect.Compile(IShaderCompiler compiler)
		{
			compiler.Compile(_ShaderText, "BoxHorizontalBlur", ref _HorizontalBlur);
			compiler.Compile(_ShaderText, "BoxVerticalBlur", ref _VerticalBlur);
		}

		public override void Apply<TEnum>(
			TEnum batchedItems,
			EffectContext<BoxBlurSettings> effectContext,
			Frost.Composition.Compositor compositionContext)
		{
			BoxBlurSettings settings = effectContext.Options;

			double blurRadiusX =
				Math.Floor(settings.Amount.Width * 3.0 * Math.Sqrt(2.0 * Math.PI) / 4.0 + 0.5);
			double blurRadiusY =
				Math.Floor(settings.Amount.Height * 3.0 * Math.Sqrt(2.0 * Math.PI) / 4.0 + 0.5);

			blurRadiusX = Math.Max(1.0, blurRadiusX);
			blurRadiusY = Math.Max(1.0, blurRadiusY);

			Constants constants;

			constants.BlurWeightX = Convert.ToSingle(1.0f / blurRadiusX);
			constants.BlurWeightY = Convert.ToSingle(1.0f / blurRadiusY);

			constants.BlurRangeX = Convert.ToSingle(blurRadiusX / 2.0f);
			constants.BlurRangeY = Convert.ToSingle(blurRadiusY / 2.0f);

			Matrix3X2 originalTransform = compositionContext.Transformation;

			compositionContext.Transformation = Matrix3X2.Identity;

			compositionContext.Translate(-constants.BlurRangeX, -constants.BlurRangeY);

			compositionContext.PushLayer();
			compositionContext.ResetState();

			///////////////////////////////////////////////////////////
			foreach(BatchedItem item in batchedItems)
			{
				compositionContext.Blend = item.Blend;

				compositionContext.Transformation = originalTransform;

				Matrix3X2 transform = item.Transformation;

				compositionContext.Transform(ref transform);

				Rectangle srcRegion = item.SourceRegion;
				Rectangle dstRegion = item.DestinationRegion;

				compositionContext.Composite(item.Canvas, srcRegion, dstRegion);
			}
			///////////////////////////////////////////////////////////

			compositionContext.ResetState();

			compositionContext.WriteConstants(ConstantRegister.One, constants);

			///////////////////////////////////////////////////////////
			for(int i = 0; i < settings.PassCount; ++i)
			{
				if(settings.Amount.Width > 0.0)
				{
					compositionContext.Blend = BlendOperation.Copy;
					compositionContext.SetShader(_HorizontalBlur);

					compositionContext.CompositeResult();
				}

				if(settings.Amount.Height > 0.0)
				{
					compositionContext.Blend = BlendOperation.Copy;
					compositionContext.SetShader(_VerticalBlur);

					compositionContext.CompositeResult();
				}
			}
			//////////////////////////////////////////////////////////

			compositionContext.PopLayer();
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)] private struct Constants : IConstantBufferData
		{
			public float BlurRangeX;
			public float BlurRangeY;
			public float BlurWeightX;
			public float BlurWeightY;

			public int ByteSize
			{
				get { return GPUData.SizeOf<Constants>(); }
			}

			public void Serialize(DataStream stream)
			{
				stream.Write(BlurWeightX);
				stream.Write(BlurWeightY);

				stream.Write(BlurRangeX);
				stream.Write(BlurRangeY);
			}
		}
	}
}