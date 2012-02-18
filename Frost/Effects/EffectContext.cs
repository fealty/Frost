// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	public sealed class EffectContext<T>
		: IEffectContext, IEquatable<EffectContext<T>>
		where T : struct, IEffectSettings, IEquatable<T>
	{
		private readonly Effect<T> _Effect;
		private readonly T _Options;

		public EffectContext(Effect<T> effect, ref T options)
		{
			Contract.Requires(effect != null);

			Trace.Assert(effect != null);

			this._Effect = effect;
			this._Options = options;
		}

		public EffectContext(Effect<T> effect, T options)
		{
			Contract.Requires(effect != null);

			Trace.Assert(effect != null);

			this._Effect = effect;
			this._Options = options;
		}

		public Effect<T> Effect
		{
			get { return this._Effect; }
		}

		public T Options
		{
			get { return this._Options; }
		}

		bool IEquatable<IEffectContext>.Equals(IEffectContext other)
		{
			return Equals(other);
		}

		IEffect IEffectContext.Effect
		{
			get { return this._Effect; }
		}

		public bool Equals(EffectContext<T> other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return other._Options.Equals(this._Options) &&
			       Equals(other._Effect, this._Effect);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			if(ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj is EffectContext<T> && Equals((EffectContext<T>)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (this._Options.GetHashCode() * 397) ^
				       (this._Effect != null ? this._Effect.GetHashCode() : 0);
			}
		}

		public static bool operator ==(
			EffectContext<T> left, EffectContext<T> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(
			EffectContext<T> left, EffectContext<T> right)
		{
			return !Equals(left, right);
		}
	}
}