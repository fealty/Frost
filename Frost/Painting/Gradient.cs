// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frost.Painting
{
	public sealed class Gradient : IEquatable<Gradient>
	{
		[ThreadStatic]
		private static Builder _Builder;

		private readonly GradientStop[] _Stops;

		private Gradient(GradientStop[] stops)
		{
			Contract.Requires(stops != null);
			Contract.Requires(stops.Length >= 2);

			_Stops = stops;

			Contract.Assert(Stops.Equals(stops));
		}

		public GradientStop[] Stops
		{
			get
			{
				Contract.Ensures(Contract.Result<GradientStop[]>().Equals(_Stops));
				Contract.Ensures(Contract.Result<GradientStop[]>() != null);

				return _Stops;
			}
		}

		public static Builder Create()
		{
			Contract.Ensures(Contract.Result<Builder>() != null);

			_Builder = _Builder ?? new Builder();

			_Builder.Reset();

			return _Builder;
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Stops != null);
		}

		public sealed class Builder
		{
			private readonly List<GradientStop> _Stops;

			internal Builder()
			{
				_Stops = new List<GradientStop>();
			}

			public Builder WithStop(float position, Color color)
			{
				Contract.Requires(Check.IsNormalized(position));
				Contract.Ensures(Contract.Result<Builder>() != null);

				if(_Stops.Count > 0)
				{
					Contract.Assert(position > _Stops[_Stops.Count - 1].Position);
				}

				_Stops.Add(new GradientStop(position, color));

				return this;
			}

			public Gradient Build()
			{
				if(_Stops.Count >= 2)
				{
					_Stops[0] = new GradientStop(0.0f, _Stops[0].Color);
					_Stops[_Stops.Count - 1] = new GradientStop(
						1.0f, _Stops[_Stops.Count - 1].Color);

					return new Gradient(_Stops.ToArray());
				}

				return null;
			}

			internal void Reset()
			{
				_Stops.Clear();
			}
		}

		public bool Equals(Gradient other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return other._Stops.SequenceEqual(_Stops);
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

			return obj is Gradient && Equals((Gradient)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = 0;

				foreach(GradientStop stop in _Stops)
				{
					result = (result * 397) ^ stop.GetHashCode();
				}

				return result;
			}
		}

		public static bool operator ==(Gradient left, Gradient right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Gradient left, Gradient right)
		{
			return !Equals(left, right);
		}
	}
}