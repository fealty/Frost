// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Composition;

namespace Frost.Effects
{
	namespace Contracts
	{
		[ContractClassFor(typeof(Effect))]
		internal abstract class EffectContract : Effect
		{
			internal override Type OptionsType
			{
				get
				{
					Contract.Ensures(Contract.Result<Type>() != null);

					throw new NotSupportedException();
				}
			}
		}

		[ContractClassFor(typeof(Effect<>))]
		internal abstract class EffectContract<T> : Effect<T>
			where T : struct, IEffectSettings, IEquatable<T>
		{
			public override void Apply<TEnum>(
				TEnum batchedItems,
				EffectContext<T> effectContext,
				Compositor compositionContext)
			{
				Contract.Requires(batchedItems != null);
				Contract.Requires(effectContext != null);
				Contract.Requires(compositionContext != null);
			}
		}
	}

	[ContractClass(typeof(Contracts.EffectContract))]
	public abstract class Effect
	{
		internal Effect()
		{
		}

		internal abstract Type OptionsType { get; }
	}

	[ContractClass(typeof(Contracts.EffectContract<>))]
	public abstract class Effect<T> : Effect, IEffect
		where T : struct, IEffectSettings, IEquatable<T>
	{
		void IEffect.Apply<TEnum>(
			TEnum batchedItems,
			IEffectContext effectContext,
			Compositor compositionContext)
		{
		}

		public abstract void Apply<TEnum>(
			TEnum batchedItems,
			EffectContext<T> effectContext,
			Compositor compositionContext)
			where TEnum : class, IEnumerable<BatchedItem>;

		internal sealed override Type OptionsType
		{
			get { return typeof(T); }
		}
	}
}