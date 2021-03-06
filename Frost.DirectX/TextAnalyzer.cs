﻿// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;
using Frost.Formatting;

using SharpDX;
using SharpDX.DirectWrite;

using DxLineBreakpoint = SharpDX.DirectWrite.LineBreakpoint;
using DxBreakCondition = SharpDX.DirectWrite.BreakCondition;

using LineBreakpoint = Frost.Formatting.LineBreakpoint;

namespace Frost.DirectX
{
	/// <summary>
	///   This class provides textual analysis of a string.
	/// </summary>
	internal sealed class TextAnalyzer
		: CallbackBase, TextAnalysisSource, TextAnalysisSink
	{
		private readonly TextTextShaper _TextShaper;

		private ReadingDirection _ReadingDirection;

		private SharpDX.DirectWrite.TextAnalyzer _TextAnalyzer;

		public TextAnalyzer(FontDevice fontDevice, TextTextShaper textShaper)
		{
			Contract.Requires(fontDevice != null);

			_TextAnalyzer = new SharpDX.DirectWrite.TextAnalyzer(fontDevice.Factory);

			_TextShaper = textShaper;
		}

		/// <summary>
		///   This method applies the results of script analysis to a range of characters.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the starting index of the range. </param>
		/// <param name="textLength"> This parameter indicates the length of the range. </param>
		/// <param name="scriptAnalysis"> This parameter contains the results to apply to the range. </param>
		void TextAnalysisSink.SetScriptAnalysis(
			int textPosition, int textLength, ScriptAnalysis scriptAnalysis)
		{
			IndexedRange range = new IndexedRange(textPosition, textLength);

			foreach(int index in range)
			{
				_TextShaper.Characters[index].ScriptAnalysis = scriptAnalysis;
			}
		}

		/// <summary>
		///   This method applies the results of breakpoint analysis to a range of characters.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the starting index of the range. </param>
		/// <param name="textLength"> This parameter indicates the length of the range. </param>
		/// <param name="lineBreakpoints"> This parameter contains the results to apply to the range. </param>
		void TextAnalysisSink.SetLineBreakpoints(
			int textPosition, int textLength, DxLineBreakpoint[] lineBreakpoints)
		{
			IndexedRange range = new IndexedRange(textPosition, textLength);

			foreach(int index in range)
			{
				var breakpoint = lineBreakpoints[index - textPosition];

				LineBreakCondition beforeCondition = LineBreakCondition.Neutral;

				switch(breakpoint.BreakConditionBefore)
				{
					case DxBreakCondition.CanBreak:
						beforeCondition = LineBreakCondition.CanBreak;
						break;
					case DxBreakCondition.MayNotBreak:
						beforeCondition = LineBreakCondition.MayNotBreak;
						break;
					case DxBreakCondition.MustBreak:
						beforeCondition = LineBreakCondition.MustBreak;
						break;
					case DxBreakCondition.Neutral:
						beforeCondition = LineBreakCondition.Neutral;
						break;
				}

				LineBreakCondition afterCondition = LineBreakCondition.Neutral;

				switch(breakpoint.BreakConditionAfter)
				{
					case DxBreakCondition.CanBreak:
						afterCondition = LineBreakCondition.CanBreak;
						break;
					case DxBreakCondition.MayNotBreak:
						afterCondition = LineBreakCondition.MayNotBreak;
						break;
					case DxBreakCondition.MustBreak:
						afterCondition = LineBreakCondition.MustBreak;
						break;
					case DxBreakCondition.Neutral:
						afterCondition = LineBreakCondition.Neutral;
						break;
				}

				_TextShaper.Characters[index].Breakpoint = new LineBreakpoint(
					beforeCondition,
					afterCondition,
					breakpoint.IsWhitespace,
					breakpoint.IsSoftHyphen);
			}
		}

		/// <summary>
		///   This method applies the results of bidi-text analysis to a range of characters.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the starting index of the range. </param>
		/// <param name="textLength"> This parameter indicates the length of the range. </param>
		/// <param name="explicitLevel"> This parameter indicates the explicit bidi level for the range. </param>
		/// <param name="resolvedLevel"> This parameter indicates the resolved bidi level for the range. </param>
		void TextAnalysisSink.SetBidiLevel(
			int textPosition, int textLength, byte explicitLevel, byte resolvedLevel)
		{
			IndexedRange range = new IndexedRange(textPosition, textLength);

			_TextShaper.SetBidiLevel(range, resolvedLevel);
		}

		/// <summary>
		///   This method applies the results of number substition analysis to a range of characters.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the starting index of the range. </param>
		/// <param name="textLength"> This parameter indicates the length of the range. </param>
		/// <param name="numberSubstitution"> This parameter references the number substition to apply to the range. </param>
		void TextAnalysisSink.SetNumberSubstitution(
			int textPosition, int textLength, NumberSubstitution numberSubstitution)
		{
			IndexedRange range = new IndexedRange(textPosition, textLength);

			foreach(int index in range)
			{
				_TextShaper.Characters[index].NumberSubstitution = numberSubstitution;
			}
		}

		/// <summary>
		///   This method retrieves the text at the given position.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the text position. </param>
		/// <returns> This method returns the text at the given position. </returns>
		string TextAnalysisSource.GetTextAtPosition(int textPosition)
		{
			if(textPosition >= _TextShaper.Text.Length)
			{
				return null;
			}

			return _TextShaper.Text.Substring(
				textPosition, _TextShaper.Text.Length - textPosition);
		}

		/// <summary>
		///   This method retrieves the text before the given position.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the text position. </param>
		/// <returns> This method returns the text before the given position. </returns>
		string TextAnalysisSource.GetTextBeforePosition(int textPosition)
		{
			if(textPosition == 0 || textPosition > _TextShaper.Text.Length)
			{
				return null;
			}

			return _TextShaper.Text.Substring(0, textPosition);
		}

		/// <summary>
		///   This method retrieves cohesive runs of text having identical locales.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the text position. </param>
		/// <param name="textLength"> This output parameter indicates the length of text remaining for processing. </param>
		/// <returns> This method returns the run of text having a consistent locale at the given position. </returns>
		string TextAnalysisSource.GetLocaleName(
			int textPosition, out int textLength)
		{
			IndexedRange range = new IndexedRange(textPosition, _TextShaper.Text.Length);

			foreach(int index in range)
			{
				if(_TextShaper.Characters[index].Culture !=
					_TextShaper.Characters[textPosition].Culture)
				{
					textLength = index - textPosition;

					return _TextShaper.Characters[textPosition].Culture.Name;
				}
			}

			textLength = _TextShaper.Text.Length - textPosition;

			return _TextShaper.Characters[textPosition].Culture.Name;
		}

		/// <summary>
		///   This method retrieves cohesive runs of text having identical number substitutions.
		/// </summary>
		/// <param name="textPosition"> This parameter indicates the text position. </param>
		/// <param name="textLength"> This output parameter indicates the length of text remaining for processing. </param>
		/// <returns> This method returns the run of text having a consistent number substition at the given position. </returns>
		NumberSubstitution TextAnalysisSource.GetNumberSubstitution(
			int textPosition, out int textLength)
		{
			IndexedRange range = new IndexedRange(textPosition, _TextShaper.Text.Length);

			foreach(int index in range)
			{
				if(_TextShaper.Characters[index].NumberSubstitution !=
					_TextShaper.Characters[textPosition].NumberSubstitution)
				{
					textLength = index - textPosition;

					return _TextShaper.Characters[textPosition].NumberSubstitution;
				}
			}

			textLength = _TextShaper.Text.Length - textPosition;

			return _TextShaper.Characters[textPosition].NumberSubstitution;
		}

		/// <summary>
		///   This property indicates the text's reading direction.
		/// </summary>
		ReadingDirection TextAnalysisSource.ReadingDirection
		{
			get { return _ReadingDirection; }
		}

		public void Analyze()
		{
			CultureInfo startingCulture;

			if(_TextShaper.Characters.Length == 0)
			{
				startingCulture = CultureInfo.InvariantCulture;
			}
			else
			{
				startingCulture = _TextShaper.Characters[0].Culture;

				if(startingCulture == null)
				{
					startingCulture = CultureInfo.InvariantCulture;
				}
			}

			if(startingCulture.TextInfo.IsRightToLeft)
			{
				_ReadingDirection = ReadingDirection.RightToLeft;
			}
			else
			{
				_ReadingDirection = ReadingDirection.LeftToRight;
			}

			try
			{
				_TextAnalyzer.AnalyzeLineBreakpoints(this, 0, _TextShaper.Text.Length, this);

				_TextAnalyzer.AnalyzeBidi(this, 0, _TextShaper.Text.Length, this);

				_TextAnalyzer.AnalyzeScript(this, 0, _TextShaper.Text.Length, this);

				_TextAnalyzer.AnalyzeNumberSubstitution(this, 0, _TextShaper.Text.Length, this);
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
	}
}