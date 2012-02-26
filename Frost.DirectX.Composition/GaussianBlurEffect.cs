// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Runtime.InteropServices;

using Frost.Effects;

using SharpDX;

using SDX = SharpDX;

namespace Frost.DirectX.Composition
{
	internal sealed class GaussianBlurEffect : Effect<GaussianBlurSettings>, IShaderEffect
	{
		private const string ShaderText =
			@"
			Texture2D mTexture : register(t0);
			SamplerState mSampler : register(s0);

			cbuffer PSConstants : register(b0)
			{
				float2 mBlurRange : packoffset(c0);
				float2 mBlurSigma : packoffset(c0.z);
			};

			struct PS_IN
			{
				float4 pos : SV_POSITION;
				float2 tex : TEXCOORD;
			};

			static const float PI = 3.14159265f;

			float ComputeWeight(int sampleDist, float sigma)
			{
				float sigmaSquared = sigma * sigma;
				float distanceSquared = sampleDist * sampleDist;

				return exp(-distanceSquared / (2.0 * sigmaSquared)) / sqrt(2.0 * PI * sigmaSquared);
			}

			float4 HorizontalBlur(PS_IN input) : SV_Target
			{
				float4 sum = float4(0.0f, 0.0f, 0.0f, 0.0f);

				for(float i = -mBlurRange.x; i < mBlurRange.x; i += 1.0f)
				{
					float weight = ComputeWeight(i, mBlurSigma.x);

					sum += mTexture.Sample(mSampler, float2(input.tex.x + (ceil(i) * cx_TargetTexelSize.x), input.tex.y)) * weight;
				}

				return sum;
			}

			float4 VerticalBlur(PS_IN input) : SV_Target
			{
				float4 sum = float4(0.0f, 0.0f, 0.0f, 0.0f);

				for(float i = -mBlurRange.y; i < mBlurRange.y; i += 1.0)
				{
					float weight = ComputeWeight(i, mBlurSigma.y);

					sum += mTexture.Sample(mSampler, float2(input.tex.x, input.tex.y + (ceil(i) * cx_TargetTexelSize.y))) * weight;
				}

				return sum;
			}
			";

		private ShaderHandle mHorizontalBlur;
		private ShaderHandle mVerticalBlur;

		public void Compile(IShaderCompiler compiler)
		{
			compiler.Compile(ShaderText, "HorizontalBlur", ref mHorizontalBlur);
			compiler.Compile(ShaderText, "VerticalBlur", ref mVerticalBlur);
		}

		public override void Apply<TEnum>(
			TEnum batchedItems,
			EffectContext<GaussianBlurSettings> effectContext,
			Cabbage.Compositor compositionContext)
		{
			GaussianBlurSettings settings = effectContext.Options;

			double blurRadiusX = Math.Max(6.0f, settings.Amount.Width * 6.0f);
			double blurRadiusY = Math.Max(6.0f, settings.Amount.Height * 6.0f);

			Constants constants;

			constants.BlurRangeX = Convert.ToSingle(blurRadiusX / 2.0f);
			constants.BlurRangeY = Convert.ToSingle(blurRadiusY / 2.0f);

			constants.BlurSigmaX = Convert.ToSingle(settings.Amount.Width);
			constants.BlurSigmaY = Convert.ToSingle(settings.Amount.Height);

			Matrix3x2 originalTransform = compositionContext.Transformation;

			compositionContext.Transform(ref Matrix3x2.Identity, TransformMode.Replace);

			compositionContext.PushLayer();
			compositionContext.ResetState();

			///////////////////////////////////////////////////////////
			foreach(BatchedItem item in batchedItems)
			{
				compositionContext.Blend = item.Blend;

				compositionContext.Transform(ref originalTransform, TransformMode.Replace);

				Matrix3x2 transform = item.Transformation;

				compositionContext.Transform(ref transform);

				Rectangle srcRegion = item.SourceRegion;
				Rectangle dstRegion = item.DestinationRegion;

				compositionContext.Composite(item.Canvas, ref srcRegion, ref dstRegion);
			}
			///////////////////////////////////////////////////////////

			compositionContext.ResetState();

			compositionContext.WriteConstants(ConstantRegister.One, constants);

			// OPTIMIZATION: transform each corner of the destination region by
			// the originalTransform
			// determine the bounding region from those four transformed corners
			// composite only the bounding region

			///////////////////////////////////////////////////////////
			if(settings.Amount.Width > 0.0)
			{
				compositionContext.Blend = BlendOperation.Copy;
				compositionContext.SetShader(mHorizontalBlur);

				// composite the entire result to account for any transformation
				compositionContext.CompositeResult();
			}

			if(settings.Amount.Height > 0.0)
			{
				compositionContext.Blend = BlendOperation.Copy;
				compositionContext.SetShader(mVerticalBlur);

				// composite the entire result to account for any transformation
				compositionContext.CompositeResult();
			}
			///////////////////////////////////////////////////////////

			compositionContext.PopLayer();
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)] private struct Constants : IConstantBufferData
		{
			public float BlurRangeX;
			public float BlurRangeY;
			public float BlurSigmaX;
			public float BlurSigmaY;

			public int ByteSize
			{
				get { return GPUData.SizeOf<Constants>(); }
			}

			public void Serialize(DataStream stream)
			{
				stream.Write(BlurRangeX);
				stream.Write(BlurRangeY);

				stream.Write(BlurSigmaX);
				stream.Write(BlurSigmaY);
			}
		}
	}
}