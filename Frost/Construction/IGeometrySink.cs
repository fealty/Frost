// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

using Frost.Construction.Contracts;

namespace Frost.Construction
{
	namespace Contracts
	{
		[ContractClassFor(typeof(IGeometrySink))]
		internal abstract class IGeometrySinkContract
			: IGeometrySink
		{
			public abstract void Begin();
			public abstract void End();
			public abstract void Close();
			public abstract void MoveTo(Point point);
			public abstract void LineTo(Point point);

			public abstract void QuadraticCurveTo(Point controlPoint, Point endPoint);

			public abstract void BezierCurveTo(
				Point controlPoint1, Point controlPoint2, Point controlPoint3);

			public void ArcTo(Point tangentStart, Point tangentEnd, Size radius)
			{
				Contract.Requires(Check.IsPositive(radius.Width));
				Contract.Requires(Check.IsPositive(radius.Height));
			}
		}
	}

	[ContractClass(typeof(IGeometrySinkContract))]
	public interface IGeometrySink
	{
		void Begin();
		void End();
		void Close();

		void MoveTo(Point point);
		void LineTo(Point point);

		void QuadraticCurveTo(Point controlPoint, Point endPoint);

		void BezierCurveTo(
			Point controlPoint1, Point controlPoint2, Point controlPoint3);

		void ArcTo(Point tangentStart, Point tangentEnd, Size radius);
	}
}