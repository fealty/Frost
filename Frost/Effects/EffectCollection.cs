// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;

namespace Frost.Effects
{
	public sealed class EffectCollection
	{
		private readonly Dictionary<Type, IEffect> _Effects;

		internal EffectCollection()
		{
			this._Effects = new Dictionary<Type, IEffect>();
		}

		public void Register<T>() where T : Effect, new()
		{
			lock(this._Effects)
			{
				Effect effect = new T();

				this._Effects[effect.OptionsType] = (IEffect)effect;
			}
		}

		public Effect<T> Find<T>()
			where T : struct, IEffectSettings, IEquatable<T>
		{
			IEffect result;

			lock(this._Effects)
			{
				this._Effects.TryGetValue(typeof(T), out result);
			}

			return result as Effect<T>;
		}

		public void Unregister<T>()
		{
			lock(this._Effects)
			{
				this._Effects.Remove(typeof(T));
			}
		}
	}
}