// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;

using Frost;
using Frost.Shaping;

namespace Demo.SDF
{
	internal sealed class Sample
	{
		private readonly List<Sample> mChildren;
		private readonly Rectangle mRegion;

		public Location BottomLeft;
		public Location BottomRight;
		public Location TopLeft;
		public Location TopRight;

		public Sample(ref Rectangle region)
		{
			mRegion = region;

			mChildren = new List<Sample>();
		}

		public Rectangle Region
		{
			get { return mRegion; }
		}

		public List<Sample> Children
		{
			get { return mChildren; }
		}

		public float SampleSample(ref Point point)
		{
			foreach(Sample sample in mChildren)
			{
				if(sample.Region.Contains(point))
				{
					return sample.SampleSample(ref point);
				}
			}

			return Interpolate(ref point);
		}

		private float Interpolate(ref Point point)
		{
			float fr1 = ((mRegion.Right - point.X) / mRegion.Width) * BottomLeft.Distance +
			            ((point.X - mRegion.Left) / mRegion.Width) * BottomRight.Distance;

			float fr2 = ((mRegion.Right - point.X) / mRegion.Width) * TopLeft.Distance +
			            ((point.X - mRegion.Left) / mRegion.Width) * TopRight.Distance;

			return ((mRegion.Top - point.Y) / mRegion.Height) * fr1 +
			       ((point.Y - mRegion.Bottom) / mRegion.Height) * fr2;
		}

		public struct Location
		{
			public float Distance;
			public Point Intersection;
		}
	}

	internal sealed class DistanceField
	{
		public const int EmLength = 2048;
		public const int ResolvedLength = 128;

		public static readonly float MinimumError;
		public static readonly float MaximumError;

		public static readonly float MaxLevel;

		private readonly GeometryDecomposer mDecomposer;

		private readonly float[,] mField;
		private readonly byte[] mRgbaData;
		private Sample mSample;

		static DistanceField()
		{
			MaxLevel = 0;

			MinimumError = (EmLength / ResolvedLength) / 8.0f;
			MaximumError = (EmLength / ResolvedLength) / 1.0f;

			for(double i = ResolvedLength; i > 1.0; i = i / 2.0)
			{
				MaxLevel = MaxLevel + 1;
			}
		}

		public DistanceField()
		{
			mField = new float[ResolvedLength,ResolvedLength];

			mRgbaData = new byte[(ResolvedLength * ResolvedLength) * 4];

			mDecomposer = new GeometryDecomposer();
		}

		public GeometryDecomposer Decomposer
		{
			get { return mDecomposer; }
		}

		public Sample Sample
		{
			get { return mSample; }
		}

		public Canvas CreateField(Shape geometry, double normalizedBaseline, Device2D device2D)
		{
			Rectangle normalizedRegion = device2D.Geometry.MeasureRegion(geometry);

			Matrix3X2 transform = Matrix3X2.Identity;

			transform.Translate(
				(0.5f - (normalizedRegion.Width / 2.0f)) * EmLength,
				(0.5f - (normalizedRegion.Height / 2.0f)) * EmLength,
				out transform);

			transform.Translate(
				-normalizedRegion.X * EmLength, -normalizedRegion.Y * EmLength, out transform);

			transform.Scale(EmLength, EmLength, out transform);

			mDecomposer.Decompose(geometry, ref transform, device2D);

			Rectangle newReg = new Rectangle(0, 0, EmLength, EmLength);

			mSample = ComputeTree(ref newReg, device2D);

			float maxNegative = float.MinValue;

			float pixel = EmLength / ResolvedLength;

			for(int y = 0; y < ResolvedLength; ++y)
			{
				for(int x = 0; x < ResolvedLength; ++x)
				{
					float distance = 0.0f;

					Point point = new Point((pixel * x), (pixel * y));

					distance += mSample.SampleSample(ref point);

					maxNegative = Math.Max(maxNegative, distance);

					mField[y, x] = distance;
				}
			}

			int rgbaIndex = 0;

			for(int y = 0; y < ResolvedLength; ++y)
			{
				for(int x = 0; x < ResolvedLength; ++x)
				{
					double distance = mField[y, x];

					const double test = 0.5;

					if(distance < 0.0)
					{
						distance = Math.Abs(distance) / Math.Abs(maxNegative);

						distance = test + ((1.0 - test) * distance);
					}
					else
					{
						distance = Math.Abs(distance) / Math.Abs(maxNegative);

						distance = test - (test * distance);
					}

					distance = Math.Min(1.0, distance);
					distance = Math.Max(0.0, distance);

					byte value = Convert.ToByte(255 - (distance * 255));

					//byte[] bytes = BitConverter.GetBytes(value);

					mRgbaData[rgbaIndex + 0] = 0;
					mRgbaData[rgbaIndex + 1] = 0;
					mRgbaData[rgbaIndex + 2] = 0;
					mRgbaData[rgbaIndex + 3] = value;

					rgbaIndex += 4;
				}
			}

			Canvas newCanvas = new Canvas(new Size(ResolvedLength, ResolvedLength), Frost.Surfacing.SurfaceUsage.Normal);

			device2D.Resources.Copy(mRgbaData, newCanvas);

			return newCanvas;
		}

		private Sample ComputeTree(ref Rectangle region, Device2D device2D)
		{
			Sample.Location empty = new Sample.Location();

			return ComputeTree(ref region, device2D, ref empty, ref empty, ref empty, ref empty);
		}

		private Sample ComputeTree(
			ref Rectangle region,
			Device2D device2D,
			ref Sample.Location parentTopLeft,
			ref Sample.Location parentTopRight,
			ref Sample.Location parentBottomLeft,
			ref Sample.Location parentBottomRight,
			double level = 1.0)
		{
			Sample sample = new Sample(ref region);

			// define the points on the rectangle to test
			Point topLeft = new Point(region.Left, region.Top);
			Point topRight = new Point(region.Right, region.Top);
			Point bottomLeft = new Point(region.Left, region.Bottom);
			Point bottomRight = new Point(region.Right, region.Bottom);

			// the method was not called recursively, so compute the distances to each point
			if(level <= 1.0)
			{
				mDecomposer.Query(
					ref topLeft, device2D, out sample.TopLeft.Distance, out sample.TopLeft.Intersection);
				mDecomposer.Query(
					ref topRight, device2D, out sample.TopRight.Distance, out sample.TopRight.Intersection);
				mDecomposer.Query(
					ref bottomLeft, device2D, out sample.BottomLeft.Distance, out sample.BottomLeft.Intersection);
				mDecomposer.Query(
					ref bottomRight, device2D, out sample.BottomRight.Distance, out sample.BottomRight.Intersection);
			}
			else
			{
				// use distances already computed by the caller
				sample.TopLeft = parentTopLeft;
				sample.TopRight = parentTopRight;
				sample.BottomLeft = parentBottomLeft;
				sample.BottomRight = parentBottomRight;
			}

			// do not subdivide lower than a resolved resolution unit
			if(level > MaxLevel)
			{
				return sample;
			}

			int boundaryContains = 0;

			Thickness thick = new Thickness(MaximumError);

			Rectangle boundary = region.Expand(thick);

			boundaryContains += boundary.Contains(sample.TopLeft.Intersection) ? 1 : 0;
			boundaryContains += boundary.Contains(sample.TopRight.Intersection) ? 1 : 0;
			boundaryContains += boundary.Contains(sample.BottomLeft.Intersection) ? 1 : 0;
			boundaryContains += boundary.Contains(sample.BottomRight.Intersection) ? 1 : 0;

			// compute the intersection of the region with the shape
			int boundaryIntersection = 0;

			boundaryIntersection += sample.TopLeft.Distance >= 0.0 ? 1 : -1;
			boundaryIntersection += sample.TopRight.Distance >= 0.0 ? 1 : -1;
			boundaryIntersection += sample.BottomLeft.Distance >= 0.0 ? 1 : -1;
			boundaryIntersection += sample.BottomRight.Distance >= 0.0 ? 1 : -1;

			double s = Math.Abs(boundaryIntersection) != 4 || boundaryContains > 0 ? 0.0 : 1.0;

			double error = MinimumError + ((MaximumError - MinimumError) * s);

			//error = MaximumError;

			float halfWidth = region.Width / 2.0f;
			float halfHeight = region.Height / 2.0f;

			Point leftCenter = new Point(region.Left, region.Top + halfHeight);
			Point topCenter = new Point(region.Left + halfWidth, region.Top);
			Point rightCenter = new Point(region.Right, region.Top + halfHeight);
			Point bottomCenter = new Point(region.Left + halfWidth, region.Bottom);
			Point center = new Point(region.Left + halfWidth, region.Top + halfHeight);

			Sample.Location centerLocation = new Sample.Location();

			mDecomposer.Query(
				ref center, device2D, out centerLocation.Distance, out centerLocation.Intersection);

			boundaryContains += boundary.Contains(centerLocation.Intersection) ? 1 : 0;

			if(boundaryContains == 0)
			{
				return sample;
			}

			Sample.Location leftCenterLocation = new Sample.Location();
			Sample.Location topCenterLocation = new Sample.Location();
			Sample.Location rightCenterLocation = new Sample.Location();
			Sample.Location bottomCenterLocation = new Sample.Location();

			mDecomposer.Query(
				ref leftCenter, device2D, out leftCenterLocation.Distance, out leftCenterLocation.Intersection);
			mDecomposer.Query(
				ref topCenter, device2D, out topCenterLocation.Distance, out topCenterLocation.Intersection);
			mDecomposer.Query(
				ref rightCenter,
				device2D,
				out rightCenterLocation.Distance,
				out rightCenterLocation.Intersection);
			mDecomposer.Query(
				ref bottomCenter,
				device2D,
				out bottomCenterLocation.Distance,
				out bottomCenterLocation.Intersection);

			double reconstructedLeftCenter = (sample.TopLeft.Distance + sample.BottomLeft.Distance) / 2.0;
			double reconstructedTopCenter = (sample.TopLeft.Distance + sample.TopRight.Distance) / 2.0;
			double reconstructedRightCenter = (sample.TopRight.Distance + sample.BottomRight.Distance) / 2.0;
			double reconstructedBottomCenter = (sample.BottomLeft.Distance + sample.BottomRight.Distance) /
			                                   2.0;
			double reconstructedCenter = (reconstructedTopCenter + reconstructedBottomCenter) / 2.0;

			bool isLeftCenterInvalid = Math.Abs(reconstructedLeftCenter - leftCenterLocation.Distance) >
			                           error;
			bool isTopCenterInvalid = Math.Abs(reconstructedTopCenter - topCenterLocation.Distance) > error;
			bool isRightCenterInvalid = Math.Abs(reconstructedRightCenter - rightCenterLocation.Distance) >
			                            error;
			bool isBottomCenterInvalid =
				Math.Abs(reconstructedBottomCenter - bottomCenterLocation.Distance) > error;
			bool isCenterInvalid = Math.Abs(reconstructedCenter - centerLocation.Distance) > error;

			if(isLeftCenterInvalid || isTopCenterInvalid || isRightCenterInvalid || isBottomCenterInvalid ||
			   isCenterInvalid)
			{
				Rectangle topLeftRegion = new Rectangle(
					topLeft.X, topLeft.Y, center.X - topLeft.X, center.Y - topLeft.Y);

				sample.Children.Add(
					ComputeTree(
						ref topLeftRegion,
						device2D,
						ref sample.TopLeft,
						ref topCenterLocation,
						ref leftCenterLocation,
						ref centerLocation,
						level + 1.0));

				Rectangle topRightRegion = new Rectangle(
					center.X, topRight.Y, topRight.X - center.X, center.Y - topRight.Y);

				sample.Children.Add(
					ComputeTree(
						ref topRightRegion,
						device2D,
						ref topCenterLocation,
						ref sample.TopRight,
						ref centerLocation,
						ref rightCenterLocation,
						level + 1.0));

				Rectangle bottomRightRegion = new Rectangle(
					center.X, center.Y, bottomRight.X - center.X, bottomRight.Y - center.Y);

				sample.Children.Add(
					ComputeTree(
						ref bottomRightRegion,
						device2D,
						ref centerLocation,
						ref rightCenterLocation,
						ref bottomCenterLocation,
						ref sample.BottomRight,
						level + 1.0));

				Rectangle bottomLeftRegion = new Rectangle(
					bottomLeft.X, center.Y, center.X - bottomLeft.X, bottomLeft.Y - center.Y);

				sample.Children.Add(
					ComputeTree(
						ref bottomLeftRegion,
						device2D,
						ref leftCenterLocation,
						ref centerLocation,
						ref sample.BottomLeft,
						ref bottomCenterLocation,
						level + 1.0));
			}

			return sample;
		}
	}
}