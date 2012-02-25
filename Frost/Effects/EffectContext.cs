// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Effects
{
	public abstract class EffectContext
	{
		internal EffectContext()
		{
		}
	}

	public sealed class EffectContext<T>
		: EffectContext, IEffectContext, IEquatable<EffectContext<T>>
		where T : struct, IEffectSettings, IEquatable<T>
	{
		private readonly Effect<T> _Effect;
		private readonly T _Options;

		public EffectContext(Effect<T> effect, ref T options)
		{
			Contract.Requires(effect != null);

			_Effect = effect;
			_Options = options;

			Contract.Assert(Effect.Equals(effect));
			Contract.Assert(Options.Equals(options));
		}

		public EffectContext(Effect<T> effect, T options)
			: this(effect, ref options)
		{
			Contract.Requires(effect != null);
		}

		public Effect<T> Effect
		{
			get
			{
				Contract.Ensures(Contract.Result<Effect<T>>() != null);

				return _Effect;
			}
		}

		public T Options
		{
			get { return _Options; }
		}

		bool IEquatable<IEffectContext>.Equals(IEffectContext other)
		{
			return Equals(other);
		}

		IEffect IEffectContext.Effect
		{
			get { return _Effect; }
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

			return other._Options.Equals(_Options) &&
			       Equals(other._Effect, _Effect);
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
				return (_Options.GetHashCode() * 397) ^
				       (_Effect != null ? _Effect.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Effect: {0}, Options: {1}", _Effect, _Options);
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