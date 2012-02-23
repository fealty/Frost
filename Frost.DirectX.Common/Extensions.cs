// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Drawing;

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

		public static ShaderResourceView GetShaderView(
			this ISurface2D surface2D)
		{
			Contract.Requires(surface2D != null);

			Surface2D surfaceD3D = (Surface2D)surface2D;

			return surfaceD3D.ShaderView;
		}

		public static RenderTargetView GetRenderTarget(
			this ISurface2D surface2D)
		{
			Contract.Requires(surface2D != null);

			Surface2D surfaceD3D = (Surface2D)surface2D;

			return surfaceD3D.TargetView;
		}

		public static IntPtr GetDeviceHandle(this ISurface2D surface2D)
		{
			Contract.Requires(surface2D != null);

			Surface2D surfaceD3D = (Surface2D)surface2D;

			return surfaceD3D.DeviceHandle;
		}

		public static SizeF ToSizeF(this Size size)
		{
			SizeF result = new SizeF {Width = size.Width, Height = size.Height};

			return result;
		}

		public static Color4 ToColor4(this Color color)
		{
			Color4 result = new Color4
			{Red = color.R, Green = color.G, Blue = color.B, Alpha = color.A};

			return result;
		}

		public static PointF ToPointF(this Point point)
		{
			PointF result = new PointF {X = point.X, Y = point.Y};

			return result;
		}
	}
}