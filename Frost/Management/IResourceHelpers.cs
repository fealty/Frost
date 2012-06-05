// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Collections;
using Frost.Effects;
using Frost.Management.Contracts;
using Frost.Formatting;
using Frost.Surfacing;

namespace Frost.Management
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IResourceHelpers))]
		internal abstract class IResourceHelpersContract
			: IResourceHelpers
		{
			public void Copy(Rectangle fromRegion, Canvas fromTarget, Canvas toTarget)
			{
				Contract.Requires(fromTarget != null);
				Contract.Requires(toTarget != null);
			}

			public void Copy(byte[] fromRgbaData, Canvas toTarget)
			{
				Contract.Requires(fromRgbaData != null);
				Contract.Requires(toTarget != null);
			}

			public void Copy(Canvas fromTarget, Canvas toTarget)
			{
				Contract.Requires(fromTarget != null);
				Contract.Requires(toTarget != null);
			}

			public IEnumerable<ISurface2D> GetPages(SurfaceUsage usage)
			{
				Contract.Ensures(Contract.Result<IEnumerable<ISurface2D>>() != null);

				throw new NotSupportedException();
			}

			public abstract event Action<IEnumerable<Canvas>> CanvasInvalidated;

			public GlyphOutline GetGlyphOutline(
				IndexedRange glyphRange,
				bool isVertical,
				bool isRightToLeft,
				FontId fontId,
				List<TextShaper.Glyph> glyphs)
			{
				Contract.Requires(!(isVertical && isRightToLeft));
				Contract.Requires(glyphs != null);

				throw new NotSupportedException();
			}

			public FontMetrics GetFontMetrics(FontId fontId)
			{
				throw new NotSupportedException();
			}

			public abstract void RegisterEffect<T>() where T : Effect, new();

			public abstract Effect<T> FindEffect<T>()
				where T : struct, IEffectSettings, IEquatable<T>;

			public abstract void UnregisterEffect<T>();

			public Size PageSize
			{
				set
				{
					Contract.Requires(Check.IsPositive(value.Width));
					Contract.Requires(Check.IsPositive(value.Height));
				}
			}

			public Canvas.ResolvedContext ResolveCanvas(Canvas target)
			{
				Contract.Requires(target != null);

				throw new NotSupportedException();
			}

			public void ForgetCanvas(Canvas target)
			{
				Contract.Requires(target != null);
			}
		}
	}

	[ContractClass(typeof(IResourceHelpersContract))]
	public interface IResourceHelpers
	{
		Size PageSize { set; }

		void Copy(Rectangle fromRegion, Canvas fromTarget, Canvas toTarget);

		void Copy(byte[] fromRgbaData, Canvas toTarget);

		void Copy(Canvas fromTarget, Canvas toTarget);

		IEnumerable<ISurface2D> GetPages(SurfaceUsage usage);

		Canvas.ResolvedContext ResolveCanvas(Canvas target);

		void ForgetCanvas(Canvas target);

		GlyphOutline GetGlyphOutline(
			IndexedRange glyphRange,
			bool isVertical,
			bool isRightToLeft,
			FontId fontId,
			List<TextShaper.Glyph> glyphs);

		FontMetrics GetFontMetrics(FontId fontId);

		void RegisterEffect<T>() where T : Effect, new();

		Effect<T> FindEffect<T>() where T : struct, IEffectSettings, IEquatable<T>;

		void UnregisterEffect<T>();
		event Action<IEnumerable<Canvas>> CanvasInvalidated;
	}
}