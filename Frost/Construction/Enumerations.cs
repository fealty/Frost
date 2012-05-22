// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

namespace Frost.Construction
{
	public enum CombinationOperation
	{
		Combine,
		Intersect,
		Xor,
		Exclude
	}

	internal enum GeometryCommand : byte
	{
		ArcTo,
		BezierCurveTo,
		Close,
		LineTo,
		MoveTo,
		QuadraticCurveTo
	}
}