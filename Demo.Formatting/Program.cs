// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

using Demo.Framework;

using Frost;
using Frost.Formatting;
using Frost.Painting;

using SD = System.Drawing;

namespace Demo.Formatting
{
	internal sealed class Application : IDemoContext
	{
		private bool _AreRegionsDisplayed;

		public void Reset(Canvas target, Device2D device2D)
		{
			ShapedText shapedOutput = new ShapedText();

			const string plainText = "Hello world!";

			device2D.TextShaper.Begin(shapedOutput, plainText);

			device2D.TextShaper.AnalyzeScripts();

			device2D.TextShaper.SetPointSize(plainText, 95.0f);

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

			device2D.Painter.Begin(target);

			device2D.Painter.SetBrush(Resources.Foreground);

			foreach(TextShaper.Span span in shapedOutput.Spans)
			{
				float emSize = span.FontMetrics.MeasureEm(span.PointSize);

				foreach(int clusterIndex in span.Clusters)
				{
					float advance = shapedOutput.Clusters[clusterIndex].Advance;

					if (_AreRegionsDisplayed)
					{
						device2D.Painter.SaveState();

						device2D.Painter.StrokeWidth = 0.5f;
						device2D.Painter.LineStyle = LineStyle.Dash;

						device2D.Painter.SetBrush(Color.Red);
						device2D.Painter.StrokeRectangle(0, 0, advance, emSize);
						device2D.Painter.SetBrush(Resources.Foreground);
						
						device2D.Painter.RestoreState();
					}

					float baseline = outlines[clusterIndex].Baseline;

					device2D.Painter.SaveState();

					device2D.Painter.Scale(emSize, emSize);
					device2D.Painter.Translate(0, baseline);

					if(outlines[clusterIndex].Shape != null)
					{
						device2D.Painter.Fill(outlines[clusterIndex].Shape);
					}

					device2D.Painter.RestoreState();

					device2D.Painter.Translate(advance, 0);
				}
			}

			device2D.Painter.End();
		}

		public IEnumerable<DemoSetting> Settings
		{
			get
			{
				yield return
					new DemoSetting(
						"Hide Regions",
						"Show Regions",
						_AreRegionsDisplayed,
						() => _AreRegionsDisplayed = !_AreRegionsDisplayed);
			}
		}

		public void Dispose()
		{
		}

		public bool KeyPressed(char keyCharacter)
		{
			switch(keyCharacter)
			{
				case '1':
					_AreRegionsDisplayed = !_AreRegionsDisplayed;
					return true;
			}

			return false;
		}

		public sealed class ShapedText : IShapedGlyphs
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

	internal static class Program
	{
		private static void Main()
		{
			using(Application application = new Application())
			{
				using(DemoApplication demo = new DemoApplication())
				{
					demo.Execute(application);
				}
			}
		}
	}
}