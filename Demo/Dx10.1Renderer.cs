// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Drawing;
using System.IO;

using Demo.Properties;

using Frost;
using Frost.DirectX.Common;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;

using Yuki.Drawing;

using Buffer = SharpDX.Direct3D10.Buffer;
using Device1 = SharpDX.Direct3D10.Device1;
using MapFlags = SharpDX.Direct3D10.MapFlags;
using RectangleF = System.Drawing.RectangleF;

namespace Demo
{
	/// <summary>
	///   The Dx101Renderer class provides a Direct3d 10.1 renderer.
	/// </summary>
	public sealed class Dx101Renderer : IDisposable
	{
		private const int Stride = 20;
		private const int Offset = 0;

		private static readonly IPrimitive mDefaultQuad;
		private readonly Effect mEffect;
		private readonly EffectPass mEffectPass;
		private readonly EffectTechnique mEffectTechnique;
		private readonly InputLayout mLayout;
		private readonly Buffer mVertexBuffer;
		private ShaderResourceView mDependentView;
		private IntPtr mSharedHandle;
		private KeyedMutex mSharedMutex;
		private Texture2D mSharedTexture;

		/// <summary>
		///   Initializes the class.
		/// </summary>
		static Dx101Renderer()
		{
			mDefaultQuad = new QuadPrimitive();
		}

		/// <summary>
		///   Initializes a new instance of the <see cref="Dx101Renderer" /> class.
		/// </summary>
		/// <param name="renderingDevice"> The rendering device. </param>
		public Dx101Renderer(Device1 renderingDevice)
		{
			RenderingDevice = renderingDevice;

			string shader = GetShaderText(Resources.RenderingEffects);

			ShaderBytecode byteCode = ShaderBytecode.Compile(
				shader, "fx_4_0", ShaderFlags.EnableStrictness, EffectFlags.None);

			// Create the effect.
			mEffect = new Effect(RenderingDevice, byteCode);

			mEffectTechnique = mEffect.GetTechniqueByIndex(0);
			mEffectPass = mEffectTechnique.GetPassByIndex(0);

			// Define the input layout.
			mLayout = new InputLayout(
				RenderingDevice,
				mEffectPass.Description.Signature,
				new[]
				{
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
				});

			mVertexBuffer = CreateVertexBuffer();
		}

		/// <inheritdoc />
		public Device1 RenderingDevice { get; private set; }

		/// <inheritdoc />
		public SizeF TargetResolution { get; set; }

		/// <inheritdoc />
		public void Dispose()
		{
			RenderingDevice.ClearState();

			mDependentView.SafeDispose();
			mSharedMutex.SafeDispose();
			mSharedTexture.SafeDispose();
			mVertexBuffer.SafeDispose();
			mLayout.SafeDispose();
			mEffect.SafeDispose();
			RenderingDevice.Dispose();

			RenderingDevice = null;
		}

		/// <inheritdoc />
		public void BeginRendering()
		{
			// Set the input layout.
			RenderingDevice.InputAssembler.InputLayout = mLayout;

			// Set the vertex buffer.
			RenderingDevice.InputAssembler.SetVertexBuffers(
				0, new VertexBufferBinding(mVertexBuffer, Stride, Offset));

			// Set the primitive topology.
			RenderingDevice.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		}

		/// <inheritdoc />
		public void Render(Canvas canvas, RectangleF globalRect)
		{
			if(canvas == null || !canvas.IsValid)
			{
				return;
			}

			if(mSharedHandle != canvas.Surface2D.GetDeviceHandle())
			{
				mSharedMutex.SafeDispose();
				mDependentView.SafeDispose();
				mSharedTexture.SafeDispose();

				mSharedTexture =
					RenderingDevice.OpenSharedResource<Texture2D>(canvas.Surface2D.GetDeviceHandle());

				mSharedMutex = mSharedTexture.QueryInterface<KeyedMutex>();

				mDependentView = new ShaderResourceView(RenderingDevice, mSharedTexture);

				mSharedHandle = canvas.Surface2D.GetDeviceHandle();
			}

			mSharedMutex.AcquireSync();

			EffectTechnique technique;

			// Get the technique used for the widget.
			if(mSharedTexture != null)
			{
				technique = mEffect.GetTechniqueByName("WithTexture");

				EffectVariable esv = mEffect.GetVariableByName("tex2D").AsShaderResource();

				esv.AsShaderResource().SetResource(mDependentView);
			}
			else
			{
				technique = mEffect.GetTechniqueByName("WithoutTexture");
			}

			// Apply the pass.
			technique.GetPassByIndex(0).Apply();

			// Draw the geometry.
			RenderingDevice.Draw(mDefaultQuad.VertexCount, 0);

			mSharedMutex.ReleaseSync();
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
				Stride * mDefaultQuad.VertexCount,
				ResourceUsage.Dynamic,
				BindFlags.VertexBuffer,
				CpuAccessFlags.Write,
				ResourceOptionFlags.None);

			using(DataStream stream = buffer.Map(MapMode.WriteDiscard, MapFlags.None))
			{
				for(int j = 0; j < mDefaultQuad.TriangleCount; j++)
				{
					Triangle t = mDefaultQuad[j];

					for(int i = 0; i < 3; ++i)
					{
						Vertex v = t[i];

						float fx = (2.0f * v.X) - 1.0f;
						float fy = (2.0f * v.Y) - 1.0f;

						stream.Write(new Vector3(fx, -fy, 0.5f));
						stream.Write(new Vector2(v.U, v.V));
					}
				}

				buffer.Unmap();
			}

			return buffer;
		}
	}
}