// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using Frost.DirectX.Common;

using SharpDX.Direct3D10;

namespace Frost.DirectX.Composition
{
	internal static class Descriptions
	{
		public static readonly BlendStateDescription SourceOverBlend;
		public static readonly BlendStateDescription SourceInBlend;
		public static readonly BlendStateDescription SourceOutBlend;
		public static readonly BlendStateDescription SourceAtopBlend;
		public static readonly BlendStateDescription DestinationOverBlend;
		public static readonly BlendStateDescription DestinationInBlend;
		public static readonly BlendStateDescription DestinationOutBlend;
		public static readonly BlendStateDescription DestinationAtopBlend;
		public static readonly BlendStateDescription LighterBlend;
		public static readonly BlendStateDescription CopyBlend;

		public static readonly BufferDescription ConstantsPerRender;
		public static readonly BufferDescription ConstantsPerFrame;

		public static readonly SamplerStateDescription LinearSampler;

		public static readonly RasterizerStateDescription Rasterizer;

		public static readonly BufferDescription VertexBuffer;

		public static readonly BufferDescription CustomConstants;

		static Descriptions()
		{
			ConfigureSourceOverBlend(out SourceOverBlend);
			ConfigureSourceInBlend(out SourceInBlend);
			ConfigureSourceOutBlend(out SourceOutBlend);
			ConfigureSourceAtopBlend(out SourceAtopBlend);
			ConfigureDestinationOverBlend(out DestinationOverBlend);
			ConfigureDestinationInBlend(out DestinationInBlend);
			ConfigureDestinationOutBlend(out DestinationOutBlend);
			ConfigureDestinationAtopBlend(out DestinationAtopBlend);
			ConfigureLighterBlend(out LighterBlend);
			ConfigureCopyBlend(out CopyBlend);

			ConfigureConstantsPerRender(out ConstantsPerRender);
			ConfigureConstantsPerFrame(out ConstantsPerFrame);

			ConfigureLinearSampler(out LinearSampler);
			ConfigureRasterizer(out Rasterizer);
			ConfigureVertexBuffer(out VertexBuffer);
			ConfigureCustomConstants(out CustomConstants);
		}

		private static void ConfigureCustomConstants(out BufferDescription description)
		{
			description.Usage = ResourceUsage.Dynamic;
			description.BindFlags = BindFlags.ConstantBuffer;
			description.CpuAccessFlags = CpuAccessFlags.Write;
			description.OptionFlags = ResourceOptionFlags.None;
			description.SizeInBytes = 0;
		}

		private static void ConfigureVertexBuffer(out BufferDescription description)
		{
			description.Usage = ResourceUsage.Immutable;
			description.BindFlags = BindFlags.VertexBuffer;
			description.CpuAccessFlags = CpuAccessFlags.None;
			description.OptionFlags = ResourceOptionFlags.None;
			description.SizeInBytes = 0;
		}

		private static void ConfigureRasterizer(out RasterizerStateDescription description)
		{
			description = new RasterizerStateDescription
			{
				CullMode = CullMode.None,
				FillMode = FillMode.Solid,
				IsFrontCounterClockwise = false,
				DepthBias = 0,
				DepthBiasClamp = 0,
				SlopeScaledDepthBias = 0,
				IsScissorEnabled = false,
				IsMultisampleEnabled = false,
				IsAntialiasedLineEnabled = false,
				IsDepthClipEnabled = false
			};
		}

		private static void ConfigureLinearSampler(out SamplerStateDescription description)
		{
			description.Filter = Filter.MinMagMipLinear;
			description.AddressU = TextureAddressMode.Clamp;
			description.AddressV = TextureAddressMode.Clamp;
			description.AddressW = TextureAddressMode.Clamp;
			description.BorderColor = Color.Transparent.ToColor4();
			description.ComparisonFunction = Comparison.Never;
			description.MaximumAnisotropy = 16;
			description.MaximumLod = float.MaxValue;
			description.MinimumLod = 0.0f;
			description.MipLodBias = 0.0f;
		}

		private static void ConfigureConstantsPerRender(out BufferDescription description)
		{
			description.Usage = ResourceUsage.Dynamic;
			description.BindFlags = BindFlags.ConstantBuffer;
			description.CpuAccessFlags = CpuAccessFlags.Write;
			description.OptionFlags = ResourceOptionFlags.None;
			description.SizeInBytes = RenderConstants.ByteSize * Renderer.BatchItemCount;
		}

		private static void ConfigureConstantsPerFrame(out BufferDescription description)
		{
			description.Usage = ResourceUsage.Dynamic;
			description.BindFlags = BindFlags.ConstantBuffer;
			description.CpuAccessFlags = CpuAccessFlags.Write;
			description.OptionFlags = ResourceOptionFlags.None;
			description.SizeInBytes = FrameConstants.ByteSize;
		}

		private static void ConfigureSourceOverBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.InverseSourceAlpha,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
				AlphaBlendOperation = BlendOperation.Add
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureSourceInBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.Zero,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.DestinationAlpha,
				DestinationAlphaBlend = BlendOption.InverseDestinationAlpha,
				AlphaBlendOperation = BlendOperation.Add
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureSourceOutBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.Zero,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.DestinationAlpha,
				AlphaBlendOperation = BlendOperation.Subtract
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureSourceAtopBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.DestinationAlpha,
				DestinationBlend = BlendOption.InverseSourceAlpha,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.DestinationAlpha,
				DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
				AlphaBlendOperation = BlendOperation.Add
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureDestinationOverBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.InverseDestinationAlpha,
				DestinationBlend = BlendOption.One,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.InverseDestinationAlpha,
				DestinationAlphaBlend = BlendOption.One,
				AlphaBlendOperation = BlendOperation.Add
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureDestinationInBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.Zero,
				DestinationBlend = BlendOption.One,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.Zero,
				DestinationAlphaBlend = BlendOption.SourceAlpha,
				AlphaBlendOperation = BlendOperation.Add
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureDestinationOutBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.Zero,
				DestinationBlend = BlendOption.One,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.SourceAlpha,
				DestinationAlphaBlend = BlendOption.One,
				AlphaBlendOperation = BlendOperation.ReverseSubtract
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureDestinationAtopBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.One,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.Zero,
				AlphaBlendOperation = BlendOperation.Add
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureLighterBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.One,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.One,
				AlphaBlendOperation = BlendOperation.Add
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}

		private static void ConfigureCopyBlend(out BlendStateDescription description)
		{
			description = new BlendStateDescription
			{
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.Zero,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.Zero,
				AlphaBlendOperation = BlendOperation.Add
			};

			description.IsBlendEnabled[0] = true;
			description.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
		}
	}
}