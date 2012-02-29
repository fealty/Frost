// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
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

namespace Demo.Framework
{
	internal sealed class D3D10Renderer : IDisposable
	{
		private const int _Stride = 20;
		private const int _Offset = 0;
		private const int _VertexCount = 6;

		private static readonly Vector2[] _QuadPrimitiveData = new[]
		{
			new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f),
			new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f),
			new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
			new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f)
		};

		private static readonly InputElement[] _InputLayoutData = new[]
		{
			new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
			new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
		};

		private readonly Device1 _Device3D;

		private readonly Effect _Effect;
		private readonly EffectPass _EffectPass;
		private readonly EffectTechnique _EffectTechnique;
		private readonly InputLayout _InputLayout;
		private readonly Buffer _VertexBuffer;
		private readonly VertexBufferBinding _VertexBufferBinding;

		private ShaderResourceView _DependentView;
		private IntPtr _SharedHandle;
		private KeyedMutex _SharedMutex;
		private Texture2D _SharedTexture;

		public D3D10Renderer(Device1 device3D)
		{
			Contract.Requires(device3D != null);

			_Device3D = device3D;

			string shader = GetShaderText(Resources.RenderingEffects);

			using(var byteCode = ShaderBytecode.Compile(shader, "fx_4_0", ShaderFlags.None, EffectFlags.None)
				)
			{
				_Effect = new Effect(_Device3D, byteCode);
			}

			_EffectTechnique = _Effect.GetTechniqueByName("WithTexture");

			Contract.Assert(_EffectTechnique != null);

			_EffectPass = _EffectTechnique.GetPassByIndex(0);

			Contract.Assert(_EffectPass != null);

			_InputLayout = new InputLayout(_Device3D, _EffectPass.Description.Signature, _InputLayoutData);

			const int byteSize = _Stride * _VertexCount;

			using(DataStream stream = new DataStream(byteSize, true, true))
			{
				for(int i = 0; i < _QuadPrimitiveData.Length; i += 2)
				{
					float fx = (2.0f * _QuadPrimitiveData[i + 0].X) - 1.0f;
					float fy = (2.0f * _QuadPrimitiveData[i + 0].Y) - 1.0f;

					stream.Write(new Vector3(fx, -fy, 0.5f));
					stream.Write(_QuadPrimitiveData[i + 1]);
				}

				stream.Seek(0, SeekOrigin.Begin);

				BufferDescription description = new BufferDescription
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					Usage = ResourceUsage.Immutable,
					SizeInBytes = byteSize
				};

				description.SizeInBytes = byteSize;

				_VertexBuffer = new Buffer(_Device3D, stream, description);

				_VertexBufferBinding = new VertexBufferBinding(_VertexBuffer, _Stride, _Offset);
			}
		}

		public void Dispose()
		{
			_Device3D.ClearState();

			_DependentView.SafeDispose();
			_SharedMutex.SafeDispose();
			_SharedTexture.SafeDispose();
			_VertexBuffer.SafeDispose();
			_InputLayout.SafeDispose();
			_Effect.SafeDispose();
			_Device3D.Dispose();
		}

		public void BeginRendering()
		{
			// Set the input layout.
			_Device3D.InputAssembler.InputLayout = _InputLayout;

			// Set the vertex buffer.
			_Device3D.InputAssembler.SetVertexBuffers(0, _VertexBufferBinding);

			// Set the primitive topology.
			_Device3D.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		}

		public void Render(Canvas canvas, Device2D device2D)
		{
			Contract.Requires(canvas != null);
			Contract.Requires(device2D != null);

			//TODO: is there a better way to get the device handle?
			IntPtr sharedHandle = canvas.GetDeviceHandle(device2D);

			if(sharedHandle != _SharedHandle)
			{
				_SharedMutex.SafeDispose();
				_DependentView.SafeDispose();
				_SharedTexture.SafeDispose();

				_SharedMutex = null;
				_DependentView = null;
				_SharedTexture = null;

				if(sharedHandle != IntPtr.Zero)
				{
					_SharedTexture = _Device3D.OpenSharedResource<Texture2D>(sharedHandle);

					_SharedMutex = _SharedTexture.QueryInterface<KeyedMutex>();

					_DependentView = new ShaderResourceView(_Device3D, _SharedTexture);

					_SharedHandle = sharedHandle;
				}
			}

			if (_SharedMutex != null)
			{
				_SharedMutex.AcquireSync();

				try
				{
					if(_SharedTexture != null)
					{
						var textureVariable = _Effect.GetVariableByName("tex2D");

						Contract.Assert(textureVariable != null);

						var shaderResource = textureVariable.AsShaderResource();

						Contract.Assert(shaderResource != null);

						shaderResource.SetResource(_DependentView);

						_EffectPass.Apply();

						_Device3D.Draw(_VertexCount, 0);
					}
				}
				finally
				{
					_SharedMutex.ReleaseSync();
				}
			}
		}

		public void EndRendering()
		{
		}

		private static string GetShaderText(byte[] pixelShaderTextBytes)
		{
			Contract.Requires(pixelShaderTextBytes != null);
			Contract.Ensures(Contract.Result<string>() != null);

			using(MemoryStream stream = new MemoryStream(pixelShaderTextBytes))
			{
				using(StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}