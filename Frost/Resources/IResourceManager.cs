// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Frost.Resources.Contracts;
using Frost.Surfacing;

namespace Frost.Resources
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IResourceManager))] internal abstract class IResourceManagerContract
			: IResourceManager
		{
			public void Copy(Rectangle fromRegion, Canvas fromTarget, Canvas toTarget)
			{
				Contract.Requires(fromTarget != null);
				Contract.Requires(toTarget != null);
			}

			public void Copy(byte[] fromRgbaData, Canvas toTarget)
			{
				Contract.Requires(fromRgbaData != null);
				Contract.Requires(toTarget != null);
			}

			public void Copy(Canvas fromTarget, Canvas toTarget)
			{
				Contract.Requires(fromTarget != null);
				Contract.Requires(toTarget != null);
			}

			public abstract event Action<IEnumerable<Canvas>> Invalidated;

			public Size PageSize
			{
				set
				{
					Contract.Requires(Check.IsPositive(value.Width));
					Contract.Requires(Check.IsPositive(value.Height));
				}
			}

			public abstract void DumpToFiles(string path, SurfaceUsage usage);

			public Canvas.ResolvedContext Resolve(Canvas target)
			{
				Contract.Requires(target != null);

				throw new NotSupportedException();
			}

			public void Forget(Canvas target)
			{
				Contract.Requires(target != null);
			}
		}
	}

	[ContractClass(typeof(IResourceManagerContract))] public interface IResourceManager
	{
		Size PageSize { set; }

		void Copy(Rectangle fromRegion, Canvas fromTarget, Canvas toTarget);

		void Copy(byte[] fromRgbaData, Canvas toTarget);

		void Copy(Canvas fromTarget, Canvas toTarget);

		//TODO: should this work on streams instead of files?
		void DumpToFiles(string path, SurfaceUsage usage);

		Canvas.ResolvedContext Resolve(Canvas target);

		void Forget(Canvas target);

		event Action<IEnumerable<Canvas>> Invalidated;
	}
}