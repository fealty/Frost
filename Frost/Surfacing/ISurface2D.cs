// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.IO;
using System.Diagnostics.Contracts;

using Frost.Surfacing.Contracts;

namespace Frost.Surfacing
{
	namespace Contracts
	{
		[ContractClassFor(typeof(ISurface2D))]
		internal abstract class ISurface2DContract : ISurface2D
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
			public abstract Rectangle Region { get; }

			public void DumpToPNG(Stream stream)
			{
				Contract.Requires(stream != null);
			}

			public void CopyTo(
				Rectangle srcRegion, ISurface2D destination, Point dstLocation)
			{
				Contract.Requires(destination != null);
			}

			public abstract void AcquireLock();
			public abstract void ReleaseLock();

			public abstract Guid Id { get; }
		}
	}

	[ContractClass(typeof(ISurface2DContract))]
	public interface ISurface2D
	{
		Device2D Device2D { get; }

		SurfaceUsage Usage { get; }

		Rectangle Region { get; }
		Guid Id { get; }

		void DumpToPNG(Stream stream);

		void CopyTo(Rectangle srcRegion, ISurface2D destination, Point dstLocation);

		void AcquireLock();
		void ReleaseLock();
	}
}