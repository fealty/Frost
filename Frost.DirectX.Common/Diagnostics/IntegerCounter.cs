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

			_Category = category;
			_Name = name;

			Reset();
		}

		public string Name
		{
			get { return _Name; }
		}

		public string Category
		{
			get { return _Category; }
		}

		public int Value
		{
			get { return Interlocked.CompareExchange(ref _Value, 0, 0); }
			set
			{
				int average = Interlocked.CompareExchange(ref _Average, 0, 0);
				int minimum = Interlocked.CompareExchange(ref _Minimum, 0, 0);
				int maximum = Interlocked.CompareExchange(ref _Maximum, 0, 0);

				average = (average + value) / 2;

				minimum = Math.Min(minimum, value);
				maximum = Math.Max(maximum, value);

				Interlocked.Exchange(ref _Average, average);
				Interlocked.Exchange(ref _Minimum, minimum);
				Interlocked.Exchange(ref _Maximum, maximum);

				Interlocked.Exchange(ref _Value, value);
			}
		}

		public int Average
		{
			get { return Interlocked.CompareExchange(ref _Average, 0, 0); }
		}

		public int Minimum
		{
			get { return Interlocked.CompareExchange(ref _Minimum, 0, 0); }
		}

		public int Maximum
		{
			get { return Interlocked.CompareExchange(ref _Maximum, 0, 0); }
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
			Interlocked.Exchange(ref _Average, 0);

			Interlocked.Exchange(ref _Minimum, int.MaxValue);
			Interlocked.Exchange(ref _Maximum, int.MinValue);

			Interlocked.Exchange(ref _Value, 0);
		}
	}
}