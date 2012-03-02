﻿// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;

using SharpDX;
using SharpDX.DirectWrite;

using DxFontMetrics = SharpDX.DirectWrite.FontMetrics;
using DxFontFeature = SharpDX.DirectWrite.FontFeature;
using FontFeature = Frost.Formatting.FontFeature;
using FontMetrics = Frost.Formatting.FontMetrics;
using FontStretch = Frost.Formatting.FontStretch;
using FontStyle = Frost.Formatting.FontStyle;
using FontWeight = Frost.Formatting.FontWeight;

namespace Frost.DirectX.Formatting
{
	// DIFFICULT: what about shaping for line breaks and stuff? To be proper,
	// the text needs to be shaped with line breaks taken into account.

	internal sealed class Shaper : IDisposable
	{
		public static readonly Result InsufficientBufferError;

		private readonly List<DxFontFeature> _FeatureList;
		private readonly List<FeatureRange> _FeatureRanges;
		private readonly FontDevice _FontDevice;
		private readonly ShaperSink _OutputSink;
		private readonly TextAnalyzer _TextAnalyzer;

		private short[] _ClusterMap;

		private int[] _FeatureRangeLengths;

		private DxFontFeature[][] _Features;
		private float[] _GlyphAdvances;
		private short[] _GlyphIndices;
		private GlyphOffset[] _GlyphOffsets;

		private int _InternalBufferLengths;
		private bool _IsDisposed;

		private ShapingGlyphProperties[] _ShapedGlyphProperties;
		private ShapingTextProperties[] _ShapedTextProperties;

		static Shaper()
		{
			InsufficientBufferError = new Result(0x8007007A);
		}

		public Shaper(FontDevice fontDevice, ShaperSink outputSink)
		{
			Contract.Requires(fontDevice != null);
			Contract.Requires(outputSink != null);

			_FontDevice = fontDevice;

			_OutputSink = outputSink;

			_Features = new DxFontFeature[0][];

			_FeatureRangeLengths = new int[0];

			_FeatureList = new List<DxFontFeature>();

			_TextAnalyzer = new TextAnalyzer(_FontDevice.Factory);

			_FeatureRanges = new List<FeatureRange>();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Shape(AggregatorSink input)
		{
			Contract.Requires(input != null);

			Contract.Ensures(_OutputSink.Glyphs.Count > 0);
			Contract.Ensures(_OutputSink.Clusters.Count > 0);

			_OutputSink.Reset(input.FullText);

			// shape each run produced from the aggregated characters
			foreach(InternalRun textRun in ProduceRuns(input))
			{
				Contract.Assert(!string.IsNullOrEmpty(textRun.Text));

				ResizeInternalBuffers(textRun.Text.Length);

				Shape(textRun, input);
			}
		}

		private void Dispose(bool disposing)
		{
			if(!_IsDisposed)
			{
				if(disposing)
				{
					_TextAnalyzer.Dispose();
				}

				_IsDisposed = true;
			}
		}

		private void Shape(InternalRun run, AggregatorSink input)
		{
			Contract.Requires(input != null);

			FontHandle fontHandle = _FontDevice.FindFont(run.Family, run.Style, run.Weight, run.Stretch);

			ExtractTextFeatures(run.Features);

			int actualGlyphCount;

			ProduceGlyphs(fontHandle, ref run, out actualGlyphCount);

			PositionGlyphs(fontHandle, ref run, actualGlyphCount);

			_OutputSink.PreallocateGlyphs(actualGlyphCount);

			int glyphsIndex = _OutputSink.Glyphs.Count;

			OutputGlyphs(actualGlyphCount);

			_OutputSink.PreallocateClusters(run.Text.Length);

			OutputClusters(glyphsIndex, ref run, fontHandle, input);
		}

		private IEnumerable<ClusterMapping> ProduceClusters(int textLength)
		{
			Contract.Requires(textLength >= 0);

			int lastIndex = 0;

			for(int i = 0; i < textLength; ++i)
			{
				if(i + 1 == textLength || _ClusterMap[i + 1] != _ClusterMap[lastIndex])
				{
					ClusterMapping mapping;

					int glyphIndex = _ClusterMap[lastIndex];
					int glyphsLength = _ClusterMap[i + 1] - glyphIndex;

					Debug.Assert(glyphIndex >= 0);

					glyphsLength = Math.Max(glyphsLength, 1);

					mapping.Glyphs = new IndexedRange(glyphIndex, glyphsLength);

					int characterIndex = lastIndex;
					int charactersLength = (i + 1) - characterIndex;

					Debug.Assert(characterIndex >= 0);
					Debug.Assert(charactersLength >= 0);

					charactersLength = Math.Max(charactersLength, 1);

					mapping.Characters = new IndexedRange(characterIndex, charactersLength);

					yield return mapping;

					lastIndex = i + 1;
				}
			}
		}

		private void OutputGlyphs(int glyphCount)
		{
			Contract.Requires(glyphCount >= 0);

			for(int i = 0; i < glyphCount; ++i)
			{
				ShapedGlyph shapedGlyph;

				shapedGlyph.Advance = _GlyphAdvances[i];
				shapedGlyph.GlyphProperties = _ShapedGlyphProperties[i];
				shapedGlyph.Index = _GlyphIndices[i];
				shapedGlyph.Offset = _GlyphOffsets[i];

				_OutputSink.Glyphs.Add(shapedGlyph);
			}
		}

		private void OutputClusters(
			int glyphsIndex, ref InternalRun run, FontHandle fontHandle, AggregatorSink input)
		{
			Contract.Requires(glyphsIndex >= 0);
			Contract.Requires(fontHandle != null);
			Contract.Requires(input != null);

			DxFontMetrics metrics = fontHandle.ResolveFace().Metrics;

			// output each cluster specified by the cluster to glyph mappings
			foreach(ClusterMapping mapping in ProduceClusters(run.Text.Length))
			{
				ShapedCluster cluster = new ShapedCluster();

				int glyphIndex = mapping.Glyphs.StartIndex + glyphsIndex;
				int glyphsLength = mapping.Glyphs.Length;

				Debug.Assert(glyphIndex >= 0);
				Debug.Assert(glyphsLength > 0);

				cluster.Glyphs = new IndexedRange(glyphIndex, glyphsLength);

				int characterIndex = mapping.Characters.StartIndex + run.Range.StartIndex;
				int charactersLength = mapping.Characters.Length;

				Debug.Assert(characterIndex >= 0);
				Debug.Assert(charactersLength > 0);

				cluster.Characters = new IndexedRange(characterIndex, charactersLength);

				CharacterFormat character = input.Characters[characterIndex];

				cluster.BidiLevel = run.BidiLevel;
				cluster.PointSize = run.PointSize;

				if(character.Inline.Area.Equals(0.0f))
				{
					// identify clusters representing formatting characters
					switch(CharUnicodeInfo.GetUnicodeCategory(_OutputSink.FullText[cluster.Characters.StartIndex]))
					{
						case UnicodeCategory.Format:
							cluster.ContentType = ContentType.Format;
							break;
						default:
							cluster.ContentType = ContentType.Normal;
							break;
					}

					cluster.Advance = new Size(
						_OutputSink.Glyphs[cluster.Glyphs.StartIndex].Advance, ComputeEmHeight(run.PointSize, ref metrics));
				}
				else
				{
					switch(character.HAlignment)
					{
						case Alignment.Stretch:
							cluster.ContentType = ContentType.Inline;

							cluster.Advance = new Size(character.Inline.Width, character.Inline.Height);
							break;
						default:
							cluster.ContentType = ContentType.Floater;
							cluster.Floater = new Rectangle(
								cluster.Floater.Location, new Size(character.Inline.Width, character.Inline.Height));
							break;
					}
				}

				cluster.Font = fontHandle;
				cluster.HAlignment = character.HAlignment;
				cluster.VAlignment = character.VAlignment;
				cluster.Breakpoint = character.Breakpoint;

				_OutputSink.Clusters.Add(cluster);
			}
		}

		private static float ComputeEmHeight(float pointSize, ref DxFontMetrics dxMetrics)
		{
			Contract.Requires(pointSize >= 0.0 && pointSize <= double.MaxValue);

			FontMetrics metrics = new FontMetrics(
				dxMetrics.Ascent, dxMetrics.Descent, dxMetrics.DesignUnitsPerEm);

			return metrics.Measure(dxMetrics.Ascent + dxMetrics.Descent + dxMetrics.LineGap, pointSize);
		}

		private void PositionGlyphs(FontHandle fontHandle, ref InternalRun run, int actualGlyphCount)
		{
			Contract.Requires(fontHandle != null);
			Contract.Requires(!string.IsNullOrEmpty(run.Text));
			Contract.Requires(run.Culture != null);
			Contract.Requires(actualGlyphCount >= 0);

			DxFontMetrics metrics = fontHandle.ResolveFace().Metrics;

			try
			{
				if(
					_TextAnalyzer.GetGlyphPlacements(
						run.Text,
						_ClusterMap,
						_ShapedTextProperties,
						run.Text.Length,
						_GlyphIndices,
						_ShapedGlyphProperties,
						actualGlyphCount,
						fontHandle.ResolveFace(),
						ComputeEmHeight(run.PointSize, ref metrics),
						false,
						Convert.ToBoolean(run.BidiLevel & 1),
						run.ScriptAnalysis,
						run.Culture.Name,
						_Features,
						_FeatureRangeLengths,
						_GlyphAdvances,
						_GlyphOffsets).Failure)
				{
					throw new ShapingException(null);
				}
			}
			catch(SharpDXException e)
			{
				throw new ShapingException(e);
			}
		}

		private void ProduceGlyphs(FontHandle fontHandle, ref InternalRun run, out int glyphCount)
		{
			Contract.Requires(fontHandle != null);
			Contract.Requires(!string.IsNullOrEmpty(run.Text));
			Contract.Requires(run.Culture != null);
			Contract.Ensures(Contract.ValueAtReturn(out glyphCount) >= 0);

			Result result;

			do
			{
				glyphCount = 0;

				try
				{
					result = _TextAnalyzer.GetGlyphs(
						run.Text,
						run.Text.Length,
						fontHandle.ResolveFace(),
						false,
						Convert.ToBoolean(run.BidiLevel & 1),
						run.ScriptAnalysis,
						run.Culture.Name,
						run.NumberSubstitution,
						_Features,
						_FeatureRangeLengths,
						_InternalBufferLengths,
						_ClusterMap,
						_ShapedTextProperties,
						_GlyphIndices,
						_ShapedGlyphProperties,
						out glyphCount);

					if(result.Failure)
					{
						throw new ShapingException(null);
					}
				}
				catch(SharpDXException e)
				{
					result = e.ResultCode;

					if(result != InsufficientBufferError)
					{
						throw new ShapingException(e);
					}

					ResizeInternalBuffers(_InternalBufferLengths * 2);
				}
			} while(result == InsufficientBufferError);
		}

		private void ResizeInternalBuffers(int textLength)
		{
			Contract.Requires(textLength >= 0);

			int originalTextLength = textLength;

			textLength = (3 * textLength / 2 + 16);

			if(_InternalBufferLengths < textLength)
			{
				_ClusterMap = new short[textLength];
				_GlyphIndices = new short[textLength];

				_ShapedTextProperties = new ShapingTextProperties[textLength];
				_ShapedGlyphProperties = new ShapingGlyphProperties[textLength];

				_GlyphAdvances = new float[textLength];
				_GlyphOffsets = new GlyphOffset[textLength];

				_InternalBufferLengths = textLength;
			}

			_ClusterMap[originalTextLength] = -1;
		}

		private void ExtractTextFeatures(List<FeatureRange> features)
		{
			Contract.Requires(features != null);

			if(_Features.Length != features.Count)
			{
				_Features = new DxFontFeature[features.Count][];

				_FeatureRangeLengths = new int[features.Count];
			}

			for(int i = 0; i < features.Count; ++i)
			{
				_FeatureList.Clear();

				if(features[i].Features != null)
				{
					foreach(FontFeature feature in features[i].Features)
					{
						DxFontFeature newFeature;

						switch(feature.Tag)
						{
							case "afrc":
								newFeature.NameTag = FontFeatureTag.AlternativeFractions;
								break;
							case "calt":
								newFeature.NameTag = FontFeatureTag.ContextualAlternates;
								break;
							case "case":
								newFeature.NameTag = FontFeatureTag.CaseSensitiveForms;
								break;
							case "ccmp":
								newFeature.NameTag = FontFeatureTag.GlyphCompositionDecomposition;
								break;
							case "clig":
								newFeature.NameTag = FontFeatureTag.ContextualLigatures;
								break;
							case "cpsp":
								newFeature.NameTag = FontFeatureTag.CapitalSpacing;
								break;
							case "cswh":
								newFeature.NameTag = FontFeatureTag.ContextualSwash;
								break;
							case "curs":
								newFeature.NameTag = FontFeatureTag.CursivePositioning;
								break;
							case "c2pc":
								newFeature.NameTag = FontFeatureTag.PetiteCapitalsFromCapitals;
								break;
							case "c2sc":
								newFeature.NameTag = FontFeatureTag.SmallCapitalsFromCapitals;
								break;
							case "dlig":
								newFeature.NameTag = FontFeatureTag.DiscretionaryLigatures;
								break;
							case "expt":
								newFeature.NameTag = FontFeatureTag.ExpertForms;
								break;
							case "frac":
								newFeature.NameTag = FontFeatureTag.Fractions;
								break;
							case "fwid":
								newFeature.NameTag = FontFeatureTag.FullWidth;
								break;
							case "half":
								newFeature.NameTag = FontFeatureTag.HalfForms;
								break;
							case "haln":
								newFeature.NameTag = FontFeatureTag.HalantForms;
								break;
							case "halt":
								newFeature.NameTag = FontFeatureTag.AlternateHalfWidth;
								break;
							case "hist":
								newFeature.NameTag = FontFeatureTag.HistoricalForms;
								break;
							case "hkna":
								newFeature.NameTag = FontFeatureTag.HorizontalKanaAlternates;
								break;
							case "hlig":
								newFeature.NameTag = FontFeatureTag.HistoricalLigatures;
								break;
							case "hojo":
								newFeature.NameTag = FontFeatureTag.HojoKanjiForms;
								break;
							case "hwid":
								newFeature.NameTag = FontFeatureTag.HalfWidth;
								break;
							case "jp78":
								newFeature.NameTag = FontFeatureTag.Jis78Forms;
								break;
							case "jp83":
								newFeature.NameTag = FontFeatureTag.Jis83Forms;
								break;
							case "jp90":
								newFeature.NameTag = FontFeatureTag.Jis90Forms;
								break;
							case "jp04":
								newFeature.NameTag = FontFeatureTag.Jis04Forms;
								break;
							case "kern":
								newFeature.NameTag = FontFeatureTag.Kerning;
								break;
							case "liga":
								newFeature.NameTag = FontFeatureTag.StandardLigatures;
								break;
							case "lnum":
								newFeature.NameTag = FontFeatureTag.LiningFigures;
								break;
							case "locl":
								newFeature.NameTag = FontFeatureTag.LocalizedForms;
								break;
							case "mark":
								newFeature.NameTag = FontFeatureTag.MarkPositioning;
								break;
							case "mgrk":
								newFeature.NameTag = FontFeatureTag.MathematicalGreek;
								break;
							case "mkmk":
								newFeature.NameTag = FontFeatureTag.MarkToMarkPositioning;
								break;
							case "nalt":
								newFeature.NameTag = FontFeatureTag.AlternateAnnotationForms;
								break;
							case "nlck":
								newFeature.NameTag = FontFeatureTag.NlcKanjiForms;
								break;
							case "onum":
								newFeature.NameTag = FontFeatureTag.OldStyleFigures;
								break;
							case "ordn":
								newFeature.NameTag = FontFeatureTag.Ordinals;
								break;
							case "palt":
								newFeature.NameTag = FontFeatureTag.ProportionalAlternateWidth;
								break;
							case "pcap":
								newFeature.NameTag = FontFeatureTag.PetiteCapitals;
								break;
							case "pnum":
								newFeature.NameTag = FontFeatureTag.ProportionalFigures;
								break;
							case "pwid":
								newFeature.NameTag = FontFeatureTag.ProportionalWidths;
								break;
							case "qwid":
								newFeature.NameTag = FontFeatureTag.QuarterWidths;
								break;
							case "rlig":
								newFeature.NameTag = FontFeatureTag.RequiredLigatures;
								break;
							case "ruby":
								newFeature.NameTag = FontFeatureTag.RubyNotationForms;
								break;
							case "salt":
								newFeature.NameTag = FontFeatureTag.StylisticAlternates;
								break;
							case "sinf":
								newFeature.NameTag = FontFeatureTag.ScientificInferiors;
								break;
							case "smcp":
								newFeature.NameTag = FontFeatureTag.SmallCapitals;
								break;
							case "smpl":
								newFeature.NameTag = FontFeatureTag.SimplifiedForms;
								break;
							case "ss01":
								newFeature.NameTag = FontFeatureTag.StylisticSet1;
								break;
							case "ss02":
								newFeature.NameTag = FontFeatureTag.StylisticSet2;
								break;
							case "ss03":
								newFeature.NameTag = FontFeatureTag.StylisticSet3;
								break;
							case "ss04":
								newFeature.NameTag = FontFeatureTag.StylisticSet4;
								break;
							case "ss05":
								newFeature.NameTag = FontFeatureTag.StylisticSet5;
								break;
							case "ss06":
								newFeature.NameTag = FontFeatureTag.StylisticSet6;
								break;
							case "ss07":
								newFeature.NameTag = FontFeatureTag.StylisticSet7;
								break;
							case "ss08":
								newFeature.NameTag = FontFeatureTag.StylisticSet8;
								break;
							case "ss09":
								newFeature.NameTag = FontFeatureTag.StylisticSet9;
								break;
							case "ss10":
								newFeature.NameTag = FontFeatureTag.StylisticSet10;
								break;
							case "ss11":
								newFeature.NameTag = FontFeatureTag.StylisticSet11;
								break;
							case "ss12":
								newFeature.NameTag = FontFeatureTag.StylisticSet12;
								break;
							case "ss13":
								newFeature.NameTag = FontFeatureTag.StylisticSet13;
								break;
							case "ss14":
								newFeature.NameTag = FontFeatureTag.StylisticSet14;
								break;
							case "ss15":
								newFeature.NameTag = FontFeatureTag.StylisticSet15;
								break;
							case "ss16":
								newFeature.NameTag = FontFeatureTag.StylisticSet16;
								break;
							case "ss17":
								newFeature.NameTag = FontFeatureTag.StylisticSet17;
								break;
							case "ss18":
								newFeature.NameTag = FontFeatureTag.StylisticSet18;
								break;
							case "ss19":
								newFeature.NameTag = FontFeatureTag.StylisticSet19;
								break;
							case "ss20":
								newFeature.NameTag = FontFeatureTag.StylisticSet20;
								break;
							case "subs":
								newFeature.NameTag = FontFeatureTag.Subscript;
								break;
							case "sups":
								newFeature.NameTag = FontFeatureTag.Superscript;
								break;
							case "swsh":
								newFeature.NameTag = FontFeatureTag.Swash;
								break;
							case "titl":
								newFeature.NameTag = FontFeatureTag.Titling;
								break;
							case "tnam":
								newFeature.NameTag = FontFeatureTag.TraditionalNameForms;
								break;
							case "tnum":
								newFeature.NameTag = FontFeatureTag.TabularFigures;
								break;
							case "trad":
								newFeature.NameTag = FontFeatureTag.TraditionalForms;
								break;
							case "twid":
								newFeature.NameTag = FontFeatureTag.ThirdWidths;
								break;
							case "unic":
								newFeature.NameTag = FontFeatureTag.Unicase;
								break;
							case "zero":
								newFeature.NameTag = FontFeatureTag.SlashedZero;
								break;
							default:
								throw new InvalidOperationException(
									string.Format("Unsupported font tag: {0} - {1}", feature.Tag, feature.Parameter));
						}

						newFeature.Parameter = feature.Parameter;

						_FeatureList.Add(newFeature);
					}
				}
				else
				{
					DxFontFeature newFeature;

					newFeature.NameTag = FontFeatureTag.Kerning;
					newFeature.Parameter = 1;

					_FeatureList.Add(newFeature);
				}

				_Features[i] = _FeatureList.ToArray();

				_FeatureRangeLengths[i] = features[i].Range.Length;
			}
		}

		private IEnumerable<InternalRun> ProduceRuns(AggregatorSink input)
		{
			Contract.Requires(input != null);

			InternalRun activeRun;

			activeRun.Range = IndexedRange.Empty;
			activeRun.Text = null;
			activeRun.Family = input.Characters[0].Family;
			activeRun.PointSize = input.Characters[0].PointSize;
			activeRun.Stretch = input.Characters[0].Stretch;
			activeRun.Style = input.Characters[0].Style;
			activeRun.Weight = input.Characters[0].Weight;
			activeRun.ScriptAnalysis = input.Characters[0].ScriptAnalysis;
			activeRun.BidiLevel = input.Characters[0].BidiLevel;
			activeRun.Culture = input.Characters[0].Culture;
			activeRun.NumberSubstitution = input.Characters[0].NumberSubstitution;
			activeRun.Features = _FeatureRanges;

			FeatureRange activeFeatures = new FeatureRange(IndexedRange.Empty, input.Characters[0].Features);

			for(int i = 0; i < input.FullText.Length; ++i)
			{
				InternalRun currentRun;

				currentRun.Range = activeRun.Range;
				currentRun.Text = null;
				currentRun.Family = input.Characters[i].Family;
				currentRun.PointSize = input.Characters[i].PointSize;
				currentRun.Stretch = input.Characters[i].Stretch;
				currentRun.Style = input.Characters[i].Style;
				currentRun.Weight = input.Characters[i].Weight;
				currentRun.ScriptAnalysis = input.Characters[i].ScriptAnalysis;
				currentRun.BidiLevel = input.Characters[i].BidiLevel;
				currentRun.Culture = input.Characters[i].Culture;
				currentRun.NumberSubstitution = input.Characters[i].NumberSubstitution;
				currentRun.Features = _FeatureRanges;

				FeatureRange currentFeatures = new FeatureRange(
					activeFeatures.Range, input.Characters[i].Features);

				if(currentRun == activeRun)
				{
					IndexedRange range = activeRun.Range;

					activeRun.Range = new IndexedRange(range.StartIndex, range.Length + 1);

					if(currentFeatures == activeFeatures)
					{
						range = activeFeatures.Range;

						activeFeatures = new FeatureRange(
							new IndexedRange(range.StartIndex, range.Length + 1), activeFeatures.Features);
					}
					else
					{
						_FeatureRanges.Add(activeFeatures);

						activeFeatures = currentFeatures;

						activeFeatures = new FeatureRange(new IndexedRange(i, 1), activeFeatures.Features);
					}
				}
				else
				{
					_FeatureRanges.Add(activeFeatures);

					activeRun.Text = input.FullText.Substring(activeRun.Range.StartIndex, activeRun.Range.Length);

					yield return activeRun;

					activeRun.Text = null;

					activeRun = currentRun;
					activeFeatures = currentFeatures;

					activeRun.Range = new IndexedRange(i, 1);

					activeFeatures = new FeatureRange(new IndexedRange(i, 1), activeFeatures.Features);

					_FeatureRanges.Clear();
				}
			}

			_FeatureRanges.Add(activeFeatures);

			activeRun.Text = input.FullText.Substring(activeRun.Range.StartIndex, activeRun.Range.Length);

			yield return activeRun;

			activeRun.Text = null;

			_FeatureRanges.Clear();
		}

		private struct ClusterMapping : IEquatable<ClusterMapping>
		{
			public IndexedRange Characters;
			public IndexedRange Glyphs;

			public bool Equals(ClusterMapping other)
			{
				return other.Characters.Equals(Characters) && other.Glyphs.Equals(Glyphs);
			}

			public override bool Equals(object obj)
			{
				if(ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is ClusterMapping && Equals((ClusterMapping)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (Characters.GetHashCode() * 397) ^ Glyphs.GetHashCode();
				}
			}

			public static bool operator ==(ClusterMapping left, ClusterMapping right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(ClusterMapping left, ClusterMapping right)
			{
				return !left.Equals(right);
			}
		}

		internal struct InternalRun : IEquatable<InternalRun>
		{
			public byte BidiLevel;
			public CultureInfo Culture;
			public string Family;
			public List<FeatureRange> Features;
			public NumberSubstitution NumberSubstitution;
			public float PointSize;
			public IndexedRange Range;
			public ScriptAnalysis ScriptAnalysis;
			public FontStretch Stretch;
			public FontStyle Style;
			public string Text;
			public FontWeight Weight;

			public bool Equals(InternalRun other)
			{
				return other.Range.Equals(Range) && Equals(other.Culture, Culture) &&
				       Equals(other.Family, Family) && other.PointSize.Equals(PointSize) &&
				       Equals(other.NumberSubstitution, NumberSubstitution) && other.BidiLevel == BidiLevel &&
				       other.ScriptAnalysis.Script == ScriptAnalysis.Script &&
				       other.ScriptAnalysis.Shapes == ScriptAnalysis.Shapes && other.Stretch == Stretch &&
				       other.Style == Style && other.Weight == Weight && other.Features == Features &&
				       other.Text == Text;
			}

			public override bool Equals(object obj)
			{
				if(ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is InternalRun && Equals((InternalRun)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int result = Range.GetHashCode();
					result = (result * 397) ^ (Culture != null ? Culture.GetHashCode() : 0);
					result = (result * 397) ^ (Family != null ? Family.GetHashCode() : 0);
					result = (result * 397) ^ PointSize.GetHashCode();
					result = (result * 397) ^ (NumberSubstitution != null ? NumberSubstitution.GetHashCode() : 0);
					result = (result * 397) ^ BidiLevel.GetHashCode();
					result = (result * 397) ^ ScriptAnalysis.GetHashCode();

					int stretch = (int)Stretch;

					result = (result * 397) ^ stretch.GetHashCode();

					int style = (int)Style;

					result = (result * 397) ^ style.GetHashCode();

					int weight = (int)Weight;

					result = (result * 397) ^ weight.GetHashCode();

					result = (result * 397) ^ (Features != null ? Features.GetHashCode() : 0);
					result = (result * 397) ^ (Text != null ? Text.GetHashCode() : 0);
					return result;
				}
			}

			public static bool operator ==(InternalRun left, InternalRun right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(InternalRun left, InternalRun right)
			{
				return !left.Equals(right);
			}
		}
	}
}