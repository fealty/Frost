// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This struct provides information about a chosen breaking point in the text.
	/// </summary>
	internal struct BreakIndex : IEquatable<BreakIndex>
	{
		private readonly int _Index;
		private readonly double _Ratio;

		/// <summary>
		///   This constructor initialize a new instance of the struct.
		/// </summary>
		/// <param name="index"> This parameter indicates the text index of the break. </param>
		/// <param name="ratio"> This parameter indicates the text spacing ratio of the break. </param>
		public BreakIndex(int index, double ratio)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(Check.IsFinite(ratio));

			_Index = index;
			_Ratio = ratio;
		}

		/// <summary>
		///   This property indicates the text spacing ratio of the break.
		/// </summary>
		public double Ratio
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<double>()));
				Contract.Ensures(Contract.Result<double>().Equals(_Ratio));

				return _Ratio;
			}
		}

		/// <summary>
		///   This property indicates the text index of the break.
		/// </summary>
		public int Index
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);
				Contract.Ensures(Contract.Result<int>().Equals(_Index));

				return _Index;
			}
		}

		public bool Equals(BreakIndex other)
		{
			return other._Index == _Index && other._Ratio.Equals(_Ratio);
		}

		public override string ToString()
		{
			return string.Format("Index: {0}, Ratio: {1}", _Index, _Ratio);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is BreakIndex && Equals((BreakIndex)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_Index * 397) ^ _Ratio.GetHashCode();
			}
		}

		public static bool operator ==(BreakIndex left, BreakIndex right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(BreakIndex left, BreakIndex right)
		{
			return !left.Equals(right);
		}
	}
}