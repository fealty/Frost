// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;

namespace Frost.DirectX.Formatting
{
	internal sealed class AnalysisException : Exception
	{
		public AnalysisException(Exception exception) : base("Text analysis failed!", exception)
		{
		}
	}
}