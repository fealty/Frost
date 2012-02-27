// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

using SharpDX;
using SharpDX.DirectWrite;

using DxFontMetrics = SharpDX.DirectWrite.FontMetrics;
using DxFontFeature = SharpDX.DirectWrite.FontFeature;

namespace Frost.DirectX.Formatting
{
	// DIFFICULT: what about shaping for line breaks and stuff? To be proper,
	// the text needs to be shaped with line breaks taken into account.

	internal sealed class Shaper : IDisposable
	{
		public static readonly Result InsufficientBufferError;

		private readonly List<DxFontFeature> mFeatureList;
		private readonly List<FeatureRange> mFeatureRanges;
		private readonly FontDevice mFontDevice;
		private readonly ShaperSink mOutputSink;
		private readonly TextAnalyzer mTextAnalyzer;

		private short[] mClusterMap;

		private int[] mFeatureRangeLengths;

		private DxFontFeature[][] mFeatures;
		private float[] mGlyphAdvances;
		private short[] mGlyphIndices;
		private GlyphOffset[] mGlyphOffsets;

		private int mInternalBufferLengths;
		private bool mIsDisposed;

		private ShapingGlyphProperties[] mShapedGlyphProperties;
		private ShapingTextProperties[] mShapedTextProperties;

		static Shaper()
		{
			InsufficientBufferError = new Result(0x8007007A);
		}

		public Shaper(FontDevice fontDevice, ShaperSink outputSink)
		{
			Contract.Requires(fontDevice != null);
			Contract.Requires(outputSink != null);

			mFontDevice = fontDevice;

			mOutputSink = outputSink;

			mFeatures = new DxFontFeature[0][];

			mFeatureRangeLengths = new int[0];

			mFeatureList = new List<DxFontFeature>();

			mTextAnalyzer = new TextAnalyzer(mFontDevice.Factory);

			mFeatureRanges = new List<FeatureRange>();
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		public void Shape(AggregatorSink input)
		{
			Contract.Requires(input != null);

			Contract.Ensures(mOutputSink.Glyphs.Count > 0);
			Contract.Ensures(mOutputSink.Clusters.Count > 0);

			mOutputSink.Reset(input.FullText);

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
			if(!mIsDisposed)
			{
				if(disposing)
				{
					mTextAnalyzer.Dispose();
				}

				mIsDisposed = true;
			}
		}

		private void Shape(InternalRun run, AggregatorSink input)
		{
			Contract.Requires(input != null);

			FontHandle fontHandle = mFontDevice.FindFont(run.Family, run.Style, run.Weight, run.Stretch);

			ExtractTextFeatures(run.Features);

			int actualGlyphCount;

			ProduceGlyphs(fontHandle, ref run, out actualGlyphCount);

			PositionGlyphs(fontHandle, ref run, actualGlyphCount);

			mOutputSink.PreallocateGlyphs(actualGlyphCount);

			int glyphsIndex = mOutputSink.Glyphs.Count;

			OutputGlyphs(actualGlyphCount);

			mOutputSink.PreallocateClusters(run.Text.Length);

			OutputClusters(glyphsIndex, ref run, fontHandle, input);
		}

		private IEnumerable<ClusterMapping> ProduceClusters(int textLength)
		{
			Contract.Requires(textLength >= 0);

			int lastIndex = 0;

			for(int i = 0; i < textLength; ++i)
			{
				if(i + 1 == textLength || mClusterMap[i + 1] != mClusterMap[lastIndex])
				{
					ClusterMapping mapping;

					int glyphIndex = mClusterMap[lastIndex];
					int glyphsLength = mClusterMap[i + 1] - glyphIndex;

					Debug.Assert(glyphIndex >= 0);

					glyphsLength = Math.Max(glyphsLength, 1);

					mapping.Glyphs = new GlyphRange(glyphIndex, glyphsLength);

					int characterIndex = lastIndex;
					int charactersLength = (i + 1) - characterIndex;

					Debug.Assert(characterIndex >= 0);
					Debug.Assert(charactersLength >= 0);

					charactersLength = Math.Max(charactersLength, 1);

					mapping.Characters = new TextRange(characterIndex, charactersLength);

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

				shapedGlyph.Advance = mGlyphAdvances[i];
				shapedGlyph.GlyphProperties = mShapedGlyphProperties[i];
				shapedGlyph.Index = mGlyphIndices[i];
				shapedGlyph.Offset = mGlyphOffsets[i];

				mOutputSink.Glyphs.Add(shapedGlyph);
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

				int glyphIndex = mapping.Glyphs.Start + glyphsIndex;
				int glyphsLength = mapping.Glyphs.Length;

				Debug.Assert(glyphIndex >= 0);
				Debug.Assert(glyphsLength > 0);

				cluster.Glyphs = new GlyphRange(glyphIndex, glyphsLength);

				int characterIndex = mapping.Characters.Start + run.Range.Start;
				int charactersLength = mapping.Characters.Length;

				Debug.Assert(characterIndex >= 0);
				Debug.Assert(charactersLength > 0);

				cluster.Characters = new TextRange(characterIndex, charactersLength);

				CharacterFormat character = input.Characters[characterIndex];

				cluster.BidiLevel = run.BidiLevel;
				cluster.PointSize = run.PointSize;

				if(character.Inline.IsEmpty)
				{
					// identify clusters representing formatting characters
					switch(CharUnicodeInfo.GetUnicodeCategory(mOutputSink.FullText[cluster.Characters.Start]))
					{
						case UnicodeCategory.Format:
							cluster.ContentType = ContentType.Format;
							break;
						default:
							cluster.ContentType = ContentType.Normal;
							break;
					}

					cluster.Advance.Width = mOutputSink.Glyphs[cluster.Glyphs.Start].Advance;
					cluster.Advance.Height = ComputeEmHeight(run.PointSize, ref metrics);
				}
				else
				{
					switch(character.HAlignment)
					{
						case Alignment.Stretch:
							cluster.ContentType = ContentType.Inline;
							cluster.Advance.Width = character.Inline.Width;
							cluster.Advance.Height = character.Inline.Height;
							break;
						default:
							cluster.ContentType = ContentType.Floater;
							cluster.Floater.Width = character.Inline.Width;
							cluster.Floater.Height = character.Inline.Height;
							break;
					}
				}

				cluster.Font = fontHandle;
				cluster.HAlignment = character.HAlignment;
				cluster.VAlignment = character.VAlignment;
				cluster.Breakpoint = character.Breakpoint;

				mOutputSink.Clusters.Add(cluster);
			}
		}

		private static float ComputeEmHeight(double pointSize, ref DxFontMetrics metrics)
		{
			Contract.Requires(pointSize >= 0.0 && pointSize <= double.MaxValue);

			double height = FontMetrics.ToPixels(
				metrics.Ascent + metrics.Descent + metrics.LineGap, pointSize, metrics.DesignUnitsPerEm);

			return Convert.ToSingle(height);
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
					mTextAnalyzer.GetGlyphPlacements(
						run.Text,
						mClusterMap,
						mShapedTextProperties,
						run.Text.Length,
						mGlyphIndices,
						mShapedGlyphProperties,
						actualGlyphCount,
						fontHandle.ResolveFace(),
						ComputeEmHeight(run.PointSize, ref metrics),
						false,
						Convert.ToBoolean(run.BidiLevel & 1),
						run.ScriptAnalysis,
						run.Culture.Name,
						mFeatures,
						mFeatureRangeLengths,
						mGlyphAdvances,
						mGlyphOffsets).Failure)
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
					result = mTextAnalyzer.GetGlyphs(
						run.Text,
						run.Text.Length,
						fontHandle.ResolveFace(),
						false,
						Convert.ToBoolean(run.BidiLevel & 1),
						run.ScriptAnalysis,
						run.Culture.Name,
						run.NumberSubstitution,
						mFeatures,
						mFeatureRangeLengths,
						mInternalBufferLengths,
						mClusterMap,
						mShapedTextProperties,
						mGlyphIndices,
						mShapedGlyphProperties,
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

					ResizeInternalBuffers(mInternalBufferLengths * 2);
				}
			} while(result == InsufficientBufferError);
		}

		private void ResizeInternalBuffers(int textLength)
		{
			Contract.Requires(textLength >= 0);

			int originalTextLength = textLength;

			textLength = (3 * textLength / 2 + 16);

			if(mInternalBufferLengths < textLength)
			{
				mClusterMap = new short[textLength];
				mGlyphIndices = new short[textLength];

				mShapedTextProperties = new ShapingTextProperties[textLength];
				mShapedGlyphProperties = new ShapingGlyphProperties[textLength];

				mGlyphAdvances = new float[textLength];
				mGlyphOffsets = new GlyphOffset[textLength];

				mInternalBufferLengths = textLength;
			}

			mClusterMap[originalTextLength] = -1;
		}

		private void ExtractTextFeatures(List<FeatureRange> features)
		{
			Contract.Requires(features != null);

			if(mFeatures.Length != features.Count)
			{
				mFeatures = new DxFontFeature[features.Count][];

				mFeatureRangeLengths = new int[features.Count];
			}

			for(int i = 0; i < features.Count; ++i)
			{
				mFeatureList.Clear();

				if(features[i].Features != null)
				{
					foreach(TextFeature feature in features[i].Features)
					{
						DxFontFeature newFeature;

						newFeature.NameTag = (FontFeatureTag)feature.Feature;
						newFeature.Parameter = feature.Parameter;

						mFeatureList.Add(newFeature);
					}
				}
				else
				{
					DxFontFeature newFeature;

					newFeature.NameTag = FontFeatureTag.Kerning;
					newFeature.Parameter = 1;

					mFeatureList.Add(newFeature);
				}

				mFeatures[i] = mFeatureList.ToArray();

				mFeatureRangeLengths[i] = features[i].Range.Length;
			}
		}

		private IEnumerable<InternalRun> ProduceRuns(AggregatorSink input)
		{
			Contract.Requires(input != null);

			InternalRun activeRun;

			activeRun.Range = TextRange.Empty;
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
			activeRun.Features = mFeatureRanges;

			FeatureRange activeFeatures;

			activeFeatures.Range = TextRange.Empty;
			activeFeatures.Features = input.Characters[0].Features;

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
				currentRun.Features = mFeatureRanges;

				FeatureRange currentFeatures;

				currentFeatures.Range = activeFeatures.Range;
				currentFeatures.Features = input.Characters[i].Features;

				if(currentRun == activeRun)
				{
					TextRange range = activeRun.Range;

					activeRun.Range = new TextRange(range.Start, range.Length + 1);

					if(currentFeatures == activeFeatures)
					{
						range = activeFeatures.Range;

						activeFeatures.Range = new TextRange(range.Start, range.Length + 1);
					}
					else
					{
						mFeatureRanges.Add(activeFeatures);

						activeFeatures = currentFeatures;

						activeFeatures.Range = new TextRange(i, 1);
					}
				}
				else
				{
					mFeatureRanges.Add(activeFeatures);

					activeRun.Text = input.FullText.Substring(activeRun.Range.Start, activeRun.Range.Length);

					yield return activeRun;

					activeRun.Text = null;

					activeRun = currentRun;
					activeFeatures = currentFeatures;

					activeRun.Range = new TextRange(i, 1);
					activeFeatures.Range = new TextRange(i, 1);

					mFeatureRanges.Clear();
				}
			}

			mFeatureRanges.Add(activeFeatures);

			activeRun.Text = input.FullText.Substring(activeRun.Range.Start, activeRun.Range.Length);

			yield return activeRun;

			activeRun.Text = null;

			mFeatureRanges.Clear();
		}

		private struct ClusterMapping : IEquatable<ClusterMapping>
		{
			public TextRange Characters;
			public GlyphRange Glyphs;

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
			public double PointSize;
			public TextRange Range;
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
				       other.ScriptAnalysis.Equals(ScriptAnalysis) && other.Stretch == Stretch &&
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
					result = (result * 397) ^ Stretch.GetHashCode();
					result = (result * 397) ^ Style.GetHashCode();
					result = (result * 397) ^ Weight.GetHashCode();
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

		~Shaper()
		{
			Dispose(false);
		}
	}
}