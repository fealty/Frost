// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Frost;
using Frost.Diagnostics;
using Frost.Formatting;

namespace Demo.Framework
{
	internal static class DemoInterface
	{
		private static readonly Stopwatch _Watch;

		static DemoInterface()
		{
			_Watch = new Stopwatch();
		}

		public static void ResetDemo(IDemoContext context, Canvas target, Device2D device2D)
		{
			Contract.Requires(context != null);
			Contract.Requires(target != null);
			Contract.Requires(device2D != null);

			Rectangle headerRegion = ComputeHeaderRegion(target, device2D);

			device2D.Painter.Begin(target);

			// clear the background to the background color
			device2D.Painter.SetBrush(Resources.Background);
			device2D.Painter.FillRectangle(target.Region);

			// draw the header bar
			Thickness barExpansion = new Thickness(0, headerRegion.Height, 0, 0);

			Rectangle headerBar = headerRegion.Expand(barExpansion);

			device2D.Painter.SetBrush(Resources.UIColor);
			device2D.Painter.FillRectangle(headerBar, new Size(headerRegion.Height));

			// generate the settings display
			float menuWidth = GenerateMenu(context, target, device2D);

			device2D.Painter.End();

			_Watch.Reset();

			Rectangle availableRegion = Rectangle.FromEdges(
				menuWidth, headerRegion.Bottom, target.Region.Right, target.Region.Bottom);

			long oldMemoryUsage = GC.GetTotalMemory(true);

			_Watch.Start();
			context.Reset(availableRegion, target, device2D);
			_Watch.Stop();

			long memoryDelta = GC.GetTotalMemory(false) - oldMemoryUsage;

			IDeviceCounter<TimeSpan> compositionFrameDuration;
			IDeviceCounter<TimeSpan> paintingFrameDuration;

			device2D.Diagnostics.Query("Composition", "FrameDuration", out compositionFrameDuration);
			device2D.Diagnostics.Query("Painting", "FrameDuration", out paintingFrameDuration);

			Paragraph stats =
				Paragraph.Create().WithAlignment(Alignment.Center).WithFamily("Lucida Console").
					WithAdditionalText(
						String.Format(
							"Memory: {0:N0} KB  \u25A0  Garbage: {1:N0} KB  \u25A0  Time: {2} ms  \u25A0  Painting: {3} ms  \u25A0  Composition: {4} ms",
							GC.GetTotalMemory(true) / 1024,
							memoryDelta / 1024,
							_Watch.ElapsedMilliseconds,
							paintingFrameDuration.Value.Milliseconds,
							compositionFrameDuration.Value.Milliseconds)).Build();

			ITextMetrics metrics = device2D.MeasureLayout(stats, headerRegion);

			device2D.Painter.Begin(target, Retention.RetainData);

			Paragraph.Draw(device2D.Painter, metrics);

			device2D.Painter.End();
		}

		private static Rectangle ComputeHeaderRegion(Canvas target, Device2D device2D)
		{
			Contract.Requires(target != null);
			Contract.Requires(device2D != null);

			float headerHeight = MeasureHeader(device2D);

			Size headerSize = new Size(target.Region.Width, headerHeight);

			Rectangle region = new Rectangle(Point.Empty, headerSize);

			return region.Contract(new Thickness(headerHeight, 0.0f));
		}

		private static float MeasureHeader(Device2D device2D)
		{
			Contract.Requires(device2D != null);

			FontMetrics metrics = device2D.MeasureFont(
				"Lucida Console", FontWeight.Regular, FontStyle.Regular, FontStretch.Regular);

			float emSize = metrics.MeasureEm(10.0f);

			return emSize + (0.5f * emSize);
		}

		private static float GenerateMenu(IDemoContext context, Canvas target, Device2D device2D)
		{
			Contract.Requires(context != null);
			Contract.Requires(target != null);
			Contract.Requires(device2D != null);

			var items = new List<KeyValuePair<ITextMetrics, bool>>();

			Rectangle region = target.Region;

			// move 25% towards the bottom of the region
			region = region.Translate(0, region.Height * 0.25f);

			float itemHeight = 0.0f;
			float itemSpacing = 0.0f;

			Thickness itemExpansion = Thickness.Empty;

			foreach(var item in context.Settings)
			{
				Paragraph paragraph =
					Paragraph.Create().WithFamily("Lucida Console").WithAdditionalText(
						String.Format("[{0}]", items.Count + 1)).Build();

				var metrics = device2D.MeasureLayout(paragraph, region);

				if(itemHeight.Equals(0.0f) && itemSpacing.Equals(0.0f))
				{
					itemHeight = metrics.TextRegion.Height * 2.00f;
					itemSpacing = metrics.TextRegion.Height * 0.25f;
					itemExpansion = new Thickness(itemHeight / 4.0f);
				}

				items.Add(new KeyValuePair<ITextMetrics, bool>(metrics, item.IsActive));

				region = region.Translate(0, itemHeight + itemSpacing);
			}

			float maxWidth = 0.0f;

			foreach(var item in items)
			{
				Rectangle textRegion = item.Key.TextRegion.Expand(itemExpansion);

				maxWidth = Math.Max(maxWidth, textRegion.Width);
			}

			foreach(var item in items)
			{
				Rectangle settingRegion = item.Key.TextRegion.Expand(itemExpansion);

				settingRegion = settingRegion.Resize(maxWidth, settingRegion.Height);
				settingRegion = settingRegion.Expand(new Thickness(itemHeight, 0, 0, 0));

				device2D.Painter.SetBrush(item.Value ? Resources.ActiveButton : Resources.UIColor);
				device2D.Painter.FillRectangle(settingRegion, new Size(itemHeight / 3.0f));

				device2D.Painter.SetBrush(item.Value ? Resources.InactiveButton : Resources.Foreground);

				Paragraph.Draw(device2D.Painter, item.Key);
			}

			return maxWidth;
		}
	}
}