// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	internal static class Extensions
	{
		internal static FontWeight ToDirectWrite(this Frost.Formatting.FontWeight weight)
		{
			switch(weight)
			{
				case Frost.Formatting.FontWeight.Regular:
					return FontWeight.Normal;
				case Frost.Formatting.FontWeight.Thin:
					return FontWeight.Thin;
				case Frost.Formatting.FontWeight.ExtraLight:
					return FontWeight.ExtraLight;
				case Frost.Formatting.FontWeight.Light:
					return FontWeight.Light;
				case Frost.Formatting.FontWeight.Medium:
					return FontWeight.Medium;
				case Frost.Formatting.FontWeight.SemiBold:
					return FontWeight.SemiBold;
				case Frost.Formatting.FontWeight.Bold:
					return FontWeight.Bold;
				case Frost.Formatting.FontWeight.ExtraBold:
					return FontWeight.ExtraBold;
				case Frost.Formatting.FontWeight.Heavy:
					return FontWeight.Heavy;
				default:
					return FontWeight.Normal;
			}
		}

		internal static FontStyle ToDirectWrite(this Frost.Formatting.FontStyle style)
		{
			switch(style)
			{
				case Frost.Formatting.FontStyle.Regular:
					return FontStyle.Normal;
				case Frost.Formatting.FontStyle.Italic:
					return FontStyle.Italic;
				case Frost.Formatting.FontStyle.Oblique:
					return FontStyle.Oblique;
				default:
					return FontStyle.Normal;
			}
		}

		internal static FontStretch ToDirectWrite(this Frost.Formatting.FontStretch stretch)
		{
			switch(stretch)
			{
				case Frost.Formatting.FontStretch.UltraCondensed:
					return FontStretch.UltraCondensed;
				case Frost.Formatting.FontStretch.ExtraCondensed:
					return FontStretch.ExtraCondensed;
				case Frost.Formatting.FontStretch.Condensed:
					return FontStretch.Condensed;
				case Frost.Formatting.FontStretch.SemiCondensed:
					return FontStretch.SemiCondensed;
				case Frost.Formatting.FontStretch.Regular:
					return FontStretch.Normal;
				case Frost.Formatting.FontStretch.SemiExpanded:
					return FontStretch.SemiExpanded;
				case Frost.Formatting.FontStretch.Expanded:
					return FontStretch.Expanded;
				case Frost.Formatting.FontStretch.ExtraExpanded:
					return FontStretch.ExtraExpanded;
				case Frost.Formatting.FontStretch.UltraExpanded:
					return FontStretch.UltraExpanded;
				case Frost.Formatting.FontStretch.Medium:
					return FontStretch.Medium;
				default:
					return FontStretch.Normal;
			}
		}
	}
}