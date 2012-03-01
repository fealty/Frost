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
		private readonly Painter _PainterContext;

		public PaintingDevice(Device2D device2D, Device device3D)
		{
			Contract.Requires(device2D != null);
			Contract.Requires(device3D != null);

			_Factory2D = new Factory(FactoryType.SingleThreaded);

			_PainterContext = new Painter(_Factory2D, device2D, device3D);

			Contract.Assert(Factory2D == _Factory2D);
			Contract.Assert(Painter == _PainterContext);
		}

		public Factory Factory2D
		{
			get
			{
				Contract.Ensures(Contract.Result<Factory>() != null);

				return _Factory2D;
			}
		}

		public Frost.Painting.Painter Painter
		{
			get
			{
				Contract.Ensures(Contract.Result<Frost.Painting.Painter>() != null);

				return _PainterContext;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void ProcessTick()
		{
			_PainterContext.FrameDuration.Reset();
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_PainterContext.Dispose();
				_Factory2D.Dispose();
			}
		}
	}
}