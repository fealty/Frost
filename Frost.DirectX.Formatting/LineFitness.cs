// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	public struct LineFitness : IEquatable<LineFitness>
	{
		public static readonly LineFitness VeryTight;
		public static readonly LineFitness Tight;
		public static readonly LineFitness Loose;
		public static readonly LineFitness VeryLoose;

		private readonly int _FitnessClass;

		static LineFitness()
		{
			VeryTight = new LineFitness(0);
			Tight = new LineFitness(1);
			Loose = new LineFitness(2);
			VeryLoose = new LineFitness(3);
		}

		private LineFitness(int fitness)
		{
			Contract.Requires(fitness >= 0 && fitness < 4);

			_FitnessClass = fitness;
		}

		public bool Equals(LineFitness other)
		{
			return other._FitnessClass == _FitnessClass;
		}

		public override string ToString()
		{
			switch(_FitnessClass)
			{
				case 0:
					return "VeryTight";
				case 1:
					return "Tight";
				case 2:
					return "Loose";
			}

			return "VeryLoose";
		}

		public int MeasureFitnessGap(LineFitness other)
		{
			return Math.Abs(_FitnessClass - other._FitnessClass);
		}

		public static LineFitness FromLineRatio(double ratio)
		{
			Contract.Requires(Check.IsFinite(ratio));

			if(ratio < -0.5)
			{
				return VeryTight;
			}

			if(ratio <= 0.5)
			{
				return Tight;
			}

			return ratio <= 1.0 ? Loose : VeryLoose;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is LineFitness && Equals((LineFitness)obj);
		}

		public override int GetHashCode()
		{
			return _FitnessClass;
		}

		public static implicit operator int(LineFitness fitness)
		{
			return fitness._FitnessClass;
		}

		public static bool operator ==(LineFitness left, LineFitness right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LineFitness left, LineFitness right)
		{
			return !left.Equals(right);
		}
	}
}