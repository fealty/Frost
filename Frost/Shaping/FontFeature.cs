// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost.Shaping
{
	public struct FontFeature : IEquatable<FontFeature>
	{
		private readonly string _Tag;
		private readonly int _Parameter;

		public const string AlternativeFractions = "afrc";
		public const string ContextualAlternates = "calt";
		public const string CaseSensitiveForms = "case";
		public const string GlyphCompositionDecomposition = "ccmp";
		public const string ContextualLigatures = "clig";
		public const string CapitalSpacing = "cpsp";
		public const string ContextualSwash = "cswh";
		public const string CursivePositioning = "curs";
		public const string PetiteCapitalsFromCapitals = "c2pc";
		public const string SmallCapitalsFromCapitals = "c2sc";
		public const string DiscretionaryLigatures = "dlig";
		public const string ExpertForms = "expt";
		public const string Fractions = "frac";
		public const string FullWidth = "fwid";
		public const string HalfForms = "half";
		public const string HalantForms = "haln";
		public const string AlternateHalfWidth = "halt";
		public const string HistoricalForms = "hist";
		public const string HorizontalKanaAlternates = "hkna";
		public const string HistoricalLigatures = "hlig";
		public const string HojoKanjiForms = "hojo";
		public const string HalfWidth = "hwid";
		public const string Jis78Forms = "jp78";
		public const string Jis83Forms = "jp83";
		public const string Jis90Forms = "jp90";
		public const string Jis04Forms = "jp04";
		public const string Kerning = "kern";
		public const string StandardLigatures = "liga";
		public const string LiningFigures = "lnum";
		public const string LocalizedForms = "locl";
		public const string MarkPositioning = "mark";
		public const string MathematicalGreek = "mgrk";
		public const string MarkToMarkPositioning = "mkmk";
		public const string AlternateAnnotationForms = "nalt";
		public const string NlcKanjiForms = "nlck";
		public const string OldStyleFigures = "onum";
		public const string Ordinals = "ordn";
		public const string ProportionalAlternateWidth = "palt";
		public const string PetiteCapitals = "pcap";
		public const string ProportionalFigures = "pnum";
		public const string ProportionalWidths = "pwid";
		public const string QuarterWidths = "qwid";
		public const string RequiredLigatures = "rlig";
		public const string RubyNotationForms = "ruby";
		public const string StylisticAlternates = "salt";
		public const string ScientificInferiors = "sinf";
		public const string SmallCapitals = "smcp";
		public const string SimplifiedForms = "smpl";
		public const string StylisticSet1 = "ss01";
		public const string StylisticSet2 = "ss02";
		public const string StylisticSet3 = "ss03";
		public const string StylisticSet4 = "ss04";
		public const string StylisticSet5 = "ss05";
		public const string StylisticSet6 = "ss06";
		public const string StylisticSet7 = "ss07";
		public const string StylisticSet8 = "ss08";
		public const string StylisticSet9 = "ss09";
		public const string StylisticSet10 = "ss10";
		public const string StylisticSet11 = "ss11";
		public const string StylisticSet12 = "ss12";
		public const string StylisticSet13 = "ss13";
		public const string StylisticSet14 = "ss14";
		public const string StylisticSet15 = "ss15";
		public const string StylisticSet16 = "ss16";
		public const string StylisticSet17 = "ss17";
		public const string StylisticSet18 = "ss18";
		public const string StylisticSet19 = "ss19";
		public const string StylisticSet20 = "ss20";
		public const string Subscript = "subs";
		public const string Superscript = "sups";
		public const string Swash = "swsh";
		public const string Titling = "titl";
		public const string TraditionalNameForms = "tnam";
		public const string TabularFigures = "tnum";
		public const string TraditionalForms = "trad";
		public const string ThirdWidths = "twid";
		public const string Unicase = "unic";
		public const string SlashedZero = "zero";

		public FontFeature(string tag, int parameter = 1)
		{
			Contract.Requires(tag != null);

			_Tag = tag;
			_Parameter = parameter;

			Contract.Assert(Tag.Equals(tag));
			Contract.Assert(Parameter.Equals(parameter));
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Tag != null);
		}

		public string Tag
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				Contract.Ensures(Contract.Result<string>().Equals(_Tag));

				return _Tag;
			}
		}

		public int Parameter
		{
			get { return _Parameter; }
		}

		public bool Equals(FontFeature other)
		{
			return Equals(other._Tag, _Tag) && other._Parameter == _Parameter;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is FontFeature && Equals((FontFeature)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_Tag != null ? _Tag.GetHashCode() : 0) * 397) ^ _Parameter;
			}
		}

		public static bool operator ==(FontFeature left, FontFeature right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FontFeature left, FontFeature right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("Tag: {0}, Parameter: {1}", _Tag, _Parameter);
		}
	}
}