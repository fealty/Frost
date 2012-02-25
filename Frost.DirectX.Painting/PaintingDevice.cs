// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using SharpDX.Direct2D1;
using SharpDX.Direct3D10;

namespace Frost.DirectX.Painting
{
	public sealed class PaintingDevice : IDisposable
	{
		private readonly Factory _Factory2D;
		private readonly Painter _PainterSink;

		public PaintingDevice(Device2D device2D, Device device3D)
		{
			Contract.Requires(device2D != null);
			Contract.Requires(device3D != null);

			_Factory2D = new Factory(FactoryType.SingleThreaded);

			_PainterSink = new Painter(_Factory2D, device2D, device3D);
		}

		public Factory Factory2D
		{
			get { return _Factory2D; }
		}

		public Frost.Painting.Painter ImmediateContext
		{
			get { return _PainterSink; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void SignalUpdate()
		{
			_PainterSink.FrameDuration.Reset();
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_PainterSink.Dispose();
				_Factory2D.Dispose();
			}
		}
	}
}