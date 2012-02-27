// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Atlasing;
using Frost.Surfacing;

namespace Frost.DirectX
{
	//TODO: add support for canvas-object association
	public sealed class DynamicAtlas<T> : ISurfaceAtlas, IDisposable
		where T : class, ISurface2D, IDisposable
	{
		private readonly Canvas _AtlasCanvas;
		private readonly LinkedList<Rectangle> _FreeRegions;
		private readonly object _Lock = new object();
		private readonly T _Surface2D;
		private readonly List<Rectangle> _UsedRegions;

		private Notification _AtlasReference;
		private Notification _ChildReference;

		private float _FreeArea;

		public DynamicAtlas(T surface2D)
		{
			Contract.Requires(surface2D != null);

			_ChildReference = new Notification(this);

			_Surface2D = surface2D;

			_FreeRegions = new LinkedList<Rectangle>();

			_FreeArea = _Surface2D.Region.Area;

			_UsedRegions = new List<Rectangle>();

			_FreeRegions.AddLast(new Rectangle(Point.Empty, _Surface2D.Region.Size));

			_AtlasReference = _ChildReference;

			_AtlasCanvas = new Canvas(surface2D.Region, _AtlasReference);

			_ChildReference = new Notification(this);
		}

		public T Surface2D
		{
			get { return _Surface2D; }
		}

		public bool InUse
		{
			get
			{
				lock(_Lock)
				{
					return _FreeArea < _Surface2D.Region.Area;
				}
			}
		}

		public IEnumerable<Rectangle> UsedRegions
		{
			get { return _UsedRegions; }
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

			lock(_Lock)
			{
				if(adjustedRegion.Area > _FreeArea)
				{
					return null;
				}
			}

			Rectangle? result = InsertIntoSurface(adjustedRegion.Size);

			if(result != null)
			{
				Rectangle region =
					new Rectangle(
						new Point(result.Value.X + adjustedRegion.X, result.Value.Y + adjustedRegion.Y), size);

				lock(_Lock)
				{
					_FreeArea -= adjustedRegion.Area;
				}

				_UsedRegions.Add(region);

				return new Canvas(region, _ChildReference);
			}

			return null;
		}

		public void Invalidate()
		{
			_ChildReference.Invalidate();

			Interlocked.Exchange(ref _ChildReference, new Notification(this));

			lock(_Lock)
			{
				_UsedRegions.Clear();
				_FreeRegions.Clear();

				_FreeRegions.AddLast(new Rectangle(Point.Empty, _Surface2D.Region.Size));

				_FreeArea = _Surface2D.Region.Area;
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

			lock(_Lock)
			{
				return IterateFreeNodes(size);
			}
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
	}
}