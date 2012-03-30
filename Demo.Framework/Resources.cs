// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Drawing;

using Frost;
using Frost.Surfacing;

using Color = Frost.Color;
using Size = Frost.Size;

namespace Demo.Framework
{
	public static class Resources
	{
		public static readonly Color Background = new RGBColor(228, 228, 228);
		public static readonly Color Foreground = new RGBColor(47, 43, 59);

		internal static readonly Color UIColor = new RGBColor(205, 203, 209);
		internal static readonly Color InactiveButton = new RGBColor(228, 228, 228);
		internal static readonly Color ActiveButton = new RGBColor(153, 148, 166);

		public static readonly Color FrostColor = new RGBColor(185, 217, 231);

		public static Canvas CreateIcon(Device2D device2D)
		{
			Contract.Requires(device2D != null);

			int width;
			int height;

			byte[] rgbaData;

			using(Icon icon = new Icon(Properties.Resources.frost_icon, 256, 256))
			{
				using(Bitmap bitmap = icon.ToBitmap())
				{
					width = bitmap.Width;
					height = bitmap.Height;

					rgbaData = new byte[width * height * 4];

					int index = 0;

					for(int y = 0; y < height; ++y)
					{
						for(int x = 0; x < width; ++x)
						{
							System.Drawing.Color pixel = bitmap.GetPixel(x, y);

							float alpha = pixel.A / 255.0f;

							rgbaData[index + 0] = Convert.ToByte(pixel.R * alpha);
							rgbaData[index + 1] = Convert.ToByte(pixel.G * alpha);
							rgbaData[index + 2] = Convert.ToByte(pixel.B * alpha);
							rgbaData[index + 3] = pixel.A;

							index += 4;
						}
					}
				}
			}

			Canvas canvas = new Canvas(new Size(width, height), SurfaceUsage.Normal);

			device2D.Resources.Copy(rgbaData, canvas);

			return canvas;
		}
	}
}