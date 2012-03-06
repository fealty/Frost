// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Drawing;

using Frost;
using Frost.Surfacing;

using Color = System.Drawing.Color;
using Size = Frost.Size;

namespace Demo.Framework
{
	public static class Resources
	{
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
							Color pixel = bitmap.GetPixel(x, y);

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

			device2D.Copy(rgbaData, canvas);

			return canvas;
		}
	}
}