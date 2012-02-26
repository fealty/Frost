// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D10;

namespace Frost.DirectX.Composition
{
	[StructLayout(LayoutKind.Sequential)] internal struct VertexPositionTexture
	{
		public static readonly InputElement[] InputElements;

		public static readonly int ByteSize;

		public static readonly int LayoutOffset;
		public static readonly int LayoutStride;

		static VertexPositionTexture()
		{
			LayoutOffset = 0;

			InputElements = new[]
			{
				new InputElement(
					"POSITION", 0, Format.R32G32_Float, GPUData.OffsetOf<VertexPositionTexture>(e => e.Position), 0)
				,
				new InputElement(
					"TEXCOORD", 0, Format.R32G32_Float, GPUData.OffsetOf<VertexPositionTexture>(e => e.TexCoord), 0)
			};

			ByteSize = GPUData.SizeOf<VertexPositionTexture>();

			LayoutStride = ByteSize;
		}

		public VertexPositionTexture(Vector2 position, Vector2 texCoord)
		{
			Position = position;
			TexCoord = texCoord;
		}

		public VertexPositionTexture(float x, float y, float u, float v)
		{
			Position = new Vector2(x, y);
			TexCoord = new Vector2(u, v);
		}

		public Vector2 Position;
		public Vector2 TexCoord;
	}
}