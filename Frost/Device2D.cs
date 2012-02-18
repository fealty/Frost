// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using Frost.Diagnostics;
using Frost.Effects;

namespace Frost
{
	public abstract class Device2D
	{
		private readonly DeviceCounterCollection _CounterCollection;
		private readonly EffectCollection _EffectCollection;

		protected Device2D()
		{
			this._CounterCollection = new DeviceCounterCollection();
			this._EffectCollection = new EffectCollection();
		}

		public DeviceCounterCollection Diagnostics
		{
			get { return this._CounterCollection; }
		}

		public EffectCollection Effects
		{
			get { return this._EffectCollection; }
		}
	}
}