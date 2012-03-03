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
	internal sealed class DynamicSurface : Surface2D, ISurfaceAtlas
	{
		private readonly LinkedList<Rectangle> _FreeRegions;
		private readonly List<Canvas.ResolvedContext> _UsedRegions;

		private float _FreeArea;

		private readonly Canvas.ResolvedContext _SurfaceCanvas;

		public DynamicSurface(ref Description surfaceDescription) : base(ref surfaceDescription)
		{
			_FreeRegions = new LinkedList<Rectangle>();

			_FreeArea = Region.Area;

			_UsedRegions = new List<Canvas.ResolvedContext>();

			_FreeRegions.AddLast(new Rectangle(Point.Empty, Region.Size));

			Canvas canvas = new Canvas(Region.Size, SurfaceUsage.Private);

			_SurfaceCanvas = new TargetContext(canvas, Region, this);

			_SurfaceCanvas.BackingContext = _SurfaceCanvas;
		}

		public bool InUse
		{
			get { return _FreeArea < Region.Area; }
		}

		public IEnumerable<Rectangle> UsedRegions
		{
			get
			{
				foreach(Canvas.ResolvedContext item in _UsedRegions)
				{
					yield return item.Region;
				}
			}
		}

		public void Purge(bool isForced, SafeList<Canvas> invalidatedResources)
		{
			if(isForced)
			{
				foreach(Canvas.ResolvedContext item in _UsedRegions)
				{
					item.BackingContext = null;
				}

				_UsedRegions.Clear();
				_FreeRegions.Clear();

				_FreeRegions.AddLast(new Rectangle(Point.Empty, Region.Size));

				_FreeArea = Region.Area;
			}
		}

		public void Forget(Canvas.ResolvedContext context)
		{
			context.BackingContext = null;

			_UsedRegions.Remove(context);
		}

		public Canvas SurfaceCanvas
		{
			get { return _SurfaceCanvas.Target; }
		}

		public Surface2D Surface2D
		{
			get { return this; }
		}

		public IEnumerable<Rectangle> FreeRegions
		{
			get { return _FreeRegions; }
		}

		public Canvas.ResolvedContext AcquireRegion(Size dimensions, Canvas target)
		{
			Rectangle adjustedRegion;

			ComputeOffsetRegion(dimensions, out adjustedRegion);

			if(adjustedRegion.Area > _FreeArea)
			{
				return null;
			}

			Rectangle? result = InsertIntoSurface(adjustedRegion.Size);

			if(result != null)
			{
				float finalX = result.Value.X + adjustedRegion.X;
				float finalY = result.Value.Y + adjustedRegion.Y;

				Rectangle region = new Rectangle(finalX, finalY, dimensions);

				_FreeArea -= adjustedRegion.Area;

				var context = new TargetContext(target, region, this);

				_UsedRegions.Add(context);

				return context;
			}

			return null;
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

		private Rectangle? InsertIntoNode(Size size, LinkedListNode<Rectangle> node)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));
			Contract.Requires(node != null);

			Rectangle nodeRegion = node.Value;

			if(nodeRegion.Contains(new Rectangle(nodeRegion.Location, size)))
			{
				_FreeRegions.Remove(node);

				int w = Convert.ToInt32(nodeRegion.Width - size.Width);
				int h = Convert.ToInt32(nodeRegion.Height - size.Height);

				Rectangle nodeLeft;
				Rectangle nodeRight;

				if(w >= h)
				{
					nodeLeft = new Rectangle(
						new Point(nodeRegion.X + size.Width, nodeRegion.Y), new Size(w, size.Height));

					nodeRight = new Rectangle(
						new Point(nodeRegion.X, nodeRegion.Y + size.Height), new Size(nodeRegion.Width, h));
				}
				else
				{
					nodeLeft = new Rectangle(
						new Point(nodeRegion.X, nodeRegion.Y + size.Height), new Size(size.Width, h));

					nodeRight = new Rectangle(
						new Point(nodeRegion.X + size.Width, nodeRegion.Y), new Size(w, nodeRegion.Height));
				}

				if(nodeLeft.Area > 0)
				{
					InsertNodeReverse(new LinkedListNode<Rectangle>(nodeLeft));
				}

				if(nodeRight.Area > 0)
				{
					InsertNodeReverse(new LinkedListNode<Rectangle>(nodeRight));
				}

				return new Rectangle(nodeRegion.Location, size);
			}

			return null;
		}

		private Rectangle? InsertIntoSurface(Size size)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			return IterateFreeNodes(size);
		}

		private Rectangle? IterateFreeNodes(Size size)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			for(var node = _FreeRegions.First; node != null; node = node.Next)
			{
				if(size.Area > node.Value.Area)
				{
					return null;
				}

				Rectangle? result = InsertIntoNode(size, node);

				if(result != null)
				{
					return result;
				}
			}

			return null;
		}

		private void InsertNodeReverse(LinkedListNode<Rectangle> node)
		{
			Contract.Requires(node != null);

			double nodeArea = node.Value.Area;

			for(var itr = _FreeRegions.Last; itr != null; itr = itr.Previous)
			{
				if(nodeArea < itr.Value.Area)
				{
					_FreeRegions.AddAfter(itr, node);

					return;
				}
			}

			_FreeRegions.AddFirst(node);
		}

		private sealed class TargetContext : Canvas.ResolvedContext
		{
			private readonly Canvas _Canvas;
			private readonly DynamicSurface _Layer;
			private readonly Rectangle _Region;

			public TargetContext(Canvas canvas, Rectangle region, DynamicSurface layer)
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