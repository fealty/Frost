// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
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

			_Position = position;
			_Color = color;

			Contract.Assert(Position.Equals(position));
			Contract.Assert(Color.Equals(color));
		}

		public Color Color
		{
			get
			{
				Contract.Ensures(Contract.Result<Color>().Equals(_Color));

				return _Color;
			}
		}

		public float Position
		{
			get
			{
				Contract.Ensures(Contract.Result<float>().Equals(_Position));
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));

				return _Position;
			}
		}

		public bool Equals(GradientStop other)
		{
			return other._Color.Equals(_Color) && other._Position.Equals(_Position);
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
				return (_Color.GetHashCode() * 397) ^ _Position.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format("Position: {0}, Color: {1}", _Position, _Color);
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsNormalized(_Position));
		}

		public static bool operator ==(GradientStop left, GradientStop right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GradientStop left, GradientStop right)
		{
			return !left.Equals(right);
		}
	}
}