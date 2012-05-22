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
		private readonly Dictionary<Type, Effect> _Effects;

		internal EffectCollection()
		{
			_Effects = new Dictionary<Type, Effect>();
		}

		public void Register<T>() where T : Effect, new()
		{
			lock(_Effects)
			{
				Effect effect = new T();

				_Effects[effect.OptionsType] = effect;
			}
		}

		public Effect<T> Find<T>() where T : struct, IEffectSettings, IEquatable<T>
		{
			Effect result;

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