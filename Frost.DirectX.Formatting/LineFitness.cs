using System;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting
{
	public struct LineFitness : IEquatable<LineFitness>
	{
		public static readonly LineFitness VeryTight;
		public static readonly LineFitness Tight;
		public static readonly LineFitness Loose;
		public static readonly LineFitness VeryLoose;

		private readonly int mFitnessClass;

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

			mFitnessClass = fitness;
		}

		public bool Equals(LineFitness other)
		{
			return other.mFitnessClass == mFitnessClass;
		}

		public override string ToString()
		{
			switch(mFitnessClass)
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
			return Math.Abs(mFitnessClass - other.mFitnessClass);
		}

		public static LineFitness FromLineRatio(double ratio)
		{
			Contract.Requires(ratio >= double.MinValue && ratio <= double.MaxValue);

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
			return mFitnessClass;
		}

		public static implicit operator int(LineFitness fitness)
		{
			return fitness.mFitnessClass;
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