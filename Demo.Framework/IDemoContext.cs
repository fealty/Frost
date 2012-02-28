using System;

using Frost;

namespace Demo.Framework
{
	public interface IDemoContext : IDisposable
	{
		string Name { get; }

		void Reset(Canvas target, Device2D device2D);
	}
}