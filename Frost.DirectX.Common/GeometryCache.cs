// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;
using Geometry = Frost.Shaping.Geometry;

namespace Frost.DirectX.Common
{
	public sealed class GeometryCache : IDisposable
	{
		public const int CacheLimit = 1000;

		private readonly GeometryBuilder _Builder;

		private readonly CacheDictionary<Geometry, DxGeometry> _Cache;

		private readonly Factory _Factory2D;

		public GeometryCache(Factory factory2D)
		{
			Contract.Requires(factory2D != null);

			_Factory2D = factory2D;

			_Builder = new GeometryBuilder(_Factory2D);

			_Cache = new CacheDictionary<Geometry, DxGeometry>(CacheLimit);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public DxGeometry ResolveGeometry(Geometry geometry)
		{
			Contract.Requires(geometry != null);
			Contract.Ensures(Contract.Result<DxGeometry>() != null);

			DxGeometry newGeometry;

			if(_Cache.TryGetValue(geometry, out newGeometry))
			{
				return newGeometry;
			}

			_Builder.Build(geometry, out newGeometry);

			try
			{
				_Cache.Add(geometry, newGeometry);
			}
			catch
			{
				newGeometry.Dispose();

				throw;
			}

			return newGeometry;
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_Cache.Dispose();
			}
		}
	}
}