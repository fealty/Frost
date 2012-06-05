// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

using Frost.Collections;
using Frost.Formatting;

using SharpDX.DirectWrite;

using FontStretch = Frost.Formatting.FontStretch;
using FontStyle = Frost.Formatting.FontStyle;
using FontWeight = Frost.Formatting.FontWeight;
using LineBreakpoint = Frost.Formatting.LineBreakpoint;

namespace Frost.DirectX
{
	internal sealed class TextTextShaper : Formatting.TextShaper, IDisposable
	{
		private readonly TextAnalyzer _Analyzer;
		private readonly GlyphShaper _Shaper;

		private CharacterFormat[] _Characters;

		private IShapedGlyphs _OutputSink;

		public TextTextShaper(Frost.Device2D device2D, FontDevice fontDevice) : base(device2D)
		{
			Contract.Requires(device2D != null);
			Contract.Requires(fontDevice != null);

			_Characters = new CharacterFormat[0];

			_Analyzer = new TextAnalyzer(fontDevice, this);
			_Shaper = new GlyphShaper(fontDevice, this);
		}

		public CharacterFormat[] Characters
		{
			get { return _Characters; }
		}

		public void Dispose()
		{
			_Analyzer.Dispose();
			_Shaper.Dispose();
		}

		protected override void OnBegin(IShapedGlyphs outputSink)
		{
			if(_Characters.Length < Text.Length)
			{
				_Characters = new CharacterFormat[Text.Length * 2];
			}

			Array.Clear(_Characters, 0, Text.Length);

			for(int i = 0; i < Text.Length; ++i)
			{
				_Characters[i].Family = "Arial";
				_Characters[i].Culture = CultureInfo.InvariantCulture;
			}

			_OutputSink = outputSink;
		}

		protected override void OnSetInline(
			IndexedRange textRange, object inlineObject)
		{
			foreach(int index in textRange)
			{
				_Characters[index].Inline = inlineObject;
			}
		}

		protected override void OnSetWeight(
			IndexedRange textRange, FontWeight weight)
		{
			foreach(int index in textRange)
			{
				_Characters[index].Weight = weight;
			}
		}

		protected override void OnSetStyle(IndexedRange textRange, FontStyle style)
		{
			foreach(int index in textRange)
			{
				_Characters[index].Style = style;
			}
		}

		protected override void OnSetStretch(
			IndexedRange textRange, FontStretch stretch)
		{
			foreach(int index in textRange)
			{
				_Characters[index].Stretch = stretch;
			}
		}

		protected override void OnSetPointSize(
			IndexedRange textRange, float pointSize)
		{
			foreach(int index in textRange)
			{
				_Characters[index].PointSize = pointSize;
			}
		}

		protected override void OnSetBreakpoint(
			IndexedRange textRange, LineBreakpoint breakpoint)
		{
			foreach(int index in textRange)
			{
				_Characters[index].Breakpoint = breakpoint;
			}
		}

		protected override void OnSetBidiLevel(
			IndexedRange textRange, byte resolvedLevel)
		{
			foreach(int index in textRange)
			{
				_Characters[index].BidiLevel = resolvedLevel;
			}
		}

		protected override void OnSetCulture(
			IndexedRange textRange, CultureInfo culture)
		{
			foreach(int index in textRange)
			{
				_Characters[index].Culture = culture;
			}
		}

		protected override void OnSetFamily(IndexedRange textRange, string family)
		{
			foreach(int index in textRange)
			{
				_Characters[index].Family = family;
			}
		}

		protected override void OnSetFeatures(
			IndexedRange textRange, FontFeatureCollection features)
		{
			foreach(int index in textRange)
			{
				_Characters[index].Features = features;
			}
		}

		protected override void OnAnalyzeScripts()
		{
			_Analyzer.Analyze();
		}

		protected override void OnEnd()
		{
			_Shaper.Shape(_OutputSink);
		}

		internal struct CharacterFormat : IEquatable<CharacterFormat>
		{
			public byte BidiLevel;
			public LineBreakpoint Breakpoint;
			public CultureInfo Culture;
			public string Family;
			public FontFeatureCollection Features;
			public object Inline;
			public NumberSubstitution NumberSubstitution;
			public float PointSize;
			public ScriptAnalysis ScriptAnalysis;
			public FontStretch Stretch;
			public FontStyle Style;
			public FontWeight Weight;

			public bool Equals(CharacterFormat other)
			{
				return other.BidiLevel == BidiLevel && other.Breakpoint.Equals(Breakpoint) &&
					Equals(other.Culture, Culture) && Equals(other.Family, Family) &&
						Equals(other.Features, Features) && Equals(other.Inline, Inline) &&
							Equals(other.NumberSubstitution, NumberSubstitution) &&
								other.PointSize.Equals(PointSize) &&
									other.ScriptAnalysis.Equals(ScriptAnalysis) &&
										Equals(other.Stretch, Stretch) && Equals(other.Style, Style) &&
											Equals(other.Weight, Weight);
			}

			public override bool Equals(object obj)
			{
				if(ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is CharacterFormat && Equals((CharacterFormat)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int result = BidiLevel.GetHashCode();
					result = (result * 397) ^ Breakpoint.GetHashCode();
					result = (result * 397) ^ (Culture != null ? Culture.GetHashCode() : 0);
					result = (result * 397) ^ (Family != null ? Family.GetHashCode() : 0);
					result = (result * 397) ^ (Features != null ? Features.GetHashCode() : 0);
					result = (result * 397) ^ (Inline != null ? Inline.GetHashCode() : 0);
					result = (result * 397) ^
						(NumberSubstitution != null ? NumberSubstitution.GetHashCode() : 0);
					result = (result * 397) ^ PointSize.GetHashCode();
					result = (result * 397) ^ ScriptAnalysis.GetHashCode();
					result = (result * 397) ^ Stretch.GetHashCode();
					result = (result * 397) ^ Style.GetHashCode();
					result = (result * 397) ^ Weight.GetHashCode();
					return result;
				}
			}

			public static bool operator ==(CharacterFormat left, CharacterFormat right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(CharacterFormat left, CharacterFormat right)
			{
				return !left.Equals(right);
			}
		}
	}
}