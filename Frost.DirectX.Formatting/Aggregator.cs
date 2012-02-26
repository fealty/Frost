// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;

using SharpDX;
using SharpDX.DirectWrite;

using DxLineBreakpoint = SharpDX.DirectWrite.LineBreakpoint;

namespace Frost.DirectX.Formatting
{
	internal sealed class Aggregator : CallbackBase, TextAnalysisSink
	{
		private readonly AggregatorSink _OutputSink;

		public Aggregator(AggregatorSink outputSink)
		{
			Contract.Requires(outputSink != null);

			_OutputSink = outputSink;
		}

		void TextAnalysisSink.SetScriptAnalysis(
			int textPosition, int textLength, ScriptAnalysis scriptAnalysis)
		{
			IndexedRange range = new IndexedRange(textPosition, textLength);

			foreach(int index in range)
			{
				_OutputSink.Characters[index].ScriptAnalysis = scriptAnalysis;
			}
		}

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

		void TextAnalysisSink.SetBidiLevel(
			int textPosition, int textLength, byte explicitLevel, byte resolvedLevel)
		{
			IndexedRange range = new IndexedRange(textPosition, textLength);

			foreach(int index in range)
			{
				_OutputSink.Characters[index].BidiLevel = resolvedLevel;
			}
		}

		void TextAnalysisSink.SetNumberSubstitution(
			int textPosition, int textLength, NumberSubstitution numberSubstitution)
		{
			IndexedRange range = new IndexedRange(textPosition, textLength);

			foreach(int index in range)
			{
				_OutputSink.Characters[index].NumberSubstitution = numberSubstitution;
			}
		}

		public void BeginAggregation(string text)
		{
			Contract.Requires(!string.IsNullOrEmpty(text));

			_OutputSink.FullText = text;
			_OutputSink.Capacity = text.Length;
		}

		public void SetCulture(IndexedRange textRange, CultureInfo culture)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Culture = culture;
			}
		}

		public void SetFamily(IndexedRange textRange, string family)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Family = family;
			}
		}

		public void SetFeatures(IndexedRange textRange, FontFeature[] features)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Features = features;
			}
		}

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

		public void SetPointSize(IndexedRange textRange, float pointSize)
		{
			Contract.Requires(Check.IsPositive(pointSize));
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].PointSize = pointSize;
			}
		}

		public void SetStretch(IndexedRange textRange, FontStretch stretch)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Stretch = stretch;
			}
		}

		public void SetStyle(IndexedRange textRange, FontStyle style)
		{
			Contract.Assert(textRange.IsWithin(_OutputSink.FullText));

			foreach(int index in textRange)
			{
				_OutputSink.Characters[index].Style = style;
			}
		}

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