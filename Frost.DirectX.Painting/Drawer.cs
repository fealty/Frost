// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Drawing;

using Frost.DirectX.Common;

using SharpDX.Direct2D1;

using Brush = SharpDX.Direct2D1.Brush;
using DxGeometry = SharpDX.Direct2D1.Geometry;
using Geometry = Frost.Shaping.Geometry;
using RectangleF = SharpDX.RectangleF;

namespace Frost.DirectX.Painting
{
	internal sealed class Drawer : IDisposable
	{
		private readonly GeometryCache _GeometryCache;

		private RenderTarget _Target;
		private Rectangle _TargetRegion;

		public Drawer(Factory factory2D)
		{
			Contract.Requires(factory2D != null);

			this._GeometryCache = new GeometryCache(factory2D);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Begin(Canvas target)
		{
			Contract.Requires(Check.IsValid(target));
			Contract.Requires((target.Surface2D as Surface2D) != null);

			Surface2D surface = (Surface2D)target.Surface2D;

			this._Target = surface.Target2D;
			this._TargetRegion = target.Region;

			this._Target.BeginDraw();
		}

		public void End()
		{
			this._Target.EndDraw();

			this._Target = null;
		}

		public void Clear()
		{
			this._Target.Clear(Color.Transparent.ToColor4());
		}

		public void Clear(Rectangle region)
		{
			// matches the effective surface region; clear everything
			if(region.Location.Equals(Point.Empty))
			{
				if(region.Size == this._TargetRegion.Size)
				{
					Clear();

					return;
				}
			}

			Rectangle newRegion = region;

			if(newRegion.Equals(this._TargetRegion))
			{
				Thickness thickness = new Thickness(1.0f);

				// include the pixel-thick border around the region
				newRegion = newRegion.Expand(thickness);
			}

			RectangleF roundedRegion = new RectangleF
			{
				Left = newRegion.Left,
				Top = newRegion.Top,
				Right = newRegion.Right,
				Bottom = newRegion.Bottom
			};

			this._Target.PushAxisAlignedClip(
				roundedRegion, AntialiasMode.Aliased);

			// clear only part of the surface as specified by newRegion
			this._Target.Clear(Color.Transparent.ToColor4());

			this._Target.PopAxisAlignedClip();
		}

		public void Stroke(
			Rectangle rectangleRegion,
			Brush brush,
			StrokeStyle style,
			float strokeWidth)
		{
			Contract.Requires(brush != null);
			Contract.Requires(style != null);
			Contract.Requires(Check.IsPositive(strokeWidth));

			Rectangle newRegion;

			ToStrokeable(
				ref rectangleRegion, strokeWidth, strokeWidth, out newRegion);

			FitStroke(ref newRegion, strokeWidth, strokeWidth, out newRegion);

			RectangleF roundedRectangle = new RectangleF
			{
				Left = newRegion.Left,
				Top = newRegion.Top,
				Right = newRegion.Right,
				Bottom = newRegion.Bottom
			};

			this._Target.DrawRectangle(
				roundedRectangle, brush, strokeWidth, style);
		}

		public void Stroke(
			Point lineStart,
			Point lineEnd,
			Brush brush,
			StrokeStyle style,
			float strokeWidth)
		{
			Contract.Requires(brush != null);
			Contract.Requires(style != null);
			Contract.Requires(Check.IsPositive(strokeWidth));

			PointF start = new PointF {X = lineStart.X, Y = lineStart.Y};
			PointF end = new PointF {X = lineEnd.X, Y = lineEnd.Y};

			this._Target.DrawLine(start, end, brush, strokeWidth, style);
		}

		public void Stroke(
			Rectangle rectangleRegion,
			Size roundedRectangleRadius,
			Brush brush,
			StrokeStyle style,
			float strokeWidth)
		{
			Contract.Requires(Check.IsPositive(roundedRectangleRadius.Width));
			Contract.Requires(Check.IsPositive(roundedRectangleRadius.Height));
			Contract.Requires(brush != null);
			Contract.Requires(style != null);
			Contract.Requires(Check.IsPositive(strokeWidth));

			Rectangle newRegion;

			ToStrokeable(
				ref rectangleRegion, strokeWidth, strokeWidth, out newRegion);

			FitStroke(ref newRegion, strokeWidth, strokeWidth, out newRegion);

			RoundedRect roundedRectangle = new RoundedRect
			{
				Rect =
					new RectangleF(
						newRegion.Left, newRegion.Top, newRegion.Right, newRegion.Bottom),
				RadiusX = roundedRectangleRadius.Width,
				RadiusY = roundedRectangleRadius.Height
			};

			this._Target.DrawRoundedRectangle(
				roundedRectangle, brush, strokeWidth, style);
		}

		public void Fill(Rectangle rectangleRegion, Brush brush)
		{
			Contract.Requires(brush != null);

			RectangleF roundedRectangle = new RectangleF
			{
				Left = rectangleRegion.Left,
				Top = rectangleRegion.Top,
				Right = rectangleRegion.Right,
				Bottom = rectangleRegion.Bottom
			};

			this._Target.FillRectangle(roundedRectangle, brush);
		}

		public void Fill(
			Rectangle rectangleRegion, Size roundedRectangleRadius, Brush brush)
		{
			Contract.Requires(Check.IsPositive(roundedRectangleRadius.Width));
			Contract.Requires(Check.IsPositive(roundedRectangleRadius.Height));
			Contract.Requires(brush != null);

			RoundedRect rectangle = new RoundedRect
			{
				Rect =
					new RectangleF(
						rectangleRegion.Left,
						rectangleRegion.Top,
						rectangleRegion.Right,
						rectangleRegion.Bottom),
				RadiusX = roundedRectangleRadius.Width,
				RadiusY = roundedRectangleRadius.Height
			};

			this._Target.FillRoundedRectangle(rectangle, brush);
		}

		public void Stroke(
			Geometry geometry,
			Brush brush,
			StrokeStyle style,
			float strokeWidth)
		{
			Contract.Requires(geometry != null);
			Contract.Requires(brush != null);
			Contract.Requires(style != null);
			Contract.Requires(Check.IsPositive(strokeWidth));

			DxGeometry resolved = this._GeometryCache.ResolveGeometry(geometry);

			this._Target.DrawGeometry(resolved, brush, strokeWidth, style);
		}

		public void Fill(Geometry geometry, Brush brush)
		{
			Contract.Requires(geometry != null);
			Contract.Requires(brush != null);

			DxGeometry resolved = this._GeometryCache.ResolveGeometry(geometry);

			this._Target.FillGeometry(resolved, brush);
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				this._GeometryCache.Dispose();
			}
		}

		private static void FitStroke(
			ref Rectangle region,
			float thicknessX,
			float thicknessY,
			out Rectangle result)
		{
			Thickness thickness = new Thickness(
				thicknessX / 2.0f, thicknessY / 2.0f, 0.0f, 0.0f);

			result = region.Contract(thickness);
		}

		private static void ToStrokeable(
			ref Rectangle region,
			float thicknessX,
			float thicknessY,
			out Rectangle result)
		{
			Thickness thickness = new Thickness(
				0.0f, 0.0f, thicknessX / 2.0f, thicknessY / 2.0f);

			result = region.Contract(thickness);
		}
	}
}