// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Frost.Composition;

namespace Frost.Effects
{
	public abstract class Effect
	{
		internal Effect()
		{
		}
	}

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
	}
}