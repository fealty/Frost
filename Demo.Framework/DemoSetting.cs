// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Demo.Framework
{
	public struct DemoSetting
	{
		private readonly Action _Action;
		private readonly string _ActiveText;
		private readonly string _InactiveText;
		private readonly bool _IsActive;

		public DemoSetting(string activeText, string inactiveText, bool isActive, Action action)
		{
			Contract.Requires(activeText != null);
			Contract.Requires(inactiveText != null);
			Contract.Requires(action != null);

			_ActiveText = activeText;
			_InactiveText = inactiveText;
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

		public string InactiveText
		{
			get { return _InactiveText; }
		}

		public string ActiveText
		{
			get { return _ActiveText; }
		}
	}
}