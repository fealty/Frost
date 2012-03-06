using System;
using System.Diagnostics.Contracts;

namespace Demo.Framework
{
	public struct DemoSetting
	{
		private readonly string _Text;
		private readonly bool _IsActive;
		private readonly Action _Action;

		public DemoSetting(string text, bool isActive, Action action)
		{
			Contract.Requires(text != null);
			Contract.Requires(action != null);

			_Text = text;
			_IsActive = isActive;
			_Action = action;
		}

		public Action Action
		{
			get { return _Action; }
		}

		public bool IsActive
		{
			get { return _IsActive; }
		}

		public string Text
		{
			get { return _Text; }
		}
	}
}