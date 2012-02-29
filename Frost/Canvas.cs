// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Atlasing;
using Frost.Surfacing;

namespace Frost
{
	public class Canvas2
	{
		private readonly Rectangle _Region;
		private readonly SurfaceUsage _Usage;

		private Device2D _Device2D;
		private int _SurfaceIndex;

		public Canvas2(Size dimensions, SurfaceUsage usage = SurfaceUsage.Normal)
		{
			Contract.Requires(Check.IsPositive(dimensions.Width));
			Contract.Requires(Check.IsPositive(dimensions.Height));

			_Region = new Rectangle(Point.Empty, dimensions);

			_Usage = usage;

			Contract.Assert(Region.Size == dimensions);
			Contract.Assert(Usage == usage);
		}

		internal int SurfaceIndex
		{
			get { return Interlocked.CompareExchange(ref _SurfaceIndex, 0, 0); }
			set { Interlocked.Exchange(ref _SurfaceIndex, value); }
		}

		internal Device2D Device2D
		{
			get { return Interlocked.CompareExchange(ref _Device2D, null, null); }
			set { Interlocked.Exchange(ref _Device2D, value); }
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
			Device2D device2D = Device2D;

			if(device2D != null)
			{
				device2D.ForgetCanvas(this);
			}
		}

		public override string ToString()
		{
			return string.Format("Region: {0}, Usage: {1}", _Region, _Usage);
		}
	}

	//TODO: tie Canvas to the 2d device... Canvas is a lazy mutable interface to an on-demand resource system. Backing resources are only taken when they are needed. They remain until the Canvas is garbage collected or changes. Canvas can also explicitly Forget() the backing resources. Canvases are valid so long as the 2d device is valid -- can these be decoupled from the device? Invalidations of backing resources can use an event on the device to notify.
	public class Canvas
	{
		private readonly bool _IsSelfValid;
		private readonly Notification _Notification;
		private readonly Rectangle _Region;

		public Canvas(Rectangle region, Notification notification)
		{
			Contract.Requires(notification != null);

			_Notification = notification;
			_Region = region;
			_IsSelfValid = true;

			Contract.Assert(Region.Equals(region));
		}

		public bool IsValid
		{
			get { return _IsSelfValid && _Notification.Value; }
		}

		public Device2D Device2D
		{
			get
			{
				Contract.Ensures(Contract.Result<Device2D>() != null);

				return Surface2D.Device2D;
			}
		}

		public ISurface2D Surface2D
		{
			get
			{
				Contract.Ensures(Contract.Result<ISurface2D>() != null);

				return _Notification.Atlas.Surface2D;
			}
		}

		public Rectangle Region
		{
			get
			{
				Contract.Ensures(Contract.Result<Rectangle>().Equals(_Region));

				return _Region;
			}
		}

		public ISurfaceAtlas Atlas
		{
			get
			{
				Contract.Ensures(Contract.Result<ISurfaceAtlas>() != null);

				return _Notification.Atlas;
			}
		}

		public void Forget()
		{
			if(IsValid)
			{
			}
		}

		public void CopyTo(Canvas destination)
		{
			Contract.Requires(destination != null);
			Contract.Requires(destination.IsValid);
			Contract.Requires(destination.Device2D == Device2D);

			Surface2D.CopyTo(_Region, destination.Surface2D, destination.Region.Location);
		}

		public void CopyTo(Rectangle srcRegion, Canvas destination)
		{
			Contract.Requires(destination != null);
			Contract.Requires(destination.IsValid);
			Contract.Requires(destination.Device2D == Device2D);

			Surface2D.CopyTo(srcRegion, destination.Surface2D, destination.Region.Location);
		}

		public override string ToString()
		{
			return string.Format("Region: {0}", _Region);
		}
	}
}