// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Demo.Framework;

using Frost;
using Frost.Collections;
using Frost.Formatting;
using Frost.Surfacing;

namespace Demo.SDF
{
	internal sealed class Application : IDemoContext
	{
		private readonly DistanceField _Distance;

		public Application()
		{
			_Distance = new DistanceField();
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

		public void Reset(Canvas target, Device2D device2D)
		{
			if(device2D.Resources.FindEffect<DistanceEffectSettings>() == null)
			{
				device2D.Resources.RegisterEffect<DistanceFieldEffect>();
			}

			ShapedText output = new ShapedText();

			const string text = "R";

			device2D.TextShaper.Begin(output, text);
			device2D.TextShaper.AnalyzeScripts();
			device2D.TextShaper.SetFamily(text, "Brioso Pro");
			device2D.TextShaper.SetPointSize(text, 12.0f);
			device2D.TextShaper.SetFeatures(text, new FontFeatureCollection(new[] {new FontFeature("swsh")}));
			device2D.TextShaper.End();

			GlyphOutline outline =
				device2D.Resources.GetGlyphOutline(
					new IndexedRange(0, 1),
					false,
					false,
					output.Spans[0].FontMetrics.FontId,
					output.Glyphs);

			GC.Collect(4);

			Canvas test2 = new Canvas(new Size(128, 128), SurfaceUsage.Normal);

			Stopwatch watch = new Stopwatch();
			watch.Start();
			Canvas test = _Distance.CreateField(
				outline.Shape, outline.Baseline, device2D);
			watch.Stop();

			//mForm.Text = string.Format("Time: {0}", watch.ElapsedMilliseconds);

			Debug.WriteLine("Time: {0}", watch.ElapsedMilliseconds);

			Rectangle reg = device2D.Geometry.MeasureRegion(outline.Shape);

			device2D.Painter.Begin(target);
			device2D.Painter.Translate(test.Region.X, test.Region.Y);

			device2D.Painter.Translate((0.5f - (reg.Width / 2.0f)) * 400, (0.5f - (reg.Height / 2.0f)) * 400);

			// translate the glyph to the left corner of the EM square
			device2D.Painter.Translate(-reg.X * 400, -reg.Y * 400);
			device2D.Painter.SetBrush(Color.Black);
			device2D.Painter.Scale(400, 400);
			device2D.Painter.Fill(outline.Shape);
			device2D.Painter.End(); //*/

			device2D.Compositor.Begin(target, Retention.RetainData);

			DistanceEffectSettings settings;
			device2D.Compositor.Translate(400, 0);
			device2D.Compositor.Scale(3.125f, 3.125f);
			device2D.Compositor.ApplyEffect(settings);
			device2D.Compositor.Composite(test);
			device2D.Compositor.End();

			device2D.Painter.Begin(target, Retention.RetainData);
			device2D.Painter.SetBrush(Color.IndianRed);
			device2D.Painter.IsAntialiased = Antialiasing.Aliased;
			device2D.Painter.StrokeWidth = DistanceField.EmLength / 400;
			device2D.Painter.Scale(1.0f / DistanceField.EmLength, 1.0f / DistanceField.EmLength);
			device2D.Painter.Scale(400, 400);
			TestTest(_Distance.Sample, device2D);
			device2D.Painter.End();

			//device2D.Resources.DumpToFiles(null, SurfaceUsage.Normal); //*/
		}

		public IEnumerable<DemoSetting> Settings
		{
			get { return Enumerable.Empty<DemoSetting>(); }
		}

		public void Dispose()
		{
			//throw new NotImplementedException();
		}

		private void TestTest(Sample s, Device2D device2D)
		{
			Rectangle region = s.Region;

			device2D.Painter.StrokeRectangle(region);

			/*float mag = 50;

			Frost.Point tl =
				new Frost.Point(
					region.Left + s.TopLeft.Intersection.X * mag,
					region.Top + s.TopLeft.Intersection.Y * mag);
			Frost.Point tr =
				new Frost.Point(
					region.Right + s.TopRight.Intersection.X * mag,
					region.Top + s.TopRight.Intersection.Y * mag);
			Frost.Point bl =
				new Frost.Point(
					region.Left + s.BottomLeft.Intersection.X * mag,
					region.Bottom + s.BottomLeft.Intersection.Y * mag);
			Frost.Point br =
				new Frost.Point(
					region.Right + s.BottomRight.Intersection.X * mag,
					region.Bottom + s.BottomRight.Intersection.Y * mag);

			tl = s.TopLeft.Intersection;
			tr = s.TopRight.Intersection;
			bl = s.BottomLeft.Intersection;
			br = s.BottomRight.Intersection;

			mDevice2D.Painter.SetBrush(Color.Green);
			if(s.TopLeft.Distance > 0.0)
				mDevice2D.Painter.Stroke(region.Left, region.Top, tl.X, tl.Y);
			if(s.TopRight.Distance > 0.0)
				mDevice2D.Painter.Stroke(region.Right, region.Top, tr.X, tr.Y);
			if(s.BottomLeft.Distance > 0.0)
				mDevice2D.Painter.Stroke(region.Left, region.Bottom, bl.X, bl.Y);
			if(s.BottomRight.Distance > 0.0)
				mDevice2D.Painter.Stroke(region.Right, region.Bottom, br.X, br.Y);//*/
			device2D.Painter.SetBrush(Color.Red);

			foreach(Sample ss in s.Children)
			{
				TestTest(ss, device2D);
			}
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