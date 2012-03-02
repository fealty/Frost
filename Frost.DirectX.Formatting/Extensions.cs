// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using SharpDX.DirectWrite;

namespace Frost.DirectX.Formatting
{
	/// <summary>
	///   This class provides convenient extension methods.
	/// </summary>
	internal static class Extensions
	{
		/// <summary>
		///   This method converts Frost's <see cref="Frost.Formatting.FontWeight" /> to a DirectWrite representation.
		/// </summary>
		/// <param name="weight"> This parameter contains the font weight to convert. </param>
		/// <returns> This method returns the converted font weight. </returns>
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

		/// <summary>
		///   This method converts Frost's <see cref="Frost.Formatting.FontStyle" /> to a DirectWrite representation.
		/// </summary>
		/// <param name="style"> This parameter contains the font style to convert. </param>
		/// <returns> This method returns the converted font style. </returns>
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

		/// <summary>
		///   This method converts Frost's <see cref="Frost.Formatting.FontStretch" /> to a DirectWrite representation.
		/// </summary>
		/// <param name="stretch"> This parameter contains the font stretch to convert. </param>
		/// <returns> This method returns the converted font stretch. </returns>
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