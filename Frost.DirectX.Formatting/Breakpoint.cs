using System;
using System.Diagnostics.Contracts;

namespace Cabbage.Formatting
{
	public sealed class Breakpoint : IEquatable<Breakpoint>
	{
		public static readonly Breakpoint Empty;

		public static readonly Breakpoint MaxDemerits;

		private readonly Demerits mDemerits;
		private readonly LineFitness mFitnessClass;
		private readonly int mLine;
		private readonly int mPosition;
		private readonly Breakpoint mPrevious;
		private readonly double mRatio;
		private readonly double mTotalShrink;
		private readonly double mTotalStretch;
		private readonly double mTotalWidth;

		static Breakpoint()
		{
			Empty = new Breakpoint(
				0, 0, LineFitness.Tight, 0.0, 0.0, 0.0, 0.0, 0.0);

			MaxDemerits = new Breakpoint(
				0, 0, LineFitness.Tight, 0.0, 0.0, 0.0, double.MaxValue, 0.0);
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
			: this(
				position,
				line,
				fitnessClass,
				totalWidth,
				totalShrink,
				totalStretch,
				demerits,
				ratio,
				null)
		{
			Contract.Requires(position >= 0);
			Contract.Requires(line >= 0);
			Contract.Requires(totalWidth >= 0.0 && totalWidth <= double.MaxValue);
			Contract.Requires(
				totalShrink >= double.MinValue && totalShrink <= double.MaxValue);
			Contract.Requires(
				totalStretch >= double.MinValue && totalStretch <= double.MaxValue);
			Contract.Requires(ratio >= double.MinValue && ratio <= double.MaxValue);
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
			Contract.Requires(totalWidth >= 0.0 && totalWidth <= double.MaxValue);
			Contract.Requires(
				totalShrink >= double.MinValue && totalShrink <= double.MaxValue);
			Contract.Requires(
				totalStretch >= double.MinValue && totalStretch <= double.MaxValue);
			Contract.Requires(ratio >= double.MinValue && ratio <= double.MaxValue);

			mPosition = position;
			mLine = line;
			mFitnessClass = fitnessClass;
			mTotalWidth = totalWidth;
			mTotalShrink = totalShrink;
			mTotalStretch = totalStretch;
			mDemerits = demerits;
			mPrevious = previous;
			mRatio = ratio;
		}

		public Demerits Demerits
		{
			get { return mDemerits; }
		}

		public LineFitness Fitness
		{
			get { return mFitnessClass; }
		}

		public int Line
		{
			get { return mLine; }
		}

		public int Position
		{
			get { return mPosition; }
		}

		public double Ratio
		{
			get { return mRatio; }
		}

		public Breakpoint Previous
		{
			get { return mPrevious; }
		}

		public double TotalShrink
		{
			get { return mTotalShrink; }
		}

		public double TotalStretch
		{
			get { return mTotalStretch; }
		}

		public double TotalWidth
		{
			get { return mTotalWidth; }
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

			return other.mDemerits.Equals(mDemerits) &&
			       other.mFitnessClass.Equals(mFitnessClass) && other.mLine == mLine &&
			       other.mPosition == mPosition && Equals(other.mPrevious, mPrevious) &&
			       other.mRatio.Equals(mRatio) && other.mTotalShrink.Equals(mTotalShrink) &&
			       other.mTotalStretch.Equals(mTotalStretch) &&
			       other.mTotalWidth.Equals(mTotalWidth);
		}

		public override string ToString()
		{
			return string.Format(
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
				int result = mDemerits.GetHashCode();
				result = (result * 397) ^ mFitnessClass.GetHashCode();
				result = (result * 397) ^ mLine;
				result = (result * 397) ^ mPosition;
				result = (result * 397) ^ (mPrevious != null ? mPrevious.GetHashCode() : 0);
				result = (result * 397) ^ mRatio.GetHashCode();
				result = (result * 397) ^ mTotalShrink.GetHashCode();
				result = (result * 397) ^ mTotalStretch.GetHashCode();
				result = (result * 397) ^ mTotalWidth.GetHashCode();
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