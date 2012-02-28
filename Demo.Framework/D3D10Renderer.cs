// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.IO;

using Demo.Framework.Properties;

using Frost;
using Frost.DirectX.Common;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;

using Buffer = SharpDX.Direct3D10.Buffer;
using Device1 = SharpDX.Direct3D10.Device1;
using MapFlags = SharpDX.Direct3D10.MapFlags;

namespace Demo.Framework
{
	public sealed class Dx101Renderer : IDisposable
	{
		private const int _Stride = 20;
		private const int _Offset = 0;

		private readonly Effect _Effect;
		private readonly EffectPass _EffectPass;
		private readonly EffectTechnique _EffectTechnique;
		private readonly InputLayout _Layout;
		private readonly Buffer _VertexBuffer;
		private ShaderResourceView _DependentView;
		private IntPtr _SharedHandle;
		private KeyedMutex _SharedMutex;
		private Texture2D _SharedTexture;

		public Dx101Renderer(Device1 renderingDevice)
		{
			RenderingDevice = renderingDevice;

			string shader = GetShaderText(Resources.RenderingEffects);

			ShaderBytecode byteCode = ShaderBytecode.Compile(
				shader, "fx_4_0", ShaderFlags.EnableStrictness, EffectFlags.None);

			// Create the effect.
			_Effect = new Effect(RenderingDevice, byteCode);

			_EffectTechnique = _Effect.GetTechniqueByIndex(0);
			_EffectPass = _EffectTechnique.GetPassByIndex(0);

			// Define the input layout.
			_Layout = new InputLayout(
				RenderingDevice,
				_EffectPass.Description.Signature,
				new[]
				{
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
				});

			_VertexBuffer = CreateVertexBuffer();
		}

		public Device1 RenderingDevice { get; private set; }

		public void Dispose()
		{
			RenderingDevice.ClearState();

			_DependentView.SafeDispose();
			_SharedMutex.SafeDispose();
			_SharedTexture.SafeDispose();
			_VertexBuffer.SafeDispose();
			_Layout.SafeDispose();
			_Effect.SafeDispose();
			RenderingDevice.Dispose();

			RenderingDevice = null;
		}

		public void BeginRendering()
		{
			// Set the input layout.
			RenderingDevice.InputAssembler.InputLayout = _Layout;

			// Set the vertex buffer.
			RenderingDevice.InputAssembler.SetVertexBuffers(
				0, new VertexBufferBinding(_VertexBuffer, _Stride, _Offset));

			// Set the primitive topology.
			RenderingDevice.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		}

		public void Render(Canvas canvas)
		{
			if(canvas == null || !canvas.IsValid)
			{
				return;
			}

			if(_SharedHandle != canvas.Surface2D.GetDeviceHandle())
			{
				_SharedMutex.SafeDispose();
				_DependentView.SafeDispose();
				_SharedTexture.SafeDispose();

				_SharedTexture =
					RenderingDevice.OpenSharedResource<Texture2D>(canvas.Surface2D.GetDeviceHandle());

				_SharedMutex = _SharedTexture.QueryInterface<KeyedMutex>();

				_DependentView = new ShaderResourceView(RenderingDevice, _SharedTexture);

				_SharedHandle = canvas.Surface2D.GetDeviceHandle();
			}

			_SharedMutex.AcquireSync();

			EffectTechnique technique;

			// Get the technique used for the widget.
			if(_SharedTexture != null)
			{
				technique = _Effect.GetTechniqueByName("WithTexture");

				EffectVariable esv = _Effect.GetVariableByName("tex2D").AsShaderResource();

				esv.AsShaderResource().SetResource(_DependentView);
			}
			else
			{
				technique = _Effect.GetTechniqueByName("WithoutTexture");
			}

			// Apply the pass.
			technique.GetPassByIndex(0).Apply();

			// Draw the geometry.
			RenderingDevice.Draw(6, 0);

			_SharedMutex.ReleaseSync();
		}

		/// <inheritdoc />
		public void EndRendering()
		{
		}

		private static string GetShaderText(byte[] pixelShaderTextBytes)
		{
			using(MemoryStream stream = new MemoryStream(pixelShaderTextBytes))
			{
				using(StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private Buffer CreateVertexBuffer()
		{
			Buffer buffer = new Buffer(
				RenderingDevice,
				_Stride * 6,
				ResourceUsage.Dynamic,
				BindFlags.VertexBuffer,
				CpuAccessFlags.Write,
				ResourceOptionFlags.None);

			using(DataStream stream = buffer.Map(MapMode.WriteDiscard, MapFlags.None))
			{
				Vector2[] quadPrimitiveData = new[]
				{
					new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f),
					new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f),
					new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
					new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f)
				};

				for(int i = 0; i < quadPrimitiveData.Length; i += 2)
				{
					float fx = (2.0f * quadPrimitiveData[i + 0].X) - 1.0f;
					float fy = (2.0f * quadPrimitiveData[i + 0].Y) - 1.0f;

					stream.Write(new Vector3(fx, -fy, 0.5f));
					stream.Write(quadPrimitiveData[i + 1]);
				}

				buffer.Unmap();
			}

			return buffer;
		}
	}
}