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
using Frost.Surfacing;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;

using BlendOperation = Frost.Composition.BlendOperation;
using Buffer = SharpDX.Direct3D10.Buffer;

namespace Frost.DirectX.Composition
{
	internal sealed class Renderer : IDisposable
	{
		public const int BatchItemCount = 512;
		public const int ActiveVertices = 6;

		public static readonly PrimitiveTopology Topology;

		private readonly BlendState[] _BlendStates;

		private readonly Device2D _Device2D;
		private readonly Device _Device3D;
		private readonly DynamicBuffer[] _DynamicBuffers;
		private readonly Buffer _FrameConstants;
		private readonly Stack<Canvas.ResolvedContext> _Layers;

		private readonly SamplerState _LinearSampler;
		private readonly RasterizerState _Rasterizer;
		private readonly Buffer _RenderConstants;

		private readonly VertexBufferBinding _VertexBinding;
		private readonly InputLayout _VertexLayout;
		private readonly VertexShader _VertexShader;
		private readonly Buffer _Vertices;

		static Renderer()
		{
			Topology = PrimitiveTopology.TriangleList;
		}

		public Renderer(Device device3D, Device2D device2D)
		{
			Contract.Requires(device3D != null);
			Contract.Requires(device2D != null);

			_Layers = new Stack<Canvas.ResolvedContext>();

			_Device3D = device3D;
			_Device2D = device2D;

			// compile the default shaders
			Compile(out _VertexShader, out _VertexLayout);

			// initialize the constant buffers
			int dynamicCount = Enum.GetValues(typeof(ConstantRegister)).Length;

			_DynamicBuffers = new DynamicBuffer[dynamicCount];

			_FrameConstants = new Buffer(_Device3D, Descriptions.ConstantsPerFrame);
			_RenderConstants = new Buffer(_Device3D, Descriptions.ConstantsPerRender);

			int blendStatesCount = Enum.GetValues(typeof(BlendOperation)).Length;

			_BlendStates = new BlendState[blendStatesCount];

			// initialize all of the blend states
			_BlendStates[(int)BlendOperation.Copy] = new BlendState(_Device3D, Descriptions.CopyBlend);
			_BlendStates[(int)BlendOperation.DestinationAtop] = new BlendState(
				_Device3D, Descriptions.DestinationAtopBlend);
			_BlendStates[(int)BlendOperation.DestinationIn] = new BlendState(
				_Device3D, Descriptions.DestinationInBlend);
			_BlendStates[(int)BlendOperation.DestinationOut] = new BlendState(
				_Device3D, Descriptions.DestinationOutBlend);
			_BlendStates[(int)BlendOperation.DestinationOver] = new BlendState(
				_Device3D, Descriptions.DestinationOverBlend);
			_BlendStates[(int)BlendOperation.Lighter] = new BlendState(_Device3D, Descriptions.LighterBlend);
			_BlendStates[(int)BlendOperation.SourceAtop] = new BlendState(
				_Device3D, Descriptions.SourceAtopBlend);
			_BlendStates[(int)BlendOperation.SourceIn] = new BlendState(
				_Device3D, Descriptions.SourceInBlend);
			_BlendStates[(int)BlendOperation.SourceOut] = new BlendState(
				_Device3D, Descriptions.SourceOutBlend);
			_BlendStates[(int)BlendOperation.SourceOver] = new BlendState(
				_Device3D, Descriptions.SourceOverBlend);

			// initialize other device states
			_LinearSampler = new SamplerState(_Device3D, Descriptions.LinearSampler);
			_Rasterizer = new RasterizerState(_Device3D, Descriptions.Rasterizer);

			// initialize the default vertex buffer
			CreateVertexBuffer(out _Vertices);

			_VertexBinding.Buffer = _Vertices;
			_VertexBinding.Offset = VertexPositionTexture.LayoutOffset;
			_VertexBinding.Stride = VertexPositionTexture.LayoutStride;

			InitializeDeviceState();
		}

		public bool HasExtraLayers
		{
			get { return _Layers.Count > 1; }
		}

		public Canvas.ResolvedContext ActiveLayer
		{
			get { return _Layers.Peek(); }
		}

		public PixelShader ActiveShader
		{
			set { _Device3D.PixelShader.Set(value); }
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

			GetDynamicBuffer(register, value.ByteSize, out constantBuffer);

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
			using(DataStream stream = _RenderConstants.Map(MapMode.WriteDiscard))
			{
				foreach(RenderConstants item in items)
				{
					stream.Write(item);
				}

				_RenderConstants.Unmap();
			}

			if(blendOperation == BlendOperation.Copy)
			{
				Surface2D dstSurface = (Surface2D)ActiveLayer.Surface2D;

				_Device3D.ClearRenderTargetView(dstSurface.TargetView, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
			}

			try
			{
				surface.AcquireLock();

				_Device3D.OutputMerger.BlendState = _BlendStates[(int)blendOperation];

				ShaderResourceView shaderView = surface.ShaderView;

				_Device3D.PixelShader.SetShaderResource(0, shaderView);

				_Device3D.DrawInstanced(ActiveVertices, items.Count, 0, 0);
			}
			finally
			{
				_Device3D.PixelShader.SetShaderResource(0, null);

				surface.ReleaseLock();
			}
		}

		public void PushLayer(Retention retentionMode, Size layerSize)
		{
			Contract.Requires(layerSize.Width >= 0.0 && layerSize.Width <= double.MaxValue);
			Contract.Requires(layerSize.Height >= 0.0 && layerSize.Height <= double.MaxValue);

			if(_Layers.Count > 0)
			{
				layerSize = new Size(
					Math.Max(layerSize.Width, ActiveLayer.Region.Width),
					Math.Max(layerSize.Height, ActiveLayer.Region.Height));
			}

			Canvas.ResolvedContext availableLayer = GetFreeLayer(layerSize);

			try
			{
				if(retentionMode == Retention.RetainData)
				{
					ActiveLayer.Surface2D.CopyTo(
						ActiveLayer.Region, availableLayer.Surface2D, availableLayer.Region.Location);
				}

				_Layers.Push(availableLayer);
			}
			catch
			{
				availableLayer.Target.Forget();

				throw;
			}

			ReconfigureRenderTarget(retentionMode);
		}

		public void FreeLayer(Canvas.ResolvedContext previousLayer)
		{
			previousLayer.Target.Forget();
		}

		public void PopLayer(out Canvas.ResolvedContext previousLayer)
		{
			previousLayer = _Layers.Pop();

			try
			{
				if(_Layers.Count > 0)
				{
					ReconfigureRenderTarget(Retention.RetainData);
				}
				else
				{
					_Device3D.OutputMerger.SetTargets((RenderTargetView)null);
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
			Canvas.ResolvedContext temporary;

			PopLayer(out temporary);

			FreeLayer(temporary);
		}

		private void GetDynamicBuffer(ConstantRegister register, int byteSize, out Buffer result)
		{
			Contract.Requires(byteSize >= 0);

			int resolvedRegister = (int)register;

			// invalidate buffers that are too small
			if(_DynamicBuffers[resolvedRegister].ByteSize < byteSize)
			{
				_DynamicBuffers[resolvedRegister].Buffer.SafeDispose();
				_DynamicBuffers[resolvedRegister].Buffer = null;
			}

			// recreate invalid buffers
			if(_DynamicBuffers[resolvedRegister].Buffer == null)
			{
				BufferDescription description = Descriptions.CustomConstants;

				// align the size in bytes to 128 bit boundaries
				_DynamicBuffers[resolvedRegister].ByteSize = byteSize % 16 == 0
				                                             	? byteSize
				                                             	: 16 * ((byteSize / 16) + 1);

				description.SizeInBytes = _DynamicBuffers[resolvedRegister].ByteSize;

				_DynamicBuffers[resolvedRegister].Buffer = new Buffer(_Device3D, description);

				_Device3D.PixelShader.SetConstantBuffer(
					resolvedRegister, _DynamicBuffers[resolvedRegister].Buffer);
			}

			result = _DynamicBuffers[resolvedRegister].Buffer;
		}

		private void ReconfigureRenderTarget(Retention retentionMode)
		{
			Canvas.ResolvedContext activeLayer = ActiveLayer;

			Viewport viewport;

			viewport.MinDepth = 0.0f;
			viewport.TopLeftX = Convert.ToInt32(activeLayer.Region.Left);
			viewport.TopLeftY = Convert.ToInt32(activeLayer.Region.Top);
			viewport.Width = Convert.ToInt32(activeLayer.Region.Width);
			viewport.Height = Convert.ToInt32(activeLayer.Region.Height);
			viewport.MaxDepth = 1.0f;

			Surface2D surface = (Surface2D)activeLayer.Surface2D;

			if(retentionMode == Retention.ClearData)
			{
				_Device3D.ClearRenderTargetView(surface.TargetView, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
			}

			using(DataStream stream = _FrameConstants.Map(MapMode.WriteDiscard))
			{
				stream.Write(
					new FrameConstants
					{
						ViewportSize = new Vector2(viewport.Width, viewport.Height),
						ViewTransform =
						Matrix.OrthoLH(viewport.Width, -viewport.Height, viewport.MinDepth, viewport.MaxDepth)
					});

				_FrameConstants.Unmap();
			}

			PredefinedConstants constants;

			constants.SizeX = Convert.ToSingle(surface.Region.Width);
			constants.SizeY = Convert.ToSingle(surface.Region.Height);

			constants.TexelSizeX = Convert.ToSingle(1.0 / surface.Region.Width);
			constants.TexelSizeY = Convert.ToSingle(1.0 / surface.Region.Height);

			SetConstants(ConstantRegister.Predefined, ref constants);

			_Device3D.Rasterizer.SetViewports(viewport);

			_Device3D.OutputMerger.SetTargets(surface.TargetView);
		}

		private Canvas.ResolvedContext GetFreeLayer(Size dimensions)
		{
			Contract.Requires(Check.IsPositive(dimensions.Width));
			Contract.Requires(Check.IsPositive(dimensions.Height));
			Contract.Ensures(Contract.Result<Canvas.ResolvedContext>() != null);

			Canvas newCanvas = new Canvas(dimensions, SurfaceUsage.Private);

			var context = _Device2D.ResolveCanvas(newCanvas);

			Surface2D surface = (Surface2D)context.Surface2D;

			_Device3D.ClearRenderTargetView(surface.TargetView, new Color4());

			return context;
		}

		private void InitializeDeviceState()
		{
			_Device3D.InputAssembler.SetVertexBuffers(0, _VertexBinding);

			_Device3D.InputAssembler.PrimitiveTopology = Topology;
			_Device3D.InputAssembler.InputLayout = _VertexLayout;

			_Device3D.Rasterizer.State = _Rasterizer;

			_Device3D.VertexShader.SetConstantBuffer(0, _FrameConstants);
			_Device3D.VertexShader.SetConstantBuffer(1, _RenderConstants);

			_Device3D.VertexShader.Set(_VertexShader);

			_Device3D.PixelShader.SetSampler(0, _LinearSampler);
		}

		private void CreateVertexBuffer(out Buffer vertices)
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

				vertices = new Buffer(_Device3D, stream, description);
			}
		}

		private void Compile(out VertexShader shader, out InputLayout layout)
		{
			using(ShaderBytecode code = ShaderBytecode.Compile(Resources.VertexShader, "Main", "vs_4_0"))
			{
				shader = new VertexShader(_Device3D, code);

				InputElement[] elements = VertexPositionTexture.InputElements;

				try
				{
					layout = new InputLayout(_Device3D, code, elements);
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
				_Device3D.ClearState();

				_FrameConstants.Dispose();
				_RenderConstants.Dispose();

				Array.ForEach(_BlendStates, item => item.Dispose());
				Array.ForEach(_DynamicBuffers, item => item.Buffer.SafeDispose());

				_VertexLayout.Dispose();
				_Rasterizer.Dispose();
				_LinearSampler.Dispose();
				_Vertices.Dispose();

				_VertexShader.Dispose();

				_Device3D.InputAssembler.PrimitiveTopology = Topology;

				_Layers.Clear();
			}
		}

		private struct DynamicBuffer
		{
			public Buffer Buffer;
			public int ByteSize;
		}
	}
}