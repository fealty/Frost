// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;
using Frost.Composition;
using Frost.Diagnostics;
using Frost.Effects;
using Frost.Shaping;
using Frost.Painting;
using Frost.Management;
using Frost.Construction;
using Frost.Surfacing;

namespace Frost
{
	public abstract class Device2D : IResourceHelpers, IFigureHelpers
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

		public IResourceHelpers Resources
		{
			get { return this; }
		}

		public DeviceCounterCollection Diagnostics
		{
			get { return _CounterCollection; }
		}

		public IFigureHelpers Geometry
		{
			get { return this; }
		}

		public abstract Shaper Shaper { get; }
		public abstract Painter Painter { get; }
		public abstract Compositor Compositor { get; }

		protected abstract Size PageSize { set; }

		bool IFigureHelpers.Contains(Figure path, Point point, float tolerance)
		{
			return OnContains(path, point, tolerance);
		}

		Figure IFigureHelpers.Simplify(Figure path, float tolerance)
		{
			return OnSimplify(path, tolerance);
		}

		Figure IFigureHelpers.Widen(Figure path, float width, float tolerance)
		{
			return OnWiden(path, width, tolerance);
		}

		Rectangle IFigureHelpers.MeasureRegion(Figure path)
		{
			return OnMeasureRegion(path);
		}

		Figure IFigureHelpers.Combine(
			Figure sourcePath,
			Figure destinationPath,
			CombinationOperation operation,
			float tolerance)
		{
			return OnCombine(sourcePath, destinationPath, tolerance, operation);
		}

		void IFigureHelpers.Tessellate(
			Figure path, ITessellationSink sink, float tolerance)
		{
			OnTessellate(path, tolerance, sink);
		}

		float IFigureHelpers.MeasureArea(Figure path, float tolerance)
		{
			return OnMeasureArea(path, tolerance);
		}

		float IFigureHelpers.MeasureLength(Figure path, float tolerance)
		{
			return OnMeasureLength(path, tolerance);
		}

		Point IFigureHelpers.DeterminePoint(
			Figure path, float length, float tolerance)
		{
			Point stub;

			return OnDeterminePoint(path, length, tolerance, out stub);
		}

		Point IFigureHelpers.DeterminePoint(
			Figure path, float length, out Point tangentVector, float tolerance)
		{
			return OnDeterminePoint(path, length, tolerance, out tangentVector);
		}

		Canvas IFigureHelpers.CreateDistanceField(
			Figure path, Size resolution, float tolerance)
		{
			throw new NotImplementedException();
		}

		void IResourceHelpers.Copy(
			Rectangle fromRegion, Canvas fromTarget, Canvas toTarget)
		{
			if(!fromTarget.IsEmpty && !fromRegion.IsEmpty)
			{
				if(fromTarget.Region.Contains(fromRegion))
				{
					Rectangle srcRegion = new Rectangle(Point.Empty, fromRegion.Size);

					if(toTarget.Region.Contains(srcRegion))
					{
						var fromContext = Resources.ResolveCanvas(fromTarget);
						var toContext = Resources.ResolveCanvas(toTarget);

						// translate to 2D surface coordinate space
						fromRegion = fromRegion.Translate(fromContext.Region.Location);

						OnCopy(fromRegion, fromContext, toContext);

						return;
					}

					throw new InvalidOperationException("Destination cannot contain source!");
				}

				throw new InvalidOperationException(
					"Source does not contain the source region!");
			}
		}

		void IResourceHelpers.Copy(byte[] fromRgbaData, Canvas toTarget)
		{
			if(fromRgbaData.Length > 0)
			{
				if(fromRgbaData.Length >= Convert.ToInt32(toTarget.Region.Area * 4))
				{
					var toContext = Resources.ResolveCanvas(toTarget);

					OnCopy(fromRgbaData, toContext);

					return;
				}

				throw new InvalidOperationException("Insufficient data provided!");
			}
		}

		void IResourceHelpers.Copy(Canvas fromTarget, Canvas toTarget)
		{
			if(!fromTarget.IsEmpty)
			{
				Resources.Copy(fromTarget.Region, fromTarget, toTarget);
			}
		}

		IEnumerable<ISurface2D> IResourceHelpers.GetPages(SurfaceUsage usage)
		{
			return OnGetSurfaces(usage);
		}

		event Action<IEnumerable<Canvas>> IResourceHelpers.CanvasInvalidated
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

		Size IResourceHelpers.PageSize
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

		Canvas.ResolvedContext IResourceHelpers.ResolveCanvas(Canvas target)
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

			Resources.ForgetCanvas(target);

			return null;
		}

		void IResourceHelpers.ForgetCanvas(Canvas target)
		{
			Canvas.ResolvedContext context = target.BackingContext;

			if(context != null)
			{
				OnForget(context);

				target.BackingContext = null;
			}
		}

		Outline IResourceHelpers.GetGlyphOutline(
			IndexedRange glyphRange,
			bool isVertical,
			bool isRightToLeft,
			FontHandle fontHandle,
			params Shaper.Glyph[] glyphs)
		{
			return OnGetGlyphOutline(
				glyphRange, isVertical, isRightToLeft, fontHandle, glyphs);
		}

		FontMetrics IResourceHelpers.GetFontMetrics(FontHandle fontHandle)
		{
			return OnGetFontMetrics(fontHandle);
		}

		void IResourceHelpers.RegisterEffect<T>()
		{
			_EffectCollection.Register<T>();
		}

		Effect<T> IResourceHelpers.FindEffect<T>()
		{
			return _EffectCollection.Find<T>();
		}

		void IResourceHelpers.UnregisterEffect<T>()
		{
			_EffectCollection.Unregister<T>();
		}

		public abstract void ProcessFrame();

		protected abstract void OnCopy(
			Rectangle fromRegion,
			Canvas.ResolvedContext fromTarget,
			Canvas.ResolvedContext toTarget);

		protected abstract void OnCopy(
			byte[] rgbaData, Canvas.ResolvedContext toTarget);

		protected abstract Point OnDeterminePoint(
			Figure path, float length, float tolerance, out Point tangentVector);

		protected abstract float OnMeasureLength(Figure path, float tolerance);

		protected abstract float OnMeasureArea(Figure path, float tolerance);

		protected abstract void OnTessellate(
			Figure path, float tolerance, ITessellationSink sink);

		protected abstract Figure OnCombine(
			Figure sourcePath,
			Figure destinationPath,
			float tolerance,
			CombinationOperation operation);

		protected abstract Rectangle OnMeasureRegion(Figure path);

		protected abstract Figure OnWiden(
			Figure path, float width, float tolerance);

		protected abstract Figure OnSimplify(Figure path, float tolerance);

		protected abstract bool OnContains(
			Figure path, Point point, float tolerance);

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

		protected abstract Outline OnGetGlyphOutline(
			IndexedRange glyphRange,
			bool isVertical,
			bool isRightToLeft,
			FontHandle fontHandle,
			params Shaper.Glyph[] glyphs);

		protected abstract FontMetrics OnGetFontMetrics(FontHandle fontHandle);
	}
}