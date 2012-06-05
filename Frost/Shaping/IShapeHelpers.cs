// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Shaping.Contracts;

namespace Frost.Shaping
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IShapeHelpers))]
		internal abstract class IShapeHelpersContract : IShapeHelpers
		{
			public Shape Combine(
				Shape sourcePath,
				Shape destinationPath,
				CombinationOperation operation,
				float tolerance)
			{
				Contract.Requires(sourcePath != null);
				Contract.Requires(destinationPath != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Shape>() != null);

				throw new NotSupportedException();
			}

			public bool Contains(Shape path, Point point, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}

			public Shape Simplify(Shape path, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Shape>() != null);

				throw new NotSupportedException();
			}

			public Shape Widen(Shape path, float width, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsFinite(width));
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Shape>() != null);

				throw new NotSupportedException();
			}

			public Rectangle MeasureRegion(Shape path)
			{
				Contract.Requires(path != null);

				throw new NotSupportedException();
			}

			public void Tessellate(
				Shape path, ITessellationSink sink, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Requires(sink != null);
			}

			public float MeasureArea(Shape path, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				throw new NotSupportedException();
			}

			public float MeasureLength(Shape path, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				throw new NotSupportedException();
			}

			public Point DeterminePoint(Shape path, float length, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(length));
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}

			public Point DeterminePoint(
				Shape path, float length, out Point tangentVector, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(length));
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}

			public Canvas CreateDistanceField(
				Shape path, Size resolution, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(resolution.Width));
				Contract.Requires(Check.IsPositive(resolution.Height));
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}
		}
	}

	[ContractClass(typeof(IShapeHelpersContract))]
	public interface IShapeHelpers
	{
		Shape Combine(
			Shape sourcePath,
			Shape destinationPath,
			CombinationOperation operation,
			float tolerance = Device2D.Flattening);

		bool Contains(
			Shape path, Point point, float tolerance = Device2D.Flattening);

		Shape Simplify(Shape path, float tolerance = Device2D.Flattening);

		Shape Widen(
			Shape path, float width, float tolerance = Device2D.Flattening);

		Rectangle MeasureRegion(Shape path);

		void Tessellate(
			Shape path,
			ITessellationSink sink,
			float tolerance = Device2D.Flattening);

		float MeasureArea(Shape path, float tolerance = Device2D.Flattening);

		float MeasureLength(Shape path, float tolerance = Device2D.Flattening);

		Point DeterminePoint(
			Shape path, float length, float tolerance = Device2D.Flattening);

		Point DeterminePoint(
			Shape path,
			float length,
			out Point tangentVector,
			float tolerance = Device2D.Flattening);

		Canvas CreateDistanceField(
			Shape path, Size resolution, float tolerance = Device2D.Flattening);
	}
}