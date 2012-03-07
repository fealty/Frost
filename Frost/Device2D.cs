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
		private const float _Flattening = 0.25f;

		private readonly DeviceCounterCollection _CounterCollection;
		private readonly EffectCollection _EffectCollection;

		protected Device2D()
		{
			_CounterCollection = new DeviceCounterCollection();
			_EffectCollection = new EffectCollection();
		}

		public static float Flattening
		{
			get { return _Flattening; }
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

			if(!fromTarget.IsEmpty)
			{
				Copy(fromTarget.Region, fromTarget, toTarget);
			}
		}

		public void Copy(Rectangle fromRegion, Canvas fromTarget, Canvas toTarget)
		{
			Contract.Requires(fromTarget != null);
			Contract.Requires(toTarget != null);

			if(!fromTarget.IsEmpty && !fromRegion.IsEmpty)
			{
				if(fromTarget.Region.Contains(fromRegion))
				{
					Rectangle srcRegion = new Rectangle(Point.Empty, fromRegion.Size);

					if(toTarget.Region.Contains(srcRegion))
					{
						Canvas.ResolvedContext fromContext = Resolve(fromTarget);
						Canvas.ResolvedContext toContext = Resolve(toTarget);

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

		public void Copy(byte[] fromRgbaData, Canvas toTarget)
		{
			Contract.Requires(fromRgbaData != null);
			Contract.Requires(toTarget != null);

			if(fromRgbaData.Length > 0)
			{
				if(fromRgbaData.Length >= Convert.ToInt32(toTarget.Region.Area * 4))
				{
					Canvas.ResolvedContext toContext = Resolve(toTarget);

					OnCopy(fromRgbaData, toContext);

					return;
				}

				throw new InvalidOperationException("Insufficient data provided!");
			}
		}

		public void SuggestPageDimensions(Size dimensions)
		{
			Contract.Requires(Check.IsPositive(dimensions.Width));
			Contract.Requires(Check.IsPositive(dimensions.Height));

			if(dimensions.Area > 0)
			{
				OnSuggestPageDimensions(dimensions);

				return;
			}

			throw new InvalidOperationException("Page size insufficient!");
		}

		public void Dump(string path, SurfaceUsage usage)
		{
			OnDump(path, usage);
		}

		public abstract void ProcessTick();

		public bool Contains(Geometry path, Point point, float tolerance = _Flattening)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));

			return OnContains(path, point, tolerance);
		}

		public Geometry Simplify(Geometry path, float tolerance = _Flattening)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return OnSimplify(path, tolerance);
		}

		public Geometry Widen(Geometry path, float width, float tolerance = _Flattening)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return OnWiden(path, width, tolerance);
		}

		public Rectangle MeasureRegion(Geometry path)
		{
			Contract.Requires(path != null);

			return OnMeasureRegion(path);
		}

		public FontMetrics MeasureFont(
			string family, FontWeight weight, FontStyle style, FontStretch stretch)
		{
			if(String.IsNullOrWhiteSpace(family))
			{
				return OnMeasureFont(Paragraph.DefaultFamily, weight, style, stretch);
			}

			return OnMeasureFont(family, weight, style, stretch);
		}

		public Geometry Combine(
			Geometry sourcePath,
			Geometry destinationPath,
			CombinationOperation operation,
			float tolerance = _Flattening)
		{
			Contract.Requires(sourcePath != null);
			Contract.Requires(destinationPath != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			return OnCombine(sourcePath, destinationPath, tolerance, operation);
		}

		public void Tessellate(Geometry path, ITessellationSink sink, float tolerance = _Flattening)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Requires(sink != null);

			OnTessellate(path, tolerance, sink);
		}

		public ITextMetrics MeasureLayout(Paragraph paragraph)
		{
			Contract.Requires(paragraph != null);

			return MeasureLayout(paragraph, new Rectangle(Point.Empty, Size.MaxValue));
		}

		public ITextMetrics MeasureLayout(Paragraph paragraph, Point location)
		{
			Contract.Requires(paragraph != null);

			return MeasureLayout(paragraph, new Rectangle(location, Size.MaxValue));
		}

		public ITextMetrics MeasureLayout(
			Paragraph paragraph, Rectangle region, params Rectangle[] obstructions)
		{
			Contract.Requires(paragraph != null);

			if(!region.IsEmpty)
			{
				return OnMeasureLayout(paragraph, region, obstructions);
			}

			return null;
		}

		public float MeasureArea(Geometry path, float tolerance = _Flattening)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return OnMeasureArea(path, tolerance);
		}

		public float MeasureLength(Geometry path, float tolerance = _Flattening)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

			return OnMeasureLength(path, tolerance);
		}

		public Point DeterminePoint(Geometry path, float length, float tolerance = _Flattening)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(length));
			Contract.Requires(Check.IsPositive(tolerance));

			Point stub;

			return OnDeterminePoint(path, length, tolerance, out stub);
		}

		public Point DeterminePoint(
			Geometry path, float length, out Point tangentVector, float tolerance = _Flattening)
		{
			Contract.Requires(path != null);
			Contract.Requires(Check.IsPositive(length));
			Contract.Requires(Check.IsPositive(tolerance));

			return OnDeterminePoint(path, length, tolerance, out tangentVector);
		}

		public Canvas.ResolvedContext Resolve(Canvas target)
		{
			Contract.Requires(target != null);

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

			Forget(target);

			return null;
		}

		public void Forget(Canvas target)
		{
			Contract.Requires(target != null);

			Canvas.ResolvedContext context = target.BackingContext;

			if(context != null)
			{
				OnForget(context);

				target.BackingContext = null;
			}
		}

		protected abstract void OnCopy(
			Rectangle fromRegion, Canvas.ResolvedContext fromTarget, Canvas.ResolvedContext toTarget);

		protected abstract void OnCopy(byte[] rgbaData, Canvas.ResolvedContext toTarget);

		protected abstract Point OnDeterminePoint(
			Geometry path, float length, float tolerance, out Point tangentVector);

		protected abstract float OnMeasureLength(Geometry path, float tolerance);

		protected abstract float OnMeasureArea(Geometry path, float tolerance);

		protected abstract ITextMetrics OnMeasureLayout(
			Paragraph paragraph, Rectangle region, params Rectangle[] obstructions);

		protected abstract void OnTessellate(Geometry path, float tolerance, ITessellationSink sink);

		protected abstract Geometry OnCombine(
			Geometry sourcePath, Geometry destinationPath, float tolerance, CombinationOperation operation);

		protected abstract FontMetrics OnMeasureFont(
			string family, FontWeight weight, FontStyle style, FontStretch stretch);

		protected abstract Rectangle OnMeasureRegion(Geometry path);

		protected abstract Geometry OnWiden(Geometry path, float width, float tolerance);

		protected abstract Geometry OnSimplify(Geometry path, float tolerance);

		protected abstract bool OnContains(Geometry path, Point point, float tolerance);

		protected abstract void OnSuggestPageDimensions(Size dimensions);

		protected abstract void OnDump(string path, SurfaceUsage usage);

		protected abstract void OnForget(Canvas.ResolvedContext target);

		protected abstract Canvas.ResolvedContext OnResolve(Canvas target);

		public abstract event Action<Canvas> ResourceInvalidated;
	}
}