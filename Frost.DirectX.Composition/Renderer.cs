// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

using Frost.DirectX.Common;
using Frost.DirectX.Composition.Properties;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;

using Buffer = SharpDX.Direct3D10.Buffer;

namespace Frost.DirectX.Composition
{
	internal sealed class Renderer : IDisposable
	{
		public const int BatchItemCount = 512;
		public const int ActiveVertices = 6;

		public static readonly PrimitiveTopology Topology;

		private readonly BlendState[] mBlendStates;

		private readonly Device2D mDevice2D;
		private readonly Device mDevice3D;
		private readonly DynamicBuffer[] mDynamicBuffers;
		private readonly Buffer mFrameConstants;
		private readonly Stack<Canvas> mLayers;

		private readonly SamplerState mLinearSampler;
		private readonly RasterizerState mRasterizer;
		private readonly Buffer mRenderConstants;

		private readonly Stack<PrivateAtlas<Surface2D>> mSurfaces;

		private readonly VertexBufferBinding mVertexBinding;
		private readonly InputLayout mVertexLayout;
		private readonly VertexShader mVertexShader;
		private readonly Buffer mVertices;

		static Renderer()
		{
			Topology = PrimitiveTopology.TriangleList;
		}

		public Renderer(Device device3D, Device2D device2D)
		{
			Contract.Requires(device3D != null);
			Contract.Requires(device2D != null);

			mLayers = new Stack<Canvas>();
			mSurfaces = new Stack<PrivateAtlas<Surface2D>>();

			mDevice3D = device3D;
			mDevice2D = device2D;

			// compile the default shaders
			_Compile(out mVertexShader, out mVertexLayout);

			// initialize the constant buffers
			int dynamicCount = Enum.GetValues(typeof(ConstantRegister)).Length;

			mDynamicBuffers = new DynamicBuffer[dynamicCount];

			mFrameConstants = new Buffer(mDevice3D, Descriptions.ConstantsPerFrame);
			mRenderConstants = new Buffer(mDevice3D, Descriptions.ConstantsPerRender);

			int blendStatesCount = Enum.GetValues(typeof(BlendOperation)).Length;

			mBlendStates = new BlendState[blendStatesCount];

			// initialize all of the blend states
			mBlendStates[(int)BlendOperation.Copy] = new BlendState(mDevice3D, Descriptions.CopyBlend);
			mBlendStates[(int)BlendOperation.DestinationAtop] = new BlendState(
				mDevice3D, Descriptions.DestinationAtopBlend);
			mBlendStates[(int)BlendOperation.DestinationIn] = new BlendState(
				mDevice3D, Descriptions.DestinationInBlend);
			mBlendStates[(int)BlendOperation.DestinationOut] = new BlendState(
				mDevice3D, Descriptions.DestinationOutBlend);
			mBlendStates[(int)BlendOperation.DestinationOver] = new BlendState(
				mDevice3D, Descriptions.DestinationOverBlend);
			mBlendStates[(int)BlendOperation.Lighter] = new BlendState(mDevice3D, Descriptions.LighterBlend);
			mBlendStates[(int)BlendOperation.SourceAtop] = new BlendState(
				mDevice3D, Descriptions.SourceAtopBlend);
			mBlendStates[(int)BlendOperation.SourceIn] = new BlendState(
				mDevice3D, Descriptions.SourceInBlend);
			mBlendStates[(int)BlendOperation.SourceOut] = new BlendState(
				mDevice3D, Descriptions.SourceOutBlend);
			mBlendStates[(int)BlendOperation.SourceOver] = new BlendState(
				mDevice3D, Descriptions.SourceOverBlend);

			// initialize other device states
			mLinearSampler = new SamplerState(mDevice3D, Descriptions.LinearSampler);
			mRasterizer = new RasterizerState(mDevice3D, Descriptions.Rasterizer);

			// initialize the default vertex buffer
			_CreateVertexBuffer(out mVertices);

			mVertexBinding.Buffer = mVertices;
			mVertexBinding.Offset = VertexPositionTexture.LayoutOffset;
			mVertexBinding.Stride = VertexPositionTexture.LayoutStride;

			_InitializeDeviceState();
		}

		public bool HasExtraLayers
		{
			get { return mLayers.Count > 1; }
		}

		public Canvas ActiveLayer
		{
			get { return mLayers.Peek(); }
		}

		public PixelShader ActiveShader
		{
			set { mDevice3D.PixelShader.Set(value); }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Begin()
		{
		}

		public void End()
		{
		}

		public void SetConstants<T>(ConstantRegister register, ref T value)
			where T : struct, IConstantBufferData
		{
			Buffer constantBuffer;

			_GetDynamicBuffer(register, value.ByteSize, out constantBuffer);

			using(DataStream stream = constantBuffer.Map(MapMode.WriteDiscard))
			{
				value.Serialize(stream);

				constantBuffer.Unmap();
			}
		}

		public void Render(List<RenderConstants> items, Surface2D surface, BlendOperation blendOperation)
		{
			Contract.Requires(items != null);
			Contract.Requires(surface != null);

			// update the vertex shader constant buffers for each item in the batch
			using(DataStream stream = mRenderConstants.Map(MapMode.WriteDiscard))
			{
				foreach(RenderConstants item in items)
				{
					stream.Write(item);
				}

				mRenderConstants.Unmap();
			}

			if(blendOperation == BlendOperation.Copy)
			{
				Surface2D dstSurface = (Surface2D)ActiveLayer.Surface2D;

				mDevice3D.ClearRenderTargetView(dstSurface.TargetView, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
			}

			try
			{
				surface.AcquireLock();

				mDevice3D.OutputMerger.BlendState = mBlendStates[(int)blendOperation];

				ShaderResourceView shaderView = surface.ShaderView;

				mDevice3D.PixelShader.SetShaderResource(0, shaderView);

				mDevice3D.DrawInstanced(ActiveVertices, items.Count, 0, 0);
			}
			finally
			{
				mDevice3D.PixelShader.SetShaderResource(0, null);

				surface.ReleaseLock();
			}
		}

		public void PushLayer(Retention retentionMode, Size layerSize)
		{
			Contract.Requires(layerSize.Width >= 0.0 && layerSize.Width <= double.MaxValue);
			Contract.Requires(layerSize.Height >= 0.0 && layerSize.Height <= double.MaxValue);

			if(mLayers.Count > 0)
			{
				layerSize.Width = Math.Max(layerSize.Width, ActiveLayer.Region.Width);
				layerSize.Height = Math.Max(layerSize.Height, ActiveLayer.Region.Height);
			}

			Canvas availableLayer = _GetFreeLayer(layerSize);

			try
			{
				if(retentionMode == Retention.RetainImage)
				{
					ActiveLayer.CopyTo(availableLayer);
				}

				mLayers.Push(availableLayer);
			}
			catch
			{
				availableLayer.Surface2D.Dispose();

				throw;
			}

			_ReconfigureRenderTarget(retentionMode);
		}

		public void FreeLayer(Canvas previousLayer)
		{
			mSurfaces.Push((PrivateAtlas<Surface2D>)previousLayer.Atlas);
		}

		public void PopLayer(out Canvas previousLayer)
		{
			previousLayer = mLayers.Pop();

			try
			{
				if(mLayers.Count > 0)
				{
					_ReconfigureRenderTarget(Retention.RetainImage);
				}
				else
				{
					mDevice3D.OutputMerger.SetTargets((RenderTargetView)null);
				}
			}
			catch
			{
				FreeLayer(previousLayer);

				throw;
			}
		}

		public void PopLayer()
		{
			Canvas temporary;

			PopLayer(out temporary);

			FreeLayer(temporary);
		}

		private void _GetDynamicBuffer(ConstantRegister register, int byteSize, out Buffer result)
		{
			Contract.Requires(byteSize >= 0);

			int resolvedRegister = (int)register;

			// invalidate buffers that are too small
			if(mDynamicBuffers[resolvedRegister].ByteSize < byteSize)
			{
				mDynamicBuffers[resolvedRegister].Buffer.SafeDispose();
				mDynamicBuffers[resolvedRegister].Buffer = null;
			}

			// recreate invalid buffers
			if(mDynamicBuffers[resolvedRegister].Buffer == null)
			{
				BufferDescription description = Descriptions.CustomConstants;

				// align the size in bytes to 128 bit boundaries
				mDynamicBuffers[resolvedRegister].ByteSize = byteSize % 16 == 0
				                                             	? byteSize
				                                             	: 16 * ((byteSize / 16) + 1);

				description.SizeInBytes = mDynamicBuffers[resolvedRegister].ByteSize;

				mDynamicBuffers[resolvedRegister].Buffer = new Buffer(mDevice3D, description);

				mDevice3D.PixelShader.SetConstantBuffer(
					resolvedRegister, mDynamicBuffers[resolvedRegister].Buffer);
			}

			result = mDynamicBuffers[resolvedRegister].Buffer;
		}

		private void _ReconfigureRenderTarget(Retention retentionMode)
		{
			Canvas activeLayer = ActiveLayer;

			Viewport viewport;

			viewport.MinDepth = 0.0f;
			viewport.TopLeftX = Convert.ToInt32(activeLayer.Region.Left);
			viewport.TopLeftY = Convert.ToInt32(activeLayer.Region.Top);
			viewport.Width = Convert.ToInt32(activeLayer.Region.Width);
			viewport.Height = Convert.ToInt32(activeLayer.Region.Height);
			viewport.MaxDepth = 1.0f;

			Surface2D surface = (Surface2D)activeLayer.Surface2D;

			if(retentionMode == Retention.ClearImage)
			{
				mDevice3D.ClearRenderTargetView(surface.TargetView, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
			}

			using(DataStream stream = mFrameConstants.Map(MapMode.WriteDiscard))
			{
				stream.Write(
					new FrameConstants
					{
						ViewportSize = new Vector2(viewport.Width, viewport.Height),
						ViewTransform =
						Matrix.OrthoLH(viewport.Width, -viewport.Height, viewport.MinDepth, viewport.MaxDepth)
					});

				mFrameConstants.Unmap();
			}

			PredefinedConstants constants;

			constants.SizeX = Convert.ToSingle(surface.Region.Width);
			constants.SizeY = Convert.ToSingle(surface.Region.Height);

			constants.TexelSizeX = Convert.ToSingle(1.0 / surface.Region.Width);
			constants.TexelSizeY = Convert.ToSingle(1.0 / surface.Region.Height);

			SetConstants(ConstantRegister.Predefined, ref constants);

			mDevice3D.Rasterizer.SetViewports(viewport);

			mDevice3D.OutputMerger.SetTargets(surface.TargetView);
		}

		private Canvas _GetFreeLayer(Size size)
		{
			Contract.Requires(size.Width >= 0.0 && size.Width <= double.MaxValue);
			Contract.Requires(size.Height >= 0.0 && size.Height <= double.MaxValue);
			Contract.Ensures(Contract.Result<Canvas>() != null);

			Size surfaceSize = size;

			surfaceSize.Width += 2;
			surfaceSize.Height += 2;

			PrivateAtlas<Surface2D> atlas;

			if(mSurfaces.Count == 0)
			{
				atlas = _CreateSurface(surfaceSize);

				try
				{
					atlas.Id = mLayers.Count;

					return atlas.AcquireRegion(size);
				}
				catch
				{
					atlas.Dispose();

					throw;
				}
			}

			atlas = mSurfaces.Pop();

			if(size.Width > atlas.Surface2D.Region.Width || size.Height > atlas.Surface2D.Region.Height)
			{
				atlas.Dispose();

				atlas = _CreateSurface(surfaceSize);
			}

			try
			{
				atlas.Id = mLayers.Count;

				mDevice3D.ClearRenderTargetView(atlas.Surface2D.TargetView, new Color4(0.0f, 0.0f, 0.0f, 0.0f));

				return atlas.AcquireRegion(size);
			}
			catch
			{
				atlas.Dispose();

				throw;
			}
		}

		private PrivateAtlas<Surface2D> _CreateSurface(Size size)
		{
			Contract.Requires(size.Width >= 0.0 && size.Width <= double.MaxValue);
			Contract.Requires(size.Height >= 0.0 && size.Height <= double.MaxValue);

			Surface2D.Description description;

			description.Device2D = mDevice2D;
			description.Device3D = mDevice3D;
			description.Factory2D = null;
			description.Format = SurfaceFormat.Standard;

			description.Usage = SurfaceUsage.Default;

			description.Size = size;

			Surface2D newSurface = Surface2D.FromDescription(ref description);

			try
			{
				return new PrivateAtlas<Surface2D>(newSurface);
			}
			catch
			{
				newSurface.Dispose();

				throw;
			}
		}

		private void _InitializeDeviceState()
		{
			mDevice3D.InputAssembler.SetVertexBuffers(0, mVertexBinding);

			mDevice3D.InputAssembler.PrimitiveTopology = Topology;
			mDevice3D.InputAssembler.InputLayout = mVertexLayout;

			mDevice3D.Rasterizer.State = mRasterizer;

			mDevice3D.VertexShader.SetConstantBuffer(0, mFrameConstants);
			mDevice3D.VertexShader.SetConstantBuffer(1, mRenderConstants);

			mDevice3D.VertexShader.Set(mVertexShader);

			mDevice3D.PixelShader.SetSampler(0, mLinearSampler);
		}

		private void _CreateVertexBuffer(out Buffer vertices)
		{
			int byteSize = VertexPositionTexture.LayoutStride * ActiveVertices;

			using(DataStream stream = new DataStream(byteSize, true, true))
			{
				stream.Write(new VertexPositionTexture(0.0f, 0.0f, 0.0f, 0.0f));
				stream.Write(new VertexPositionTexture(1.0f, 0.0f, 1.0f, 0.0f));
				stream.Write(new VertexPositionTexture(0.0f, 1.0f, 0.0f, 1.0f));
				stream.Write(new VertexPositionTexture(1.0f, 1.0f, 1.0f, 1.0f));
				stream.Write(new VertexPositionTexture(0.0f, 1.0f, 0.0f, 1.0f));
				stream.Write(new VertexPositionTexture(1.0f, 0.0f, 1.0f, 0.0f));

				stream.Seek(0, SeekOrigin.Begin);

				BufferDescription description = Descriptions.VertexBuffer;

				description.SizeInBytes = byteSize;

				vertices = new Buffer(mDevice3D, stream, description);
			}
		}

		private void _Compile(out VertexShader shader, out InputLayout layout)
		{
			using(ShaderBytecode code = ShaderBytecode.Compile(Resources.VertexShader, "Main", "vs_4_0"))
			{
				shader = new VertexShader(mDevice3D, code);

				InputElement[] elements = VertexPositionTexture.InputElements;

				try
				{
					layout = new InputLayout(mDevice3D, code, elements);
				}
				catch
				{
					shader.Dispose();

					throw;
				}
			}
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				mDevice3D.ClearState();

				mFrameConstants.Dispose();
				mRenderConstants.Dispose();

				Array.ForEach(mBlendStates, item => item.Dispose());
				Array.ForEach(mDynamicBuffers, item => item.Buffer.SafeDispose());

				mVertexLayout.Dispose();
				mRasterizer.Dispose();
				mLinearSampler.Dispose();
				mVertices.Dispose();

				mVertexShader.Dispose();

				mDevice3D.InputAssembler.PrimitiveTopology = Topology;

				foreach(Canvas item in mLayers)
				{
					item.Atlas.Surface2D.Dispose();
				}

				mLayers.Clear();

				foreach(PrivateAtlas<Surface2D> item in mSurfaces)
				{
					item.Dispose();
				}

				mSurfaces.Clear();
			}
		}

		private struct DynamicBuffer
		{
			public Buffer Buffer;
			public int ByteSize;
		}
	}
}