// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Surfacing;

namespace Frost
{
	//TODO: tie Canvas to the 2d device... Canvas is a lazy mutable interface to an on-demand resource system. Backing resources are only taken when they are needed. They remain until the Canvas is garbage collected or changes. Canvas can also explicitly Forget() the backing resources. Canvases are valid so long as the 2d device is valid -- can these be decoupled from the device? Invalidations of backing resources can use an event on the device to notify.
	public sealed class Canvas
	{
		private readonly Rectangle _Region;
		private readonly SurfaceUsage _Usage;

		private ResolvedContext _BackingContext;

		public Canvas(Size dimensions, SurfaceUsage usage = SurfaceUsage.Normal)
		{
			Contract.Requires(Check.IsPositive(dimensions.Width));
			Contract.Requires(Check.IsPositive(dimensions.Height));

			_Region = new Rectangle(Point.Empty, dimensions);

			_Usage = usage;

			Contract.Assert(Region.Size == dimensions);
			Contract.Assert(Usage == usage);
		}

		internal ResolvedContext BackingContext
		{
			get { return Interlocked.CompareExchange(ref _BackingContext, null, null); }
			set { Interlocked.Exchange(ref _BackingContext, value); }
		}

		public Rectangle Region
		{
			get
			{
				Contract.Ensures(Contract.Result<Rectangle>().Equals(_Region));

				return _Region;
			}
		}

		public SurfaceUsage Usage
		{
			get
			{
				Contract.Ensures(Contract.Result<SurfaceUsage>() == _Usage);

				return _Usage;
			}
		}

		public void Forget()
		{
			Device2D device2D = BackingContext.Device2D;

			if(device2D != null)
			{
				device2D.ForgetCanvas(this);
			}
		}

		public override string ToString()
		{
			return string.Format("Region: {0}, Usage: {1}", _Region, _Usage);
		}

		public abstract class ResolvedContext
		{
			public abstract Device2D Device2D { get; }
			public abstract Rectangle Region { get; }
			public abstract ISurface2D Surface2D { get; }
			public abstract Canvas Target { get; }
		}
	}
}