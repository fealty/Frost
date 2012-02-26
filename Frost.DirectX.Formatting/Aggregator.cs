using System;
using System.Diagnostics.Contracts;
using System.Globalization;

using SharpDX;
using SharpDX.DirectWrite;

using DxLineBreakpoint = SharpDX.DirectWrite.LineBreakpoint;

namespace Frost.DirectX.Formatting
{
	internal sealed class Aggregator : CallbackBase, TextAnalysisSink
	{
		private readonly AggregatorSink mOutputSink;

		public Aggregator(AggregatorSink outputSink)
		{
			Contract.Requires(outputSink != null);

			mOutputSink = outputSink;
		}

		void TextAnalysisSink.SetScriptAnalysis(
			int textPosition, int textLength, ScriptAnalysis scriptAnalysis)
		{
			for(int i = textPosition; i < textPosition + textLength; ++i)
			{
				mOutputSink.Characters[i].ScriptAnalysis = scriptAnalysis;
			}
		}

		void TextAnalysisSink.SetLineBreakpoints(
			int textPosition, int textLength, DxLineBreakpoint[] lineBreakpoints)
		{
			for(int i = textPosition; i < textPosition + textLength; ++i)
			{
				mOutputSink.Characters[i].Breakpoint =
					new LineBreakpoint(lineBreakpoints[i - textPosition]);
			}
		}

		void TextAnalysisSink.SetBidiLevel(
			int textPosition, int textLength, byte explicitLevel, byte resolvedLevel)
		{
			for(int i = textPosition; i < textPosition + textLength; ++i)
			{
				mOutputSink.Characters[i].BidiLevel = resolvedLevel;
			}
		}

		void TextAnalysisSink.SetNumberSubstitution(
			int textPosition, int textLength, NumberSubstitution numberSubstitution)
		{
			for(int i = textPosition; i < textPosition + textLength; ++i)
			{
				mOutputSink.Characters[i].NumberSubstitution = numberSubstitution;
			}
		}

		public void BeginAggregation(string text)
		{
			Contract.Requires(!string.IsNullOrEmpty(text));

			mOutputSink.FullText = text;
			mOutputSink.Capacity = text.Length;
		}

		public void SetCulture(TextRange range, CultureInfo culture)
		{
			Contract.Assert(range.Start < mOutputSink.FullText.Length);

			int rangeEnd = Math.Min(range.End, mOutputSink.FullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mOutputSink.Characters[i].Culture = culture;
			}
		}

		public void SetFamily(TextRange range, string family)
		{
			Contract.Assert(range.Start < mOutputSink.FullText.Length);

			int rangeEnd = Math.Min(range.End, mOutputSink.FullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mOutputSink.Characters[i].Family = family;
			}
		}

		public void SetFeatures(TextRange range, TextFeature[] features)
		{
			Contract.Assert(range.Start < mOutputSink.FullText.Length);

			int rangeEnd = Math.Min(range.End, mOutputSink.FullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mOutputSink.Characters[i].Features = features;
			}
		}

		public void SetInline(
			TextRange range, Size inline, Alignment hAlignment, Alignment vAlignment)
		{
			Contract.Assert(range.Start < mOutputSink.FullText.Length);

			int rangeEnd = Math.Min(range.End, mOutputSink.FullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mOutputSink.Characters[i].Inline = inline;
				mOutputSink.Characters[i].HAlignment = hAlignment;
				mOutputSink.Characters[i].VAlignment = vAlignment;
			}
		}

		public void SetPointSize(TextRange range, double pointSize)
		{
			Contract.Requires(pointSize >= 0.0 && pointSize <= double.MaxValue);
			Contract.Assert(range.Start < mOutputSink.FullText.Length);

			int rangeEnd = Math.Min(range.End, mOutputSink.FullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mOutputSink.Characters[i].PointSize = pointSize;
			}
		}

		public void SetStretch(TextRange range, FontStretch stretch)
		{
			Contract.Assert(range.Start < mOutputSink.FullText.Length);

			int rangeEnd = Math.Min(range.End, mOutputSink.FullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mOutputSink.Characters[i].Stretch = stretch;
			}
		}

		public void SetStyle(TextRange range, FontStyle style)
		{
			Contract.Assert(range.Start < mOutputSink.FullText.Length);

			int rangeEnd = Math.Min(range.End, mOutputSink.FullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mOutputSink.Characters[i].Style = style;
			}
		}

		public void SetWeight(TextRange range, FontWeight weight)
		{
			Contract.Assert(range.Start < mOutputSink.FullText.Length);

			int rangeEnd = Math.Min(range.End, mOutputSink.FullText.Length - 1);

			for(int i = range.Start; i <= rangeEnd; ++i)
			{
				mOutputSink.Characters[i].Weight = weight;
			}
		}
	}
}