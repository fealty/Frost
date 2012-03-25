// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.DirectX.Common;
using Frost.Surfacing;

namespace Frost.DirectX
{
	internal sealed class PrivateSurface : Surface2D, ISurfaceAtlas
	{
		private readonly WeakReference _CanvasContext;

		private readonly Canvas.ResolvedContext _SurfaceCanvas;

		public PrivateSurface(ref Description surfaceDescription) : base(ref surfaceDescription)
		{
			_CanvasContext = new WeakReference(null);

			Canvas canvas = new Canvas(Region.Size, SurfaceUsage.Private);

			_SurfaceCanvas = new TargetContext(canvas, Region, this);

			_SurfaceCanvas.BackingContext = _SurfaceCanvas;
		}

		public IEnumerable<Rectangle> UsedRegions
		{
			get
			{
				var canvas = (Canvas.ResolvedContext)_CanvasContext.Target;

				if(canvas != null)
				{
					yield return canvas.Region;
				}
			}
		}

		public Surface2D Surface2D
		{
			get { return this; }
		}

		public IEnumerable<Rectangle> FreeRegions
		{
			get
			{
				var canvas = (Canvas.ResolvedContext)_CanvasContext.Target;

				if(canvas == null)
				{
					yield return Region;
				}
			}
		}

		public bool InUse
		{
			get { return _CanvasContext.IsAlive; }
		}

		public Canvas.ResolvedContext AcquireRegion(Size dimensions, Canvas target)
		{
			Rectangle region;

			if(!Region.Contains(new Rectangle(Region.Location, dimensions)))
			{
				return null;
			}

			ComputeOffsetRegion(dimensions, out region);

			TargetContext context = new TargetContext(target, region, this);

			Contract.Assert(!_CanvasContext.IsAlive);

			_CanvasContext.Target = context;

			return context;
		}

		public void Purge(bool isForced, SafeList<Canvas> invalidatedResources)
		{
			if(isForced)
			{
				var context = (Canvas.ResolvedContext)_CanvasContext.Target;

				if(context != null)
				{
					context.BackingContext = null;

					invalidatedResources.Add(context.Target);
				}

				_CanvasContext.Target = null;
			}
			else if(!_CanvasContext.IsAlive)
			{
				_CanvasContext.Target = null;
			}
		}

		public void Forget(Canvas.ResolvedContext context)
		{
			context.BackingContext = null;

			_CanvasContext.Target = null;
		}

		public Canvas SurfaceCanvas
		{
			get { return _SurfaceCanvas.Target; }
		}

		private void ComputeOffsetRegion(Size desiredSize, out Rectangle result)
		{
			Contract.Requires(Check.IsPositive(desiredSize.Width));
			Contract.Requires(Check.IsPositive(desiredSize.Height));

			result = Rectangle.Empty;

			if(!desiredSize.Width.Equals(Region.Width))
			{
				result = new Rectangle(new Point(1, result.Y), new Size(2, result.Height));
			}

			if(!desiredSize.Height.Equals(Region.Height))
			{
				result = new Rectangle(new Point(result.X, 1), new Size(result.Width, 2));
			}

			result = new Rectangle(result.Location, result.Size + desiredSize);
		}

		private sealed class TargetContext : Canvas.ResolvedContext
		{
			private readonly Canvas _Canvas;
			private readonly PrivateSurface _Layer;
			private readonly Rectangle _Region;

			public TargetContext(Canvas canvas, Rectangle region, PrivateSurface layer)
			{
				Contract.Requires(canvas != null);
				Contract.Requires(layer != null);

				_Canvas = canvas;
				_Region = region;
				_Layer = layer;

				Contract.Assert(Region.Equals(region));
				Contract.Assert(ReferenceEquals(Surface2D, layer));
				Contract.Assert(ReferenceEquals(Target, canvas));
				Contract.Assert(ReferenceEquals(Device2D, layer.Device2D));
			}

			public override Rectangle Region
			{
				get { return _Region; }
			}

			public override Canvas SurfaceTarget
			{
				get { return _Layer.SurfaceCanvas; }
			}

			public override ISurface2D Surface2D
			{
				get { return _Layer; }
			}

			public override Canvas Target
			{
				get { return _Canvas; }
			}

			public override Frost.Device2D Device2D
			{
				get { return _Layer.Device2D; }
			}
		}
	}
}