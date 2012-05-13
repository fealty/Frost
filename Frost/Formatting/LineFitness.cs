// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	/// <summary>
	///   indicates the fitness of the spacing of items on a line
	/// </summary>
	internal struct LineFitness : IEquatable<LineFitness>
	{
		/// <summary>
		///   the items on the line are very tightly spaced
		/// </summary>
		public static readonly LineFitness VeryTight;

		/// <summary>
		///   the items on the line are tightly spaced
		/// </summary>
		public static readonly LineFitness Tight;

		/// <summary>
		///   the items on the line are loosely spaced
		/// </summary>
		public static readonly LineFitness Loose;

		/// <summary>
		///   the items on the line are very loosely spaced
		/// </summary>
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

		/// <summary>
		///   measures the fitness gap between the <see cref="LineFitness" /> and another
		/// </summary>
		/// <param name="other"> the fitness to compare </param>
		/// <returns> the integral fitness gap between the <see cref="LineFitness" /> and <paramref name="other" /> </returns>
		public int MeasureFitnessGap(LineFitness other)
		{
			return Math.Abs(_FitnessClass - other._FitnessClass);
		}

		/// <summary>
		///   determines the line fitness from the line ratio
		/// </summary>
		/// <param name="ratio"> the line ratio </param>
		/// <returns> the line fitness for <paramref name="ratio" /> </returns>
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