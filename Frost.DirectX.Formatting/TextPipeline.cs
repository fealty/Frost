// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

using Frost.Formatting;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides the text processing pipeline that produces formatted and typeset text.
	/// </summary>
	public sealed class TextPipeline : IDisposable
	{
		private readonly Aggregator _Aggregator;
		private readonly AggregatorSink _AggregatorSink;
		private readonly FontDevice _FontDevice;
		private readonly Formatter _Formatter;
		private readonly FormatterSink _FormatterSink;
		private readonly TextGeometryCache _GeometryCache;
		private readonly Shaper _Shaper;
		private readonly ShaperSink _ShaperSink;

		private readonly Analyzer _TextAnalyzer;
		private readonly Typesetter _Typesetter;
		private readonly TypesetterSink _TypesetterSink;

		public TextPipeline(FontDevice fontDevice)
		{
			Contract.Requires(fontDevice != null);

			_FontDevice = fontDevice;

			_TextAnalyzer = new Analyzer(_FontDevice.Factory);
			_AggregatorSink = new AggregatorSink();
			_Aggregator = new Aggregator(_AggregatorSink);
			_ShaperSink = new ShaperSink();
			_Shaper = new Shaper(_FontDevice, _ShaperSink);
			_FormatterSink = new FormatterSink();
			_Formatter = new Formatter(_FormatterSink);
			_TypesetterSink = new TypesetterSink();
			_Typesetter = new Typesetter(_TypesetterSink);
			_GeometryCache = new TextGeometryCache();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		///   This method measures (or formats and typesets) a paragraph of text.
		/// </summary>
		/// <param name="paragraph"> This parameter references the paragraph to measure. </param>
		/// <param name="region"> This parameter contains the region to measure the paragraph within. </param>
		/// <param name="obstructions"> This parameter references floating obstructions that text will flow around. </param>
		/// <returns> This method returns the final measured text metrics. </returns>
		public ITextMetrics Measure(
			Paragraph paragraph, Rectangle region, params Rectangle[] obstructions)
		{
			Contract.Requires(paragraph != null);

			_TextAnalyzer.BeginAnalysis(paragraph.Text, paragraph.Culture);

			foreach(TextRun run in paragraph.Runs)
			{
				_TextAnalyzer.SetCulture(run.TextRange, run.Culture);
				_TextAnalyzer.SetNumberSubstitution(run.TextRange, null);
			}

			_TextAnalyzer.Analyze(_Aggregator);

			foreach(TextRun run in paragraph.Runs)
			{
				_Aggregator.SetCulture(run.TextRange, run.Culture);
				_Aggregator.SetFamily(run.TextRange, run.Family);
				_Aggregator.SetFeatures(run.TextRange, run.Features);
				_Aggregator.SetInline(run.TextRange, run.Inline, run.HAlignment, run.VAlignment);
				_Aggregator.SetPointSize(run.TextRange, run.PointSize);
				_Aggregator.SetStretch(run.TextRange, run.Stretch);
				_Aggregator.SetStyle(run.TextRange, run.Style);
				_Aggregator.SetWeight(run.TextRange, run.Weight);
			}

			_Shaper.Shape(_AggregatorSink);

			_Typesetter.Typeset(_ShaperSink, paragraph, region, obstructions);

			_Formatter.Format(_TypesetterSink);

			return new ParagraphMetrics(paragraph, _FormatterSink, _GeometryCache);
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				_GeometryCache.Dispose();
				_TextAnalyzer.Dispose();
				_Shaper.Dispose();
				_Aggregator.Dispose();
			}
		}
	}
}