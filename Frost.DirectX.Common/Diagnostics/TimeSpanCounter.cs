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
	public sealed class TimeSpanCounter : IDeviceCounter<TimeSpan>
	{
		private readonly string _Category;
		private readonly string _Name;

		private long _Average;
		private long _Maximum;
		private long _Minimum;
		private long _Value;

		public TimeSpanCounter(string category, string name)
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

		public TimeSpan Value
		{
			get { return TimeSpan.FromTicks(Interlocked.Read(ref _Value)); }
			set
			{
				long average = Interlocked.Read(ref _Average);
				long minimum = Interlocked.Read(ref _Minimum);
				long maximum = Interlocked.Read(ref _Maximum);

				average = (average + value.Ticks) / 2;

				minimum = Math.Min(minimum, value.Ticks);
				maximum = Math.Max(maximum, value.Ticks);

				Interlocked.Exchange(ref _Average, average);
				Interlocked.Exchange(ref _Minimum, minimum);
				Interlocked.Exchange(ref _Maximum, maximum);

				Interlocked.Exchange(ref _Value, value.Ticks);
			}
		}

		public TimeSpan Average
		{
			get { return TimeSpan.FromTicks(Interlocked.Read(ref _Average)); }
		}

		public TimeSpan Minimum
		{
			get { return TimeSpan.FromTicks(Interlocked.Read(ref _Minimum)); }
		}

		public TimeSpan Maximum
		{
			get { return TimeSpan.FromTicks(Interlocked.Read(ref _Maximum)); }
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

		object IDeviceCounter.Value
		{
			get { return Value; }
		}

		public void Reset()
		{
			Interlocked.Exchange(ref _Average, 0);

			Interlocked.Exchange(ref _Minimum, long.MaxValue);
			Interlocked.Exchange(ref _Maximum, long.MinValue);

			Interlocked.Exchange(ref _Value, 0);
		}
	}
}