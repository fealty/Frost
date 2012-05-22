// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Surfacing;

using SharpDX;
using SharpDX.Direct3D10;

namespace Frost.DirectX.Common
{
	public static class Extensions
	{
		public static void SafeDispose(this IDisposable disposable)
		{
			if(disposable != null)
			{
				disposable.Dispose();
			}
		}

		internal static ShaderResourceView GetShaderView(this ISurface2D surface2D)
		{
			Contract.Requires(surface2D != null);

			Surface2D surfaceD3D = (Surface2D)surface2D;

			return surfaceD3D.ShaderView;
		}

		internal static RenderTargetView GetRenderTarget(this ISurface2D surface2D)
		{
			Contract.Requires(surface2D != null);

			Surface2D surfaceD3D = (Surface2D)surface2D;

			return surfaceD3D.TargetView;
		}

		public static IntPtr GetDeviceHandle(this Canvas canvas, Device2D device2D)
		{
			Contract.Requires(canvas != null);
			Contract.Requires(device2D != null);

			Canvas.ResolvedContext context = device2D.Resources.ResolveCanvas(canvas);

			if(context != null)
			{
				return ((Surface2D)context.Surface2D).DeviceHandle;
			}

			return IntPtr.Zero;
		}

		public static DrawingSizeF ToSizeF(this Size size)
		{
			DrawingSizeF result = new DrawingSizeF { Width = size.Width, Height = size.Height };

			return result;
		}

		public static Color4 ToColor4(this Color color)
		{
			Color4 result = new Color4 {Red = color.R, Green = color.G, Blue = color.B, Alpha = color.A};

			return result;
		}

		public static DrawingPointF ToPointF(this Point point)
		{
			DrawingPointF result = new DrawingPointF { X = point.X, Y = point.Y };

			return result;
		}
	}
}