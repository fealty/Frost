// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IEffectContext))] internal abstract class
			IEffectContextContract : IEffectContext
		{
			public abstract bool Equals(IEffectContext other);

			public IEffect Effect
			{
				get
				{
					Contract.Ensures(Contract.Result<IEffect>() != null);

					throw new NotSupportedException();
				}
			}
		}
	}

	[ContractClass(typeof(Contracts.IEffectContextContract))] internal
		interface IEffectContext : IEquatable<IEffectContext>
	{
		IEffect Effect { get; }
	}
}