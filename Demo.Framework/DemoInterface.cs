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

			// create a new canvas for the demo contents
			Canvas demoTarget = new Canvas(target.Region.Size);

			// resolve the new resource to avoid timing problems
			device2D.Resolve(demoTarget);

			// track memory usage and reset the clock
			long oldMemoryUsage = GC.GetTotalMemory(true);

			_Watch.Reset();
			_Watch.Start();

			// execute the demo
			context.Reset(demoTarget, device2D);

			// stop the clock and determine the garbage produced
			_Watch.Stop();

			long memoryDelta = GC.GetTotalMemory(false) - oldMemoryUsage;

			// query the subsystem timers
			IDeviceCounter<TimeSpan> compositionFrameDuration;
			IDeviceCounter<TimeSpan> paintingFrameDuration;

			device2D.Diagnostics.Query("Composition", "FrameDuration", out compositionFrameDuration);
			device2D.Diagnostics.Query("Painting", "FrameDuration", out paintingFrameDuration);

			Paragraph overviewStats = Paragraph.Create()
				.WithFamily("Lucida Console")
				.AddText("Memory: {0:N0} KB", GC.GetTotalMemory(true) / 1024)
				.AddText(" \u2219 ")
				.AddText("Garbage: {0:N0} KB", memoryDelta / 1024)
				.AddText(" \u2219 ")
				.AddText("Time: {0} ms", _Watch.ElapsedMilliseconds)
				.Build();

			Paragraph subsystemStats = Paragraph.Create()
				.WithFamily("Lucida Console")
				.AddText("Painting: {0} ms", paintingFrameDuration.Value.Milliseconds)
				.AddText(" \u2219 ")
				.AddText("Composition: {0} ms", compositionFrameDuration.Value.Milliseconds)
				.Build();

			ITextMetrics overviewMetrics = device2D.MeasureLayout(overviewStats);
			ITextMetrics subsystemMetrics = device2D.MeasureLayout(subsystemStats);

			Point overviewLocation =
				overviewMetrics.TextRegion.AlignRelativeTo(target.Region, Alignment.Center, Axis.Horizontal).
					Location;
			Point subsystemLocation =
				subsystemMetrics.TextRegion.AlignRelativeTo(target.Region, Alignment.Center, Axis.Horizontal).
					Location;

			float lineHeight = overviewMetrics.TextRegion.Height + overviewMetrics.Leading;

			subsystemLocation = subsystemLocation.Translate(0, lineHeight);

			device2D.Painter.Begin(target);

			// clear the background to the background color
			device2D.Painter.SetBrush(Resources.Background);
			device2D.Painter.FillRectangle(target.Region);

			device2D.Painter.SetBrush(Resources.UIColor);

			Rectangle statsPanel = target.Region.Contract(
				overviewMetrics.TextRegion.Height, 0);

			float overviewX = Math.Max(statsPanel.X, overviewLocation.X);
			float subsystemX = Math.Max(statsPanel.X + lineHeight, subsystemLocation.X);

			overviewLocation = new Point(overviewX, overviewLocation.Y);
			subsystemLocation = new Point(subsystemX, subsystemLocation.Y);

			float idealWidth = Math.Max(statsPanel.Width, overviewMetrics.TextRegion.Width);

			statsPanel = statsPanel.Resize(idealWidth, lineHeight * 2f);

			statsPanel = statsPanel.Expand(0, lineHeight, 0, 0);

			device2D.Painter.FillRectangle(statsPanel, new Size(lineHeight));

			device2D.Painter.SetBrush(Resources.Foreground);

			device2D.Painter.SaveState();
			device2D.Painter.Translate(overviewLocation.X, overviewLocation.Y);
			Paragraph.Draw(device2D.Painter, overviewMetrics);
			device2D.Painter.RestoreState();
			device2D.Painter.SaveState();
			device2D.Painter.Translate(subsystemLocation.X, subsystemLocation.Y);
			Paragraph.Draw(device2D.Painter, subsystemMetrics);
			device2D.Painter.RestoreState();

			// generate the settings display
			float menuWidth = GenerateMenu(context, target, device2D, lineHeight);

			device2D.Painter.End();

			device2D.Compositor.Begin(target, Retention.RetainData);
			device2D.Compositor.Composite(
				demoTarget, menuWidth + (lineHeight * 0.25f), statsPanel.Bottom + lineHeight);
			device2D.Compositor.End();
		}

		private static float GenerateMenu(
			IDemoContext context, Canvas target, Device2D device2D, float lineHeight)
		{
			Contract.Requires(context != null);
			Contract.Requires(target != null);
			Contract.Requires(device2D != null);

			var items = new List<KeyValuePair<ITextMetrics, bool>>();

			Rectangle region = target.Region;

			// move 25% towards the bottom of the region
			region = region.Translate(0, lineHeight * 4);

			float itemHeight = 0.0f;
			float itemSpacing = 0.0f;

			Thickness itemExpansion = Thickness.Empty;

			foreach(var item in context.Settings)
			{
				string currentText = item.IsActive ? item.ActiveText : item.InactiveText;

				Paragraph paragraph = Paragraph.Create()
					.WithFamily("Lucida Console")
					.AddText(
						String.Format("[{0}] {1}", items.Count + 1, currentText))
					.Build();

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
				settingRegion = settingRegion.Expand(itemHeight, 0, 0, 0);

				device2D.Painter.SetBrush(item.Value ? Resources.ActiveButton : Resources.UIColor);
				device2D.Painter.FillRectangle(settingRegion, new Size(itemHeight / 3.0f));

				device2D.Painter.SetBrush(item.Value ? Resources.InactiveButton : Resources.Foreground);

				Paragraph.Draw(device2D.Painter, item.Key);
			}

			return maxWidth;
		}
	}
}