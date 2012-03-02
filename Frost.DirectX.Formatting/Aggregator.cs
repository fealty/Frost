// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;
using Frost.Formatting;

using SharpDX;
using SharpDX.DirectWrite;

using DxLineBreakpoint = SharpDX.DirectWrite.LineBreakpoint;
using FontStretch = Frost.Formatting.FontStretch;
using FontStyle = Frost.Formatting.FontStyle;
using FontWeight = Frost.Formatting.FontWeight;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides an interface to apply formatting properties to individual ranges of characters.
	/// </summary>
	internal sealed class Aggregator : CallbackBase, TextAnalysisSink
	{
		private readonly AggregatorSink _OutputSink;

		/// <summary>
		///   This constructor links a new instance of this class to an output sink.
		/// </summary>
		/// <param name="outputSink"> This parameter references the output sink where results are stored. </param>
		public Aggregator(AggregatorSink outputSink)
		{
			Contract.Requires(outputSink != null);

			_OutputSink = outputSink;
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
				_OutputSink.Characters[index].ScriptAnalysis = scriptAnalysis;
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
				_OutputSink.Characters[index].Breakpoint =
					new LineBreakpoint(lineBreakpoints[index - textPosition]);
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

			foreach(int index in range)
			{
				_OutputSink.Characters[index].BidiLevel = resolvedLevel;
			}
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
				_OutputSink.Characters[index].NumberSubstitution = numberSubstitution;
			}
		}

		/// <summary>
		///   This method prepares the output sink for aggregation.
		/// </summary>
		/// <param name="text"> This parameter references the text applied to by ranges. </param>
		public void BeginAggregation(string text)
		{
			Contract.Requires(!string.IsNullOrEmpty(text));

			_OutputSink.FullText = text;
			_OutputSink.Capacity = text.Length;
		}

		/// <summary>
		///   This method applies the given culture to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="culture"> This parameter references the culture to apply to the range. </param>
		public void SetCulture(IndexedRange textRange, CultureInfo culture)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Culture = culture;
			}
		}

		/// <summary>
		///   This method applies the given font family to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="family"> This parameter references the font family to apply to the range. </param>
		public void SetFamily(IndexedRange textRange, string family)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Family = family;
			}
		}

		/// <summary>
		///   This method applies the given font features to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="features"> This parameter references the collection of font features to apply to the range. </param>
		public void SetFeatures(IndexedRange textRange, FontFeatureCollection features)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Features = features;
			}
		}

		/// <summary>
		///   This method transforms the characters of the given text range to inline objects.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="inline"> This parameter indicates the size of the inline object. </param>
		/// <param name="hAlignment"> This parameter indicates the horizontal alignment of the inline object. </param>
		/// <param name="vAlignment"> This parameter indicates the vertical alignment of the inline object. </param>
		public void SetInline(
			IndexedRange textRange, Size inline, Alignment hAlignment, Alignment vAlignment)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Inline = inline;
				_OutputSink.Characters[index].HAlignment = hAlignment;
				_OutputSink.Characters[index].VAlignment = vAlignment;
			}
		}

		/// <summary>
		///   This method applies the given font point size to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="pointSize"> This parameter contains the font size to apply to the range. </param>
		public void SetPointSize(IndexedRange textRange, float pointSize)
		{
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].PointSize = pointSize;
			}
		}

		/// <summary>
		///   This method applies the given font stretch to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="stretch"> This parameter indicates the font stretch to apply to the range. </param>
		public void SetStretch(IndexedRange textRange, FontStretch stretch)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Stretch = stretch;
			}
		}

		/// <summary>
		///   This method applies the given font style to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="style"> This parameter indicates the font style to apply to the range. </param>
		public void SetStyle(IndexedRange textRange, FontStyle style)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Style = style;
			}
		}

		/// <summary>
		///   This method applies the given font weight to a range of characters.
		/// </summary>
		/// <param name="textRange"> This parameter indicates the range of characters to modify. </param>
		/// <param name="weight"> This parameter indicates the font weight to apply to the range. </param>
		public void SetWeight(IndexedRange textRange, FontWeight weight)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Weight = weight;
			}
		}
	}
}