// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using SharpDX.DirectWrite;

namespace Frost.DirectX
{
	internal static class Extensions
	{
		internal static FontWeight ToDirectWrite(this Shaping.FontWeight weight)
		{
			switch(weight)
			{
				case Shaping.FontWeight.Regular:
					return FontWeight.Normal;
				case Shaping.FontWeight.Thin:
					return FontWeight.Thin;
				case Shaping.FontWeight.ExtraLight:
					return FontWeight.ExtraLight;
				case Shaping.FontWeight.Light:
					return FontWeight.Light;
				case Shaping.FontWeight.Medium:
					return FontWeight.Medium;
				case Shaping.FontWeight.SemiBold:
					return FontWeight.SemiBold;
				case Shaping.FontWeight.Bold:
					return FontWeight.Bold;
				case Shaping.FontWeight.ExtraBold:
					return FontWeight.ExtraBold;
				case Shaping.FontWeight.Heavy:
					return FontWeight.Heavy;
				default:
					return FontWeight.Normal;
			}
		}

		internal static FontStyle ToDirectWrite(this Shaping.FontStyle style)
		{
			switch(style)
			{
				case Shaping.FontStyle.Regular:
					return FontStyle.Normal;
				case Shaping.FontStyle.Italic:
					return FontStyle.Italic;
				case Shaping.FontStyle.Oblique:
					return FontStyle.Oblique;
				default:
					return FontStyle.Normal;
			}
		}

		internal static FontStretch ToDirectWrite(this Shaping.FontStretch stretch)
		{
			switch(stretch)
			{
				case Shaping.FontStretch.UltraCondensed:
					return FontStretch.UltraCondensed;
				case Shaping.FontStretch.ExtraCondensed:
					return FontStretch.ExtraCondensed;
				case Shaping.FontStretch.Condensed:
					return FontStretch.Condensed;
				case Shaping.FontStretch.SemiCondensed:
					return FontStretch.SemiCondensed;
				case Shaping.FontStretch.Regular:
					return FontStretch.Normal;
				case Shaping.FontStretch.SemiExpanded:
					return FontStretch.SemiExpanded;
				case Shaping.FontStretch.Expanded:
					return FontStretch.Expanded;
				case Shaping.FontStretch.ExtraExpanded:
					return FontStretch.ExtraExpanded;
				case Shaping.FontStretch.UltraExpanded:
					return FontStretch.UltraExpanded;
				case Shaping.FontStretch.Medium:
					return FontStretch.Medium;
				default:
					return FontStretch.Normal;
			}
		}
	}
}