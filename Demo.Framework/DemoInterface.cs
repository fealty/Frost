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
using Frost.Painting;

namespace Demo.Framework
{
	internal static class DemoInterface
	{
		private static readonly Stopwatch _Watch;

		private static readonly ShapedText _ShapedText;

		static DemoInterface()
		{
			_Watch = new Stopwatch();
			_ShapedText = new ShapedText();
		}

		public static void ResetDemo(
			IDemoContext context, Canvas target, Device2D device2D)
		{
			Contract.Requires(context != null);
			Contract.Requires(target != null);
			Contract.Requires(device2D != null);

			Rectangle left = Rectangle.FromEdges(
				0, 0, 213, target.Region.Bottom);
			Rectangle middle = Rectangle.FromEdges(
				left.Right, 0, target.Region.Right - 213, target.Region.Bottom);
			Rectangle right = Rectangle.FromEdges(
				middle.Right, 0, target.Region.Right, target.Region.Bottom);

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

			device2D.Diagnostics.Query(
				"Composition", "FrameDuration", out compositionFrameDuration);
			device2D.Diagnostics.Query(
				"Painting", "FrameDuration", out paintingFrameDuration);

			////////////////////////////////
			string timeString = string.Format(
				"Time Taken: {0} ms", _Watch.ElapsedMilliseconds);

			Rectangle timeMetrics = MeasureText(timeString, device2D);

			Rectangle timeRegion = timeMetrics.AlignRelativeTo(
				middle, Alignment.Center, Axis.Horizontal);

			/////////////////////////////
			string subsystemString =
				string.Format(
					"Painting: {0} ms \u2219 Composition: {1} ms",
					paintingFrameDuration.Value.Milliseconds,
					compositionFrameDuration.Value.Milliseconds);

			Rectangle subsystemMetrics = MeasureText(subsystemString, device2D);

			Rectangle subsystemRegion = subsystemMetrics.AlignRelativeTo(
				middle, Alignment.Center, Axis.Horizontal);

			subsystemRegion = subsystemRegion.Translate(0, middle.Bottom - subsystemRegion.Height);

			////////////////////////////////
			string memoryString = string.Format("Memory: {0:N0} KB", GC.GetTotalMemory(true) / 1024);

			Rectangle memoryMetrics = MeasureText(memoryString, device2D);

			string garbageString = string.Format("Garbage: {0:N0} KB", memoryDelta / 1024);

			Rectangle garbageMetrics = MeasureText(garbageString, device2D);

			string snapshotString = "SNAPSHOT";

			Rectangle snapshotMetrics = MeasureText(snapshotString, device2D);

			string optionsString = "OPTIONS";

			Rectangle optionsMetrics = MeasureText(optionsString, device2D);

			snapshotMetrics = snapshotMetrics.AlignRelativeTo(right, Alignment.Center, Axis.Horizontal);
			optionsMetrics = optionsMetrics.AlignRelativeTo(left, Alignment.Center, Axis.Horizontal);

			device2D.Painter.Begin(target);

			// clear the background to the background color
			device2D.Painter.SetBrush(Resources.Background);
			device2D.Painter.FillRectangle(target.Region);

			device2D.Painter.SetBrush(Resources.UIColor);

			device2D.Painter.FillRectangle(left);
			device2D.Painter.FillRectangle(right);

			Rectangle timePanel = timeRegion.Expand(
				timeRegion.Height, timeRegion.Height, timeRegion.Height, timeRegion.Height / 4.0f);

			device2D.Painter.FillRectangle(timePanel, new Size(timeRegion.Height / 2.0f));

			Rectangle subsystemPanel = subsystemRegion.Expand(
				subsystemRegion.Height,
				subsystemRegion.Height / 4.0f,
				subsystemRegion.Height,
				subsystemRegion.Height);

			device2D.Painter.FillRectangle(subsystemPanel, new Size(subsystemRegion.Height / 2.0f));

			device2D.Painter.SetBrush(Resources.ActiveButton);

			device2D.Painter.SaveState();
			device2D.Painter.LineStyle = LineStyle.Dash;
			device2D.Painter.StrokeRectangle(demoRegion.Expand(1));
			device2D.Painter.RestoreState();

			device2D.Painter.StrokeLine(
				left.Right, left.Top, left.Right, left.Bottom);
			device2D.Painter.StrokeLine(
				right.Left, right.Top, right.Left, right.Bottom);

			device2D.Painter.SetBrush(Resources.Foreground);

			device2D.Painter.SaveState();
			device2D.Painter.Translate(timeRegion.X, timeRegion.Y);
			DrawText(Point.Empty, timeString, device2D);
			device2D.Painter.RestoreState();

			device2D.Painter.SaveState();
			device2D.Painter.Translate(subsystemRegion.X, subsystemRegion.Y);
			DrawText(Point.Empty, subsystemString, device2D);
			device2D.Painter.RestoreState();

			device2D.Painter.SaveState();

			device2D.Painter.Translate(0, timeRegion.Height);

			DrawText(snapshotMetrics.Location, snapshotString, device2D);

			device2D.Painter.StrokeLine(
				snapshotMetrics.Left,
				snapshotMetrics.Bottom,
				snapshotMetrics.Right,
				snapshotMetrics.Bottom);

			device2D.Painter.Translate(right.Left + (timeRegion.Height / 2.0f), (timeRegion.Height * 2) + (timeRegion.Height * 0.25f));

			DrawText(Point.Empty, memoryString, device2D);

			device2D.Painter.Translate(0, (timeRegion.Height * 2) + (timeRegion.Height * 0.25f));

			DrawText(Point.Empty, garbageString, device2D);
			
			device2D.Painter.RestoreState();

			device2D.Painter.SaveState();

			device2D.Painter.Translate(0, timeRegion.Height);

			DrawText(optionsMetrics.Location, optionsString, device2D);

			device2D.Painter.StrokeLine(
				optionsMetrics.Left,
				optionsMetrics.Bottom,
				optionsMetrics.Right,
				optionsMetrics.Bottom);

			device2D.Painter.RestoreState();

			GenerateMenu(
				context, target, device2D, (timeRegion.Height * 3) + (timeRegion.Height * 0.25f), left.Width);
			

			device2D.Painter.End();

			device2D.Compositor.Begin(target, Retention.RetainData);
			device2D.Compositor.Composite(demoTarget, demoRegion.Location);
			device2D.Compositor.End();
		}

		private static void GenerateMenu(
			IDemoContext context,
			Canvas target,
			Device2D device2D,
			float listTop,
			float listWidth)
		{
			Contract.Requires(context != null);
			Contract.Requires(target != null);
			Contract.Requires(device2D != null);

			var items = new List<KeyValuePair<string, KeyValuePair<Rectangle, bool>>>();

			Rectangle region = target.Region;

			region = region.Translate(0, listTop);

			float itemHeight = 0.0f;
			float itemSpacing = 0.0f;

			Thickness itemExpansion = Thickness.Empty;

			foreach(var item in context.Settings)
			{
				string currentText = item.IsActive ? item.ActiveText : item.InactiveText;

				string finalText = string.Format(
					"[{0}] {1}", items.Count + 1, currentText);

				var metrics = MeasureText(finalText, device2D);

				metrics = metrics.Translate(region.Location);

				if(itemHeight.Equals(0.0f) && itemSpacing.Equals(0.0f))
				{
					itemHeight = metrics.Height * 2.00f;
					itemSpacing = metrics.Height * 0.25f;
					itemExpansion = new Thickness(itemHeight / 4.0f);
				}

				items.Add(
					new KeyValuePair<string, KeyValuePair<Rectangle, bool>>(
						finalText, new KeyValuePair<Rectangle, bool>(metrics, item.IsActive)));

				region = region.Translate(0, itemHeight + itemSpacing);
			}

			for(int index = 0; index < items.Count; index++)
			{
				var item = items[index];

				Rectangle settingRegion = item.Value.Key.Expand(itemExpansion);

				settingRegion = settingRegion.Resize(listWidth, settingRegion.Height);
				settingRegion = settingRegion.Expand(itemHeight, 0, 0, 0);

				device2D.Painter.SetBrush(
					item.Value.Value ? Resources.ActiveButton : Resources.UIColor);
				device2D.Painter.FillRectangle(settingRegion, new Size(itemHeight / 3.0f));

				device2D.Painter.SetBrush(Resources.ActiveButton);
				device2D.Painter.StrokeRectangle(
					settingRegion, new Size(itemHeight / 3.0f));

				device2D.Painter.SetBrush(
					item.Value.Value ? Resources.InactiveButton : Resources.Foreground);

				DrawText(item.Value.Key.Location, item.Key, device2D);
			}
		}

		private static Rectangle MeasureText(string text, Device2D device2D)
		{
			ShapedText shapedOutput = new ShapedText();

			device2D.TextShaper.Begin(shapedOutput, text);
			device2D.TextShaper.AnalyzeScripts();
			device2D.TextShaper.SetFamily(text, "Lucida Console");
			device2D.TextShaper.SetPointSize(text, 12.0f);
			device2D.TextShaper.End();

			float maxWidth = 0.0f;
			float maxHeight = 0.0f;

			foreach (TextShaper.Span span in shapedOutput.Spans)
			{
				maxHeight = Math.Max(maxHeight, 
					span.FontMetrics.MeasureEm(span.PointSize));

				foreach (int clusterIndex in span.Clusters)
				{
					maxWidth += shapedOutput.Clusters[clusterIndex].Advance;
				}
			}

			return new Rectangle(Point.Empty, maxWidth, maxHeight);
		}

		private static void DrawText(Point location, string text, Device2D device2D)
		{
			ShapedText shapedOutput = new ShapedText();

			device2D.TextShaper.Begin(shapedOutput, text);
			device2D.TextShaper.AnalyzeScripts();
			device2D.TextShaper.SetFamily(text, "Lucida Console");
			device2D.TextShaper.SetPointSize(text, 12.0f);
			device2D.TextShaper.End();

			List<GlyphOutline> outlines = new List<GlyphOutline>();

			foreach(TextShaper.Span span in shapedOutput.Spans)
			{
				foreach(int clusterIndex in span.Clusters)
				{
					TextShaper.Cluster cluster = shapedOutput.Clusters[clusterIndex];

					outlines.Add(
						device2D.Resources.GetGlyphOutline(
							cluster.Glyphs,
							false,
							false,
							span.FontMetrics.FontId,
							shapedOutput.Glyphs));
				}
			}

			device2D.Painter.SaveState();
			device2D.Painter.Translate(location);

			foreach(TextShaper.Span span in shapedOutput.Spans)
			{
				float emSize = span.FontMetrics.MeasureEm(span.PointSize);

				foreach(int clusterIndex in span.Clusters)
				{
					device2D.Painter.SaveState();

					device2D.Painter.Scale(emSize, emSize);
					device2D.Painter.Translate(0, outlines[clusterIndex].Baseline);

					if(outlines[clusterIndex].Shape != null)
					{
						device2D.Painter.Fill(outlines[clusterIndex].Shape);
					}

					device2D.Painter.RestoreState();

					device2D.Painter.Translate(
						shapedOutput.Clusters[clusterIndex].Advance, 0);
				}
			}

			device2D.Painter.RestoreState();
		}

		private sealed class ShapedText : IShapedGlyphs
		{
			public ShapedText()
			{
				Glyphs = new List<TextShaper.Glyph>();
				Clusters = new List<TextShaper.Cluster>();
				Spans = new List<TextShaper.Span>();
			}

			public List<TextShaper.Glyph> Glyphs { get; private set; }
			public List<TextShaper.Cluster> Clusters { get; private set; }
			public List<TextShaper.Span> Spans { get; private set; }
		}
	}
}