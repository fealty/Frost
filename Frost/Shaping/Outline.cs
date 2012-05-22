// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Construction;

namespace Frost.Shaping
{
	public struct Outline : IEquatable<Outline>
	{
		private readonly float _Baseline;
		private readonly Figure _Outline;

		public Outline(Figure outline, float baseline)
		{
			Contract.Requires(Check.IsFinite(baseline));

			_Outline = outline;
			_Baseline = baseline;

			Contract.Assert(ReferenceEquals(Figure, outline));
			Contract.Assert(Baseline.Equals(baseline));
		}

		public Figure Figure
		{
			get
			{
				Contract.Ensures(Contract.Result<Figure>() == _Outline);

				return _Outline;
			}
		}

		public float Baseline
		{
			get
			{
				Contract.Ensures(Contract.Result<float>().Equals(_Baseline));
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));

				return _Baseline;
			}
		}

		public bool Equals(Outline other)
		{
			return
				other._Baseline.Equals(_Baseline) &&
					Equals(other._Outline, _Outline);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Outline && Equals((Outline)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _Baseline.GetHashCode();
				result = (result * 397) ^ (_Outline != null ? _Outline.GetHashCode() : 0);
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"Baseline: {0}, Outline: {1}",
				_Baseline,
				_Outline);
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsFinite(_Baseline));
		}

		public static bool operator ==(Outline left, Outline right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Outline left, Outline right)
		{
			return !left.Equals(right);
		}
	}
}