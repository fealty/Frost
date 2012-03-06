// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.IO;

using SharpDX;
using SharpDX.Direct3D10;

namespace Frost.DirectX.Common
{
	public sealed class StagingTexture : IDisposable
	{
		private readonly Device _Device3D;

		private Texture2DDescription _Description;
		private Texture2D _Texture;

		public StagingTexture(Device device3D)
		{
			Contract.Requires(device3D != null);

			_Device3D = device3D;

			_Description = Descriptions.StagingDescription;
		}

		public void Dispose()
		{
			_Texture.SafeDispose();
		}

		public void UploadData(Size size, byte[] rgbaData)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));
			Contract.Requires(rgbaData != null);
			Contract.Requires(
				rgbaData.Length >= (Convert.ToInt32(size.Width) * Convert.ToInt32(size.Height)) * 4);

			int regionWidth = Convert.ToInt32(size.Width);
			int regionHeight = Convert.ToInt32(size.Height);

			if(regionWidth != _Description.Width || regionHeight != _Description.Height)
			{
				_Description.Width = regionWidth;
				_Description.Height = regionHeight;

				_Texture.SafeDispose();

				_Texture = new Texture2D(_Device3D, _Description);
			}

			DataRectangle data = _Texture.Map(0, MapMode.Write, MapFlags.None);

			try
			{
				int stageSize = data.Pitch * regionHeight;

				using(var stream = new DataStream(data.DataPointer, stageSize, false, true))
				{
					int stride = regionWidth * 4;
					int indexTotal = regionHeight * stride;

					for(int index = 0; index < indexTotal; index += stride)
					{
						stream.Write(rgbaData, index, stride);

						stream.Seek(data.Pitch - stride, SeekOrigin.Current);
					}
				}
			}
			finally
			{
				_Texture.Unmap(0);
			}
		}

		public void CopyTo(Rectangle srcRegion, Canvas.ResolvedContext destination)
		{
			Contract.Requires(destination != null);

			Surface2D dstSurface = (Surface2D)destination.Surface2D;

			int offsetX = Convert.ToInt32(destination.Region.X);
			int offsetY = Convert.ToInt32(destination.Region.Y);

			ResourceRegion sourceRegion = new ResourceRegion
			{
				Front = 0,
				Left = Convert.ToInt32(srcRegion.Left),
				Top = Convert.ToInt32(srcRegion.Top),
				Right = Convert.ToInt32(srcRegion.Right),
				Bottom = Convert.ToInt32(srcRegion.Bottom),
				Back = 1
			};

			try
			{
				destination.Surface2D.AcquireLock();

				_Device3D.CopySubresourceRegion(
					_Texture, 0, sourceRegion, dstSurface.Texture2D, 0, offsetX, offsetY, 0);
			}
			finally
			{
				destination.Surface2D.ReleaseLock();
			}
		}
	}
}