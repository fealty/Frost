// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Frost.DirectX.Common;
using Frost.Surfacing;

namespace Frost.DirectX
{
	internal sealed class SharedSurface : Surface2D, ISurfaceAtlas
	{
		private readonly LinkedList<Rectangle> _FreeRegions;
		private readonly LinkedList<CanvasData> _UsedRegions;

		private float _FragmentedArea;
		private float _OccupiedArea;

		public SharedSurface(ref Description surfaceDescription) : base(ref surfaceDescription)
		{
			_UsedRegions = new LinkedList<CanvasData>();
			_FreeRegions = new LinkedList<Rectangle>();

			_FreeRegions.AddLast(new Rectangle(Point.Empty, Region.Size));
		}

		public float Occupancy
		{
			get
			{
				float totalArea = Region.Area;

				return (_OccupiedArea / totalArea) * 100.0f;
			}
		}

		public float Fragmentation
		{
			get
			{
				float totalArea = Region.Area;

				return (_FragmentedArea / totalArea) * 100.0f;
			}
		}

		public bool InUse
		{
			get { return _UsedRegions.Count > 0; }
		}

		public IEnumerable<Rectangle> UsedRegions
		{
			get { return _UsedRegions.Select(item => item.ActualRegion); }
		}

		public void Purge(bool isForced, SafeList<Canvas> invalidatedResources)
		{
			if(isForced)
			{
				_OccupiedArea = 0;
				_FragmentedArea = 0;

				_FreeRegions.Clear();

				_FreeRegions.AddLast(new Rectangle(Point.Empty, Region.Size));

				PurgeUsedRegions(true, invalidatedResources);
			}
			else
			{
				PurgeUsedRegions(false, invalidatedResources);
			}
		}

		public void Forget(Canvas.ResolvedContext context)
		{
			TargetContext targetContext = (TargetContext)context;

			context.Invalidate();

			Rectangle region = targetContext.Node.Value.ActualRegion;

			_UsedRegions.Remove(targetContext.Node);

			_FragmentedArea += region.Area;
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

			Rectangle? result = InsertIntoSurface(adjustedRegion.Size);

			if(result != null)
			{
				float finalX = result.Value.X + adjustedRegion.X;
				float finalY = result.Value.Y + adjustedRegion.Y;

				Rectangle region = new Rectangle(finalX, finalY, dimensions);

				CanvasData data;

				data.ActualRegion = result.Value;
				data.CanvasReference = new WeakReference(null);

				var context = new TargetContext(target, region, this, new LinkedListNode<CanvasData>(data));

				context.Node.Value.CanvasReference.Target = context;

				_UsedRegions.AddLast(context.Node);

				return context;
			}

			return null;
		}

		private void PurgeUsedRegions(bool forcePurge, SafeList<Canvas> invalidatedResources)
		{
			for(var item = _UsedRegions.First; item != null; item = item.Next)
			{
				if(!item.Value.CanvasReference.IsAlive || forcePurge)
				{
					if(forcePurge)
					{
						var context = (Canvas.ResolvedContext)item.Value.CanvasReference.Target;

						if(context != null)
						{
							context.Invalidate();

							invalidatedResources.Add(context.Target);
						}
					}

					Rectangle region = item.Value.ActualRegion;

					_UsedRegions.Remove(item);

					_FragmentedArea += region.Area;
				}
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

			result = new Rectangle(result.Location, result.Size + desiredSize);
		}

		private bool CheckAvailableArea(double desiredArea)
		{
			Contract.Requires(Check.IsPositive(desiredArea));

			double totalArea = Region.Area;

			return desiredArea <= totalArea - _OccupiedArea;
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

			double desiredArea = size.Area;

			if(!CheckAvailableArea(desiredArea))
			{
				return null;
			}

			for(var node = _FreeRegions.First; node != null; node = node.Next)
			{
				if(desiredArea > node.Value.Area)
				{
					continue;
				}

				Rectangle? result = InsertIntoNode(size, node);

				if(result != null)
				{
					_OccupiedArea += result.Value.Area;

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
				if(nodeArea > itr.Value.Area)
				{
					_FreeRegions.AddAfter(itr, node);

					return;
				}
			}

			_FreeRegions.AddFirst(node);
		}

		private struct CanvasData
		{
			public Rectangle ActualRegion;
			public WeakReference CanvasReference;
		}

		private sealed class TargetContext : Canvas.ResolvedContext
		{
			private readonly Canvas _Canvas;
			private readonly SharedSurface _Layer;
			private readonly LinkedListNode<CanvasData> _Node;
			private readonly Rectangle _Region;

			public TargetContext(
				Canvas canvas, Rectangle region, SharedSurface layer, LinkedListNode<CanvasData> node)
			{
				Contract.Requires(canvas != null);
				Contract.Requires(layer != null);
				Contract.Requires(node != null);

				_Canvas = canvas;
				_Region = region;
				_Layer = layer;
				_Node = node;
			}

			public LinkedListNode<CanvasData> Node
			{
				get { return _Node; }
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