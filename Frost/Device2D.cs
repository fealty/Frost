// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Composition;
using Frost.Diagnostics;
using Frost.Effects;
using Frost.Formatting;
using Frost.Painting;
using Frost.Resources;
using Frost.Shaping;
using Frost.Surfacing;

namespace Frost
{
	public abstract class Device2D : IResourceManager, IShaper
	{
		public const float Flattening = 0.25f;

		private readonly DeviceCounterCollection _CounterCollection;
		private readonly EffectCollection _EffectCollection;
		private readonly object _Lock = new object();
		private Action<IEnumerable<Canvas>> _ResourcesInvalidated;

		protected Device2D()
		{
			_CounterCollection = new DeviceCounterCollection();
			_EffectCollection = new EffectCollection();
		}

		public IResourceManager Resources
		{
			get { return this; }
		}

		public DeviceCounterCollection Diagnostics
		{
			get { return _CounterCollection; }
		}

		public EffectCollection Effects
		{
			get { return _EffectCollection; }
		}

		public IShaper Shaper
		{
			get { return this; }
		}

		public abstract Painter Painter { get; }
		public abstract Compositor Compositor { get; }

		protected abstract Size PageSize { set; }

		bool IShaper.Contains(Geometry path, Point point, float tolerance)
		{
			return OnContains(path, point, tolerance);
		}

		Geometry IShaper.Simplify(Geometry path, float tolerance)
		{
			return OnSimplify(path, tolerance);
		}

		Geometry IShaper.Widen(Geometry path, float width, float tolerance)
		{
			return OnWiden(path, width, tolerance);
		}

		Rectangle IShaper.MeasureRegion(Geometry path)
		{
			return OnMeasureRegion(path);
		}

		Geometry IShaper.Combine(
			Geometry sourcePath,
			Geometry destinationPath,
			CombinationOperation operation,
			float tolerance)
		{
			return OnCombine(sourcePath, destinationPath, tolerance, operation);
		}

		void IShaper.Tessellate(Geometry path, ITessellationSink sink, float tolerance)
		{
			OnTessellate(path, tolerance, sink);
		}

		float IShaper.MeasureArea(Geometry path, float tolerance)
		{
			return OnMeasureArea(path, tolerance);
		}

		float IShaper.MeasureLength(Geometry path, float tolerance)
		{
			return OnMeasureLength(path, tolerance);
		}

		Point IShaper.DeterminePoint(Geometry path, float length, float tolerance)
		{
			Point stub;

			return OnDeterminePoint(path, length, tolerance, out stub);
		}

		Point IShaper.DeterminePoint(
			Geometry path, float length, out Point tangentVector, float tolerance)
		{
			return OnDeterminePoint(path, length, tolerance, out tangentVector);
		}

		Canvas IShaper.CreateDistanceField(Geometry path, Size resolution, float tolerance)
		{
			throw new NotImplementedException();
		}

		void IResourceManager.Copy(Rectangle fromRegion, Canvas fromTarget, Canvas toTarget)
		{
			if(!fromTarget.IsEmpty && !fromRegion.IsEmpty)
			{
				if(fromTarget.Region.Contains(fromRegion))
				{
					Rectangle srcRegion = new Rectangle(Point.Empty, fromRegion.Size);

					if(toTarget.Region.Contains(srcRegion))
					{
						var fromContext = Resources.Resolve(fromTarget);
						var toContext = Resources.Resolve(toTarget);

						// translate to 2D surface coordinate space
						fromRegion = fromRegion.Translate(fromContext.Region.Location);

						OnCopy(fromRegion, fromContext, toContext);

						return;
					}

					throw new InvalidOperationException("Destination cannot contain source!");
				}

				throw new InvalidOperationException("Source does not contain the source region!");
			}
		}

		void IResourceManager.Copy(byte[] fromRgbaData, Canvas toTarget)
		{
			if(fromRgbaData.Length > 0)
			{
				if(fromRgbaData.Length >= Convert.ToInt32(toTarget.Region.Area * 4))
				{
					var toContext = Resources.Resolve(toTarget);

					OnCopy(fromRgbaData, toContext);

					return;
				}

				throw new InvalidOperationException("Insufficient data provided!");
			}
		}

		void IResourceManager.Copy(Canvas fromTarget, Canvas toTarget)
		{
			if(!fromTarget.IsEmpty)
			{
				Resources.Copy(fromTarget.Region, fromTarget, toTarget);
			}
		}

		IEnumerable<ISurface2D> IResourceManager.GetSurfaces(SurfaceUsage usage)
		{
			return OnGetSurfaces(usage);
		}

		event Action<IEnumerable<Canvas>> IResourceManager.Invalidated
		{
			add
			{
				lock(_Lock)
				{
					_ResourcesInvalidated += value;
				}
			}
			remove
			{
				lock(_Lock)
				{
					if(_ResourcesInvalidated != null)
					{
						_ResourcesInvalidated -= value;
					}
				}
			}
		}

		Size IResourceManager.PageSize
		{
			set
			{
				if(value.Area > 0)
				{
					PageSize = value;

					return;
				}

				throw new InvalidOperationException("Page size insufficient!");
			}
		}

		Canvas.ResolvedContext IResourceManager.Resolve(Canvas target)
		{
			if(!target.IsEmpty)
			{
				Canvas.ResolvedContext context = target.BackingContext;

				if(context != null)
				{
					if(context.Device2D == this)
					{
						return context;
					}
				}

				context = OnResolve(target);

				target.BackingContext = context;

				return context;
			}

			Resources.Forget(target);

			return null;
		}

		void IResourceManager.Forget(Canvas target)
		{
			Canvas.ResolvedContext context = target.BackingContext;

			if(context != null)
			{
				OnForget(context);

				target.BackingContext = null;
			}
		}

		public abstract void ProcessTick();

		protected abstract void OnCopy(
			Rectangle fromRegion, Canvas.ResolvedContext fromTarget, Canvas.ResolvedContext toTarget);

		protected abstract void OnCopy(byte[] rgbaData, Canvas.ResolvedContext toTarget);

		protected abstract Point OnDeterminePoint(
			Geometry path, float length, float tolerance, out Point tangentVector);

		protected abstract float OnMeasureLength(Geometry path, float tolerance);

		protected abstract float OnMeasureArea(Geometry path, float tolerance);

		protected abstract void OnTessellate(Geometry path, float tolerance, ITessellationSink sink);

		protected abstract Geometry OnCombine(
			Geometry sourcePath, Geometry destinationPath, float tolerance, CombinationOperation operation);

		protected abstract Rectangle OnMeasureRegion(Geometry path);

		protected abstract Geometry OnWiden(Geometry path, float width, float tolerance);

		protected abstract Geometry OnSimplify(Geometry path, float tolerance);

		protected abstract bool OnContains(Geometry path, Point point, float tolerance);

		protected abstract IEnumerable<ISurface2D> OnGetSurfaces(SurfaceUsage usage);

		protected abstract void OnForget(Canvas.ResolvedContext target);

		protected abstract Canvas.ResolvedContext OnResolve(Canvas target);

		protected void DispatchInvalidated(IEnumerable<Canvas> resources)
		{
			Contract.Requires(resources != null);

			Action<IEnumerable<Canvas>> evt;

			lock(_Lock)
			{
				evt = _ResourcesInvalidated;
			}

			if(evt != null)
			{
				evt(resources);
			}
		}
	}
}