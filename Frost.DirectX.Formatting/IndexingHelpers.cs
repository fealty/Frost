using System.Collections.Generic;

namespace Frost.DirectX.Formatting
{
	internal static class IndexingHelpers
	{
		public static bool TryGetNextItem<T>(
			this List<T> @this, int currentIndex, out T item)
		{
			if(currentIndex >= -1 && currentIndex + 1 < @this.Count)
			{
				item = @this[currentIndex + 1];

				return true;
			}

			item = default(T);

			return false;
		}

		public static bool TryGetPreviousItem<T>(
			this List<T> @this, int currentIndex, out T item)
		{
			if(currentIndex >= 1 && currentIndex - 1 < @this.Count)
			{
				item = @this[currentIndex - 1];

				return true;
			}

			item = default(T);

			return false;
		}

		public static bool TryCurrentOrDefault<T>(
			this List<T> @this, int currentIndex, out T item)
		{
			if(currentIndex >= 0 && currentIndex < @this.Count)
			{
				item = @this[currentIndex];

				return true;
			}

			item = default(T);

			return false;
		}
	}
}