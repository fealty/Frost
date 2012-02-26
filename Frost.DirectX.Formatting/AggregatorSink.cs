// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal sealed class AggregatorSink
	{
		private CharacterFormat[] _Characters;

		private string _FullText;

		public AggregatorSink()
		{
			_Characters = new CharacterFormat[0];
		}

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

		public CharacterFormat[] Characters
		{
			get { return _Characters; }
		}
	}
}