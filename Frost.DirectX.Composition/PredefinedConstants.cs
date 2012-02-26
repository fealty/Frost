// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Runtime.InteropServices;

using SharpDX;

namespace Frost.DirectX.Composition
{
	[StructLayout(LayoutKind.Sequential)] internal struct PredefinedConstants : IConstantBufferData
	{
		public const string ShaderText =
			@"
			cbuffer PredefinedConstants : register(b5)
			{
				float2 cx_TargetSize : packoffset(c0);
				float2 cx_TargetTexelSize : packoffset(c0.z);				
			};
		";

		public float SizeX;
		public float SizeY;
		public float TexelSizeX;
		public float TexelSizeY;

		public int ByteSize
		{
			get { return GPUData.SizeOf<PredefinedConstants>(); }
		}

		public void Serialize(DataStream stream)
		{
			stream.Write(SizeX);
			stream.Write(SizeY);

			stream.Write(TexelSizeX);
			stream.Write(TexelSizeY);
		}
	}
}