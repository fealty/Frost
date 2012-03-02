// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Collections.Generic;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides convenient extension methods.
	/// </summary>
	internal static class IndexingHelpers
	{
		/// <summary>
		///   This method tries to get the next item in a list.
		/// </summary>
		/// <typeparam name="T"> This type parameter indicates the type of item. </typeparam>
		/// <param name="this"> This parameter references a <see cref="List{T}" /> instance. </param>
		/// <param name="currentIndex"> This parameter indicates the current index. </param>
		/// <param name="item"> This output parameter contains the next item. </param>
		/// <returns> This method returns <c>true</c> if a next item exists; otherwise, this method returns <c>false</c> . </returns>
		public static bool TryGetNextItem<T>(this List<T> @this, int currentIndex, out T item)
		{
			if(currentIndex >= -1 && currentIndex + 1 < @this.Count)
			{
				item = @this[currentIndex + 1];

				return true;
			}

			item = default(T);

			return false;
		}

		/// <summary>
		///   This method tries to get the previous item in a list.
		/// </summary>
		/// <typeparam name="T"> This type parameter indicates the type of item. </typeparam>
		/// <param name="this"> This parameter references a <see cref="List{T}" /> instance. </param>
		/// <param name="currentIndex"> This parameter indicates the current index. </param>
		/// <param name="item"> This output parameter contains the previous item. </param>
		/// <returns> This method returns <c>true</c> if a previous item exists; otherwise, this method returns <c>false</c> . </returns>
		public static bool TryGetPreviousItem<T>(this List<T> @this, int currentIndex, out T item)
		{
			if(currentIndex >= 1 && currentIndex - 1 < @this.Count)
			{
				item = @this[currentIndex - 1];

				return true;
			}

			item = default(T);

			return false;
		}

		/// <summary>
		///   This method tries to get the value of the current index.
		/// </summary>
		/// <typeparam name="T"> This type parameter indicates the type of item. </typeparam>
		/// <param name="this"> This parameter references a <see cref="List{T}" /> instance. </param>
		/// <param name="currentIndex"> This parameter indicates the current index. </param>
		/// <param name="item"> This output parameter contains the current item. </param>
		/// <returns> This method returns <c>true</c> if a current item exists; otherwise, this method returns <c>false</c> . </returns>
		public static bool TryCurrentOrDefault<T>(this List<T> @this, int currentIndex, out T item)
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