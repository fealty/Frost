// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using SharpDX.DXGI;
using SharpDX.Direct3D10;

using Device = SharpDX.Direct3D10.Device;
using Device1 = SharpDX.Direct3D10.Device1;

namespace Frost.DirectX.Composition
{
	public sealed class CompositionDevice : IDisposable
	{
		private readonly Device _Device3D;

		private readonly Lazy<Factory1> _DxgiFactory;
		private readonly Compositor _ImmediateContext;

		public CompositionDevice(Adapter1 adapter, Device2D device2D)
		{
			Contract.Requires(device2D != null);

			_DxgiFactory = new Lazy<Factory1>();

			Adapter1 newAdapter = adapter;

			if(adapter == null)
			{
				newAdapter = _DxgiFactory.Value.GetAdapter1(0);
			}
#if DEBUG
			_Device3D = new Device1(
				newAdapter, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug, FeatureLevel.Level_10_0);
#else
			_Device3D = new Device1(
				newAdapter,
				DeviceCreationFlags.BgraSupport,
				FeatureLevel.Level_10_0);
#endif
			if(adapter == null)
			{
				newAdapter.Dispose();
			}

			_ImmediateContext = new Compositor(_Device3D, device2D);

			device2D.Effects.Register<ColorOutputEffect>();
			device2D.Effects.Register<GaussianBlurEffect>();
			device2D.Effects.Register<DropShadowEffect>();
			device2D.Effects.Register<BoxBlurEffect>();
		}

		public Device Device3D
		{
			get { return _Device3D; }
		}

		public Frost.Composition.Compositor ImmediateContext
		{
			get { return _ImmediateContext; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void ProcessTick()
		{
			_ImmediateContext.FrameBatchCount.Reset();
			_ImmediateContext.FrameDuration.Reset();
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_ImmediateContext.Dispose();

				if(_DxgiFactory.IsValueCreated)
				{
					_DxgiFactory.Value.Dispose();
				}

				_Device3D.Dispose();
			}
		}
	}
}