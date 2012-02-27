// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;

using SharpDX;
using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	internal sealed class Analyzer : CallbackBase, TextAnalysisSource
	{
		private readonly TextAnalyzer _TextAnalyzer;

		private CharacterProperties[] _Characters;
		private string _FullText;
		private ReadingDirection _ReadingDirection;

		public Analyzer(Factory factory)
		{
			Contract.Requires(factory != null);

			_Characters = new CharacterProperties[0];

			_TextAnalyzer = new TextAnalyzer(factory);
		}

		string TextAnalysisSource.GetTextAtPosition(int textPosition)
		{
			if(textPosition >= _FullText.Length)
			{
				return null;
			}

			return _FullText.Substring(textPosition, _FullText.Length - textPosition);
		}

		string TextAnalysisSource.GetTextBeforePosition(int textPosition)
		{
			if(textPosition == 0 || textPosition > _FullText.Length)
			{
				return null;
			}

			return _FullText.Substring(0, textPosition);
		}

		string TextAnalysisSource.GetLocaleName(int textPosition, out int textLength)
		{
			IndexedRange range = new IndexedRange(textPosition, _FullText.Length);

			foreach(int index in range)
			{
				if(_Characters[index].Culture != _Characters[textPosition].Culture)
				{
					textLength = index - textPosition;

					return _Characters[textPosition].Culture.Name;
				}
			}

			textLength = _FullText.Length - textPosition;

			return _Characters[textPosition].Culture.Name;
		}

		NumberSubstitution TextAnalysisSource.GetNumberSubstitution(int textPosition, out int textLength)
		{
			IndexedRange range = new IndexedRange(textPosition, _FullText.Length);

			foreach(int index in range)
			{
				if(_Characters[index].NumberSubstitution != _Characters[textPosition].NumberSubstitution)
				{
					textLength = index - textPosition;

					return _Characters[textPosition].NumberSubstitution;
				}
			}

			textLength = _FullText.Length - textPosition;

			return _Characters[textPosition].NumberSubstitution;
		}

		ReadingDirection TextAnalysisSource.ReadingDirection
		{
			get { return _ReadingDirection; }
		}

		public void BeginAnalysis(string text, CultureInfo paragraphCulture)
		{
			Contract.Requires(!string.IsNullOrEmpty(text));
			Contract.Requires(paragraphCulture != null);

			_FullText = text;

			if(text.Length > _Characters.Length)
			{
				_Characters = new CharacterProperties[text.Length * 2];
			}

			if(paragraphCulture.TextInfo.IsRightToLeft)
			{
				_ReadingDirection = ReadingDirection.RightToLeft;
			}
			else
			{
				_ReadingDirection = ReadingDirection.LeftToRight;
			}
		}

		public void SetCulture(IndexedRange textRange, CultureInfo culture)
		{
			Contract.Assert(textRange.IsWithin(_FullText));

			foreach(int index in textRange)
			{
				_Characters[index].Culture = culture;
			}
		}

		public void SetNumberSubstitution(IndexedRange textRange, NumberSubstitution numberSubstitution)
		{
			Contract.Assert(textRange.IsWithin(_FullText));

			foreach(int index in textRange)
			{
				_Characters[index].NumberSubstitution = numberSubstitution;
			}
		}

		public void Analyze(Aggregator formatter)
		{
			Contract.Requires(formatter != null);

			formatter.BeginAggregation(_FullText);

			try
			{
				if(_TextAnalyzer.AnalyzeLineBreakpoints(this, 0, _FullText.Length, formatter).Failure)
				{
					throw new AnalysisException(null);
				}

				if(_TextAnalyzer.AnalyzeBidi(this, 0, _FullText.Length, formatter).Failure)
				{
					throw new AnalysisException(null);
				}

				if(_TextAnalyzer.AnalyzeScript(this, 0, _FullText.Length, formatter).Failure)
				{
					throw new AnalysisException(null);
				}

				if(_TextAnalyzer.AnalyzeNumberSubstitution(this, 0, _FullText.Length, formatter).Failure)
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
				_TextAnalyzer.Dispose();
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