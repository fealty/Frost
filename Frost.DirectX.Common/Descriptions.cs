// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.Direct3D10;

using FeatureLevel = SharpDX.Direct2D1.FeatureLevel;
using Texture2D = SharpDX.Direct3D10.Texture2DDescription;
using RenderTarget = SharpDX.Direct2D1.RenderTargetProperties;

namespace Frost.DirectX.Common
{
	internal static class Descriptions
	{
		public static readonly BitmapProperties BitmapProperties;
		public static readonly BitmapBrushProperties BitmapBrush;

		public static readonly RenderTarget RenderTarget;

		public static readonly PixelFormat PixelFormat;

		public static readonly Texture2D PrivateTexture;
		public static readonly Texture2D SharedTexture;

		public static readonly SampleDescription SampleDescription;

		public static readonly LinearGradientBrushProperties LinearGradient;
		public static readonly RadialGradientBrushProperties RadialGradient;

		public static readonly Texture2D StagingDescription;

		static Descriptions()
		{
			ConfigurePixelFormat(out PixelFormat);
			ConfigureRenderTarget(out RenderTarget);

			ConfigureSample(out SampleDescription);
			ConfigurePrivateTexture(out PrivateTexture);
			ConfigureSharedTexture(out SharedTexture);

			ConfigureBitmap(out BitmapProperties);
			ConfigureBitmapBrush(out BitmapBrush);

			ConfigureLinearGradient(out LinearGradient);
			ConfigureRadialGradient(out RadialGradient);

			ConfigureStagingDescription(out StagingDescription);
		}

		private static void ConfigureStagingDescription(
			out Texture2D description)
		{
			description.Width = 0;
			description.ArraySize = 1;
			description.MipLevels = 1;
			description.Format = Format.R8G8B8A8_UNorm;
			description.Usage = ResourceUsage.Staging;
			description.BindFlags = BindFlags.None;
			description.SampleDescription = new SampleDescription(1, 0);
			description.OptionFlags = ResourceOptionFlags.None;
			description.CpuAccessFlags = CpuAccessFlags.Write;
			description.Height = 0;
		}

		private static void ConfigureRadialGradient(
			out RadialGradientBrushProperties description)
		{
			description = new RadialGradientBrushProperties();
		}

		private static void ConfigureLinearGradient(
			out LinearGradientBrushProperties description)
		{
			description = new LinearGradientBrushProperties();
		}

		private static void ConfigureBitmapBrush(
			out BitmapBrushProperties description)
		{
			description.InterpolationMode = BitmapInterpolationMode.Linear;
			description.ExtendModeX = ExtendMode.Wrap;
			description.ExtendModeY = ExtendMode.Wrap;
		}

		private static void ConfigureBitmap(
			out BitmapProperties description)
		{
			description.DpiX = 96.0f;
			description.DpiY = 96.0f;
			description.PixelFormat = PixelFormat;
		}

		private static void ConfigurePixelFormat(
			out PixelFormat description)
		{
			description.Format = Format.R8G8B8A8_UNorm;
			description.AlphaMode = AlphaMode.Premultiplied;
		}

		private static void ConfigureRenderTarget(
			out RenderTarget description)
		{
			description.Type = RenderTargetType.Hardware;
			description.PixelFormat = PixelFormat;
			description.DpiX = 96.0f;
			description.DpiY = 96.0f;
			description.Usage = RenderTargetUsage.None;
			description.MinLevel = FeatureLevel.Level_10;
		}

		private static void ConfigureSample(
			out SampleDescription description)
		{
			description.Count = 1;
			description.Quality = 0;
		}

		private static void ConfigurePrivateTexture(
			out Texture2D description)
		{
			BindFlags binding = BindFlags.None;

			binding |= BindFlags.ShaderResource;
			binding |= BindFlags.RenderTarget;

			description.MipLevels = 1;
			description.ArraySize = 1;
			description.Format = Format.R8G8B8A8_UNorm;
			description.Usage = ResourceUsage.Default;
			description.BindFlags = binding;
			description.SampleDescription = SampleDescription;
			description.OptionFlags = ResourceOptionFlags.None;
			description.CpuAccessFlags = CpuAccessFlags.None;
			description.Height = 0;
			description.Width = 0;
		}

		private static void ConfigureSharedTexture(
			out Texture2D description)
		{
			BindFlags binding = BindFlags.None;

			binding |= BindFlags.ShaderResource;
			binding |= BindFlags.RenderTarget;

			description.MipLevels = 1;
			description.ArraySize = 1;
			description.Format = Format.R8G8B8A8_UNorm;
			description.Usage = ResourceUsage.Default;
			description.BindFlags = binding;
			description.SampleDescription = SampleDescription;
			description.OptionFlags = ResourceOptionFlags.None;
			description.CpuAccessFlags = CpuAccessFlags.None;
			description.OptionFlags |= ResourceOptionFlags.SharedKeyedmutex;
			description.Width = 0;
			description.Height = 0;
		}
	}
}