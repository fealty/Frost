// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using Demo.Framework;

using Frost;

namespace Demo.Formatting
{
	internal sealed class Application : IDemoContext
	{
		public string Name
		{
			get { return "Frost.Formatting Demo"; }
		}

		public void Reset(Canvas target, Device2D device2D)
		{
			device2D.Painter.Begin(target);
			device2D.Painter.SetBrush(Color.Tomato);
			device2D.Painter.Fill(new Rectangle(0, 0, 100, 100));
			device2D.Painter.End();
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