// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Construction.Contracts;

namespace Frost.Construction
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IFigureHelpers))]
		internal abstract class IFigureHelpersContract : IFigureHelpers
		{
			public Figure Combine(
				Figure sourcePath,
				Figure destinationPath,
				CombinationOperation operation,
				float tolerance)
			{
				Contract.Requires(sourcePath != null);
				Contract.Requires(destinationPath != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Figure>() != null);

				throw new NotSupportedException();
			}

			public bool Contains(Figure path, Point point, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}

			public Figure Simplify(Figure path, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Figure>() != null);

				throw new NotSupportedException();
			}

			public Figure Widen(Figure path, float width, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsFinite(width));
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Contract.Result<Figure>() != null);

				throw new NotSupportedException();
			}

			public Rectangle MeasureRegion(Figure path)
			{
				Contract.Requires(path != null);

				throw new NotSupportedException();
			}

			public void Tessellate(
				Figure path, ITessellationSink sink, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Requires(sink != null);
			}

			public float MeasureArea(Figure path, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				throw new NotSupportedException();
			}

			public float MeasureLength(Figure path, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(tolerance));
				Contract.Ensures(Check.IsPositive(Contract.Result<float>()));

				throw new NotSupportedException();
			}

			public Point DeterminePoint(Figure path, float length, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(length));
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}

			public Point DeterminePoint(
				Figure path, float length, out Point tangentVector, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(length));
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}

			public Canvas CreateDistanceField(
				Figure path, Size resolution, float tolerance)
			{
				Contract.Requires(path != null);
				Contract.Requires(Check.IsPositive(resolution.Width));
				Contract.Requires(Check.IsPositive(resolution.Height));
				Contract.Requires(Check.IsPositive(tolerance));

				throw new NotSupportedException();
			}
		}
	}

	[ContractClass(typeof(IFigureHelpersContract))]
	public interface IFigureHelpers
	{
		Figure Combine(
			Figure sourcePath,
			Figure destinationPath,
			CombinationOperation operation,
			float tolerance = Device2D.Flattening);

		bool Contains(
			Figure path, Point point, float tolerance = Device2D.Flattening);

		Figure Simplify(Figure path, float tolerance = Device2D.Flattening);

		Figure Widen(
			Figure path, float width, float tolerance = Device2D.Flattening);

		Rectangle MeasureRegion(Figure path);

		void Tessellate(
			Figure path,
			ITessellationSink sink,
			float tolerance = Device2D.Flattening);

		float MeasureArea(Figure path, float tolerance = Device2D.Flattening);

		float MeasureLength(Figure path, float tolerance = Device2D.Flattening);

		Point DeterminePoint(
			Figure path, float length, float tolerance = Device2D.Flattening);

		Point DeterminePoint(
			Figure path,
			float length,
			out Point tangentVector,
			float tolerance = Device2D.Flattening);

		Canvas CreateDistanceField(
			Figure path, Size resolution, float tolerance = Device2D.Flattening);
	}
}