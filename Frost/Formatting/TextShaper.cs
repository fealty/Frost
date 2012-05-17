// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;

namespace Frost.Formatting
{
	public abstract class TextShaper
	{
		private Paragraph _Paragraph;

		public Paragraph Paragraph
		{
			get { return _Paragraph; }
		}

		public void Begin(IShapedText outputSink, Paragraph paragraph)
		{
			Contract.Requires(Paragraph == null);
			Contract.Requires(outputSink != null);
			Contract.Requires(paragraph != null);

			_Paragraph = paragraph;

			OnBegin(outputSink);
		}

		public void SetBreakpoint(IndexedRange textRange, LineBreakpoint breakpoint)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));

			OnSetBreakpoints(textRange, breakpoint);
		}

		public void SetBidiLevel(IndexedRange textRange, byte resolvedLevel)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));

			OnSetBidiLevel(textRange, resolvedLevel);
		}

		public void SetCulture(IndexedRange textRange, CultureInfo culture)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));

			OnSetCulture(textRange, culture);
		}

		public void SetFamily(IndexedRange textRange, string family)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));

			OnSetFamily(textRange, family);
		}

		public void SetFeatures(
			IndexedRange textRange, FontFeatureCollection features)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));

			OnSetFeatures(textRange, features);
		}

		public void SetPointSize(IndexedRange textRange, float pointSize)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));
			Contract.Requires(Check.IsPositive(pointSize));

			OnSetPointSize(textRange, pointSize);
		}

		public void SetStretch(IndexedRange textRange, FontStretch stretch)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));

			OnSetStretch(textRange, stretch);
		}

		public void SetStyle(IndexedRange textRange, FontStyle style)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));

			OnSetStyle(textRange, style);
		}

		public void SetWeight(IndexedRange textRange, FontWeight weight)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));

			OnSetWeight(textRange, weight);
		}

		public void SetInline(
			IndexedRange textRange,
			Size inline,
			Alignment hAlignment,
			Alignment vAlignment)
		{
			Contract.Requires(Paragraph != null);
			Contract.Requires(textRange.IsWithin(Paragraph.Text));
			Contract.Requires(Check.IsPositive(inline.Width));
			Contract.Requires(Check.IsPositive(inline.Height));

			OnSetInline(textRange, inline, hAlignment, vAlignment);
		}

		public void End()
		{
			Contract.Requires(Paragraph != null);

			OnEnd();

			_Paragraph = null;
		}

		protected abstract void OnBegin(IShapedText outputSink);

		protected abstract void OnSetInline(
			IndexedRange textRange,
			Size inline,
			Alignment hAlignment,
			Alignment vAlignment);

		protected abstract void OnSetWeight(
			IndexedRange textRange, FontWeight weight);

		protected abstract void OnSetStyle(
			IndexedRange textRange, FontStyle style);

		protected abstract void OnSetStretch(
			IndexedRange textRange, FontStretch stretch);

		protected abstract void OnSetPointSize(
			IndexedRange textRange, float pointSize);

		protected abstract void OnSetBreakpoints(
			IndexedRange textRange, LineBreakpoint breakpoint);

		protected abstract void OnSetBidiLevel(
			IndexedRange textRange, byte resolvedLevel);

		protected abstract void OnSetCulture(
			IndexedRange textRange, CultureInfo culture);

		protected abstract void OnSetFamily(
			IndexedRange textRange, string family);

		protected abstract void OnSetFeatures(
			IndexedRange textRange, FontFeatureCollection features);

		protected abstract void OnEnd();
	}
}