// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

using Frost.Atlasing;
using Frost.Surfacing;

namespace Frost.DirectX
{
	//TODO: add support for canvas-object association
	public sealed class SharedAtlas<T> : ISurfaceAtlas, IDisposable
		where T : class, ISurface2D, IDisposable
	{
		private readonly Canvas _AtlasCanvas;
		private readonly LinkedList<Rectangle> _FreeRegions;
		private readonly object _Lock = new object();
		private readonly T _Surface2D;
		private readonly LinkedList<CanvasData> _UsedRegions;

		private Notification _AtlasReference;
		private Notification _ChildReference;

		private double _FragmentedArea;
		private double _OccupiedArea;

		public SharedAtlas(T surface2D)
		{
			Contract.Requires(surface2D != null);

			_ChildReference = new Notification(this);

			_Surface2D = surface2D;

			_UsedRegions = new LinkedList<CanvasData>();
			_FreeRegions = new LinkedList<Rectangle>();

			_FreeRegions.AddLast(new Rectangle(Point.Empty, _Surface2D.Region.Size));

			_AtlasReference = _ChildReference;

			_AtlasCanvas = new SharedCanvas(surface2D.Region, _AtlasReference);

			_ChildReference = new Notification(this);
		}

		public T Surface2D
		{
			get { return _Surface2D; }
		}

		public double Occupancy
		{
			get
			{
				lock(_Lock)
				{
					double totalArea = _Surface2D.Region.Area;

					return (_OccupiedArea / totalArea) * 100.0f;
				}
			}
		}

		public double Fragmentation
		{
			get
			{
				lock(_Lock)
				{
					double totalArea = _Surface2D.Region.Area;

					return (_FragmentedArea / totalArea) * 100.0f;
				}
			}
		}

		public bool InUse
		{
			get
			{
				lock(_Lock)
				{
					PurgeUsedRegions(false);

					return _UsedRegions.Count > 0;
				}
			}
		}

		public IEnumerable<Rectangle> UsedRegions
		{
			get { return _UsedRegions.Select(item => item.ActualRegion); }
		}

		public IEnumerable<Rectangle> FreeRegions
		{
			get { return _FreeRegions; }
		}

		public void Dispose()
		{
			_ChildReference.Invalidate();
			_AtlasReference.Invalidate();

			_ChildReference = null;
			_AtlasReference = null;

			_Surface2D.Dispose();
		}

		ISurface2D ISurfaceAtlas.Surface2D
		{
			get { return _Surface2D; }
		}

		public Canvas Canvas
		{
			get { return _AtlasCanvas; }
		}

		public Canvas AcquireRegion(Size size, object owner = null)
		{
			Rectangle adjustedRegion;

			ComputeOffsetRegion(size, out adjustedRegion);

			adjustedRegion = new Rectangle(
				adjustedRegion.Location,
				new Size(adjustedRegion.Width + size.Width, adjustedRegion.Height + size.Height));

			Rectangle? result = InsertIntoSurface(adjustedRegion.Size);

			if(result != null)
			{
				Rectangle region =
					new Rectangle(
						new Point(result.Value.X + adjustedRegion.X, result.Value.Y + adjustedRegion.Y),
						new Size(size.Width, size.Height));

				CanvasData data;

				data.ActualRegion = result.Value;

				SharedCanvas newCanvas = new SharedCanvas(region, _ChildReference);

				data.CanvasReference = new WeakReference(newCanvas);

				newCanvas.Node = new LinkedListNode<CanvasData>(data);

				lock(_Lock)
				{
					_UsedRegions.AddLast(newCanvas.Node);
				}

				int area = Convert.ToInt32(result.Value.Area * 4);

				GC.AddMemoryPressure(area);

				return newCanvas;
			}

			return null;
		}

		public void Invalidate()
		{
			_ChildReference.Invalidate();

			Interlocked.Exchange(ref _ChildReference, new Notification(this));

			_ChildReference = new Notification(this);

			lock(_Lock)
			{
				_OccupiedArea = 0;
				_FragmentedArea = 0;

				_FreeRegions.Clear();

				_FreeRegions.AddLast(new Rectangle(Point.Empty, _Surface2D.Region.Size));

				PurgeUsedRegions(true);
			}
		}

		private void PurgeUsedRegions(bool forcePurge)
		{
			for(var item = _UsedRegions.First; item != null; item = item.Next)
			{
				if(!item.Value.CanvasReference.IsAlive || forcePurge)
				{
					Rectangle region = item.Value.ActualRegion;

					_UsedRegions.Remove(item);

					int area = Convert.ToInt32(region.Area * 4);

					GC.RemoveMemoryPressure(area);

					_FragmentedArea += region.Area;
				}
			}
		}

		private void ComputeOffsetRegion(Size desiredSize, out Rectangle result)
		{
			Contract.Requires(Check.IsPositive(desiredSize.Width));
			Contract.Requires(Check.IsPositive(desiredSize.Height));

			result = Rectangle.Empty;

			if(!desiredSize.Width.Equals(_Surface2D.Region.Width))
			{
				result = new Rectangle(new Point(1, result.Y), new Size(2, result.Height));
			}

			if(!desiredSize.Height.Equals(_Surface2D.Region.Height))
			{
				result = new Rectangle(new Point(result.X, 1), new Size(result.Width, 2));
			}
		}

		private bool CheckAvailableArea(double desiredArea)
		{
			Contract.Requires(Check.IsPositive(desiredArea));

			double totalArea = _Surface2D.Region.Area;

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

			lock(_Lock)
			{
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

		private sealed class SharedCanvas : Canvas
		{
			public LinkedListNode<CanvasData> Node;

			public SharedCanvas(Rectangle region, Notification atlas) : base(region, atlas)
			{
			}
		}
	}
}