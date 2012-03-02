// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides storage for results from an <see cref="Aggregator" /> .
	/// </summary>
	internal sealed class AggregatorSink
	{
		private CharacterFormat[] _Characters;

		private string _FullText;

		/// <summary>
		///   This constructor initializes a new instance of this class.
		/// </summary>
		public AggregatorSink()
		{
			_Characters = new CharacterFormat[0];
		}

		/// <summary>
		///   This property contains the text provided to the <see cref="Aggregator" /> for processing.
		/// </summary>
		public string FullText
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);

				return _FullText;
			}
			set
			{
				Contract.Requires(!string.IsNullOrEmpty(value));

				_FullText = value;
			}
		}

		/// <summary>
		///   This property indicates the current capacity of the characters array.
		/// </summary>
		public int Capacity
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return _Characters.Length;
			}
			set
			{
				Contract.Requires(value >= 0);

				if(value > _Characters.Length)
				{
					_Characters = new CharacterFormat[value * 2];
				}
			}
		}

		/// <summary>
		///   This property exposes the array of formatted characters.
		/// </summary>
		public CharacterFormat[] Characters
		{
			get
			{
				Contract.Ensures(Contract.Result<CharacterFormat[]>() != null);

				return _Characters;
			}
		}
	}
}