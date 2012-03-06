// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

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
		private bool _AreLinesDisplayed;
		private bool _AreRegionsDisplayed;

		public void Reset(Rectangle region, Canvas target, Device2D device2D)
		{
			Rectangle columnRegion = new Rectangle(0, region.Y + 20, 440, region.Height);

			columnRegion = columnRegion.AlignWithin(region, Alignment.Center, Axis.Horizontal);

			Painter painter = device2D.Painter;

			painter.Begin(target, Retention.RetainData);

			Canvas inlineIcon = Resources.CreateIcon(device2D);

			Size inlineSize = new Size(inlineIcon.Region.Width + 5, inlineIcon.Region.Height);

			Paragraph title =
				Paragraph.Create().WithTracking(0.1f).WithAlignment(Alignment.Center).WithFamily("Cambria").
					WithWeight(FontWeight.Bold).WithAdditionalText("A BRIEF MANIFESTO").Build();

			Paragraph foreword =
				Paragraph.Create().WithAdditionalInline(inlineSize, Alignment.Trailing, Alignment.Center).
					WithFamily("Calibri").WithLeading(0.25f).WithAdditionalText(
						"ost chal\u00ADlenges the con\u00ADven\u00ADtion\u00ADal lack of el\u00ADeg\u00ADant and ex\u00ADpress\u00ADive text in video games. Why should play\u00ADers struggle to read small quest text with in\u00ADad\u00ADequate lead\u00ADing and jagged rags? We care for the read\u00ADer's eyes in prin\u00ADted works. Can't we care for the eyes star\u00ADing at a glow\u00ADing mon\u00ADit\u00ADor? We can, and we will.")
					.Build();

			Paragraph features =
				Paragraph.Create().WithIndentation(1.5f).WithFamily("Calibri").WithLeading(0.25f).
					WithAdditionalText(
						"With sup\u00ADport for Uni\u00ADcode, Open\u00ADType, and ad\u00ADvanced lay\u00ADout and format\u00ADting, Frost provides many ty\u00ADpo\u00ADgraph\u00ADic fea\u00ADtures in\u00ADclud\u00ADing lead\u00ADing, track\u00ADing, spa\u00ADcing, in\u00ADdent\u00ADa\u00ADtion, op\u00ADtim\u00ADal line break\u00ADing, in\u00ADline ob\u00ADjects, bi\u00ADd\u00ADirec\u00ADtion\u00ADal text, float\u00ADing in\u00ADlines, hy\u00ADphen\u00ADa\u00ADtion, and flow ob\u00ADstruc\u00ADtions. The sys\u00ADtem also ex\u00ADposes the glyph cluster geo\u00ADmetry as ")
					.SaveState().WithFamily("Lucida Console").WithAdditionalText("Geometry").RestoreState().
					WithAdditionalText(
						" to en\u00ADable ad\u00ADvanced or al\u00ADtern\u00ADat\u00ADive font ras\u00ADter\u00ADiz\u00ADa\u00ADtion tech\u00ADniques.")
					.Build();

			Paragraph api =
				Paragraph.Create().WithIndentation(1.5f).WithFamily("Calibri").WithLeading(0.25f).
					WithAdditionalText(
						"These fea\u00ADtures in\u00ADteg\u00ADrate in\u00ADto the flu\u00ADent and flex\u00ADible design of Frost. Users may con\u00ADtrol in\u00ADdi\u00ADvidu\u00ADal char\u00ADac\u00ADters through trans\u00ADform\u00ADa\u00ADtions when ras\u00ADter\u00ADiz\u00ADing text through either the ")
					.SaveState().WithFamily("Lucida Console").WithAdditionalText("Painter").RestoreState().
					WithAdditionalText(" or ").SaveState().WithFamily("Lucida Console").WithAdditionalText(
						"Compositor").RestoreState().WithAdditionalText(
							". The sys\u00ADtem op\u00ADer\u00ADates upon in\u00ADdi\u00ADvidu\u00ADal para\u00ADgraphs. This block-based ap\u00ADproach gives max\u00ADim\u00ADum flex\u00ADib\u00ADil\u00ADity to ap\u00ADplic\u00ADa\u00ADtions wish\u00ADing to finely con\u00ADtrol the lay\u00ADout of lar\u00ADger se\u00ADmant\u00ADic units.")
					.Build();

			ITextMetrics titleMetrics = device2D.MeasureLayout(title, columnRegion);
			ITextMetrics forewordMetrics = device2D.MeasureLayout(
				foreword, columnRegion.Translate(new Size(0, 25)));
			ITextMetrics featuresMetrics = device2D.MeasureLayout(
				features,
				new Rectangle(
					columnRegion.X, forewordMetrics.TextRegion.Bottom + 7, columnRegion.Width, columnRegion.Height));
			ITextMetrics apiMetrics = device2D.MeasureLayout(
				api,
				new Rectangle(
					columnRegion.X, featuresMetrics.TextRegion.Bottom + 7, columnRegion.Width, columnRegion.Height));

			painter.SetBrush(Resources.FrostColor);
			painter.FillRectangle(
				Rectangle.FromEdges(
					columnRegion.Expand(new Thickness(10)).Left,
					columnRegion.Expand(new Thickness(10)).Top,
					apiMetrics.TextRegion.Expand(new Thickness(10)).Right,
					apiMetrics.TextRegion.Expand(new Thickness(10)).Bottom),
				new Size(10));

			painter.SetBrush(Resources.Foreground);

			Paragraph.Draw(painter, titleMetrics);
			Paragraph.Draw(painter, forewordMetrics);
			Paragraph.Draw(painter, featuresMetrics);
			Paragraph.Draw(painter, apiMetrics);

			painter.SetBrush(Color.Green);

			/*painter.FillRectangle(
				metrics.Regions[0].X,
				metrics.Regions[0].Y,
				metrics.Regions[0].Width,
				metrics.Regions[0].Height,
				10,
				10);*/

			painter.SetBrush(Color.Green);
			painter.IsAntialiased = Antialiasing.Aliased;
			painter.StrokeWidth = 1.0f;

			painter.LineStyle = LineStyle.Dash;

			for(int i = 0; i < forewordMetrics.Regions.Count; ++i)
			{
				region = forewordMetrics.Regions[i];

				if(!region.IsEmpty)
				{
					//painter.StrokeRectangle(region);
				}
			}

			painter.SetBrush(Color.OrangeRed);

			foreach(IndexedRange line in forewordMetrics.Lines)
			{
				forewordMetrics.ComputeRegion(line, out region);

				//painter.StrokeRectangle(region);
			}

			painter.SetBrush(Color.Yellow);

			/*painter.StrokeRectangle(
				metrics.TextRegion.X,
				metrics.TextRegion.Y,
				metrics.TextRegion.Width,
				metrics.TextRegion.Height,
				0,
				0);

			painter.SetBrush(Color.HotPink);

			painter.StrokeRectangle(
				metrics.LayoutRegion.X,
				metrics.LayoutRegion.Y,
				metrics.LayoutRegion.Width,
				metrics.LayoutRegion.Height,
				0,
				0);*/

			painter.End();

			device2D.Compositor.Begin(target, Retention.RetainData);
			device2D.Compositor.Composite(
				inlineIcon,
				forewordMetrics.Regions[0].X,
				forewordMetrics.Regions[0].Y,
				inlineIcon.Region.Width,
				forewordMetrics.Regions[0].Height);
			device2D.Compositor.End();
		}

		public IEnumerable<DemoSetting> Settings
		{
			get
			{
				yield return
					new DemoSetting(
						"", _AreLinesDisplayed, () => _AreLinesDisplayed = !_AreLinesDisplayed);
				yield return
					new DemoSetting(
						"",
						_AreRegionsDisplayed,
						() => _AreRegionsDisplayed = !_AreRegionsDisplayed);
			}
		}

		public void Dispose()
		{
			//throw new NotImplementedException();
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

	/*Rectangle obs = new Rectangle(150, 55, 150, 100);
			Rectangle obs2 = new Rectangle(150, 320, 175, 100);
			Rectangle obs3 = new Rectangle(550, 80, 150, 280);

			painter.SetBrush(Color.Green);

			painter.FillRectangle(obs2.X, obs2.Y, obs2.Width, obs2.Height, 10, 10);
			painter.FillRectangle(obs3.X, obs3.Y, obs3.Width, obs3.Height, 10, 10);

			Paragraph.Builder builder = Paragraph.Create();

			builder.WithPointSize(12);
			builder.WithAlignment(Alignment.Stretch);
			//builder.WithIndentation(2.0f);

			builder.WithCulture(new CultureInfo("he"));

			builder.WithAdditionalInline(obs.Size, Alignment.Leading);

			builder.WithFamily("David");

			builder.WithAdditionalText(
				"כיצד למנוע שינויים בה חפש, יסוד הבקשה כתב את. בשפות האטמוספירה אם בדף. על אנגלית קישורים האטמוספירה צעד, הרוח שתפו מונחונים של רבה, זאת את מונרכיה מונחונים. הטבע מתוך בישול חפש מה. לוח אם ספרדית התפתחות וספציפיים. זקוק ניווט סדר את, כלכלה למאמרים לוח בה.");
			//builder.Append(new Cabbage.Size(100, 25), Cabbage.Alignment.Stretch);
			//	paragraph.Culture = new System.Globalization.CultureInfo("en-US");
			builder.WithFamily("Arno Pro");
			builder.WithStyle(FontStyle.Italic);
			//paragraph.Features = new [] {new Yuki.Cabbage.TextFeature(Yuki.Cabbage.FontFeature.Swash, 1)};

			builder.WithAdditionalText(
				"\u202AOn the oth-er hand, we de-nounce with right-eous in-dig-na-tion and dis-like men who are so be-guiled and de-mor-al-ized by the charms of pleas-ure of the mo-ment, so blinded by de-sire, that they can-not fore-see the pain and trouble that are bound to en-sue; and equal blame be-longs to those who fail in their duty through (weak-ness of will) which is the same (as say-ing through shrink-ing) from toil and pain.\u202c "
					.Replace('-', (char)0x00AD));

			builder.WithFamily("David");
			builder.WithStyle(FontStyle.Regular);

			builder.WithAdditionalText(
				"בקר תורת משפטית לויקיפדים ב, ביוני טכניים סטטיסטיקה את תנך, מה קרן הרוח מיתולוגיה. בה ליום נוסחאות זאת, סדר שאלות לעריכת ב. שתי את יוני שימושי תיקונים, על מונחים פיסיקה מאמרשיחהצפה שמו. את ראשי כניסה מתן, גם מלא הבאים זכויות, בדף אודות לחיבור על. מה החברה תיאטרון בדף, לערכים תיאטרון צעד גם. מה רפואה כלכלה ויש. מיזמי ביולי תאולוגיה ויש את, כימיה לחשבון מאמרשיחהצפה רבה אם. מוגש לעתים של היא, מה ערבית לציין כלל. של העיר ביולי קישורים אחר. צ'ט כדור כלכלה צרפתית מה, ויקי המלחמה את שמו.");
			*/
}