// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;

using SharpDX;
using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides textual analysis of a string.
	/// </summary>
	internal sealed class Analyzer : CallbackBase, TextAnalysisSource
	{
		private CharacterProperties[] _Characters;
		private string _FullText;
		private ReadingDirection _ReadingDirection;
		private TextAnalyzer _TextAnalyzer;

		/// <summary>
		///   This constructor initializes a new instance of this class.
		/// </summary>
		/// <param name="factory"> This parameter references the DirectWrite factory. </param>
		public Analyzer(Factory factory)
		{
			Contract.Requires(factory != null);

			_Characters = new CharacterProperties[0];

			_TextAnalyzer = new TextAnalyzer(factory);
		}

		/// <summary>
		///   This method retrieves the text at the given position.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the text position. </param>
		/// <returns> This method returns the text at the given position. </returns>
		string TextAnalysisSource.GetTextAtPosition(int textPosition)
		{
			if(textPosition >= _FullText.Length)
			{
				return null;
			}

			return _FullText.Substring(textPosition, _FullText.Length - textPosition);
		}

		/// <summary>
		///   This method retrieves the text before the given position.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the text position. </param>
		/// <returns> This method returns the text before the given position. </returns>
		string TextAnalysisSource.GetTextBeforePosition(int textPosition)
		{
			if(textPosition == 0 || textPosition > _FullText.Length)
			{
				return null;
			}

			return _FullText.Substring(0, textPosition);
		}

		/// <summary>
		///   This method retrieves cohesive runs of text having identical locales.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the text position. </param>
		/// <param name="textLength"> This output parameter indicates the length of text remaining for processing. </param>
		/// <returns> This method returns the run of text having a consistent locale at the given position. </returns>
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

		/// <summary>
		///   This method retrieves cohesive runs of text having identical number substitutions.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the text position. </param>
		/// <param name="textLength"> This output parameter indicates the length of text remaining for processing. </param>
		/// <returns> This method returns the run of text having a consistent number substition at the given position. </returns>
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

		/// <summary>
		///   This property indicates the text's reading direction.
		/// </summary>
		ReadingDirection TextAnalysisSource.ReadingDirection
		{
			get { return _ReadingDirection; }
		}

		/// <summary>
		///   This method prepares for analyzing the given text.
		/// </summary>
		/// <param name="text"> This parameter references the text to analyze. </param>
		/// <param name="paragraphCulture"> This parameter references the culture of the text to analyze. </param>
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

		/// <summary>
		///   This method applies the given culture to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="culture"> This parameter references the culture to apply to the range. </param>
		public void SetCulture(IndexedRange textRange, CultureInfo culture)
		{
			Contract.Assert(textRange.IsWithin(_FullText));

			foreach(int index in textRange)
			{
				_Characters[index].Culture = culture;
			}
		}

		/// <summary>
		///   This method applies the given number substition to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="numberSubstitution"> This parameter references the number substitution to apply to the range. </param>
		public void SetNumberSubstitution(IndexedRange textRange, NumberSubstitution numberSubstitution)
		{
			Contract.Assert(textRange.IsWithin(_FullText));

			foreach(int index in textRange)
			{
				_Characters[index].NumberSubstitution = numberSubstitution;
			}
		}

		/// <summary>
		///   This method performs an analysis of the prepared text.
		/// </summary>
		/// <param name="formatter"> This parameter references the output sink where results are stored. </param>
		public void Analyze(Aggregator formatter)
		{
			Contract.Requires(formatter != null);

			formatter.BeginAggregation(_FullText);

			try
			{
				if(_TextAnalyzer.AnalyzeLineBreakpoints(this, 0, _FullText.Length, formatter).Failure)
				{
					throw new InvalidOperationException("Analysis of line breaking points failed!");
				}

				if(_TextAnalyzer.AnalyzeBidi(this, 0, _FullText.Length, formatter).Failure)
				{
					throw new InvalidOperationException("Analysis of bidi-text failed!");
				}

				if(_TextAnalyzer.AnalyzeScript(this, 0, _FullText.Length, formatter).Failure)
				{
					throw new InvalidOperationException("Analysis of scripts failed!");
				}

				if(_TextAnalyzer.AnalyzeNumberSubstitution(this, 0, _FullText.Length, formatter).Failure)
				{
					throw new InvalidOperationException("Analysis of number substition failed!");
				}
			}
			catch(SharpDXException e)
			{
				throw new InvalidOperationException(e.Message, e);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				_TextAnalyzer.Dispose();

				_TextAnalyzer = null;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		///   This struct provides analysis-required properties for characters.
		/// </summary>
		private struct CharacterProperties
		{
			public CultureInfo Culture;
			public NumberSubstitution NumberSubstitution;
		}
	}
}