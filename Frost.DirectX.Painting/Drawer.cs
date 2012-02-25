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

			_GeometryCache = new GeometryCache(factory2D);
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

			_Target = surface.Target2D;
			_TargetRegion = target.Region;

			_Target.BeginDraw();
		}

		public void End()
		{
			_Target.EndDraw();

			_Target = null;
		}

		public void Clear()
		{
			_Target.Clear(Color.Transparent.ToColor4());
		}

		public void Clear(Rectangle region)
		{
			// matches the effective surface region; clear everything
			if(region.Location.Equals(Point.Empty))
			{
				if(region.Size == _TargetRegion.Size)
				{
					Clear();

					return;
				}
			}

			Rectangle newRegion = region;

			if(newRegion.Equals(_TargetRegion))
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

			_Target.PushAxisAlignedClip(
				roundedRegion, AntialiasMode.Aliased);

			// clear only part of the surface as specified by newRegion
			_Target.Clear(Color.Transparent.ToColor4());

			_Target.PopAxisAlignedClip();
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

			_Target.DrawRectangle(
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

			_Target.DrawLine(start, end, brush, strokeWidth, style);
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

			_Target.DrawRoundedRectangle(
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

			_Target.FillRectangle(roundedRectangle, brush);
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

			_Target.FillRoundedRectangle(rectangle, brush);
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

			DxGeometry resolved = _GeometryCache.ResolveGeometry(geometry);

			_Target.DrawGeometry(resolved, brush, strokeWidth, style);
		}

		public void Fill(Geometry geometry, Brush brush)
		{
			Contract.Requires(geometry != null);
			Contract.Requires(brush != null);

			DxGeometry resolved = _GeometryCache.ResolveGeometry(geometry);

			_Target.FillGeometry(resolved, brush);
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_GeometryCache.Dispose();
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