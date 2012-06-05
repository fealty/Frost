// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

using Frost.Shaping;

using DxGeometry = SharpDX.Direct2D1.Geometry;

namespace Frost.DirectX
{
	internal sealed class WideningSink : GeometrySinkBase
	{
		public Shape CreateWidened(DxGeometry resolvedSource, float width, float tolerance)
		{
			Contract.Requires(resolvedSource != null);
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Shape>() != null);

			_Builder = Shape.Create();

			resolvedSource.Widen(width, null, null, tolerance, this);

			Shape result = _Builder.Build();

			_Builder = null;

			return result;
		}
	}
}