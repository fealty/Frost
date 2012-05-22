// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frost.Shaping
{
	public static class Helpers
	{
		public static bool IsBrokenAfter(
			this List<Shaper.Cluster> clusters,
			int index,
			ref bool isForced,
			bool isStandaloneCall = true)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(clusters != null);

			Shaper.Cluster cluster;

			if(clusters.TryCurrentOrDefault(index, out cluster))
			{
				switch(cluster.Breakpoint.BreakConditionAfter)
				{
					case LineBreakCondition.MustBreak:
						isForced = true;
						return true;
					case LineBreakCondition.CanBreak:
						// check the neighboring break condition
						return !isStandaloneCall ||
							clusters.IsBrokenBefore(index + 1, ref isForced, false);
					case LineBreakCondition.Neutral:
						// check the neighboring break condition
						return !isStandaloneCall ||
							clusters.IsBrokenBefore(index + 1, ref isForced, false);
				}
			}

			return false;
		}

		public static bool IsBrokenBefore(
			this List<Shaper.Cluster> clusters,
			int index,
			ref bool isForced,
			bool isStandaloneCall = true)
		{
			Contract.Requires(index >= 0);
			Contract.Requires(clusters != null);

			Shaper.Cluster cluster;

			if(clusters.TryCurrentOrDefault(index, out cluster))
			{
				switch(cluster.Breakpoint.BreakConditionBefore)
				{
					case LineBreakCondition.MustBreak:
						isForced = true;
						return true;
					case LineBreakCondition.CanBreak:
						// check the neighboring break condition
						return !isStandaloneCall ||
							clusters.IsBrokenAfter(index - 1, ref isForced, false);
					case LineBreakCondition.Neutral:
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