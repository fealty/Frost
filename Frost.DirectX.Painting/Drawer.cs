// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.DirectX.Common;

using SharpDX;
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

		public void Begin(Canvas.ResolvedContext target)
		{
			Contract.Requires(target != null);
			Contract.Assert(_Target == null);
			
			Surface2D surface = target.Surface2D as Surface2D;

			if (surface != null)
			{
				_Target = surface.Target2D;
				_TargetRegion = target.Region;
				
				_Target.BeginDraw();

				return;
			}

			throw new InvalidOperationException();
		}

		public void End()
		{
			Contract.Ensures(_Target == null);
			Contract.Assert(_Target != null);

			_Target.EndDraw();

			_Target = null;
		}

		public void Clear()
		{
			Contract.Assert(_Target != null);

			_Target.Clear(Color.Transparent.ToColor4());
		}

		public void Clear(Rectangle region)
		{
			Contract.Assert(_Target != null);

			// matches the effective surface region; clear everything
			if(region.Location.Equals(_TargetRegion.Location))
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

			RectangleF roundedRegion = RectangleF.Empty;
			
			roundedRegion.Left = newRegion.Left;
			roundedRegion.Top = newRegion.Top;
			roundedRegion.Right = newRegion.Right;
			roundedRegion.Bottom = newRegion.Bottom;

			_Target.PushAxisAlignedClip(roundedRegion, AntialiasMode.Aliased);

			try
			{
				// clear only part of the surface as specified by newRegion
				_Target.Clear(Color.Transparent.ToColor4());
			}
			finally
			{
				_Target.PopAxisAlignedClip();
			}
		}

		public void Stroke(Rectangle rectangleRegion, Brush brush, StrokeStyle style, float strokeWidth)
		{
			Contract.Requires(brush != null);
			Contract.Requires(style != null);
			Contract.Requires(Check.IsPositive(strokeWidth));
			Contract.Assert(_Target != null);

			Rectangle newRegion;

			ToStrokeable(ref rectangleRegion, strokeWidth, strokeWidth, out newRegion);

			FitStroke(ref newRegion, strokeWidth, strokeWidth, out newRegion);

			RectangleF roundedRectangle = RectangleF.Empty;

			roundedRectangle.Left = newRegion.Left;
			roundedRectangle.Top = newRegion.Top;
			roundedRectangle.Right = newRegion.Right;
			roundedRectangle.Bottom = newRegion.Bottom;

			_Target.DrawRectangle(roundedRectangle, brush, strokeWidth, style);
		}

		public void Stroke(
			Point lineStart, Point lineEnd, Brush brush, StrokeStyle style, float strokeWidth)
		{
			Contract.Requires(brush != null);
			Contract.Requires(style != null);
			Contract.Requires(Check.IsPositive(strokeWidth));
			Contract.Assert(_Target != null);

			DrawingPointF start = new DrawingPointF { X = lineStart.X, Y = lineStart.Y };
			DrawingPointF end = new DrawingPointF { X = lineEnd.X, Y = lineEnd.Y };

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
			Contract.Assert(_Target != null);

			Rectangle newRegion;

			ToStrokeable(ref rectangleRegion, strokeWidth, strokeWidth, out newRegion);

			FitStroke(ref newRegion, strokeWidth, strokeWidth, out newRegion);

			RectangleF rectangle = RectangleF.Empty;

			rectangle.Left = newRegion.Left;
			rectangle.Top = newRegion.Top;
			rectangle.Right = newRegion.Right;
			rectangle.Bottom = newRegion.Bottom;

			RoundedRect roundedRectangle = new RoundedRect
			{
				Rect = rectangle,
				RadiusX = roundedRectangleRadius.Width,
				RadiusY = roundedRectangleRadius.Height
			};

			_Target.DrawRoundedRectangle(roundedRectangle, brush, strokeWidth, style);
		}

		public void Fill(Rectangle rectangleRegion, Brush brush)
		{
			Contract.Requires(brush != null);
			Contract.Assert(_Target != null);

			RectangleF roundedRectangle = new RectangleF
			{
				Left = rectangleRegion.Left,
				Top = rectangleRegion.Top,
				Right = rectangleRegion.Right,
				Bottom = rectangleRegion.Bottom
			};

			_Target.FillRectangle(roundedRectangle, brush);
		}

		public void Fill(Rectangle rectangleRegion, Size roundedRectangleRadius, Brush brush)
		{
			Contract.Requires(Check.IsPositive(roundedRectangleRadius.Width));
			Contract.Requires(Check.IsPositive(roundedRectangleRadius.Height));
			Contract.Requires(brush != null);
			Contract.Assert(_Target != null);

			RectangleF rectangle = RectangleF.Empty;

			rectangle.Left = rectangleRegion.Left;
			rectangle.Top = rectangleRegion.Top;
			rectangle.Right = rectangleRegion.Right;
			rectangle.Bottom = rectangleRegion.Bottom;

			RoundedRect roundedRect = new RoundedRect
			{
				Rect = rectangle,
				RadiusX = roundedRectangleRadius.Width,
				RadiusY = roundedRectangleRadius.Height
			};

			_Target.FillRoundedRectangle(roundedRect, brush);
		}

		public void Stroke(Geometry geometry, Brush brush, StrokeStyle style, float strokeWidth)
		{
			Contract.Requires(geometry != null);
			Contract.Requires(brush != null);
			Contract.Requires(style != null);
			Contract.Requires(Check.IsPositive(strokeWidth));
			Contract.Assert(_Target != null);

			DxGeometry resolved = _GeometryCache.ResolveGeometry(geometry);

			Contract.Assert(resolved != null);

			_Target.DrawGeometry(resolved, brush, strokeWidth, style);
		}

		public void Fill(Geometry geometry, Brush brush)
		{
			Contract.Requires(geometry != null);
			Contract.Requires(brush != null);
			Contract.Assert(_Target != null);

			DxGeometry resolved = _GeometryCache.ResolveGeometry(geometry);

			Contract.Assert(resolved != null);

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
			ref Rectangle region, float thicknessX, float thicknessY, out Rectangle result)
		{
			Contract.Requires(Check.IsPositive(thicknessX));
			Contract.Requires(Check.IsPositive(thicknessY));

			float left = thicknessX / 2.0f;
			float top = thicknessY / 2.0f;

			const float right = 0.0f;
			const float bottom = 0.0f;

			Thickness thickness = new Thickness(left, top, right, bottom);

			result = region.Contract(thickness);
		}

		private static void ToStrokeable(
			ref Rectangle region, float thicknessX, float thicknessY, out Rectangle result)
		{
			Contract.Requires(Check.IsPositive(thicknessX));
			Contract.Requires(Check.IsPositive(thicknessY));

			const float left = 0.0f;
			const float top = 0.0f;

			float right = thicknessX / 2.0f;
			float bottom = thicknessY / 2.0f;

			Thickness thickness = new Thickness(left, top, right, bottom);

			result = region.Contract(thickness);
		}
	}
}