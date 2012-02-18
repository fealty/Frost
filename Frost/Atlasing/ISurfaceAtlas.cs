// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Surfacing;

namespace Frost.Atlasing
{
	namespace Contracts
	{
		[ContractClassFor(typeof(ISurfaceAtlas))] internal abstract class
			ISurfaceAtlasContract : ISurfaceAtlas
		{
			public abstract bool InUse { get; }

			public ISurface2D Surface2D
			{
				get
				{
					Contract.Ensures(Contract.Result<ISurface2D>() != null);

					throw new NotSupportedException();
				}
			}

			public Canvas Canvas
			{
				get
				{
					Contract.Ensures(Contract.Result<Canvas>() != null);

					throw new NotSupportedException();
				}
			}

			public Canvas AcquireRegion(Size size, object owner = null)
			{
				Contract.Requires(Check.IsPositive(size.Width));
				Contract.Requires(Check.IsPositive(size.Height));

				throw new NotSupportedException();
			}

			public abstract void Invalidate();
		}
	}

	[ContractClass(typeof(Contracts.ISurfaceAtlasContract))] public
		interface ISurfaceAtlas
	{
		bool InUse { get; }

		ISurface2D Surface2D { get; }

		Canvas Canvas { get; }

		Canvas AcquireRegion(Size size, object owner = null);

		void Invalidate();
	}
}