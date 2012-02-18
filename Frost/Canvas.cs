// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics;
using System.Diagnostics.Contracts;

using Frost.Atlasing;
using Frost.Surfacing;

namespace Frost
{
	public class Canvas
	{
		private readonly Notification _Notification;
		private readonly Rectangle _Region;

		public Canvas(Rectangle region, Notification notification)
		{
			Contract.Requires(notification != null);

			Trace.Assert(notification != null);

			this._Notification = notification;
			this._Region = region;

			Contract.Assert(Region.Equals(region));
		}

		public bool IsValid
		{
			get { return this._Notification.Value; }
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
				
				return this._Notification.Atlas.Surface2D;
			}
		}

		public Rectangle Region
		{
			get { return this._Region; }
		}

		public ISurfaceAtlas Atlas
		{
			get
			{
				Contract.Ensures(Contract.Result<ISurfaceAtlas>() != null);
				
				return this._Notification.Atlas;
			}
		}

		public void CopyTo(Canvas destination)
		{
			Contract.Requires(destination != null);
			Contract.Requires(destination.IsValid);
			Contract.Requires(destination.Device2D == Device2D);
			Contract.Requires(destination.Surface2D.Format == Surface2D.Format);

			Trace.Assert(destination != null);
			Trace.Assert(destination.IsValid);
			Trace.Assert(destination.Device2D == Device2D);
			Trace.Assert(destination.Surface2D.Format == Surface2D.Format);

			Surface2D.CopyTo(
				this._Region, destination.Surface2D, destination.Region.Location);
		}

		public void CopyTo(Rectangle srcRegion, Canvas destination)
		{
			Contract.Requires(destination != null);
			Contract.Requires(destination.IsValid);
			Contract.Requires(destination.Device2D == Device2D);
			Contract.Requires(destination.Surface2D.Format == Surface2D.Format);

			Trace.Assert(destination != null);
			Trace.Assert(destination.IsValid);
			Trace.Assert(destination.Device2D == Device2D);
			Trace.Assert(destination.Surface2D.Format == Surface2D.Format);

			Surface2D.CopyTo(
				srcRegion, destination.Surface2D, destination.Region.Location);
		}

		public override string ToString()
		{
			return string.Format("Region: {0}", this._Region);
		}
	}
}