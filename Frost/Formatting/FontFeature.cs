// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	public struct FontFeature : IEquatable<FontFeature>
	{
		private readonly string _Tag;
		private readonly int _Parameter;

		public FontFeature(string tag, int parameter = 1)
		{
			Trace.Assert(tag != null);

			this._Tag = tag;
			this._Parameter = parameter;

			Contract.Assert(Tag.Equals(tag));
			Contract.Assert(Parameter.Equals(parameter));
		}

		public string Tag
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);

				return this._Tag;
			}
		}

		public int Parameter
		{
			get { return this._Parameter; }
		}

		public bool Equals(FontFeature other)
		{
			return Equals(other._Tag, this._Tag) && other._Parameter == this._Parameter;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is FontFeature && Equals((FontFeature)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((this._Tag != null ? this._Tag.GetHashCode() : 0) * 397) ^
				       this._Parameter;
			}
		}

		public static bool operator ==(FontFeature left, FontFeature right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FontFeature left, FontFeature right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("Tag: {0}, Parameter: {1}", this._Tag, this._Parameter);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.Equal(string.Empty, new FontFeature(string.Empty).Tag);
			Assert.Equal(2, new FontFeature(string.Empty, 2).Parameter);

			Assert.TestObject(new FontFeature("kern"), new FontFeature("sset", 2));
		}
#endif
	}
}