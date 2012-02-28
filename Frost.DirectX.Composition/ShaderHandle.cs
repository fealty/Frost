// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Composition
{
	public struct ShaderHandle : IEquatable<ShaderHandle>
	{
		private readonly int _Index;
		private readonly object _Reference;

		internal ShaderHandle(int index, object reference)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(reference != null);

			_Index = index + 1;
			_Reference = reference;
		}

		public bool IsValid
		{
			get { return _Index > 0; }
		}

		public int Index
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return _Index - 1;
			}
		}

		//TODO: can this be made internal?
		public object Reference
		{
			get { return _Reference; }
		}

		public bool Equals(ShaderHandle other)
		{
			return other._Index == _Index && Equals(other._Reference, _Reference);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is ShaderHandle && Equals((ShaderHandle)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_Index * 397) ^ (_Reference != null ? _Reference.GetHashCode() : 0);
			}
		}

		public static bool operator ==(ShaderHandle left, ShaderHandle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ShaderHandle left, ShaderHandle right)
		{
			return !left.Equals(right);
		}
	}
}