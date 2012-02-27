// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	public sealed class Breakpoint : IEquatable<Breakpoint>
	{
		public static readonly Breakpoint Empty;

		public static readonly Breakpoint MaxDemerits;

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
			Empty = new Breakpoint(0, 0, LineFitness.Tight, 0.0, 0.0, 0.0, 0.0, 0.0);

			MaxDemerits = new Breakpoint(0, 0, LineFitness.Tight, 0.0, 0.0, 0.0, double.MaxValue, 0.0);
		}

		public Breakpoint(
			int position,
			int line,
			LineFitness fitnessClass,
			double totalWidth,
			double totalStretch,
			double totalShrink,
			Demerits demerits,
			double ratio)
			: this(position, line, fitnessClass, totalWidth, totalShrink, totalStretch, demerits, ratio, null)
		{
			Contract.Requires(position >= 0);
			Contract.Requires(line >= 0);
			Contract.Requires(Check.IsPositive(totalWidth));
			Contract.Requires(Check.IsFinite(totalShrink));
			Contract.Requires(Check.IsFinite(totalStretch));
			Contract.Requires(Check.IsFinite(ratio));
		}

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

		public Demerits Demerits
		{
			get { return _Demerits; }
		}

		public LineFitness Fitness
		{
			get { return _FitnessClass; }
		}

		public int Line
		{
			get { return _Line; }
		}

		public int Position
		{
			get { return _Position; }
		}

		public double Ratio
		{
			get { return _Ratio; }
		}

		public Breakpoint Previous
		{
			get { return _Previous; }
		}

		public double TotalShrink
		{
			get { return _TotalShrink; }
		}

		public double TotalStretch
		{
			get { return _TotalStretch; }
		}

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

			return other._Demerits.Equals(_Demerits) && other._FitnessClass.Equals(_FitnessClass) &&
			       other._Line == _Line && other._Position == _Position && Equals(other._Previous, _Previous) &&
			       other._Ratio.Equals(_Ratio) && other._TotalShrink.Equals(_TotalShrink) &&
			       other._TotalStretch.Equals(_TotalStretch) && other._TotalWidth.Equals(_TotalWidth);
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
				result = (result * 397) ^ (_Previous != null ? _Previous.GetHashCode() : 0);
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