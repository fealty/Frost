// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics;
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
			this._CounterCollection = new DeviceCounterCollection();
			this._EffectCollection = new EffectCollection();
		}

		public static float FlatteningTolerance
		{
			get { return _FlatteningTolerance; }
		}

		public DeviceCounterCollection Diagnostics
		{
			get { return this._CounterCollection; }
		}

		public EffectCollection Effects
		{
			get { return this._EffectCollection; }
		}

		public abstract Painter Painter { get; }
		public abstract Compositor Compositor { get; }

		public void ResizeSurfaces(Size size, SurfaceUsage usage)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			OnResizeSurfaces(size, usage);
		}

		public void DumpSurfaces(string path, SurfaceUsage usage)
		{
			Contract.Requires(!string.IsNullOrWhiteSpace(path));

			OnDumpSurfaces(path, usage);
		}

		public abstract void SignalUpdate();

		public Canvas CreateCanvas(
			Size size,
			SurfaceUsage usage = SurfaceUsage.Normal,
			object owner = null)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));

			return OnCreateCanvas(size, usage, owner);
		}

		public Canvas CreateCanvas(
			Size size,
			byte[] data,
			SurfaceUsage usage = SurfaceUsage.Normal,
			object owner = null)
		{
			Contract.Requires(Check.IsPositive(size.Width));
			Contract.Requires(Check.IsPositive(size.Height));
			Contract.Requires(data != null);

			return OnCreateCanvas(size, data, usage, owner);
		}

		public abstract bool ContainsPoint(
			Geometry path, Point point, float tolerance = _FlatteningTolerance);

		public abstract Geometry Simplify(
			Geometry path, float tolerance = _FlatteningTolerance);

		public abstract Geometry Widen(
			Geometry path, float width, float tolerance = _FlatteningTolerance);

		public abstract Rectangle ComputeRegion(Geometry path);

		public abstract FontMetrics Measure(
			string family,
			FontWeight weight,
			FontStyle style,
			FontStretch stretch);

		public abstract Geometry Combine(
			Geometry sourcePath,
			Geometry destinationPath,
			float tolerance,
			CombinationOperation operation);

		public abstract void Tessellate(
			Geometry path, float tolerance, ITessellationSink sink);

		public abstract ITextMetrics Measure(
			Paragraph paragraph,
			Rectangle region,
			params Rectangle[] obstructions);

		public abstract float ComputeArea(Geometry path, float tolerance);

		public abstract float ComputeLength(Geometry path, float tolerance);

		public abstract Point ComputePointAlongPath(
			Geometry path,
			float lenght,
			float tolerance,
			out Point tangentVector);

		protected abstract Canvas OnCreateCanvas(
			Size size, byte[] data, SurfaceUsage usage, object owner);

		protected abstract Canvas OnCreateCanvas(
			Size size, SurfaceUsage usage, object owner);

		protected abstract void OnResizeSurfaces(
			Size size, SurfaceUsage usage);

		protected abstract void OnDumpSurfaces(
			string path, SurfaceUsage usage);
	}
}