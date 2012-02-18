// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using Frost.Diagnostics;

namespace Frost
{
	public abstract class Device2D
	{
		private readonly DeviceCounterCollection _CounterCollection;

		protected Device2D()
		{
			this._CounterCollection = new DeviceCounterCollection();
		}

		public DeviceCounterCollection Diagnostics
		{
			get { return this._CounterCollection; }
		}
	}
}