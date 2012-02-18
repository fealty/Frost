// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.Painting
{
	public struct GradientStop : IEquatable<GradientStop>
	{
		private readonly Color _Color;
		private readonly float _Position;

		public GradientStop(float position, Color color)
		{
			Contract.Requires(Check.IsNormalized(position));

			this._Position = position;
			this._Color = color;

			Contract.Assert(Position.Equals(position));
			Contract.Assert(Color.Equals(color));
		}

		public Color Color
		{
			get
			{
				Contract.Ensures(Contract.Result<Color>().Equals(this._Color));

				return this._Color;
			}
		}

		public float Position
		{
			get
			{
				Contract.Ensures(Contract.Result<float>().Equals(this._Position));
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));

				return this._Position;
			}
		}

		public bool Equals(GradientStop other)
		{
			return other._Color.Equals(this._Color) &&
			       other._Position.Equals(this._Position);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is GradientStop && Equals((GradientStop)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (this._Color.GetHashCode() * 397) ^
				       this._Position.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Position: {0}, Color: {1}", this._Position, this._Color);
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(Check.IsNormalized(this._Position));
		}

		public static bool operator ==(GradientStop left, GradientStop right
			)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GradientStop left, GradientStop right
			)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.TestObject(
				new GradientStop(0.0f, Color.Red),
				new GradientStop(1.0f, Color.Blue));
		}
#endif
	}
}