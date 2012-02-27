// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Atlasing;
using Frost.DirectX.Common;
using Frost.Surfacing;

namespace Frost.DirectX
{
	//TODO: add support for canvas-object association
	internal sealed class PrivateAtlas<T> : ISurfaceAtlas, IDisposable
		where T : class, ISurface2D
	{
		private readonly Canvas _AtlasCanvas;
		private readonly WeakReference _Canvas;
		private readonly T _Surface2D;

		private Notification _AtlasReference;
		private Notification _ChildReference;

		public PrivateAtlas(T surface2D)
		{
			Contract.Requires(surface2D != null);

			_ChildReference = new Notification(this);

			_Surface2D = surface2D;

			_Canvas = new WeakReference(null);

			_AtlasReference = _ChildReference;

			_AtlasCanvas = new Canvas(surface2D.Region, _AtlasReference);

			_ChildReference = new Notification(this);
		}

		public T Surface2D
		{
			get { return _Surface2D; }
		}

		public IEnumerable<Rectangle> UsedRegions
		{
			get
			{
				lock(_Canvas)
				{
					Canvas canvas = (Canvas)_Canvas.Target;

					if(canvas != null)
					{
						yield return canvas.Region;
					}
				}
			}
		}

		public IEnumerable<Rectangle> FreeRegions
		{
			get
			{
				lock(_Canvas)
				{
					Canvas canvas = (Canvas)_Canvas.Target;

					if(canvas == null)
					{
						yield return _Surface2D.Region;
					}
				}
			}
		}

		public bool InUse
		{
			get
			{
				lock(_Canvas)
				{
					if(!_Canvas.IsAlive)
					{
						Invalidate();

						return false;
					}

					return true;
				}
			}
		}

		public void Dispose()
		{
			_ChildReference.Invalidate();
			_AtlasReference.Invalidate();

			_ChildReference = null;
			_AtlasReference = null;

			IDisposable disposable = _Surface2D as IDisposable;

			disposable.SafeDispose();
		}

		public Canvas Canvas
		{
			get { return _AtlasCanvas; }
		}

		public Canvas AcquireRegion(Size size, object owner = null)
		{
			Rectangle region = new Rectangle(_Surface2D.Region.Location, size);

			if(!_Surface2D.Region.Contains(region))
			{
				return null;
			}

			Rectangle offsetRegion;

			ComputeOffsetRegion(size, out offsetRegion);

			region = new Rectangle(
				offsetRegion.Location,
				new Size(offsetRegion.Width + size.Width, offsetRegion.Height + size.Height));

			Invalidate();

			Canvas canvas = new Canvas(region, _ChildReference);

			lock(_Canvas)
			{
				_Canvas.Target = canvas;
			}

			return canvas;
		}

		ISurface2D Atlasing.ISurfaceAtlas.Surface2D
		{
			get { return _Surface2D; }
		}

		public void Invalidate()
		{
			_ChildReference.Invalidate();

			Interlocked.Exchange(ref _ChildReference, new Notification(this));

			lock(_Canvas)
			{
				_Canvas.Target = null;
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
	}
}