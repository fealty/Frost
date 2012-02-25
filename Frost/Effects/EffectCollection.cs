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
			_Effects = new Dictionary<Type, IEffect>();
		}

		public void Register<T>() where T : Effect, new()
		{
			lock(_Effects)
			{
				Effect effect = new T();

				_Effects[effect.OptionsType] = (IEffect)effect;
			}
		}

		public Effect<T> Find<T>() where T : struct, IEffectSettings, IEquatable<T>
		{
			IEffect result;

			lock(_Effects)
			{
				_Effects.TryGetValue(typeof(T), out result);
			}

			return result as Effect<T>;
		}

		public void Unregister<T>()
		{
			lock(_Effects)
			{
				_Effects.Remove(typeof(T));
			}
		}
	}
}