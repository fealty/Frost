// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Atlasing;
using Frost.DirectX.Common;
using Frost.Surfacing;

namespace Frost.DirectX.Composition
{
	public sealed class TargetLayerAtlas<T> : ISurfaceAtlas, IDisposable
		where T : class, ISurface2D
	{
		private readonly Canvas _AtlasCanvas;
		private readonly WeakReference _Canvas;
		private readonly T _Surface2D;

		private Notification _AtlasReference;
		private Notification _ChildReference;

		public TargetLayerAtlas(T surface2D)
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

		public void Dispose()
		{
			_ChildReference.Invalidate();
			_AtlasReference.Invalidate();

			_ChildReference = null;
			_AtlasReference = null;

			IDisposable disposable = Surface2D as IDisposable;

			disposable.SafeDispose();
		}

		ISurface2D ISurfaceAtlas.Surface2D
		{
			get { return Surface2D; }
		}

		public Canvas Canvas
		{
			get { return _AtlasCanvas; }
		}

		public Canvas AcquireRegion(Size size, object owner = null)
		{
			if(!_Surface2D.Region.Contains(new Rectangle(_Surface2D.Region.Location, size)))
			{
				return null;
			}

			Rectangle offsetRegion;

			ComputeOffsetRegion(size, out offsetRegion);

			offsetRegion = new Rectangle(
				new Point(offsetRegion.X, offsetRegion.Y),
				new Size(offsetRegion.Width + size.Width, offsetRegion.Height + size.Height));

			Rectangle boundary = new Rectangle(offsetRegion.Location, size);

			Invalidate();

			Canvas canvas = new Canvas(boundary, _ChildReference);

			lock(_Canvas)
			{
				_Canvas.Target = canvas;
			}

			return canvas;
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