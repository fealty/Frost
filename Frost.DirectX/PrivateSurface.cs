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
	internal sealed class PrivateSurface : Surface2D.ExternalSurface2D, ISurfaceAtlas
	{
		private readonly WeakReference _CanvasContext;

		public PrivateSurface(ref Description surfaceDescription) : base(ref surfaceDescription)
		{
			_CanvasContext = new WeakReference(null);
		}

		public IEnumerable<Rectangle> UsedRegions
		{
			get
			{
				lock(_CanvasContext)
				{
					var canvas = (Canvas.ResolvedContext)_CanvasContext.Target;

					if(canvas != null)
					{
						yield return canvas.Region;
					}
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
				lock(_CanvasContext)
				{
					var canvas = (Canvas.ResolvedContext)_CanvasContext.Target;

					if(canvas == null)
					{
						yield return Region;
					}
				}
			}
		}

		public bool InUse
		{
			get
			{
				lock(_CanvasContext)
				{
					if(!_CanvasContext.IsAlive)
					{
						Invalidate();

						return false;
					}

					return true;
				}
			}
		}

		public Canvas.ResolvedContext AcquireRegion(Size dimensions, Canvas target)
		{
			if(!Region.Contains(new Rectangle(Region.Location, dimensions)))
			{
				return null;
			}

			Rectangle offsetRegion;

			ComputeOffsetRegion(dimensions, out offsetRegion);

			Rectangle region = new Rectangle(offsetRegion.Location, offsetRegion.Size + dimensions);

			Invalidate();

			var context = new TargetContext(target, region, this);

			lock(_CanvasContext)
			{
				_CanvasContext.Target = context;
			}

			Canvas.Implementation.Assign(target, context);

			return context;
		}

		public void Invalidate()
		{
			lock(_CanvasContext)
			{
				var context = (Canvas.ResolvedContext)_CanvasContext.Target;

				if(context != null)
				{
					Canvas.Implementation.Assign(context.Target, null);
				}

				_CanvasContext.Target = null;
			}
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
			}

			public override Rectangle Region
			{
				get { return _Region; }
			}

			public override ISurface2D Surface2D
			{
				get { return _Layer; }
			}

			public override Canvas Target
			{
				get { return _Canvas; }
			}

			public override void Forget()
			{
				_Layer.Invalidate();
			}
		}
	}
}