// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

namespace Frost.Shaping
{
	public interface ITessellationSink
	{
		void Begin();
		void AddTriangle(Point p1, Point p2, Point p3);
		void End();
	}
}