// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Construction;

using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;

namespace Frost.DirectX.Common
{
	public sealed class GeometryCache : IDisposable
	{
		public const int CacheLimit = 1000;

		private readonly GeometryBuilder _Builder;

		private readonly CacheDictionary<Figure, DxGeometry> _Cache;

		private readonly Factory _Factory2D;

		public GeometryCache(Factory factory2D)
		{
			Contract.Requires(factory2D != null);

			_Factory2D = factory2D;

			_Builder = new GeometryBuilder(_Factory2D);

			_Cache = new CacheDictionary<Figure, DxGeometry>(CacheLimit);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public DxGeometry ResolveGeometry(Figure figure)
		{
			Contract.Requires(figure != null);
			Contract.Ensures(Contract.Result<DxGeometry>() != null);

			DxGeometry newGeometry;

			if(_Cache.TryGetValue(figure, out newGeometry))
			{
				return newGeometry;
			}

			_Builder.Build(figure, out newGeometry);

			try
			{
				_Cache.Add(figure, newGeometry);
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