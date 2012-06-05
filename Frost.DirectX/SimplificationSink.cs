// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

using Frost.Shaping;

using SharpDX.Direct2D1;

using DxGeometry = SharpDX.Direct2D1.Geometry;

namespace Frost.DirectX
{
	internal sealed class SimplificationSink : GeometrySinkBase
	{
		public Shape CreateSimplification(DxGeometry resolvedSource, float tolerance)
		{
			Contract.Requires(resolvedSource != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Shape>() != null);

			_Builder = Shape.Create();

			resolvedSource.Simplify(GeometrySimplificationOption.Lines, tolerance, this);

			Shape result = _Builder.Build();

			_Builder = null;

			return result;
		}
	}
}