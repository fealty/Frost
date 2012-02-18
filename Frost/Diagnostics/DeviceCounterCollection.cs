// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using CounterKey =
	System.Collections.Generic.KeyValuePair<string, string>;

namespace Frost.Diagnostics
{
	public sealed class DeviceCounterCollection
	{
		private readonly Dictionary<CounterKey, IDeviceCounter> _Counters;

		internal DeviceCounterCollection()
		{
			this._Counters = new Dictionary<CounterKey, IDeviceCounter>();
		}

		public bool Query(
			string category, string name, out IDeviceCounter result)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(category));
			Contract.Requires(!String.IsNullOrWhiteSpace(name));

			Trace.Assert(!String.IsNullOrWhiteSpace(category));
			Trace.Assert(!String.IsNullOrWhiteSpace(name));

			lock(this._Counters)
			{
				CounterKey key = new CounterKey(category, name);

				return this._Counters.TryGetValue(key, out result);
			}
		}

		public bool Query<T>(
			string category, string name, out IDeviceCounter<T> result)
			where T : struct
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(category));
			Contract.Requires(!String.IsNullOrWhiteSpace(name));

			Trace.Assert(!String.IsNullOrWhiteSpace(category));
			Trace.Assert(!String.IsNullOrWhiteSpace(name));

			lock(this._Counters)
			{
				IDeviceCounter counterResult;

				CounterKey key = new CounterKey(category, name);

				bool value = this._Counters.TryGetValue(key, out counterResult);

				result = (IDeviceCounter<T>)counterResult;

				return value;
			}
		}

		public void Register(IDeviceCounter counter)
		{
			CounterKey key = new CounterKey(counter.Category, counter.Name);

			lock(this._Counters)
			{
				this._Counters.Add(key, counter);
			}
		}
	}
}