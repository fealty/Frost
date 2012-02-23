// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Threading;

using Frost.Diagnostics;

namespace Frost.DirectX.Common.Diagnostics
{
	public sealed class IntegerCounter : IDeviceCounter<int>
	{
		private readonly string _Category;
		private readonly string _Name;

		private int _Average;
		private int _Maximum;
		private int _Minimum;
		private int _Value;

		public IntegerCounter(string category, string name)
		{
			Contract.Requires(!String.IsNullOrEmpty(category));
			Contract.Requires(!String.IsNullOrEmpty(name));

			this._Category = category;
			this._Name = name;

			Reset();
		}

		public string Name
		{
			get { return this._Name; }
		}

		public string Category
		{
			get { return this._Category; }
		}

		public int Value
		{
			get { return Interlocked.CompareExchange(ref this._Value, 0, 0); }
			set
			{
				int average = Interlocked.CompareExchange(ref this._Average, 0, 0);
				int minimum = Interlocked.CompareExchange(ref this._Minimum, 0, 0);
				int maximum = Interlocked.CompareExchange(ref this._Maximum, 0, 0);

				average = (average + value) / 2;

				minimum = Math.Min(minimum, value);
				maximum = Math.Max(maximum, value);

				Interlocked.Exchange(ref this._Average, average);
				Interlocked.Exchange(ref this._Minimum, minimum);
				Interlocked.Exchange(ref this._Maximum, maximum);

				Interlocked.Exchange(ref this._Value, value);
			}
		}

		public int Average
		{
			get { return Interlocked.CompareExchange(ref this._Average, 0, 0); }
		}

		public int Minimum
		{
			get { return Interlocked.CompareExchange(ref this._Minimum, 0, 0); }
		}

		public int Maximum
		{
			get { return Interlocked.CompareExchange(ref this._Maximum, 0, 0); }
		}

		object IDeviceCounter.Value
		{
			get { return Value; }
		}

		object IDeviceCounter.Average
		{
			get { return Average; }
		}

		object IDeviceCounter.Minimum
		{
			get { return Minimum; }
		}

		object IDeviceCounter.Maximum
		{
			get { return Maximum; }
		}

		public void Reset()
		{
			Interlocked.Exchange(ref this._Average, 0);

			Interlocked.Exchange(ref this._Minimum, int.MaxValue);
			Interlocked.Exchange(ref this._Maximum, int.MinValue);

			Interlocked.Exchange(ref this._Value, 0);
		}
	}
}