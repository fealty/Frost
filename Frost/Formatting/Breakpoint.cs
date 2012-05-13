// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	/// <summary>
	///   stores information for possible breaking point in the Knuth-Plass line breaking algorithm
	/// </summary>
	internal sealed class Breakpoint : IEquatable<Breakpoint>
	{
		private static readonly Breakpoint _Empty;
		private static readonly Breakpoint _MaxDemerits;

		private readonly Demerits _Demerits;
		private readonly LineFitness _FitnessClass;

		private readonly int _Line;
		private readonly int _Position;

		private readonly Breakpoint _Previous;

		private readonly double _Ratio;
		private readonly double _TotalShrink;
		private readonly double _TotalStretch;
		private readonly double _TotalWidth;

		static Breakpoint()
		{
			_Empty = new Breakpoint(0, 0, LineFitness.Tight, 0.0, 0.0, 0.0, 0.0, 0.0);

			_MaxDemerits = new Breakpoint(
				0, 0, LineFitness.Tight, 0.0, 0.0, 0.0, double.MaxValue, 0.0);
		}

		/// <summary>
		///   constructs a solitary instance of this class
		/// </summary>
		/// <param name="position"> indicates the item index position of the breaking point </param>
		/// <param name="line"> indicates the line number of the breaking point </param>
		/// <param name="fitnessClass"> indicates the line's fitness when theoretically broken at the breaking point </param>
		/// <param name="totalWidth"> indicates the total width of the line before the breaking point </param>
		/// <param name="totalStretch"> indicates the total stretch of the line before the breaking point </param>
		/// <param name="totalShrink"> indicates the total shrink of the line before the breaking point </param>
		/// <param name="demerits"> indicates the penalty incurred for breaking at this breaking point </param>
		/// <param name="ratio"> indicates the line's ratio when theoretically broken at the breaking point </param>
		public Breakpoint(
			int position,
			int line,
			LineFitness fitnessClass,
			double totalWidth,
			double totalStretch,
			double totalShrink,
			Demerits demerits,
			double ratio)
			: this(
				position,
				line,
				fitnessClass,
				totalWidth,
				totalShrink,
				totalStretch,
				demerits,
				ratio,
				null
				)
		{
			Contract.Requires(position >= 0);
			Contract.Requires(line >= 0);
			Contract.Requires(Check.IsPositive(totalWidth));
			Contract.Requires(Check.IsFinite(totalShrink));
			Contract.Requires(Check.IsFinite(totalStretch));
			Contract.Requires(Check.IsFinite(ratio));
		}

		/// <summary>
		///   constructs a chained instance of this class
		/// </summary>
		/// <param name="position"> indicates the textual position of the breaking point </param>
		/// <param name="line"> indicates the line number of the breaking point </param>
		/// <param name="fitnessClass"> indicates the line's fitness when theoretically broken at the breaking point </param>
		/// <param name="totalWidth"> indicates the total width of the line before the breaking point </param>
		/// <param name="totalStretch"> indicates the total stretch of the line before the breaking point </param>
		/// <param name="totalShrink"> indicates the total shrink of the line before the breaking point </param>
		/// <param name="demerits"> indicates the penalty incurred for breaking at this breaking point </param>
		/// <param name="ratio"> indicates the line's ratio when theoretically broken at the breaking point </param>
		/// <param name="previous"> references the previous breaking point in the chain </param>
		public Breakpoint(
			int position,
			int line,
			LineFitness fitnessClass,
			double totalWidth,
			double totalStretch,
			double totalShrink,
			Demerits demerits,
			double ratio,
			Breakpoint previous)
		{
			Contract.Requires(position >= 0);
			Contract.Requires(line >= 0);
			Contract.Requires(Check.IsPositive(totalWidth));
			Contract.Requires(Check.IsFinite(totalShrink));
			Contract.Requires(Check.IsFinite(totalStretch));
			Contract.Requires(Check.IsFinite(ratio));

			_Position = position;
			_Line = line;
			_FitnessClass = fitnessClass;
			_TotalWidth = totalWidth;
			_TotalShrink = totalShrink;
			_TotalStretch = totalStretch;
			_Demerits = demerits;
			_Previous = previous;
			_Ratio = ratio;
		}

		public static Breakpoint MaxDemerits
		{
			get { return _MaxDemerits; }
		}

		public static Breakpoint Empty
		{
			get { return _Empty; }
		}

		/// <summary>
		///   indicates the penalty incurred when breaking at this breakpoint
		/// </summary>
		public Demerits Demerits
		{
			get { return _Demerits; }
		}

		/// <summary>
		///   indicates the fitness class of the line when breaking at this breakpoint
		/// </summary>
		public LineFitness Fitness
		{
			get { return _FitnessClass; }
		}

		/// <summary>
		///   indicates the line number of the line when breaking at this breakpoint
		/// </summary>
		public int Line
		{
			get { return _Line; }
		}

		/// <summary>
		///   indicates the textual position of the breakpoint
		/// </summary>
		public int Position
		{
			get { return _Position; }
		}

		/// <summary>
		///   indicates the line ratio when breaking at this breakpoint
		/// </summary>
		public double Ratio
		{
			get { return _Ratio; }
		}

		/// <summary>
		///   references the previous breakpoint in the chain
		/// </summary>
		public Breakpoint Previous
		{
			get { return _Previous; }
		}

		/// <summary>
		///   indicates the total shrink of the line when broken at this breakpoint
		/// </summary>
		public double TotalShrink
		{
			get { return _TotalShrink; }
		}

		/// <summary>
		///   indicates the total stretch of the line when broken at this breakpoint
		/// </summary>
		public double TotalStretch
		{
			get { return _TotalStretch; }
		}

		/// <summary>
		///   indicates the total width of the line when broken at this breakpoint
		/// </summary>
		public double TotalWidth
		{
			get { return _TotalWidth; }
		}

		public bool Equals(Breakpoint other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return other._Demerits.Equals(_Demerits) &&
				other._FitnessClass.Equals(_FitnessClass) &&
					other._Line == _Line && other._Position == _Position &&
						Equals(other._Previous, _Previous) &&
							other._Ratio.Equals(_Ratio) && other._TotalShrink.Equals(_TotalShrink) &&
								other._TotalStretch.Equals(_TotalStretch) &&
									other._TotalWidth.Equals(_TotalWidth);
		}

		public override string ToString()
		{
			return
				string.Format(
					"Demerits: {0}, Ratio: {1} ({2}), Line: {3}, Position: {4}, Width: {5}, Stretch: {6}, Shrink: {7}",
					Demerits,
					Ratio,
					Fitness,
					Line,
					Position,
					TotalWidth,
					TotalStretch,
					TotalShrink);
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

			return obj is Breakpoint && Equals((Breakpoint)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _Demerits.GetHashCode();
				result = (result * 397) ^ _FitnessClass.GetHashCode();
				result = (result * 397) ^ _Line;
				result = (result * 397) ^ _Position;
				result = (result * 397) ^
					(_Previous != null ? _Previous.GetHashCode() : 0);
				result = (result * 397) ^ _Ratio.GetHashCode();
				result = (result * 397) ^ _TotalShrink.GetHashCode();
				result = (result * 397) ^ _TotalStretch.GetHashCode();
				result = (result * 397) ^ _TotalWidth.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(Breakpoint left, Breakpoint right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Breakpoint left, Breakpoint right)
		{
			return !Equals(left, right);
		}
	}
}