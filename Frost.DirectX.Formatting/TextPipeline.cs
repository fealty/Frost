using System;
using System.Diagnostics.Contracts;

namespace Frost.DirectX.Formatting
{
	public sealed class TextPipeline : IDisposable
	{
		private readonly Aggregator mAggregator;
		private readonly AggregatorSink mAggregatorSink;
		private readonly FontDevice mFontDevice;
		private readonly Formatter mFormatter;
		private readonly FormatterSink mFormatterSink;
		private readonly TextGeometryCache mGeometryCache;
		private readonly Shaper mShaper;
		private readonly ShaperSink mShaperSink;

		private readonly Analyzer mTextAnalyzer;
		private readonly Typesetter mTypesetter;
		private readonly TypesetterSink mTypesetterSink;

		public TextPipeline(FontDevice fontDevice)
		{
			Contract.Requires(fontDevice != null);

			mFontDevice = fontDevice;

			mTextAnalyzer = new Analyzer(mFontDevice.Factory);
			mAggregatorSink = new AggregatorSink();
			mAggregator = new Aggregator(mAggregatorSink);
			mShaperSink = new ShaperSink();
			mShaper = new Shaper(mFontDevice, mShaperSink);
			mFormatterSink = new FormatterSink();
			mFormatter = new Formatter(mFormatterSink);
			mTypesetterSink = new TypesetterSink();
			mTypesetter = new Typesetter(mTypesetterSink);
			mGeometryCache = new TextGeometryCache();
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		public ITextMetrics Measure(
			Paragraph paragraph, Rectangle region, params Rectangle[] obstructions)
		{
			Contract.Requires(paragraph != null);

			mTextAnalyzer.BeginAnalysis(paragraph.Text, paragraph.Culture);

			foreach(TextRun run in paragraph)
			{
				mTextAnalyzer.SetCulture(run.Range, run.Culture);
				mTextAnalyzer.SetNumberSubstitution(run.Range, null);
			}

			mTextAnalyzer.Analyze(mAggregator);

			foreach(TextRun run in paragraph)
			{
				mAggregator.SetCulture(run.Range, run.Culture);
				mAggregator.SetFamily(run.Range, run.Family);
				mAggregator.SetFeatures(run.Range, run.Features);
				mAggregator.SetInline(
					run.Range, run.Inline, run.HAlignment, run.VAlignment);
				mAggregator.SetPointSize(run.Range, run.PointSize);
				mAggregator.SetStretch(run.Range, run.Stretch);
				mAggregator.SetStyle(run.Range, run.Style);
				mAggregator.SetWeight(run.Range, run.Weight);
			}

			mShaper.Shape(mAggregatorSink);

			mTypesetter.Break(mShaperSink, paragraph, region, obstructions);

			mFormatter.Format(mTypesetterSink);

			return new ParagraphMetrics(
				paragraph, mFormatterSink, mGeometryCache);
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				mGeometryCache.Dispose();
				mTextAnalyzer.Dispose();
				mShaper.Dispose();
				mAggregator.Dispose();
			}
		}

		~TextPipeline()
		{
			Dispose(false);
		}
	}
}