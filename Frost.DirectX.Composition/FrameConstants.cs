// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Runtime.InteropServices;

using SharpDX;

namespace Frost.DirectX.Composition
{
	[StructLayout(LayoutKind.Sequential)] internal struct FrameConstants
	{
		public static readonly int ByteSize;

		static FrameConstants()
		{
			ByteSize = GPUData.SizeOf<FrameConstants>();
		}

		public Vector2 ViewportSize;
		public Vector2 Reserved;
		public Matrix ViewTransform;
	}
}