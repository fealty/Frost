// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.DirectX.Composition.Effects;

using SharpDX.DXGI;
using SharpDX.Direct3D10;

using Device = SharpDX.Direct3D10.Device;
using Device1 = SharpDX.Direct3D10.Device1;

namespace Frost.DirectX.Composition
{
	public sealed class CompositionDevice : IDisposable
	{
		private readonly BoxBlurEffect mBoxBlurEffect;
		private readonly ColorOutputEffect mColorOutputEffect;
		private readonly Compositor2 mCompositorSink;
		private readonly Device mDevice3D;

		private readonly DropShadow mDropShadowEffect;
		private readonly Lazy<Factory1> mDxgiFactory;
		private readonly GaussianBlurEffect mGaussianBlurEffect;
		private readonly Cabbage.Compositor mImmediateCompositor;

		public CompositionDevice(Adapter1 adapter, Device2D device2D)
		{
			Contract.Requires(device2D != null);

			mDxgiFactory = new Lazy<Factory1>();

			Adapter1 newAdapter = adapter;

			if(adapter == null)
			{
				newAdapter = mDxgiFactory.Value.GetAdapter1(0);
			}
#if DEBUG
			mDevice3D = new Device1(
				newAdapter, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug, FeatureLevel.Level_10_0);
#else
			mDevice3D = new Device1(
				newAdapter,
				DeviceCreationFlags.BgraSupport,
				FeatureLevel.Level_10_0);
#endif
			if(adapter == null)
			{
				newAdapter.Dispose();
			}

			mCompositorSink = new Compositor2(mDevice3D, device2D);
			mImmediateCompositor = new Cabbage.Compositor(mCompositorSink, device2D);

			mColorOutputEffect = new ColorOutputEffect();
			mGaussianBlurEffect = new GaussianBlurEffect();
			mDropShadowEffect = new DropShadow();
			mBoxBlurEffect = new BoxBlurEffect();

			device2D.RegisterEffect(mColorOutputEffect);
			device2D.RegisterEffect(mGaussianBlurEffect);
			device2D.RegisterEffect(mDropShadowEffect);
			device2D.RegisterEffect(mBoxBlurEffect);
		}

		public Device Device3D
		{
			get { return mDevice3D; }
		}

		public Cabbage.Compositor ImmediateContext
		{
			get { return mImmediateCompositor; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void SignalUpdate()
		{
			mCompositorSink.FrameBatchCount.Reset();
			mCompositorSink.FrameDuration.Reset();
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				mCompositorSink.Dispose();

				if(mDxgiFactory.IsValueCreated)
				{
					mDxgiFactory.Value.Dispose();
				}
			}
		}
	}
}