// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

using Frost.Collections;
using Frost.Painting;

namespace Frost.Formatting
{
	//TODO: add support for decorations to Paragraph?
	public sealed class Paragraph
	{
		private static readonly CultureInfo _DefaultCulture;
		private static readonly string _DefaultFamily;
		private static readonly float _DefaultPointSize;
		private static readonly float _DefaultIndentation;
		private static readonly float _DefaultLeading;
		private static readonly float _DefaultSpacing;
		private static readonly float _DefaultTracking;

		[ThreadStatic] private static Builder _Builder;

		private readonly Alignment _Alignment;

		private readonly float _Indentation;
		private readonly float _Leading;
		private readonly TextRunCollection _Runs;

		private readonly float _Spacing;

		private readonly string _Text;
		private readonly float _Tracking;

		static Paragraph()
		{
			_DefaultCulture = CultureInfo.InvariantCulture;
			_DefaultFamily = "arial";
			_DefaultPointSize = 10.0f;
			_DefaultIndentation = 0.0f;
			_DefaultLeading = 0.0f;
			_DefaultSpacing = 1.0f / 3.0f;
			_DefaultTracking = 1.0f / 50.0f;
		}

		private Paragraph(
			string text,
			Alignment alignment,
			float indentation,
			float leading,
			float spacing,
			float tracking,
			TextRunCollection runs)
		{
			Contract.Requires(text != null);
			Contract.Requires(Check.IsPositive(indentation));
			Contract.Requires(Check.IsPositive(leading));
			Contract.Requires(Check.IsPositive(spacing));
			Contract.Requires(Check.IsPositive(tracking));
			Contract.Requires(runs != null);

			_Text = text;
			_Alignment = alignment;
			_Indentation = indentation;
			_Leading = leading;
			_Spacing = spacing;
			_Tracking = tracking;
			_Runs = runs;
		}

		public static void Draw(Painter painterSink, ITextMetrics metrics)
		{
			Contract.Requires(painterSink != null);
			Contract.Requires(metrics != null);

			Draw(painterSink, metrics, metrics.Paragraph.Text);
		}

		public static void Draw(Painter painterSink, ITextMetrics metrics, IndexedRange range)
		{
			Contract.Requires(painterSink != null);
			Contract.Requires(metrics != null);

			for(int i = range.StartIndex; i <= range.LastIndex; ++i)
			{
				if(metrics.IsClusterStart(i))
				{
					if(metrics.IsVisible(i))
					{
						Rectangle region = metrics.Regions[i];

						Point baseline = new Point(
							region.Left + metrics.BaselineOffset.Width, region.Top + metrics.BaselineOffset.Height);

						if(metrics.IsRightToLeft(i))
						{
							baseline = new Point(region.Right + metrics.BaselineOffset.Width, baseline.Y);
						}

						painterSink.SaveState();

						painterSink.Translate(baseline.X, baseline.Y);

						Outline outline = metrics.Outlines[i];

						painterSink.Scale(outline.EmSize, outline.EmSize);

						if (outline.NormalizedOutline != null)
						{
							painterSink.Fill(outline.NormalizedOutline);
						}

						painterSink.RestoreState();
					}
				}
			}
		}

		public float Tracking
		{
			get { return _Tracking; }
		}

		public string Text
		{
			get { return _Text; }
		}

		public CultureInfo Culture
		{
			get { return _Runs[0].Culture ?? CultureInfo.InvariantCulture; }
		}

		public float Spacing
		{
			get { return _Spacing; }
		}

		public TextRunCollection Runs
		{
			get { return _Runs; }
		}

		public float Leading
		{
			get { return _Leading; }
		}

		public float Indentation
		{
			get { return _Indentation; }
		}

		public Alignment Alignment
		{
			get { return _Alignment; }
		}

		public static float DefaultSpacing
		{
			get { return _DefaultSpacing; }
		}

		public static float DefaultTracking
		{
			get { return _DefaultTracking; }
		}

		public static float DefaultLeading
		{
			get { return _DefaultLeading; }
		}

		public static float DefaultIndentation
		{
			get { return _DefaultIndentation; }
		}

		public static float DefaultPointSize
		{
			get { return _DefaultPointSize; }
		}

		public static string DefaultFamily
		{
			get { return _DefaultFamily; }
		}

		public static CultureInfo DefaultCulture
		{
			get { return _DefaultCulture; }
		}

		public static Builder Create()
		{
			Contract.Ensures(Contract.Result<Builder>() != null);

			_Builder = _Builder ?? new Builder();

			_Builder.Reset();

			return _Builder;
		}

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(_Runs != null);
			Contract.Invariant(_Text != null);
			Contract.Invariant(Check.IsPositive(_Indentation));
			Contract.Invariant(Check.IsPositive(_Leading));
			Contract.Invariant(Check.IsPositive(_Spacing));
			Contract.Invariant(Check.IsPositive(_Tracking));
		}

		public sealed class Builder
		{
			private readonly List<TextRun> _Runs;
			private readonly Stack<TextRun> _States;
			private readonly StringBuilder _Text;

			private CultureInfo _ActiveCulture;
			private string _ActiveFamily;
			private FontFeatureCollection _ActiveFeatures;

			private Alignment _ActiveHAlignment;

			private Size _ActiveInline;
			private float _ActivePointSize;

			private FontStretch _ActiveStretch;
			private FontStyle _ActiveStyle;
			private IndexedRange _ActiveTextRange;
			private Alignment _ActiveVAlignment;
			private FontWeight _ActiveWeight;
			private Alignment _Alignment;

			private float _Indentation;
			private float _Leading;
			private float _Spacing;
			private float _Tracking;

			internal Builder()
			{
				_Runs = new List<TextRun>();
				_States = new Stack<TextRun>();

				_Text = new StringBuilder();
			}

			public Builder SaveState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_States.Push(
					new TextRun(
						_ActiveTextRange,
						_ActiveCulture,
						_ActiveFamily,
						_ActiveStretch,
						_ActiveStyle,
						_ActiveWeight,
						_ActivePointSize,
						_ActiveHAlignment,
						_ActiveVAlignment,
						_ActiveInline,
						_ActiveFeatures));

				return this;
			}

			public Builder ResetState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_ActiveTextRange = IndexedRange.Empty;
				_ActiveCulture = DefaultCulture;
				_ActiveFamily = DefaultFamily;
				_ActiveFeatures = null;
				_ActivePointSize = DefaultPointSize;
				_ActiveStretch = FontStretch.Regular;
				_ActiveStyle = FontStyle.Regular;
				_ActiveWeight = FontWeight.Regular;
				_ActiveInline = Size.Empty;
				_ActiveHAlignment = Alignment.Stretch;
				_ActiveVAlignment = Alignment.Stretch;

				return this;
			}

			public Builder RestoreState()
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				TextRun activeState = _States.Pop();

				_ActiveTextRange = activeState.TextRange;
				_ActiveCulture = activeState.Culture;
				_ActiveFamily = activeState.Family;
				_ActiveFeatures = activeState.Features;
				_ActivePointSize = activeState.PointSize;
				_ActiveStretch = activeState.Stretch;
				_ActiveStyle = activeState.Style;
				_ActiveWeight = activeState.Weight;
				_ActiveInline = activeState.Inline;
				_ActiveHAlignment = activeState.HAlignment;
				_ActiveVAlignment = activeState.VAlignment;

				return this;
			}

			public Builder WithFeatures(FontFeatureCollection features)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_ActiveFeatures = features;

				return this;
			}

			public Builder WithLeading(float leading)
			{
				Contract.Requires(Check.IsPositive(leading));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Leading = leading;

				return this;
			}

			public Builder WithTracking(float tracking)
			{
				Contract.Requires(Check.IsPositive(tracking));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Tracking = tracking;

				return this;
			}

			public Builder WithIndentation(float indentation)
			{
				Contract.Requires(Check.IsPositive(indentation));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Indentation = indentation;

				return this;
			}

			public Builder WithSpacing(float spacing)
			{
				Contract.Requires(Check.IsPositive(spacing));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Spacing = spacing;

				return this;
			}

			public Builder WithAlignment(Alignment alignment)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_Alignment = alignment;

				return this;
			}

			public Builder WithCulture(CultureInfo culture)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_ActiveCulture = culture;

				return this;
			}

			public Builder WithPointSize(float pointSize)
			{
				Contract.Requires(Check.IsPositive(pointSize));
				Contract.Ensures(Contract.Result<Builder>() != null);

				_ActivePointSize = pointSize;

				return this;
			}

			public Builder WithFamily(string family)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_ActiveFamily = family;

				return this;
			}

			public Builder WithStretch(FontStretch stretch)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_ActiveStretch = stretch;

				return this;
			}

			public Builder WithStyle(FontStyle style)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_ActiveStyle = style;

				return this;
			}

			public Builder WithWeight(FontWeight weight)
			{
				Contract.Ensures(Contract.Result<Builder>() != null);

				_ActiveWeight = weight;

				return this;
			}

			public Builder AddInline(
				Size inline, Alignment hAlignment = Alignment.Stretch, Alignment vAlignment = Alignment.Stretch)
			{
				Contract.Requires(Check.IsPositive(inline.Width));
				Contract.Requires(Check.IsPositive(inline.Height));
				Contract.Ensures(Contract.Result<Builder>() != null);

				// append a new run with a text length of one
				_Runs.Add(
					new TextRun(
						new IndexedRange(_Text.Length, 1),
						_ActiveCulture,
						_ActiveFamily,
						_ActiveStretch,
						_ActiveStyle,
						_ActiveWeight,
						_ActivePointSize,
						hAlignment,
						vAlignment,
						inline,
						_ActiveFeatures));

				try
				{
					_Text.Append('\u00A0');
				}
				catch
				{
					// rollback the change to runs on failure
					_Runs.RemoveAt(_Runs.Count - 1);

					// rethrow the exception
					throw;
				}

				return this;
			}

			public Builder AddText(string text, params object[] objects)
			{
				Contract.Requires(text != null);
				Contract.Ensures(Contract.Result<Builder>() != null);

				if(objects != null)
				{
					text = string.Format(text, objects);
				}

				TextRun newRun = new TextRun(
					_ActiveTextRange,
					_ActiveCulture,
					_ActiveFamily,
					_ActiveStretch,
					_ActiveStyle,
					_ActiveWeight,
					_ActivePointSize,
					_ActiveHAlignment,
					_ActiveVAlignment,
					_ActiveInline,
					_ActiveFeatures);

				if(_Runs.Count > 0)
				{
					TextRun oldRun = _Runs[_Runs.Count - 1];

					if((newRun.Culture == oldRun.Culture) && (newRun.Family == oldRun.Family) &&
					   (newRun.Stretch == oldRun.Stretch) && (newRun.Style == oldRun.Style) &&
					   (newRun.Weight == oldRun.Weight) && (newRun.PointSize.Equals(oldRun.PointSize)) &&
					   (newRun.HAlignment == oldRun.HAlignment) && (newRun.VAlignment == oldRun.VAlignment) &&
					   (newRun.Inline == oldRun.Inline) && (newRun.Features == oldRun.Features))
					{
						// modify the existing run to include the appended text
						_Runs[_Runs.Count - 1] =
							new TextRun(
								new IndexedRange(oldRun.TextRange.StartIndex, oldRun.TextRange.Length + text.Length),
								oldRun.Culture,
								oldRun.Family,
								oldRun.Stretch,
								oldRun.Style,
								oldRun.Weight,
								oldRun.PointSize,
								oldRun.HAlignment,
								oldRun.VAlignment,
								oldRun.Inline,
								oldRun.Features);

						try
						{
							_Text.Append(text);
						}
						catch
						{
							// rollback the change to run on failure
							_Runs.RemoveAt(_Runs.Count - 1);

							// rethrow the exception
							throw;
						}

						return this;
					}
				}

				// append a run at the current text position
				_Runs.Add(
					new TextRun(
						new IndexedRange(_Text.Length, text.Length),
						newRun.Culture,
						newRun.Family,
						newRun.Stretch,
						newRun.Style,
						newRun.Weight,
						newRun.PointSize,
						newRun.HAlignment,
						newRun.VAlignment,
						newRun.Inline,
						newRun.Features));

				try
				{
					_Text.Append(text);
				}
				catch
				{
					// rollback the change to runs on failure
					_Runs.RemoveAt(_Runs.Count - 1);

					// rethrow the exception
					throw;
				}

				return this;
			}

			public Paragraph Build()
			{
				if(_Runs.Count > 0)
				{
					return new Paragraph(
						_Text.ToString(),
						_Alignment,
						_Indentation,
						_Leading,
						_Spacing,
						_Tracking,
						new TextRunCollection(_Runs));
				}

				return null;
			}

			internal void Reset()
			{
				_Runs.Clear();
				_States.Clear();

				_Alignment = Alignment.Stretch;
				_Indentation = DefaultIndentation;
				_Leading = DefaultLeading;
				_Spacing = DefaultSpacing;
				_Tracking = DefaultTracking;

				_Text.Clear();

				_ActiveCulture = DefaultCulture;
				_ActiveFamily = DefaultFamily;
				_ActivePointSize = DefaultPointSize;
				_ActiveHAlignment = Alignment.Stretch;
				_ActiveVAlignment = Alignment.Stretch;
				_ActiveInline = Size.Empty;
				_ActiveTextRange = IndexedRange.Empty;
				_ActiveStretch = FontStretch.Regular;
				_ActiveStyle = FontStyle.Regular;
				_ActiveWeight = FontWeight.Regular;
				_ActiveFeatures = null;
			}

#if(UNIT_TESTING)
			[Fact] internal static void Test0()
			{
				Builder builder = Create();

				builder.SaveState();

				builder.WithWeight(FontWeight.Bold);
				builder.WithCulture(new CultureInfo("ja-JP"));
				builder.WithFamily("georgia");
				builder.WithFeatures(
					new FontFeatureCollection(new[] {new FontFeature("kern"), new FontFeature("liga")}));
				builder.WithPointSize(12.0f);
				builder.WithStretch(FontStretch.Expanded);
				builder.WithStyle(FontStyle.Italic);
				builder.WithTracking(1.0f);
				builder.WithSpacing(2.0f);
				builder.WithLeading(3.0f);
				builder.WithIndentation(4.0f);
				builder.WithAlignment(Alignment.Center);

				Assert.Equal(builder._ActiveWeight, FontWeight.Bold);
				Assert.Equal(builder._ActiveCulture, new CultureInfo("ja-JP"));
				Assert.Equal(builder._ActiveFamily, "georgia");
				Assert.Equal(
					builder._ActiveFeatures,
					new FontFeatureCollection(new[] {new FontFeature("kern"), new FontFeature("liga")}));
				Assert.Equal(builder._ActivePointSize, 12.0);
				Assert.Equal(builder._ActiveStretch, FontStretch.Expanded);
				Assert.Equal(builder._ActiveStyle, FontStyle.Italic);
				Assert.Equal(builder._Tracking, 1.0);
				Assert.Equal(builder._Spacing, 2.0);
				Assert.Equal(builder._Leading, 3.0);
				Assert.Equal(builder._Indentation, 4.0);
				Assert.Equal(builder._Alignment, Alignment.Center);

				builder.RestoreState();

				Assert.Equal(builder._ActiveWeight, FontWeight.Regular);
				Assert.Equal(builder._ActiveCulture, DefaultCulture);
				Assert.Equal(builder._ActiveFamily, DefaultFamily);
				Assert.Equal(builder._ActiveFeatures, null);
				Assert.Equal(builder._ActivePointSize, DefaultPointSize);
				Assert.Equal(builder._ActiveStretch, FontStretch.Regular);
				Assert.Equal(builder._ActiveStyle, FontStyle.Regular);
				Assert.Equal(builder._Tracking, 1.0);
				Assert.Equal(builder._Spacing, 2.0);
				Assert.Equal(builder._Leading, 3.0);
				Assert.Equal(builder._Indentation, 4.0);
				Assert.Equal(builder._Alignment, Alignment.Center);

				builder = Create();

				Assert.Equal(builder._ActiveWeight, FontWeight.Regular);
				Assert.Equal(builder._ActiveCulture, DefaultCulture);
				Assert.Equal(builder._ActiveFamily, DefaultFamily);
				Assert.Equal(builder._ActiveFeatures, null);
				Assert.Equal(builder._ActivePointSize, DefaultPointSize);
				Assert.Equal(builder._ActiveStretch, FontStretch.Regular);
				Assert.Equal(builder._ActiveStyle, FontStyle.Regular);
				Assert.Equal(builder._Tracking, DefaultTracking);
				Assert.Equal(builder._Spacing, DefaultSpacing);
				Assert.Equal(builder._Leading, DefaultLeading);
				Assert.Equal(builder._Indentation, DefaultIndentation);
				Assert.Equal(builder._Alignment, Alignment.Stretch);

				builder.WithWeight(FontWeight.Bold);
				builder.WithCulture(new CultureInfo("ja-JP"));
				builder.WithFamily("georgia");
				builder.WithFeatures(
					new FontFeatureCollection(new[] {new FontFeature("kern"), new FontFeature("liga")}));
				builder.WithPointSize(12.0f);
				builder.WithStretch(FontStretch.Expanded);
				builder.WithStyle(FontStyle.Italic);
				builder.WithTracking(1.0f);
				builder.WithSpacing(2.0f);
				builder.WithLeading(3.0f);
				builder.WithIndentation(4.0f);
				builder.WithAlignment(Alignment.Center);

				builder.SaveState();
				builder.ResetState();

				Assert.Equal(builder._ActiveWeight, FontWeight.Regular);
				Assert.Equal(builder._ActiveCulture, DefaultCulture);
				Assert.Equal(builder._ActiveFamily, DefaultFamily);
				Assert.Equal(builder._ActiveFeatures, null);
				Assert.Equal(builder._ActivePointSize, DefaultPointSize);
				Assert.Equal(builder._ActiveStretch, FontStretch.Regular);
				Assert.Equal(builder._ActiveStyle, FontStyle.Regular);
				Assert.Equal(builder._Tracking, 1.0);
				Assert.Equal(builder._Spacing, 2.0);
				Assert.Equal(builder._Leading, 3.0);
				Assert.Equal(builder._Indentation, 4.0);
				Assert.Equal(builder._Alignment, Alignment.Center);

				builder.RestoreState();

				Assert.Equal(builder._ActiveWeight, FontWeight.Bold);
				Assert.Equal(builder._ActiveCulture, new CultureInfo("ja-JP"));
				Assert.Equal(builder._ActiveFamily, "georgia");
				Assert.Equal(
					builder._ActiveFeatures,
					new FontFeatureCollection(new[] {new FontFeature("kern"), new FontFeature("liga")}));
				Assert.Equal(builder._ActivePointSize, 12.0);
				Assert.Equal(builder._ActiveStretch, FontStretch.Expanded);
				Assert.Equal(builder._ActiveStyle, FontStyle.Italic);
				Assert.Equal(builder._Tracking, 1.0);
				Assert.Equal(builder._Spacing, 2.0);
				Assert.Equal(builder._Leading, 3.0);
				Assert.Equal(builder._Indentation, 4.0);
				Assert.Equal(builder._Alignment, Alignment.Center);
			}
#endif
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Paragraph paragraph =
				Create().WithAlignment(Alignment.Center).AddText("para").WithCulture(
					new CultureInfo("en-us")).WithWeight(FontWeight.Bold).AddText("graph").WithPointSize
					(12).AddText("-").WithStyle(FontStyle.Regular).AddText("test").
					WithTracking(5).WithIndentation(1).WithSpacing(3).WithLeading(7).AddInline(
						new Size(5, 5)).Build();

			Assert.Equal(paragraph.Runs.Count, 4);
			Assert.Equal(paragraph.Runs[0].TextRange.StartIndex, 0);
			Assert.Equal(paragraph.Runs[0].TextRange.Length, 4);
			Assert.Equal(paragraph.Runs[1].TextRange.StartIndex, 4);
			Assert.Equal(paragraph.Runs[1].TextRange.Length, 5);
			Assert.Equal(paragraph.Runs[1].Weight, FontWeight.Bold);
			Assert.Equal(paragraph.Runs[2].TextRange.StartIndex, 9);
			Assert.Equal(paragraph.Runs[2].TextRange.Length, 5);
			Assert.Equal(paragraph.Runs[2].Culture, new CultureInfo("en-us"));
			Assert.Equal(paragraph.Text, "paragraph-test\u00A0");
			Assert.Equal(paragraph.Alignment, Alignment.Center);
			Assert.Equal(paragraph.Tracking, 5);
			Assert.Equal(paragraph.Indentation, 1);
			Assert.Equal(paragraph.Spacing, 3);
			Assert.Equal(paragraph.Leading, 7);
		}
#endif
	}
}