// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

#if TEST

using System;

using NUnit.Framework;

namespace Frost.Tests
{
	[TestFixture]
	internal static class PointTests
	{
		[Test]
		public static void Test0()
		{
			Matrix3x2 value = Matrix3x2.Identity;

			Assert.IsTrue(Point.Empty.Transform(ref value) == Point.Empty);
		}

		[Test]
		public static void Test1()
		{
			Point point = new Point(0);

			Assert.IsTrue(
				point.FindIntersectionWith(
					new Point(
						-5,
						5),
					new Point(
						0,
						+0)) == new Point(
						        	0,
						        	0));
			Assert.IsTrue(
				point.FindIntersectionWith(
					new Point(
						+0,
						0),
					new Point(
						5,
						-5)) == new Point(
						        	0,
						        	0));
			Assert.IsTrue(
				point.FindIntersectionWith(
					new Point(
						+0,
						0),
					new Point(
						0,
						+0)) == new Point(
						        	0,
						        	0));
			Assert.IsTrue(
				point.FindIntersectionWith(
					new Point(
						+1,
						5),
					new Point(
						1,
						-5)) == new Point(
						        	1,
						        	0));
			Assert.IsTrue(
				point.FindIntersectionWith(
					new Point(
						+5,
						5),
					new Point(
						5,
						-5)) == new Point(
						        	5,
						        	0));
		}

		[Test]
		public static void Test2()
		{
			Point point = new Point(0);

			Assert.AreEqual(
				point.SquaredDistanceTo(
					new Point(
						1,
						1)),
				2f);
			Assert.AreEqual(
				point.SquaredDistanceTo(
					new Point(
						0,
						0)),
				0f);
		}

		[Test]
		public static void Test3()
		{
			Assert.AreEqual(
				Math.Round(
					Point.Empty.DistanceTo(
						new Point(
							1,
							1)),
					4),
				Math.Round(
					1.41422f,
					4));
			Assert.AreEqual(
				Math.Round(
					Point.Empty.DistanceTo(
						new Point(
							0,
							0)),
					4),
				Math.Round(
					0.00000f,
					4));
		}
	}
}

#endif