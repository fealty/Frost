// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Surfacing;

namespace Frost
{
	// TODO: put the invalidated event on this class?
	public sealed class Canvas
	{
		private readonly Rectangle _Region;
		private readonly object _Tag;
		private readonly SurfaceUsage _Usage;

		private ResolvedContext _BackingContext;

		public Canvas(
			Size dimensions,
			SurfaceUsage usage = SurfaceUsage.Dynamic,
			object tag = null) :
				this(dimensions.Width, dimensions.Height, usage, tag)
		{
			Contract.Requires(Check.IsPositive(dimensions.Width));
			Contract.Requires(Check.IsPositive(dimensions.Height));
		}

		public Canvas(
			float width,
			float height,
			SurfaceUsage usage = SurfaceUsage.Dynamic,
			object yourTag = null)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			_Tag = yourTag;

			_Region = new Rectangle(Point.Empty, width, height);

			_Usage = usage;
		}

		public object Tag
		{
			get { return _Tag; }
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

		public bool IsEmpty
		{
			get { return _Region.IsEmpty; }
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
			ResolvedContext context = BackingContext;

			if(context != null)
			{
				Device2D device2D = context.Device2D;

				if(device2D != null)
				{
					device2D.Resources.ForgetCanvas(this);
				}
			}
		}

		public override string ToString()
		{
			return string.Format("Region: {0}, Usage: {1}", _Region, _Usage);
		}

		public abstract class ResolvedContext
		{
			public abstract Rectangle Region { get; }
			public abstract ISurface2D Surface2D { get; }
			public abstract Canvas Target { get; }
			public abstract Canvas SurfaceTarget { get; }
			public abstract Device2D Device2D { get; }

			public ResolvedContext BackingContext
			{
				get { return Target.BackingContext; }
				set { Target.BackingContext = value; }
			}
		}
	}
}