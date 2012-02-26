// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Runtime.InteropServices;

using SharpDX;

namespace Frost.DirectX.Composition
{
	[StructLayout(LayoutKind.Sequential)] internal struct RenderConstants
	{
		public static readonly int ByteSize;

		static RenderConstants()
		{
			ByteSize = GPUData.SizeOf<RenderConstants>();
		}

		public Vector4 TextureRegion;
		public Vector4 DrawnRegion;
		public Matrix Transform;
	}
}