// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

namespace Frost.DirectX.Formatting
{
	internal sealed class ShapingException : Exception
	{
		public ShapingException(Exception exception) : base("Text shaping failed!", exception)
		{
		}
	}
}