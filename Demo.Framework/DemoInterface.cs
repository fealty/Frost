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
using Frost.Painting;

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

			Rectangle left = Rectangle.FromEdges(
				0, 0, 213, target.Region.Bottom);
			Rectangle middle = Rectangle.FromEdges(
				left.Right, 0, target.Region.Right - 213, target.Region.Bottom);
			Rectangle right = Rectangle.FromEdges(middle.Right, 0, target.Region.Right, target.Region.Bottom);

			Rectangle demoRegion = middle.Contract(25);

			// create a new canvas for the demo contents
			Canvas demoTarget = new Canvas(demoRegion.Size);

			// resolve the new resource to avoid timing problems
			device2D.Resources.ResolveCanvas(demoTarget);

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

			/*Paragraph snapshot = Paragraph.Create()
				.WithFamily("Lucida Console")
				.WithTracking(0.2f)
				.WithAlignment(Alignment.Center)
				.AddText("SNAPSHOT")
				.Build();

			Paragraph options = Paragraph.Create()
				.WithFamily("Lucida Console")
				.WithTracking(0.2f)
				.WithAlignment(Alignment.Center)
				.AddText("OPTIONS")
				.Build();

			Paragraph timeTaken = Paragraph.Create()
				.WithFamily("Lucida Console")
				.AddText("Time Taken: {0} ms", _Watch.ElapsedMilliseconds)
				.Build();

			Paragraph subsystemTimes = Paragraph.Create()
				.WithFamily("Lucida Console")
				.AddText("Painting: {0} ms", paintingFrameDuration.Value.Milliseconds)
				.AddText(" \u2219 ")
				.AddText("Composition: {0} ms", compositionFrameDuration.Value.Milliseconds)
				.Build();

			Paragraph totalMemory = Paragraph.Create()
				.WithFamily("Lucida Console")
				.AddText("Memory: {0:N0} KB", GC.GetTotalMemory(true) / 1024)
				.Build();

			Paragraph totalGarbage = Paragraph.Create()
				.WithFamily("Lucida Console")
				.AddText("Garbage: {0:N0} KB", memoryDelta / 1024)
				.Build();

			ITextMetrics timeMetrics = device2D.Formatter.MeasureLayout(timeTaken);

			Rectangle timeRegion = timeMetrics.TextRegion.AlignRelativeTo(
				middle, Alignment.Center, Axis.Horizontal);

			ITextMetrics subsystemMetrics = device2D.Formatter.MeasureLayout(subsystemTimes);

			Rectangle subsystemRegion = subsystemMetrics.TextRegion.AlignRelativeTo(
				middle, Alignment.Center, Axis.Horizontal);

			subsystemRegion = subsystemRegion.Translate(0, middle.Bottom - subsystemRegion.Height);

			ITextMetrics memoryMetrics = device2D.Formatter.MeasureLayout(totalMemory);
			ITextMetrics garbageMetrics = device2D.Formatter.MeasureLayout(totalGarbage);

			ITextMetrics snapshotMetrics = device2D.Formatter.MeasureLayout(snapshot, right);
			ITextMetrics optionsMetrics = device2D.Formatter.MeasureLayout(options, left);*/

			device2D.Painter.Begin(target);

			// clear the background to the background color
			device2D.Painter.SetBrush(Resources.Background);
			device2D.Painter.FillRectangle(target.Region);

			device2D.Painter.SetBrush(Resources.UIColor);

			device2D.Painter.FillRectangle(left);
			device2D.Painter.FillRectangle(right);

		/*	Rectangle timePanel = timeRegion.Expand(
				timeRegion.Height, timeRegion.Height, timeRegion.Height, timeRegion.Height / 4.0f);

			device2D.Painter.FillRectangle(timePanel, new Size(timeRegion.Height / 2.0f));

			Rectangle subsystemPanel = subsystemRegion.Expand(
				subsystemRegion.Height,
				subsystemRegion.Height / 4.0f,
				subsystemRegion.Height,
				subsystemRegion.Height);

			device2D.Painter.FillRectangle(subsystemPanel, new Size(subsystemRegion.Height / 2.0f));*/

			device2D.Painter.SetBrush(Resources.ActiveButton);

			device2D.Painter.SaveState();
			device2D.Painter.LineStyle = LineStyle.Dash;
			device2D.Painter.StrokeRectangle(demoRegion.Expand(1));
			device2D.Painter.RestoreState();

			device2D.Painter.StrokeLine(left.Right, left.Top, left.Right, left.Bottom);
			device2D.Painter.StrokeLine(right.Left, right.Top, right.Left, right.Bottom);

			device2D.Painter.SetBrush(Resources.Foreground);

			device2D.Painter.SaveState();
			/*device2D.Painter.Translate(timeRegion.X, timeRegion.Y);
			Paragraph.Draw(device2D.Painter, timeMetrics);*/
			device2D.Painter.RestoreState();

			device2D.Painter.SaveState();
			/*device2D.Painter.Translate(subsystemRegion.X, subsystemRegion.Y);
			Paragraph.Draw(device2D.Painter, subsystemMetrics);*/
			device2D.Painter.RestoreState();

			device2D.Painter.SaveState();

			/*device2D.Painter.Translate(0, timeRegion.Height);
			Paragraph.Draw(device2D.Painter, snapshotMetrics);
			device2D.Painter.StrokeLine(
				snapshotMetrics.TextRegion.Left,
				snapshotMetrics.TextRegion.Bottom,
				snapshotMetrics.TextRegion.Right,
				snapshotMetrics.TextRegion.Bottom);

			device2D.Painter.Translate(
				right.Left + (timeRegion.Height / 2.0f), (timeRegion.Height * 2) + (timeRegion.Height * 0.25f));
			Paragraph.Draw(device2D.Painter, memoryMetrics);

			device2D.Painter.Translate(0, (timeRegion.Height * 2) + (timeRegion.Height * 0.25f));
			Paragraph.Draw(device2D.Painter, garbageMetrics);*/

			device2D.Painter.RestoreState();

			device2D.Painter.SaveState();

			/*device2D.Painter.Translate(0, timeRegion.Height);
			Paragraph.Draw(device2D.Painter, optionsMetrics);
			device2D.Painter.StrokeLine(
				optionsMetrics.TextRegion.Left,
				optionsMetrics.TextRegion.Bottom,
				optionsMetrics.TextRegion.Right,
				optionsMetrics.TextRegion.Bottom);*/

			device2D.Painter.RestoreState();

			/*GenerateMenu(
				context, target, device2D, (timeRegion.Height * 3) + (timeRegion.Height * 0.25f), left.Width);
			*/
			device2D.Painter.End();

			device2D.Compositor.Begin(target, Retention.RetainData);
			device2D.Compositor.Composite(demoTarget, demoRegion.Location);
			device2D.Compositor.End();
		}

		private static void GenerateMenu(
			IDemoContext context, Canvas target, Device2D device2D, float listTop, float listWidth)
		{
			Contract.Requires(context != null);
			Contract.Requires(target != null);
			Contract.Requires(device2D != null);

			/*var items = new List<KeyValuePair<ITextMetrics, bool>>();

			Rectangle region = target.Region;

			region = region.Translate(0, listTop);

			float itemHeight = 0.0f;
			float itemSpacing = 0.0f;

			Thickness itemExpansion = Thickness.Empty;

			foreach(var item in context.Settings)
			{
				string currentText = item.IsActive ? item.ActiveText : item.InactiveText;

				Paragraph paragraph = Paragraph.Create()
					.WithFamily("Lucida Console")
					.AddText("[{0}] {1}", items.Count + 1, currentText)
					.Build();

				var metrics = device2D.Formatter.MeasureLayout(paragraph, region);

				if(itemHeight.Equals(0.0f) && itemSpacing.Equals(0.0f))
				{
					itemHeight = metrics.TextRegion.Height * 2.00f;
					itemSpacing = metrics.TextRegion.Height * 0.25f;
					itemExpansion = new Thickness(itemHeight / 4.0f);
				}

				items.Add(new KeyValuePair<ITextMetrics, bool>(metrics, item.IsActive));

				region = region.Translate(0, itemHeight + itemSpacing);
			} 

			foreach(var item in items)
			{
				Rectangle settingRegion = item.Key.TextRegion.Expand(itemExpansion);

				settingRegion = settingRegion.Resize(listWidth, settingRegion.Height);
				settingRegion = settingRegion.Expand(itemHeight, 0, 0, 0);

				device2D.Painter.SetBrush(item.Value ? Resources.ActiveButton : Resources.UIColor);
				device2D.Painter.FillRectangle(settingRegion, new Size(itemHeight / 3.0f));

				device2D.Painter.SetBrush(Resources.ActiveButton);
				device2D.Painter.StrokeRectangle(settingRegion, new Size(itemHeight / 3.0f));

				device2D.Painter.SetBrush(item.Value ? Resources.InactiveButton : Resources.Foreground);

				//Paragraph.Draw(device2D.Painter, item.Key);
			}*/
		}
	}
}