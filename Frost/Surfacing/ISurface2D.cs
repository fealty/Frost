// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Surfacing
{
	namespace Contracts
	{
		[ContractClassFor(typeof(ISurface2D))] internal abstract class
			ISurface2DContract : ISurface2D
		{
			public Device2D Device2D
			{
				get
				{
					Contract.Ensures(Contract.Result<Device2D>() != null);

					throw new NotSupportedException();
				}
			}

			public abstract SurfaceUsage Usage { get; }
			public abstract SurfaceFormat Format { get; }
			public abstract Rectangle Region { get; }

			public abstract void DumpToFile(string file);

			public void CopyTo(
				Rectangle srcRegion, ISurface2D destination, Point dstLocation)
			{
				Contract.Requires(destination != null);
			}

			public abstract void AcquireLock();
			public abstract void ReleaseLock();
		}
	}

	[ContractClass(typeof(Contracts.ISurface2DContract))] public interface
		ISurface2D
	{
		Device2D Device2D { get; }

		SurfaceUsage Usage { get; }
		SurfaceFormat Format { get; }

		Rectangle Region { get; }

		void DumpToFile(string file);

		void CopyTo(Rectangle srcRegion, ISurface2D destination, Point dstLocation);

		void AcquireLock();
		void ReleaseLock();
	}
}