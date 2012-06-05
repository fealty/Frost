// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using SharpDX.DirectWrite;

namespace Frost.DirectX
{
	internal static class Extensions
	{
		internal static FontWeight ToDirectWrite(this Formatting.FontWeight weight)
		{
			switch(weight)
			{
				case Formatting.FontWeight.Regular:
					return FontWeight.Normal;
				case Formatting.FontWeight.Thin:
					return FontWeight.Thin;
				case Formatting.FontWeight.ExtraLight:
					return FontWeight.ExtraLight;
				case Formatting.FontWeight.Light:
					return FontWeight.Light;
				case Formatting.FontWeight.Medium:
					return FontWeight.Medium;
				case Formatting.FontWeight.SemiBold:
					return FontWeight.SemiBold;
				case Formatting.FontWeight.Bold:
					return FontWeight.Bold;
				case Formatting.FontWeight.ExtraBold:
					return FontWeight.ExtraBold;
				case Formatting.FontWeight.Heavy:
					return FontWeight.Heavy;
				default:
					return FontWeight.Normal;
			}
		}

		internal static FontStyle ToDirectWrite(this Formatting.FontStyle style)
		{
			switch(style)
			{
				case Formatting.FontStyle.Regular:
					return FontStyle.Normal;
				case Formatting.FontStyle.Italic:
					return FontStyle.Italic;
				case Formatting.FontStyle.Oblique:
					return FontStyle.Oblique;
				default:
					return FontStyle.Normal;
			}
		}

		internal static FontStretch ToDirectWrite(this Formatting.FontStretch stretch)
		{
			switch(stretch)
			{
				case Formatting.FontStretch.UltraCondensed:
					return FontStretch.UltraCondensed;
				case Formatting.FontStretch.ExtraCondensed:
					return FontStretch.ExtraCondensed;
				case Formatting.FontStretch.Condensed:
					return FontStretch.Condensed;
				case Formatting.FontStretch.SemiCondensed:
					return FontStretch.SemiCondensed;
				case Formatting.FontStretch.Regular:
					return FontStretch.Normal;
				case Formatting.FontStretch.SemiExpanded:
					return FontStretch.SemiExpanded;
				case Formatting.FontStretch.Expanded:
					return FontStretch.Expanded;
				case Formatting.FontStretch.ExtraExpanded:
					return FontStretch.ExtraExpanded;
				case Formatting.FontStretch.UltraExpanded:
					return FontStretch.UltraExpanded;
				case Formatting.FontStretch.Medium:
					return FontStretch.Medium;
				default:
					return FontStretch.Normal;
			}
		}
	}
}