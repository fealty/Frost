﻿// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
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

			public void Reset(Canvas target, Device2D device2D)
			{
				Contract.Requires(device2D != null);
				Contract.Requires(target != null);
			}

			public IEnumerable<DemoSetting> Settings
			{
				get
				{
					Contract.Ensures(Contract.Result<IEnumerable<DemoSetting>>() != null);

					throw new NotSupportedException();
				}
			}
		}
	}

	[ContractClass(typeof(IDemoContextContract))] public interface IDemoContext : IDisposable
	{
		void Reset(Canvas target, Device2D device2D);

		IEnumerable<DemoSetting> Settings { get; }
	}
}