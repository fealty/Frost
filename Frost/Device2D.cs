// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Composition;
using Frost.Diagnostics;
using Frost.Effects;
using Frost.Formatting;
using Frost.Painting;
using Frost.Shaping;
using Frost.Surfacing;

namespace Frost
{
	public abstract class Device2D
	{
		private const float _FlatteningTolerance = 0.25f;

		private readonly DeviceCounterCollection _CounterCollection;
		private readonly EffectCollection _EffectCollection;

		protected Device2D()
		{
			_CounterCollection = new DeviceCounterCollection();
			_EffectCollection = new EffectCollection();
		}

		public static float FlatteningTolerance
		{
			get { return _FlatteningTolerance; }
		}

		public DeviceCounterCollection Diagnostics
		{
			get { return _CounterCollection; }
		}

		public EffectCollection Effects
		{
			get { return _EffectCollection; }
		}

		public abstract Painter Painter { get; }
		public abstract Compositor Compositor { get; }

		public void Copy(Canvas fromTarget, Canvas toTarget)
		{
			Contract.Requires(fromTarget != null);
			Contract.Requires(toTarget != null);
			Contract.Requires(fromTarget.Region.Size == toTarget.Region.Size);

			Copy(fromTarget.Region, fromTarget, toTarget);
		}

		public void Copy(Rectangle fromRegion, Canvas fromTarget, Canvas toTarget)
		{
			Contract.Requires(fromTarget != null);
			Contract.Requires(toTarget != null);
			Contract.Requires(fromTarget.Region.Contains(fromRegion));
			Contract.Requires(fromRegion.Size == toTarget.Region.Size);

			Canvas.ResolvedContext fromContext = ResolveCanvas(fromTarget);
			Canvas.ResolvedContext toContext = ResolveCanvas(toTarget);

			// translate to 2D surface coordinate space
			fromRegion = fromRegion.Translate(fromContext.Region.Location);

			OnCopy(fromRegion, fromContext, toContext);
		}

		public void Copy(byte[] fromRgbaData, Canvas toTarget)
		{
			Contract.Requires(fromRgbaData != null);
			Contract.Requires(toTarget != null);
			Contract.Requires(fromRgbaData.Length >= toTarget.Region.Size.Area * 4);

			Canvas.ResolvedContext toContext = ResolveCanvas(toTarget);

			OnCopy(fromRgbaData, toContext);
		}

		//TODO: SuggestBackingSurfaceSize?
		public void ResizeSurfaces(Size size, SurfaceUsage usage)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			OnResizeSurfaces(size, usage);
		}

		public void DumpSurfaces(string path, SurfaceUsage usage)
		{
			OnDumpSurfaces(path, usage);
		}

		public abstract void SignalUpdate();

		public bool ContainsPoint(Geometry path, Point point, float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));

			return OnContainsPoint(path, point, tolerance);
		}

		public Geometry Simplify(Geometry path, float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return OnSimplify(path, tolerance);
		}

		public Geometry Widen(Geometry path, float width, float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return OnWiden(path, width, tolerance);
		}

		public Rectangle ComputeRegion(Geometry path)
		{
			Contract.Requires(path != null);

			return OnComputeRegion(path);
		}

		public FontMetrics Measure(string family, FontWeight weight, FontStyle style, FontStretch stretch)
		{
			if(string.IsNullOrWhiteSpace(family))
			{
				return OnMeasure(Paragraph.DefaultFamily, weight, style, stretch);
			}

			return OnMeasure(family, weight, style, stretch);
		}

		public Geometry Combine(
			Geometry sourcePath,
			Geometry destinationPath,
			CombinationOperation operation,
			float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(sourcePath != null);
			Contract.Requires(destinationPath != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return OnCombine(sourcePath, destinationPath, tolerance, operation);
		}

		public void Tessellate(
			Geometry path, ITessellationSink sink, float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Requires(sink != null);

			OnTessellate(path, tolerance, sink);
		}

		public ITextMetrics Measure(
			Paragraph paragraph, Rectangle region, params Rectangle[] obstructions)
		{
			Contract.Requires(paragraph != null);
			Contract.Ensures(Contract.Result<ITextMetrics>() != null);

			return OnMeasure(paragraph, region, obstructions);
		}

		public float ComputeArea(Geometry path, float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return OnComputeArea(path, tolerance);
		}

		public float ComputeLength(Geometry path, float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return OnComputeLength(path, tolerance);
		}

		public Point ComputePointAlongPath(
			Geometry path, float length, float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(length));
			Contract.Requires(Check.IsPositive(tolerance));

			Point stub;

			return OnComputePointAlongPath(path, length, tolerance, out stub);
		}

		public Point ComputePointAlongPath(
			Geometry path, float length, out Point tangentVector, float tolerance = _FlatteningTolerance)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(length));
			Contract.Requires(Check.IsPositive(tolerance));

			return OnComputePointAlongPath(path, length, tolerance, out tangentVector);
		}

		protected abstract void OnCopy(
			Rectangle fromRegion, Canvas.ResolvedContext fromTarget, Canvas.ResolvedContext toTarget);

		protected abstract void OnCopy(byte[] rgbaData, Canvas.ResolvedContext toTarget);

		protected abstract Point OnComputePointAlongPath(
			Geometry path, float length, float tolerance, out Point tangentVector);

		protected abstract float OnComputeLength(Geometry path, float tolerance);

		protected abstract float OnComputeArea(Geometry path, float tolerance);

		protected abstract ITextMetrics OnMeasure(
			Paragraph paragraph, Rectangle region, params Rectangle[] obstructions);

		protected abstract void OnTessellate(Geometry path, float tolerance, ITessellationSink sink);

		protected abstract Geometry OnCombine(
			Geometry sourcePath, Geometry destinationPath, float tolerance, CombinationOperation operation);

		protected abstract FontMetrics OnMeasure(
			string family, FontWeight weight, FontStyle style, FontStretch stretch);

		protected abstract Rectangle OnComputeRegion(Geometry path);

		protected abstract Geometry OnWiden(Geometry path, float width, float tolerance);

		protected abstract Geometry OnSimplify(Geometry path, float tolerance);

		protected abstract bool OnContainsPoint(Geometry path, Point point, float tolerance);

		protected abstract void OnResizeSurfaces(Size size, SurfaceUsage usage);

		protected abstract void OnDumpSurfaces(string path, SurfaceUsage usage);

		public Canvas.ResolvedContext ResolveCanvas(Canvas targetResource)
		{
			return OnResolveCanvas(targetResource);
			//TODO: have OnResolveCanvas(ICanvasImpl targetResource?) This would expose the BackingContext
			// Do the same for ForgetCanvas()? IDeviceBinder?
			//throw new NotImplementedException();
		}

		protected abstract Canvas.ResolvedContext OnResolveCanvas(Canvas targetResource);

		public abstract event Action<object> CanvasInvalidated;
	}
}