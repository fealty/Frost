// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frost.Painting
{
	public sealed class Gradient : IEquatable<Gradient>
	{
		[ThreadStatic] private static Builder _Builder;

		private readonly GradientStop[] _Stops;

		private Gradient(GradientStop[] stops)
		{
			Contract.Requires(stops != null);

			this._Stops = stops;

			Contract.Assert(Stops.Equals(stops));
		}

		public GradientStop[] Stops
		{
			get
			{
				Contract.Ensures(
					Contract.Result<GradientStop[]>().Equals(this._Stops));
				Contract.Ensures(Contract.Result<GradientStop[]>() != null);

				return this._Stops;
			}
		}

		public static Builder Create()
		{
			Contract.Ensures(Contract.Result<Builder>() != null);

			_Builder = _Builder ?? new Builder();

			_Builder.Reset();

			return _Builder;
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(this._Stops != null);
		}

		public sealed class Builder
		{
			private readonly List<GradientStop> _Stops;

			internal Builder()
			{
				this._Stops = new List<GradientStop>();
			}

			public Builder WithStop(float position, Color color)
			{
				Trace.Assert(Check.IsNormalized(position));

				if(this._Stops.Count > 0)
				{
					Trace.Assert(
						position > this._Stops[this._Stops.Count - 1].Position);
				}

				this._Stops.Add(new GradientStop(position, color));

				return this;
			}

			public Gradient Build()
			{
				Trace.Assert(this._Stops.Count >= 2);
				Trace.Assert(this._Stops[0].Position.Equals(0.0f));
				Trace.Assert(
					this._Stops[this._Stops.Count - 1].Position.Equals(1.0f));

				return new Gradient(this._Stops.ToArray());
			}

			internal void Reset()
			{
				this._Stops.Clear();
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

			return other._Stops.SequenceEqual(this._Stops);
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

				foreach(GradientStop stop in this._Stops)
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

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Gradient gradient =
				Create().WithStop(0.0f, new Color(0.0f, 0.0f, 0.0f)).WithStop(
					0.5f, new Color(0.5f, 0.5f, 0.5f)).WithStop(
						1.0f, new Color(1.0f, 1.0f, 1.0f)).Build();

			Assert.Equal(0.0f, gradient.Stops[0].Position);
			Assert.Equal(new Color(0.0f, 0.0f, 0.0f), gradient.Stops[0].Color);
			Assert.Equal(0.5f, gradient.Stops[1].Position);
			Assert.Equal(new Color(0.5f, 0.5f, 0.5f), gradient.Stops[1].Color);
			Assert.Equal(1.0f, gradient.Stops[2].Position);
			Assert.Equal(new Color(1.0f, 1.0f, 1.0f), gradient.Stops[2].Color);

			Gradient gradient2 =
				Create().WithStop(0.0f, Color.AliceBlue).WithStop(
					1.0f, Color.AntiqueWhite).Build();

			Assert.TestObject(gradient, gradient2);
		}
#endif
	}
}