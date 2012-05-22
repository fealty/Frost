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
		private readonly Compositor _Compositor;
		private readonly Device _Device3D;

		private readonly Lazy<Factory1> _DxgiFactory;

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

			_Compositor = new Compositor(_Device3D, device2D);

			device2D.Resources.RegisterEffect<ColorOutputEffect>();
			device2D.Resources.RegisterEffect<GaussianBlurEffect>();
			device2D.Resources.RegisterEffect<DropShadowEffect>();
			device2D.Resources.RegisterEffect<BoxBlurEffect>();
		}

		public Device Device3D
		{
			get
			{
				Contract.Ensures(Contract.Result<Device>() != null);

				return _Device3D;
			}
		}

		public Frost.Composition.Compositor Compositor
		{
			get
			{
				Contract.Ensures(Contract.Result<Frost.Composition.Compositor>() != null);

				return _Compositor;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void ProcessTick()
		{
			_Compositor.FrameBatchCount.Reset();
			_Compositor.FrameDuration.Reset();
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_Compositor.Dispose();

				if(_DxgiFactory.IsValueCreated)
				{
					_DxgiFactory.Value.Dispose();
				}

				_Device3D.Dispose();
			}
		}
	}
}