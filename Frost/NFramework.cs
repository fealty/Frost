// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

#if UNIT_TESTING

using System;

using NU = NUnit.Framework;

public sealed class TestAttribute : NU.TestAttribute
{
}

[AttributeUsage(AttributeTargets.Class)] public sealed class
	TestFixtureAttribute : NU.TestAttribute
{
}

public sealed class Assert : NU.Assert
{
}

#endif