// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using CounterKey =
	System.Collections.Generic.KeyValuePair<string, string>;

namespace Frost.Diagnostics
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IDeviceCounterCollection))] internal abstract class IDeviceCounterCollectionContract
			: IDeviceCounterCollection
		{
			public abstract
				IEnumerator<KeyValuePair<CounterKey, IDeviceCounter>>
				GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			public bool Query(
				string category, string name, out IDeviceCounter result)
			{
				Contract.Requires(!string.IsNullOrWhiteSpace(category));
				Contract.Requires(!string.IsNullOrWhiteSpace(name));

				throw new NotSupportedException();
			}

			public bool Query<T>(
				string category, string name, out IDeviceCounter<T> result)
				where T : struct
			{
				Contract.Requires(!string.IsNullOrWhiteSpace(category));
				Contract.Requires(!string.IsNullOrWhiteSpace(name));

				throw new NotSupportedException();
			}

			public void Register(IDeviceCounter counter)
			{
				Contract.Requires(counter != null);
			}
		}
	}

	[ContractClass(typeof(Contracts.IDeviceCounterCollectionContract))] public interface IDeviceCounterCollection
		: IEnumerable<KeyValuePair<CounterKey, IDeviceCounter>>
	{
		bool Query(string category, string name, out IDeviceCounter result);

		bool Query<T>(
			string category, string name, out IDeviceCounter<T> result)
			where T : struct;

		void Register(IDeviceCounter counter);
	}
}