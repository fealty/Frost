using System;
using System.Diagnostics.Contracts;
using System.Globalization;

using SharpDX;
using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	internal sealed class Analyzer : CallbackBase, TextAnalysisSource
	{
		private readonly TextAnalyzer mTextAnalyzer;

		private CharacterProperties[] mCharacters;
		private string mFullText;
		private ReadingDirection mReadingDirection;

		public Analyzer(Factory factory)
		{
			Contract.Requires(factory != null);

			mCharacters = new CharacterProperties[0];

			mTextAnalyzer = new TextAnalyzer(factory);
		}

		string TextAnalysisSource.GetTextAtPosition(int textPosition)
		{
			if(textPosition >= mFullText.Length)
			{
				return null;
			}

			return mFullText.Substring(textPosition, mFullText.Length - textPosition);
		}

		string TextAnalysisSource.GetTextBeforePosition(int textPosition)
		{
			if(textPosition == 0 || textPosition > mFullText.Length)
			{
				return null;
			}

			return mFullText.Substring(0, textPosition);
		}

		string TextAnalysisSource.GetLocaleName(
			int textPosition, out int textLength)
		{
			for(int i = textPosition; i < mFullText.Length; ++i)
			{
				if(mCharacters[i].Culture != mCharacters[textPosition].Culture)
				{
					textLength = i - textPosition;

					return mCharacters[textPosition].Culture.Name;
				}
			}

			textLength = mFullText.Length - textPosition;

			return mCharacters[textPosition].Culture.Name;
		}

		NumberSubstitution TextAnalysisSource.GetNumberSubstitution(
			int textPosition, out int textLength)
		{
			for(int i = textPosition; i < mFullText.Length; ++i)
			{
				if(mCharacters[i].NumberSubstitution !=
				   mCharacters[textPosition].NumberSubstitution)
				{
					textLength = i - textPosition;

					return mCharacters[textPosition].NumberSubstitution;
				}
			}

			textLength = mFullText.Length - textPosition;

			return mCharacters[textPosition].NumberSubstitution;
		}

		ReadingDirection TextAnalysisSource.ReadingDirection
		{
			get { return mReadingDirection; }
		}

		public void BeginAnalysis(string text, CultureInfo paragraphCulture)
		{
			Contract.Requires(!string.IsNullOrEmpty(text));
			Contract.Requires(paragraphCulture != null);

			mFullText = text;

			if(text.Length > mCharacters.Length)
			{
				mCharacters = new CharacterProperties[text.Length * 2];
			}

			if(paragraphCulture.TextInfo.IsRightToLeft)
			{
				mReadingDirection = ReadingDirection.RightToLeft;
			}
			else
			{
				mReadingDirection = ReadingDirection.LeftToRight;
			}
		}

		public void SetCulture(TextRange range, CultureInfo culture)
		{
			Contract.Assert(range.Start < mFullText.Length);

			int rangeEnd = Math.Min(range.End, mFullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mCharacters[i].Culture = culture;
			}
		}

		public void SetNumberSubstitution(
			TextRange range, NumberSubstitution numberSubstitution)
		{
			Contract.Assert(range.Start < mFullText.Length);

			int rangeEnd = Math.Min(range.End, mFullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mCharacters[i].NumberSubstitution = numberSubstitution;
			}
		}

		public void Analyze(Aggregator formatter)
		{
			Contract.Requires(formatter != null);

			formatter.BeginAggregation(mFullText);

			try
			{
				if(mTextAnalyzer.AnalyzeLineBreakpoints(
					this, 0, mFullText.Length, formatter).Failure)
				{
					throw new AnalysisException(null);
				}

				if(mTextAnalyzer.AnalyzeBidi(
					this, 0, mFullText.Length, formatter).Failure)
				{
					throw new AnalysisException(null);
				}

				if(mTextAnalyzer.AnalyzeScript(
					this, 0, mFullText.Length, formatter).Failure)
				{
					throw new AnalysisException(null);
				}

				if(mTextAnalyzer.AnalyzeNumberSubstitution(
					this, 0, mFullText.Length, formatter).Failure)
				{
					throw new AnalysisException(null);
				}
			}
			catch(SharpDXException e)
			{
				throw new AnalysisException(e);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				mTextAnalyzer.Dispose();
			}

			base.Dispose(disposing);
		}

		private struct CharacterProperties
		{
			public CultureInfo Culture;
			public NumberSubstitution NumberSubstitution;
		}
	}
}