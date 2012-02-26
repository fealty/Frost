// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Composition;
using Frost.Effects.Contracts;

namespace Frost.Effects
{
	namespace Contracts
	{
		[ContractClassFor(typeof(Effect))] internal abstract class EffectContract : Effect
		{
			public override Type OptionsType
			{
				get
				{
					Contract.Ensures(Contract.Result<Type>() != null);

					throw new NotSupportedException();
				}
			}
		}

		[ContractClassFor(typeof(Effect<>))] internal abstract class EffectContract<T> : Effect<T>
			where T : struct, IEffectSettings, IEquatable<T>
		{
			public override void Apply<TEnum>(
				TEnum batchedItems, EffectContext<T> effectContext, Compositor compositionContext)
			{
				Contract.Requires(batchedItems != null);
				Contract.Requires(effectContext != null);
				Contract.Requires(compositionContext != null);
			}
		}
	}

	[ContractClass(typeof(EffectContract))] public abstract class Effect
	{
		internal Effect()
		{
		}

		public abstract Type OptionsType { get; }

		public abstract void Apply<TEnum>(
			TEnum batchedItems, EffectContext effectContext, Compositor compositionContext)
			where TEnum : class, IEnumerable<BatchedItem>;
	}

	[ContractClass(typeof(EffectContract<>))] public abstract class Effect<T> : Effect
		where T : struct, IEffectSettings, IEquatable<T>
	{
		public override sealed Type OptionsType
		{
			get { return typeof(T); }
		}

		public abstract void Apply<TEnum>(
			TEnum batchedItems, EffectContext<T> effectContext, Compositor compositionContext)
			where TEnum : class, IEnumerable<BatchedItem>;

		public override void Apply<T1>(
			T1 batchedItems, EffectContext effectContext, Compositor compositionContext)
		{
			Apply(batchedItems, (EffectContext<T>)effectContext, compositionContext);
		}
	}
}