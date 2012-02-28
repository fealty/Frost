// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Demo.Framework.Contracts;

using Frost;

namespace Demo.Framework
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IDemoContext))] internal abstract class IDemoContextContract
			: IDemoContext
		{
			public abstract void Dispose();

			public string Name
			{
				get
				{
					Contract.Ensures(Contract.Result<string>() != null);

					throw new NotSupportedException();
				}
			}

			public void Reset(Canvas target, Device2D device2D)
			{
				Contract.Requires(device2D != null);
				Contract.Requires(Check.IsValid(target, device2D));
			}
		}
	}

	[ContractClass(typeof(IDemoContextContract))] public interface IDemoContext : IDisposable
	{
		string Name { get; }

		void Reset(Canvas target, Device2D device2D);
	}
}