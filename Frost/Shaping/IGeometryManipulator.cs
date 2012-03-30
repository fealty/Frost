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
		[ContractClassFor(typeof(IGeometryManipulator))] internal abstract class
			IGeometryManipulatorContract : IGeometryManipulator
		{
			public Geometry Combine(
				Geometry sourcePath,
				Geometry destinationPath,
				CombinationOperation operation,
				float tolerance = Device2D.Flattening)
			{
				Contract.Requires(sourcePath != null);
				Contract.Requires(destinationPath != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Geometry>() != null);

				throw new NotSupportedException();
			}

			public bool Contains(Geometry path, Point point, float tolerance = Device2D.Flattening)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}

			public Geometry Simplify(Geometry path, float tolerance = Device2D.Flattening)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Geometry>() != null);

				throw new NotSupportedException();
			}

			public Geometry Widen(Geometry path, float width, float tolerance = Device2D.Flattening)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsFinite(width));
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Geometry>() != null);

				throw new NotSupportedException();
			}

			public Rectangle MeasureRegion(Geometry path)
			{
				Contract.Requires(path != null);

				throw new NotSupportedException();
			}

			public void Tessellate(
				Geometry path, ITessellationSink sink, float tolerance = Device2D.Flattening)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Requires(sink != null);
			}

			public float MeasureArea(Geometry path, float tolerance = Device2D.Flattening)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				throw new NotSupportedException();
			}

			public float MeasureLength(Geometry path, float tolerance = Device2D.Flattening)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				throw new NotSupportedException();
			}

			public Point DeterminePoint(Geometry path, float length, float tolerance = Device2D.Flattening)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(length));
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}

			public Point DeterminePoint(
				Geometry path, float length, out Point tangentVector, float tolerance = Device2D.Flattening)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(length));
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}
		}
	}

	[ContractClass(typeof(IGeometryManipulatorContract))] public interface IGeometryManipulator
	{
		Geometry Combine(
			Geometry sourcePath,
			Geometry destinationPath,
			CombinationOperation operation,
			float tolerance = Device2D.Flattening);

		bool Contains(Geometry path, Point point, float tolerance = Device2D.Flattening);

		Geometry Simplify(Geometry path, float tolerance = Device2D.Flattening);

		Geometry Widen(Geometry path, float width, float tolerance = Device2D.Flattening);

		Rectangle MeasureRegion(Geometry path);

		void Tessellate(Geometry path, ITessellationSink sink, float tolerance = Device2D.Flattening);

		float MeasureArea(Geometry path, float tolerance = Device2D.Flattening);

		float MeasureLength(Geometry path, float tolerance = Device2D.Flattening);

		Point DeterminePoint(Geometry path, float length, float tolerance = Device2D.Flattening);

		Point DeterminePoint(
			Geometry path, float length, out Point tangentVector, float tolerance = Device2D.Flattening);
	}
}