// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.Formatting
{
	public static class Helpers
	{
		public static bool IsBrokenAfter(
			this List<ShapedCluster> clusters,
			int index,
			ref bool isForced,
			bool isStandaloneCall = true)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(clusters != null);

			ShapedCluster cluster;

			if(clusters.TryCurrentOrDefault(index, out cluster))
			{
				switch(cluster.Breakpoint.BreakConditionAfter)
				{
					case BreakCondition.MustBreak:
						isForced = true;
						return true;
					case BreakCondition.CanBreak:
						// check the neighboring break condition
						return !isStandaloneCall ||
							clusters.IsBrokenBefore(index + 1, ref isForced, false);
					case BreakCondition.Neutral:
						// check the neighboring break condition
						return !isStandaloneCall ||
							clusters.IsBrokenBefore(index + 1, ref isForced, false);
				}
			}

			return false;
		}

		public static bool IsBrokenBefore(
			this List<ShapedCluster> clusters,
			int index,
			ref bool isForced,
			bool isStandaloneCall = true)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(clusters != null);

			ShapedCluster cluster;

			if(clusters.TryCurrentOrDefault(index, out cluster))
			{
				switch(cluster.Breakpoint.BreakConditionBefore)
				{
					case BreakCondition.MustBreak:
						isForced = true;
						return true;
					case BreakCondition.CanBreak:
						// check the neighboring break condition
						return !isStandaloneCall ||
							clusters.IsBrokenAfter(index - 1, ref isForced, false);
					case BreakCondition.Neutral:
						// check the neighboring break condition
						return !isStandaloneCall ||
							clusters.IsBrokenAfter(index + 1, ref isForced, false);
				}
			}

			return false;
		}

		public static bool TryCurrentOrDefault<T>(
			this List<T> list, int currentIndex, out T item)
		{
			if(currentIndex >= 0 && currentIndex < list.Count)
			{
				item = list[currentIndex];

				return true;
			}

			item = default(T);

			return false;
		}
	}
}