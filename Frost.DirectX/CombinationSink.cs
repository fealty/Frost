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
	internal sealed class CombinationSink : GeometrySinkBase
	{
		public Shape CreateCombination(
			DxGeometry resolvedDestination,
			DxGeometry resolvedSource,
			CombinationOperation combination,
			float tolerance)
		{
			Contract.Requires(resolvedDestination != null);
			Contract.Requires(resolvedSource != null);
			Contract.Requires(Check.IsPositive(tolerance));
			Contract.Ensures(Contract.Result<Shape>() != null);

			_Builder = Shape.Create();

			CombineMode mode = CombineMode.Union;

			switch(combination)
			{
				case CombinationOperation.Intersect:
					mode = CombineMode.Intersect;
					break;
				case CombinationOperation.Combine:
					mode = CombineMode.Union;
					break;
				case CombinationOperation.Exclude:
					mode = CombineMode.Exclude;
					break;
				case CombinationOperation.Xor:
					mode = CombineMode.Xor;
					break;
			}

			resolvedDestination.Combine(resolvedSource, mode, tolerance, this);

			Shape result = _Builder.Build();

			_Builder = null;

			return result;
		}
	}
}