using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	internal sealed class AggregatorSink
	{
		private CharacterFormat[] mCharacters;

		private string mFullText;

		public AggregatorSink()
		{
			mCharacters = new CharacterFormat[0];
		}

		public string FullText
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);

				return mFullText;
			}
			set
			{
				Contract.Requires(!string.IsNullOrEmpty(value));

				mFullText = value;
			}
		}

		public int Capacity
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return mCharacters.Length;
			}
			set
			{
				Contract.Requires(value >= 0);

				if(value > mCharacters.Length)
				{
					mCharacters = new CharacterFormat[value * 2];
				}
			}
		}

		public CharacterFormat[] Characters
		{
			get { return mCharacters; }
		}
	}
}