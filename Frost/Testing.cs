// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Reflection;

#if(UNIT_TESTING)

namespace Frost
{
	internal class FactAttribute : Xunit.FactAttribute
	{
	}

	internal class Assert : Xunit.Assert
	{
		public static void TestObject<T>(T item1, T item2)
		{
			Contract.Requires(!item1.Equals(item2));

			object object1 = item1;
			object object2 = item2;

			// test IEquatable<T> if any on the type
			Equal(item1, item1);
			NotEqual(item1, item2);
			NotEqual(item2, item1);
			Equal(item2, item2);

			// test null on IEquatable
			IEquatable<T> equate = item1 as IEquatable<T>;

			if(equate != null)
			{
				// ReSharper disable ReturnValueOfPureMethodIsNotUsed
				equate.Equals(default(T));
				// ReSharper restore ReturnValueOfPureMethodIsNotUsed
			}

			// test the object.Equals() on the type
			True(item1.Equals(object1));
			False(item1.Equals(null));
			False(item1.Equals(object2));
			False(item2.Equals(object1));
			True(item2.Equals(object2));
			False(item2.Equals(null));

			// test ToString() on the objects
			NotNull(object1.ToString());
			NotNull(object2.ToString());

			// test GetHashCode() on the objects
			Equal(object1.GetHashCode(), object1.GetHashCode());
			Equal(object2.GetHashCode(), object2.GetHashCode());

			// test the == operator if any is defined on the type
			MethodInfo opEquality1 = object1.GetType().GetMethod("op_Equality");
			MethodInfo opEquality2 = object2.GetType().GetMethod("op_Equality");

			if(opEquality1 != null && opEquality2 != null)
			{
				False((bool)opEquality1.Invoke(null, new[] {object1, object2}));
				False((bool)opEquality2.Invoke(null, new[] {object2, object1}));
				True((bool)opEquality1.Invoke(null, new[] {object1, object1}));
				True((bool)opEquality2.Invoke(null, new[] {object2, object2}));
			}

			// test the != operator if any is defined on the type
			MethodInfo opInequality1 = object1.GetType().GetMethod("op_Inequality");
			MethodInfo opInequality2 = object2.GetType().GetMethod("op_Inequality");

			if(opInequality1 != null && opInequality2 != null)
			{
				True((bool)opInequality1.Invoke(null, new[] {object1, object2}));
				True((bool)opInequality2.Invoke(null, new[] {object2, object1}));
				False((bool)opInequality1.Invoke(null, new[] {object1, object1}));
				False((bool)opInequality2.Invoke(null, new[] {object2, object2}));
			}
		}
	}
}

#endif