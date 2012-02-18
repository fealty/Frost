// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Frost.Shaping;

namespace Frost.Formatting
{
	public struct Outline : IEquatable<Outline>
	{
		private readonly float _EmSize;
		private readonly float _NormalizedBaseline;
		private readonly Geometry _NormalizedOutline;

		public Outline(
			Geometry normalizedOutline, float emSize, float normalizedBaseline)
		{
			Contract.Requires(normalizedOutline != null);
			Contract.Requires(Check.IsPositive(emSize));
			Contract.Requires(Check.IsFinite(normalizedBaseline));

			this._NormalizedOutline = normalizedOutline;
			this._EmSize = emSize;
			this._NormalizedBaseline = normalizedBaseline;

			Contract.Assert(NormalizedOutline.Equals(normalizedOutline));
			Contract.Assert(NormalizedBaseline.Equals(normalizedBaseline));
			Contract.Assert(EmSize.Equals(emSize));
		}

		public Geometry NormalizedOutline
		{
			get
			{
				Contract.Ensures(
					Contract.Result<Geometry>() == this._NormalizedOutline);
				Contract.Ensures(Contract.Result<Geometry>() != null);

				return this._NormalizedOutline;
			}
		}

		public float NormalizedBaseline
		{
			get
			{
				Contract.Ensures(
					Contract.Result<float>().Equals(this._NormalizedBaseline));
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));

				return this._NormalizedBaseline;
			}
		}

		public float EmSize
		{
			get
			{
				Contract.Ensures(Contract.Result<float>().Equals(this._EmSize));
				Contract.Ensures(Check.IsPositive(this._EmSize));

				return this._EmSize;
			}
		}

		public bool Equals(Outline other)
		{
			return other._EmSize.Equals(this._EmSize) &&
			       other._NormalizedBaseline.Equals(this._NormalizedBaseline) &&
			       Equals(other._NormalizedOutline, this._NormalizedOutline);
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
				int result = this._EmSize.GetHashCode();
				result = (result * 397) ^ this._NormalizedBaseline.GetHashCode();
				result = (result * 397) ^
				         (this._NormalizedOutline != null
				          	? this._NormalizedOutline.GetHashCode()
				          	: 0);
				return result;
			}
		}

		public override string ToString()
		{
			return
				string.Format(
					"NormalizedOutline: {0}, EmSize: {1}, NormalizedBaseline: {2}",
					this._NormalizedOutline,
					this._EmSize,
					this._NormalizedBaseline);
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(this._NormalizedOutline != null);
			Contract.Invariant(Check.IsFinite(this._NormalizedBaseline));
			Contract.Invariant(Check.IsPositive(this._EmSize));
		}

		public static bool operator ==(Outline left, Outline right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Outline left, Outline right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Outline o1 = new Outline(Geometry.Square, 0.0f, 0.0f);
			Outline o2 = new Outline(Geometry.Circle, 0.0f, 0.0f);

			Assert.TestObject(o1, o2);
		}
#endif
	}
}