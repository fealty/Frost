// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;
using Geometry = Frost.Shaping.Geometry;

namespace Frost.DirectX
{
	internal sealed class SimplificationSink : GeometrySinkBase
	{
		public Geometry CreateSimplification(
			DxGeometry resolvedSource, float tolerance)
		{
			Contract.Requires(resolvedSource != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Geometry>() != null);

			_Builder = Geometry.Create();

			resolvedSource.Simplify(
				GeometrySimplificationOption.Lines, tolerance, this);

			Geometry result = _Builder.Build();

			_Builder = null;

			return result;
		}
	}
}