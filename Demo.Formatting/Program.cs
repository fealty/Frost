// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Globalization;

using Demo.Framework;

using Frost;
using Frost.Collections;
using Frost.Formatting;
using Frost.Painting;

namespace Demo.Formatting
{
	internal sealed class Application : IDemoContext
	{
		public void Reset(Canvas target, Device2D device2D)
		{
			Painter painter = device2D.Painter;

			painter.Begin(target);

			painter.SetBrush(Color.DarkBlue);

			painter.Fill(target.Region.X, target.Region.Y, target.Region.Width, target.Region.Height, 0, 0);

			Rectangle obs = new Rectangle(150, 55, 150, 100);
			Rectangle obs2 = new Rectangle(150, 320, 175, 100);
			Rectangle obs3 = new Rectangle(550, 80, 150, 280);

			painter.SetBrush(Color.Green);

			painter.Fill(obs2.X, obs2.Y, obs2.Width, obs2.Height, 10, 10);
			painter.Fill(obs3.X, obs3.Y, obs3.Width, obs3.Height, 10, 10);

			painter.SetBrush(Color.WhiteSmoke);

			Paragraph.Builder builder = Paragraph.Create();

			builder.WithPointSize(10);
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

			ITextMetrics metrics = device2D.MeasureLayout(
				builder.Build(), new Rectangle(180, 50, 440, 500), obs2, obs3);

			Paragraph.Draw(painter, metrics);

			painter.SetBrush(Color.Green);

			painter.Fill(
				metrics.Regions[0].X,
				metrics.Regions[0].Y,
				metrics.Regions[0].Width,
				metrics.Regions[0].Height,
				10,
				10);

			painter.SetBrush(Color.Green);
			painter.IsAntialiased = Antialiasing.Aliased;
			painter.StrokeWidth = 1.0f;

			painter.LineStyle = LineStyle.Dash;

			for(int i = 0; i < metrics.Regions.Count; ++i)
			{
				Rectangle region = metrics.Regions[i];

				if(region.Area > 0)
				{
					//painter.Stroke(region);
				}
			}

			painter.SetBrush(Color.OrangeRed);

			foreach(IndexedRange line in metrics.Lines)
			{
				Rectangle region;

				metrics.ComputeRegion(line, out region);

				//painter.Stroke(region);
			}

			painter.SetBrush(Color.Yellow);

			painter.Stroke(
				metrics.TextRegion.X,
				metrics.TextRegion.Y,
				metrics.TextRegion.Width,
				metrics.TextRegion.Height,
				0,
				0);

			painter.SetBrush(Color.HotPink);

			painter.Stroke(
				metrics.LayoutRegion.X,
				metrics.LayoutRegion.Y,
				metrics.LayoutRegion.Width,
				metrics.LayoutRegion.Height,
				0,
				0);

			painter.End();
		}

		public void Dispose()
		{
			//throw new NotImplementedException();
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