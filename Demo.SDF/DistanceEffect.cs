// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

using Frost;
using Frost.DirectX.Composition;
using Frost.Effects;

using Compositor = Frost.Composition.Compositor;

namespace Demo.SDF
{
	internal struct DistanceEffectSettings : IEffectSettings, IEquatable<DistanceEffectSettings>
	{
		public bool Equals(DistanceEffectSettings other)
		{
			return true;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is DistanceEffectSettings && Equals((DistanceEffectSettings)obj);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public static bool operator ==(DistanceEffectSettings left, DistanceEffectSettings right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DistanceEffectSettings left, DistanceEffectSettings right)
		{
			return !left.Equals(right);
		}
	}

	internal sealed class DistanceFieldEffect : Effect<DistanceEffectSettings>, IShaderEffect
	{
		private const string _ShaderText =
			@"
			Texture2D mTexture : register(t0);
			SamplerState mSampler : register(s0);

			struct PS_IN
			{
				float4 pos : SV_POSITION;
				float2 tex : TEXCOORD;
			};

			float4 DrawText(PS_IN input) : SV_Target
			{
				float center = 0.5;

				float4 rgba = mTexture.Sample(mSampler, input.tex);

				float alpha = rgba.a;

				float delta = (abs(ddx(alpha)) + abs(ddy(alpha))) * 0.65;

				return float4(float3(0.0, 0.0, 0.0), smoothstep(center-delta,center+delta,alpha));
			}
			";

		private ShaderHandle _ReplaceShader;

		public void Compile(IShaderCompiler compiler)
		{
			compiler.Compile(_ShaderText, "DrawText", ref _ReplaceShader);
		}

		public override void Apply<TEnum>(
			TEnum batchedItems,
			EffectContext<DistanceEffectSettings> effectContext,
			Compositor compositionContext)
		{
			compositionContext.SetShader(_ReplaceShader);

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
	}
}