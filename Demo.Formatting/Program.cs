// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Demo.Framework;

using Frost;
using Frost.Collections;
using Frost.Formatting;
using Frost.Painting;

using SD = System.Drawing;

namespace Demo.Formatting
{
	internal sealed class Application : IDemoContext
	{
		//private readonly List<ITextMetrics> _Paragraphs;

		private bool _AreLinesDisplayed;
		private bool _AreRegionsDisplayed;

		public Application()
		{
			//_Paragraphs = new List<ITextMetrics>();
		}

		public sealed class Test : IShapedGlyphs
		{
			public Test()
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
			Test tt = new Test();

			device2D.TextShaper.Begin(tt, "Hello\u00ADWorld!");

			//device2D.Shaper.AnalyzeScripts();

			device2D.TextShaper.SetPointSize("Hello\u00ADWorld!", 12.0f);
			
			device2D.TextShaper.End();

			int g = 0;

			/*_Paragraphs.Clear();

			FontMetrics fontMetrics = device2D.Formatter.MeasureFont(
				"Calibri", FontWeight.Regular, FontStyle.Regular, FontStretch.Regular);

			float emSize = fontMetrics.MeasureEm(10.0f);

			Rectangle region = target.Region;

			Rectangle columnRegion = new Rectangle(
				region.X + (emSize / 2.0f), region.Y + (emSize / 2.0f), 30 * emSize, region.Height);

			columnRegion = columnRegion.AlignRelativeTo(region, Alignment.Center, Axis.Horizontal);

			Canvas inlineIcon = Resources.CreateIcon(device2D);

			Size inlineSize = new Size(inlineIcon.Region.Width + (emSize * 0.25f), inlineIcon.Region.Height);

			Paragraph title = Paragraph.Create()
				.WithTracking(0.1f)
				.WithAlignment(Alignment.Center)
				.WithFamily("Cambria")
				.WithWeight(FontWeight.Bold)
				.AddText("A BRIEF MANIFESTO")
				.Build();

			Paragraph foreword = Paragraph.Create()
				.AddInline(inlineSize, Alignment.Trailing, Alignment.Center)
				.WithFamily("Georgia")
				.WithLeading(0.25f)
				.AddText(
					"ost chal\u00ADlenges the con\u00ADven\u00ADtion\u00ADal lack of el\u00ADeg\u00ADant and ex\u00ADpress\u00ADive text in video games. Why should play\u00ADers struggle to read small quest text with in\u00ADad\u00ADequate lead\u00ADing and jagged rags? We care for the read\u00ADer's eyes in prin\u00ADted works. Can't we care for the eyes star\u00ADing at a glow\u00ADing mon\u00ADit\u00ADor? We can, and we will.")
				.Build();

			Paragraph features = Paragraph.Create()
				.WithIndentation(1.5f)
				.WithFamily("Georgia")
				.WithLeading(0.25f)
				.AddText(
					"With sup\u00ADport for Uni\u00ADcode, Open\u00ADType, and ad\u00ADvanced lay\u00ADout and format\u00ADting, Frost provides many ty\u00ADpo\u00ADgraph\u00ADic fea\u00ADtures in\u00ADclud\u00ADing lead\u00ADing, track\u00ADing, spa\u00ADcing, in\u00ADdent\u00ADa\u00ADtion, op\u00ADtim\u00ADal line break\u00ADing, in\u00ADline ob\u00ADjects, bi\u00ADd\u00ADirec\u00ADtion\u00ADal text, float\u00ADing in\u00ADlines, hy\u00ADphen\u00ADa\u00ADtion, and flow ob\u00ADstruc\u00ADtions. The sys\u00ADtem also ex\u00ADposes the glyph cluster geo\u00ADmetry as ")
				.SaveState()
				.WithFamily("Lucida Console")
				.AddText("Geometry")
				.RestoreState()
				.AddText(
					" to en\u00ADable ad\u00ADvanced or al\u00ADtern\u00ADat\u00ADive font ras\u00ADter\u00ADiz\u00ADa\u00ADtion tech\u00ADniques.")
				.Build();

			Paragraph api = Paragraph.Create()
				.WithIndentation(1.5f)
				.WithFamily("Georgia")
				.WithLeading(0.25f)
				.WithTracking(0.2f)
				.AddText(
					"These fea\u00ADtures in\u00ADteg\u00ADrate in\u00ADto the flu\u00ADent and flex\u00ADible design of Frost. Users may con\u00ADtrol in\u00ADdi\u00ADvidu\u00ADal char\u00ADac\u00ADters through trans\u00ADform\u00ADa\u00ADtions when ras\u00ADter\u00ADiz\u00ADing text through either the ")
				.SaveState()
				.WithFamily("Lucida Console")
				.AddText("Painter")
				.RestoreState()
				.AddText(" or ")
				.SaveState()
				.WithFamily("Lucida Console")
				.AddText("Compositor")
				.RestoreState()
				.AddText(
					". The sys\u00ADtem op\u00ADer\u00ADates upon in\u00ADdi\u00ADvidu\u00ADal para\u00ADgraphs. This block-based ap\u00ADproach gives max\u00ADim\u00ADum flex\u00ADib\u00ADil\u00ADity to ap\u00ADplic\u00ADa\u00ADtions wish\u00ADing to finely con\u00ADtrol the lay\u00ADout of lar\u00ADger se\u00ADmant\u00ADic units.")
				.Build();

			ITextMetrics titleMetrics = device2D.Formatter.MeasureLayout(title, columnRegion);

			ITextMetrics forewordMetrics = device2D.Formatter.MeasureLayout(
				foreword,
				new Rectangle(
					columnRegion.X,
					titleMetrics.TextRegion.Bottom + (emSize / 4.0f),
					columnRegion.Width,
					columnRegion.Height));

			ITextMetrics featuresMetrics = device2D.Formatter.MeasureLayout(
				features,
				new Rectangle(
					columnRegion.X,
					forewordMetrics.TextRegion.Bottom + (emSize / 4.0f),
					columnRegion.Width,
					columnRegion.Height));

			ITextMetrics apiMetrics = device2D.Formatter.MeasureLayout(
				api,
				new Rectangle(
					columnRegion.X,
					featuresMetrics.TextRegion.Bottom + (emSize / 4.0f),
					columnRegion.Width,
					columnRegion.Height));

			_Paragraphs.Add(titleMetrics);
			_Paragraphs.Add(forewordMetrics);
			_Paragraphs.Add(featuresMetrics);
			_Paragraphs.Add(apiMetrics);

			Painter painter = device2D.Painter;

			painter.Begin(target, Retention.RetainData);

			painter.SetBrush(Resources.FrostColor);
			painter.FillRectangle(
				Rectangle.FromEdges(
					columnRegion.Expand(new Thickness(emSize / 2.0f)).Left,
					columnRegion.Expand(new Thickness(emSize / 2.0f)).Top,
					_Paragraphs[_Paragraphs.Count - 1].TextRegion.Expand(new Thickness(emSize / 2.0f)).Right,
					_Paragraphs[_Paragraphs.Count - 1].TextRegion.Expand(new Thickness(emSize / 2.0f)).Bottom),
				new Size(emSize / 2.0f));

			painter.SetBrush(Resources.Foreground);

			foreach(var paragraph in _Paragraphs)
			{
				Paragraph.Draw(painter, paragraph);
			}

			painter.StrokeWidth = 1.0f;
			painter.IsAntialiased = Antialiasing.Aliased;
			painter.LineStyle = LineStyle.Dash;

			OutlineLines(painter);
			OutlineCharacters(painter);

			painter.End();

			device2D.Compositor.Begin(target, Retention.RetainData);
			device2D.Compositor.Composite(
				inlineIcon,
				forewordMetrics.Regions[0].X,
				forewordMetrics.Regions[0].Y,
				inlineIcon.Region.Width,
				forewordMetrics.Regions[0].Height);
			device2D.Compositor.End();*/
		}

		public IEnumerable<DemoSetting> Settings
		{
			get
			{
				yield return
					new DemoSetting(
						"Hide Lines",
						"Show Lines",
						_AreLinesDisplayed,
						() => _AreLinesDisplayed = !_AreLinesDisplayed);
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
					_AreLinesDisplayed = !_AreLinesDisplayed;
					return true;
				case '2':
					_AreRegionsDisplayed = !_AreRegionsDisplayed;
					return true;
			}

			return false;
		}

		/*private void OutlineLines(Painter painter)
		{
			Contract.Requires(painter != null);

			if(_AreLinesDisplayed)
			{
				painter.SetBrush(Color.Black);

				foreach(var paragraph in _Paragraphs)
				{
					foreach(IndexedRange line in paragraph.Lines)
					{
						Rectangle region;

						paragraph.ComputeRegion(line, out region);

						painter.StrokeRectangle(region);
					}
				}
			}
		}

		private void OutlineCharacters(Painter painter)
		{
			Contract.Requires(painter != null);

			if(_AreRegionsDisplayed)
			{
				painter.SetBrush(Color.Black);

				foreach(var paragraph in _Paragraphs)
				{
					foreach(var item in paragraph.Regions)
					{
						if(!item.IsEmpty)
						{
							painter.StrokeRectangle(item);
						}
					}
				}
			}
		}*/
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