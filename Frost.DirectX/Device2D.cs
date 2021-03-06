﻿// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;
using Frost.Composition;
using Frost.DirectX.Common;
using Frost.DirectX.Composition;
using Frost.DirectX.Painting;
using Frost.Painting;
using Frost.Shaping;
using Frost.Formatting;
using Frost.Surfacing;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.DirectWrite;

using DxGeometry = SharpDX.Direct2D1.Geometry;
using FontMetrics = Frost.Formatting.FontMetrics;

namespace Frost.DirectX
{
	public sealed class Device2D : Frost.Device2D, IDisposable
	{
		public const int DeferredCacheLimit = 10;

		// DeferredCacheLimit = 10
		// deferred-cache-limit = 10

		private readonly CombinationSink _CombinationSink;
		private readonly CompositionDevice _CompositionDevice;

		private readonly SafeList<SharedSurface> _DefaultSurfaces;
		private readonly PaintingDevice _DrawingDevice;
		private readonly SafeList<DynamicSurface> _DynamicSurfaces;
		private readonly SafeList<ExternalSurface> _ExternalSurfaces;
		private readonly FontDevice _FontDevice;

		private readonly GeometryCache _GeometryCache;
		private readonly TextGeometryCache _TextGeometryCache;
		private readonly SafeList<Canvas> _InvalidatedResources;

		private readonly object _Lock = new object();
		private readonly SafeList<PrivateSurface> _PrivateSurfaces;

		private readonly SimplificationSink _SimplificationSink;
		private readonly StagingTexture _StagingTexture;
		private readonly TessellationSink _TessellationSink;
		private readonly WideningSink _WideningSink;

		private readonly TextTextShaper _TextShaper;

		private Size _DefaultAtlasSize;
		private Size _DynamicAtlasSize;

		private long _LastInvalidationTickTime;

		public Device2D() : this(null)
		{
		}

		public Device2D(Adapter1 adapter)
		{
			_DefaultSurfaces = new SafeList<SharedSurface>();
			_ExternalSurfaces = new SafeList<ExternalSurface>();
			_DynamicSurfaces = new SafeList<DynamicSurface>();
			_PrivateSurfaces = new SafeList<PrivateSurface>();
			_InvalidatedResources = new SafeList<Canvas>();

			_CompositionDevice = new CompositionDevice(adapter, this);
			_DrawingDevice = new PaintingDevice(this, _CompositionDevice.Device3D);
			_FontDevice = new FontDevice();
			_TextGeometryCache = new TextGeometryCache();
			_TextShaper = new TextTextShaper(this, _FontDevice);

			_GeometryCache = new GeometryCache(_DrawingDevice.Factory2D);

			_SimplificationSink = new SimplificationSink();
			_TessellationSink = new TessellationSink();
			_CombinationSink = new CombinationSink();
			_WideningSink = new WideningSink();

			_StagingTexture = new StagingTexture(_CompositionDevice.Device3D);
		}

		public override Compositor Compositor
		{
			get { return _CompositionDevice.Compositor; }
		}

		public override Formatting.TextShaper TextShaper
		{
			get { return _TextShaper; }
		}

		public override Painter Painter
		{
			get { return _DrawingDevice.Painter; }
		}

		protected override Size PageSize
		{
			set
			{
				lock(_Lock)
				{
					if(!value.Equals(_DynamicAtlasSize))
					{
						_DynamicAtlasSize = value;

						PurgeSurfaces(_DynamicSurfaces, true);

						ReleaseSurfaces(_DynamicSurfaces);
					}

					if(!value.Equals(_DefaultAtlasSize))
					{
						_DefaultAtlasSize = value;

						PurgeSurfaces(_DefaultSurfaces, true);

						ReleaseSurfaces(_DefaultSurfaces);
					}
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public override void ProcessFrame()
		{
			InvalidateResources();

			_DrawingDevice.ProcessTick();
			_CompositionDevice.ProcessTick();
		}

		protected override void OnCopy(
			Rectangle fromRegion,
			Canvas.ResolvedContext fromTarget,
			Canvas.ResolvedContext toTarget)
		{
			fromTarget.Surface2D.CopyTo(
				fromRegion, toTarget.Surface2D, toTarget.Region.Location);
		}

		protected override void OnCopy(
			byte[] rgbaData, Canvas.ResolvedContext toTarget)
		{
			_StagingTexture.UploadData(toTarget.Region.Size, rgbaData);

			var sourceRegion = new Rectangle(Point.Empty, toTarget.Region.Size);

			_StagingTexture.CopyTo(sourceRegion, toTarget);
		}

		protected override Point OnDeterminePoint(
			Shape path, float length, float tolerance, out Point tangentVector)
		{
			lock(_Lock)
			{
				DxGeometry resolvedPath = _GeometryCache.ResolveGeometry(path);

				if(resolvedPath != null)
				{
					DrawingPointF resultVector;

					DrawingPointF resultPoint = resolvedPath.ComputePointAtLength(
						length, tolerance, out resultVector);

					tangentVector = new Point(resultVector.X, resultVector.Y);

					return new Point(resultPoint.X, resultPoint.Y);
				}

				throw new InvalidOperationException("Failed to resolve geometry path!");
			}
		}

		protected override float OnMeasureLength(Shape path, float tolerance)
		{
			lock(_Lock)
			{
				DxGeometry resolvedPath = _GeometryCache.ResolveGeometry(path);

				if(resolvedPath != null)
				{
					return resolvedPath.ComputeLength(null, tolerance);
				}

				throw new InvalidOperationException("Failed to resolve geometry path!");
			}
		}

		protected override float OnMeasureArea(Shape path, float tolerance)
		{
			lock(_Lock)
			{
				DxGeometry resolvedPath = _GeometryCache.ResolveGeometry(path);

				if(resolvedPath != null)
				{
					return resolvedPath.ComputeArea(null, tolerance);
				}

				throw new InvalidOperationException("Failed to resolve geometry path!");
			}
		}

		protected override void OnTessellate(
			Shape path, float tolerance, ITessellationSink sink)
		{
			lock(_Lock)
			{
				DxGeometry resolvedPath = _GeometryCache.ResolveGeometry(path);

				if(resolvedPath != null)
				{
					_TessellationSink.Tessellate(resolvedPath, sink, tolerance);
				}

				throw new InvalidOperationException("Failed to resolve geometry path!");
			}
		}

		protected override Shape OnCombine(
			Shape sourcePath,
			Shape destinationPath,
			float tolerance,
			CombinationOperation operation)
		{
			lock(_Lock)
			{
				DxGeometry resolvedDestination =
					_GeometryCache.ResolveGeometry(destinationPath);

				if(resolvedDestination != null)
				{
					DxGeometry resolvedSource = _GeometryCache.ResolveGeometry(sourcePath);

					if(resolvedSource != null)
					{
						return _CombinationSink.CreateCombination(
							resolvedDestination, resolvedSource, operation, tolerance);
					}

					throw new InvalidOperationException(
						"Failed to resolve source geometry path!");
				}

				throw new InvalidOperationException(
					"Failed to resolve destination geometry path!");
			}
		}

		protected override Rectangle OnMeasureRegion(Shape path)
		{
			lock(_Lock)
			{
				DxGeometry resolvedPath = _GeometryCache.ResolveGeometry(path);

				if(resolvedPath != null)
				{
					RectangleF dxResult = resolvedPath.GetBounds();

					return new Rectangle(
						dxResult.Left, dxResult.Top, dxResult.Width, dxResult.Height);
				}

				throw new InvalidOperationException("Failed to resolve geometry path!");
			}
		}

		protected override Shape OnWiden(
			Shape path, float width, float tolerance)
		{
			lock(_Lock)
			{
				DxGeometry resolvedPath = _GeometryCache.ResolveGeometry(path);

				if(resolvedPath != null)
				{
					return _WideningSink.CreateWidened(resolvedPath, width, tolerance);
				}

				throw new InvalidOperationException("Failed to resolve geometry path!");
			}
		}

		protected override Shape OnSimplify(Shape path, float tolerance)
		{
			lock(_Lock)
			{
				DxGeometry resolvedPath = _GeometryCache.ResolveGeometry(path);

				if(resolvedPath != null)
				{
					return _SimplificationSink.CreateSimplification(resolvedPath, tolerance);
				}

				throw new InvalidOperationException("Failed to resolve geometry path!");
			}
		}

		protected override bool OnContains(
			Shape path, Point point, float tolerance)
		{
			lock(_Lock)
			{
				DxGeometry resolvedPath = _GeometryCache.ResolveGeometry(path);

				if(resolvedPath != null)
				{
					return resolvedPath.FillContainsPoint(point.ToPointF(), tolerance);
				}

				throw new InvalidOperationException("Failed to resolve geometry path!");
			}
		}

		protected override IEnumerable<ISurface2D> OnGetSurfaces(SurfaceUsage usage)
		{
			lock(_Lock)
			{
				switch(usage)
				{
					case SurfaceUsage.Dynamic:
						return DumpAtlases(_DynamicSurfaces);
					case SurfaceUsage.External:
						return DumpAtlases(_ExternalSurfaces);
					case SurfaceUsage.Private:
						return DumpAtlases(_PrivateSurfaces);
					default:
						return DumpAtlases(_DefaultSurfaces);
				}
			}
		}

		protected override void OnForget(Canvas.ResolvedContext target)
		{
			ISurfaceAtlas atlas = target.Surface2D as ISurfaceAtlas;

			if(atlas != null)
			{
				atlas.Forget(target);
			}
		}

		protected override Canvas.ResolvedContext OnResolve(Canvas target)
		{
			return CreateCanvas(target.Region.Size, target);
		}

		protected override GlyphOutline OnGetGlyphOutline(
			IndexedRange glyphRange,
			bool isVertical,
			bool isRightToLeft,
			Formatting.FontId fontId,
			List<TextShaper.Glyph> glyphs)
		{
			lock (_Lock)
			{
				FontHandle handle = _FontDevice.FindFont(
					fontId.Family,
					fontId.Style,
					fontId.Weight,
					fontId.Stretch);

				FontFace face = handle.ResolveFace();

				float emSize = face.Metrics.Ascent + face.Metrics.Descent;

				float baseline = face.Metrics.Ascent / emSize;

				Shape shape = _TextGeometryCache.Retrieve(
					glyphRange, isVertical, isRightToLeft, handle, glyphs);

				return new GlyphOutline(shape, baseline);
			}
		}

		protected override FontMetrics OnGetFontMetrics(
			Formatting.FontId fontId)
		{
			lock (_Lock)
			{
				FontHandle handle = _FontDevice.FindFont(
					fontId.Family,
					fontId.Style,
					fontId.Weight,
					fontId.Stretch);

				var metrics = handle.ResolveFace().Metrics;

				return new FontMetrics(
					metrics.Ascent, metrics.Descent, metrics.DesignUnitsPerEm, fontId);
			}
		}

		private IEnumerable<T> DumpAtlases<T>(SafeList<T> atlasCollections)
			where T : ISurfaceAtlas
		{
			Contract.Requires(atlasCollections != null);

			foreach(ISurfaceAtlas atlas in atlasCollections)
			{
				MarkAtlas(atlas);
			}

			return atlasCollections.ToArray();
		}

		private Canvas.ResolvedContext CreateCanvas(Size size, Canvas target)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			SurfaceUsage usage = target.Usage;

			Size atlasSize = Size.MaxValue;

			lock(_Lock)
			{
				switch(usage)
				{
					case SurfaceUsage.Dynamic:
						atlasSize = _DynamicAtlasSize;
						break;
					case SurfaceUsage.Normal:
						atlasSize = _DefaultAtlasSize;
						break;
				}
			}

			if(size.Width >= atlasSize.Width || size.Height >= atlasSize.Height)
			{
				usage = SurfaceUsage.Private;
			}

			if(size.Width > 0.0 && size.Height > 0.0)
			{
				switch(usage)
				{
					case SurfaceUsage.Dynamic:
						return InsertIntoDynamicAtlas(size, target);
					case SurfaceUsage.External:
						return AddToExternalCollection(size, target);
					case SurfaceUsage.Private:
						return AddToPrivateCollection(size, target);
					default:
						return InsertIntoDefaultAtlas(size, target);
				}
			}

			return null;
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				ReleaseSurfaces(_DefaultSurfaces);
				ReleaseSurfaces(_DynamicSurfaces);
				ReleaseSurfaces(_ExternalSurfaces);
				ReleaseSurfaces(_PrivateSurfaces);

				_StagingTexture.SafeDispose();

				_WideningSink.Dispose();
				_SimplificationSink.Dispose();
				_TessellationSink.Dispose();
				_CombinationSink.Dispose();

				_CompositionDevice.Dispose();
				_DrawingDevice.Dispose();
				_TextShaper.Dispose();
				_FontDevice.Dispose();

				_TextGeometryCache.Dispose();
				_GeometryCache.Dispose();
			}
		}

		private void InvalidateResources()
		{
			long tickTime = DateTime.UtcNow.Ticks;

			if(Math.Abs(tickTime - _LastInvalidationTickTime) >
				(TimeSpan.TicksPerMillisecond * 500))
			{
				PurgeSurfaces(_DefaultSurfaces, false);
				PurgeSurfaces(_DynamicSurfaces, false);

				_LastInvalidationTickTime = tickTime;
			}

			PurgeSurfaces(_ExternalSurfaces, false);
			PurgeSurfaces(_PrivateSurfaces, false);

			foreach(DynamicSurface surface in _DynamicSurfaces)
			{
				surface.Purge(true, _InvalidatedResources);

				surface.Clear();
			}

			foreach(SharedSurface surface in _DefaultSurfaces)
			{
				if(surface.Fragmentation >= 25.0f)
				{
					surface.Purge(true, _InvalidatedResources);
				}
			}

			DispatchInvalidated(_InvalidatedResources);

			_InvalidatedResources.Clear();
		}

		private void PurgeSurfaces<T>(SafeList<T> atlasCollection, bool isForced)
			where T : ISurfaceAtlas
		{
			Contract.Requires(atlasCollection != null);

			using(var context = atlasCollection.AcquireLock())
			{
				for(int i = 0; i < context.Count; ++i)
				{
					T item = context.GetItem(i);

					item.Purge(isForced, _InvalidatedResources);

					if(!item.InUse)
					{
						item.Dispose();

						i = context.RemoveAt(i);
					}
				}
			}
		}

		private void MarkAtlas(ISurfaceAtlas atlas)
		{
			Contract.Requires(atlas != null);

			Painter.Begin(atlas.SurfaceCanvas, Retention.RetainData);

			Painter.IsAntialiased = Antialiasing.Aliased;
			Painter.LineStyle = LineStyle.Dash;

			Painter.SetBrush(Color.Magenta);

			foreach(Rectangle region in atlas.UsedRegions)
			{
				Painter.StrokeRectangle(region);
			}

			Painter.SetBrush(Color.Cyan);

			foreach(Rectangle region in atlas.FreeRegions)
			{
				Painter.StrokeRectangle(region);
			}

			Painter.End();
		}

		private static void ReleaseSurfaces<T>(SafeList<T> atlasCollection)
			where T : ISurfaceAtlas
		{
			Contract.Requires(atlasCollection != null);

			foreach(T item in atlasCollection)
			{
				item.SafeDispose();
			}

			atlasCollection.Clear();
		}

		private static Canvas.ResolvedContext AddNewSurfaceAtlas<T>(
			Canvas target,
			ref Size canvasSize,
			ref Size surfaceSize,
			SafeList<T> atlasCollection,
			Func<Size, T> surfaceConstructor) where T : ISurface2D, ISurfaceAtlas
		{
			Contract.Requires(target != null);
			Contract.Requires(Check.IsPositive(canvasSize.Width));
			Contract.Requires(Check.IsPositive(canvasSize.Height));
			Contract.Requires(Check.IsPositive(surfaceSize.Width));
			Contract.Requires(Check.IsPositive(surfaceSize.Height));
			Contract.Requires(atlasCollection != null);
			Contract.Requires(surfaceConstructor != null);

			// create a new surface to add to the atlas
			T surface2D = surfaceConstructor(surfaceSize);

			using(var context = atlasCollection.AcquireLock())
			{
				// give the new atlas to the atlas collection
				atlasCollection.Add(surface2D);

				try
				{
					return surface2D.AcquireRegion(canvasSize, target);
				}
				catch
				{
					// failure: rollback change to atlas collection
					context.RemoveLast();

					throw;
				}
			}
		}

		private Canvas.ResolvedContext AddToExternalCollection(
			Size canvasSize, Canvas target)
		{
			Contract.Requires(canvasSize.Area > 0);
			Contract.Requires(Check.IsPositive(canvasSize.Width));
			Contract.Requires(Check.IsPositive(canvasSize.Height));
			Contract.Requires(target != null);

			return AddNewSurfaceAtlas(
				target,
				ref canvasSize,
				ref canvasSize,
				_ExternalSurfaces,
				CreateExternalSurface);
		}

		private Canvas.ResolvedContext AddToPrivateCollection(
			Size canvasSize, Canvas target)
		{
			Contract.Requires(canvasSize.Area > 0);
			Contract.Requires(Check.IsPositive(canvasSize.Width));
			Contract.Requires(Check.IsPositive(canvasSize.Height));
			Contract.Requires(target != null);

			return AddNewSurfaceAtlas(
				target,
				ref canvasSize,
				ref canvasSize,
				_PrivateSurfaces,
				CreatePrivateSurface);
		}

		private static Canvas.ResolvedContext InsertIntoAtlas<T>(
			Size canvasSize, SafeList<T> atlasCollection, Canvas target)
			where T : ISurfaceAtlas
		{
			Contract.Requires(Check.IsPositive(canvasSize.Width));
			Contract.Requires(Check.IsPositive(canvasSize.Height));
			Contract.Requires(atlasCollection != null);
			Contract.Requires(target != null);

			foreach(T item in atlasCollection)
			{
				Canvas.ResolvedContext canvas = item.AcquireRegion(canvasSize, target);

				if(canvas != null)
				{
					return canvas;
				}
			}

			return null;
		}

		private Canvas.ResolvedContext InsertIntoDefaultAtlas(
			Size canvasSize, Canvas target)
		{
			Contract.Requires(canvasSize.Area > 0);
			Contract.Requires(Check.IsPositive(canvasSize.Width));
			Contract.Requires(Check.IsPositive(canvasSize.Height));
			Contract.Requires(target != null);

			Canvas.ResolvedContext canvas = InsertIntoAtlas(
				canvasSize, _DefaultSurfaces, target);

			if(canvas == null)
			{
				lock(_Lock)
				{
					return AddNewSurfaceAtlas(
						target,
						ref canvasSize,
						ref _DefaultAtlasSize,
						_DefaultSurfaces,
						CreateDefaultSurface);
				}
			}

			return canvas;
		}

		private Canvas.ResolvedContext InsertIntoDynamicAtlas(
			Size canvasSize, Canvas target)
		{
			Contract.Requires(canvasSize.Area > 0);
			Contract.Requires(Check.IsPositive(canvasSize.Width));
			Contract.Requires(Check.IsPositive(canvasSize.Height));
			Contract.Requires(target != null);

			Canvas.ResolvedContext canvas = InsertIntoAtlas(
				canvasSize, _DynamicSurfaces, target);

			if(canvas == null)
			{
				lock(_Lock)
				{
					return AddNewSurfaceAtlas(
						target,
						ref canvasSize,
						ref _DynamicAtlasSize,
						_DynamicSurfaces,
						CreateDynamicSurface);
				}
			}

			return canvas;
		}

		private DynamicSurface CreateDynamicSurface(Size surfaceSize)
		{
			Contract.Requires(surfaceSize.Area > 0);
			Contract.Requires(Check.IsPositive(surfaceSize.Width));
			Contract.Requires(Check.IsPositive(surfaceSize.Height));
			Contract.Ensures(Contract.Result<ISurface2D>() != null);

			Surface2D.Description description;

			description.Size = surfaceSize;
			description.Device3D = _CompositionDevice.Device3D;
			description.Usage = SurfaceUsage.Dynamic;
			description.Device2D = this;
			description.Factory2D = _DrawingDevice.Factory2D;

			return new DynamicSurface(ref description);
		}

		private ExternalSurface CreateExternalSurface(Size surfaceSize)
		{
			Contract.Requires(surfaceSize.Area > 0);
			Contract.Requires(Check.IsPositive(surfaceSize.Width));
			Contract.Requires(Check.IsPositive(surfaceSize.Height));
			Contract.Ensures(Contract.Result<ISurface2D>() != null);

			Surface2D.Description description;

			description.Size = surfaceSize;
			description.Device3D = _CompositionDevice.Device3D;
			description.Usage = SurfaceUsage.External;
			description.Device2D = this;
			description.Factory2D = _DrawingDevice.Factory2D;

			return new ExternalSurface(ref description);
		}

		private PrivateSurface CreatePrivateSurface(Size surfaceSize)
		{
			Contract.Requires(surfaceSize.Area > 0);
			Contract.Requires(Check.IsPositive(surfaceSize.Width));
			Contract.Requires(Check.IsPositive(surfaceSize.Height));
			Contract.Ensures(Contract.Result<ISurface2D>() != null);

			Surface2D.Description description;

			description.Size = surfaceSize;
			description.Device3D = _CompositionDevice.Device3D;
			description.Usage = SurfaceUsage.Normal;
			description.Device2D = this;
			description.Factory2D = _DrawingDevice.Factory2D;

			return new PrivateSurface(ref description);
		}

		private SharedSurface CreateDefaultSurface(Size surfaceSize)
		{
			Contract.Requires(surfaceSize.Area > 0);
			Contract.Requires(Check.IsPositive(surfaceSize.Width));
			Contract.Requires(Check.IsPositive(surfaceSize.Height));
			Contract.Ensures(Contract.Result<ISurface2D>() != null);

			Surface2D.Description description;

			description.Size = surfaceSize;
			description.Device3D = _CompositionDevice.Device3D;
			description.Usage = SurfaceUsage.Normal;
			description.Device2D = this;
			description.Factory2D = _DrawingDevice.Factory2D;

			return new SharedSurface(ref description);
		}

		//protected override event Action<Canvas> ResourceInvalidated;
	}
}