using Frost.Formatting;

namespace Frost.DirectX.Formatting
{
	internal static class Extensions
	{
		internal static SharpDX.DirectWrite.FontWeight ToDirectWrite(
			this FontWeight weight)
		{
			switch(weight)
			{
				case FontWeight.Regular:
					return SharpDX.DirectWrite.FontWeight.Normal;
				case FontWeight.Thin:
					return SharpDX.DirectWrite.FontWeight.Thin;
				case FontWeight.ExtraLight:
					return SharpDX.DirectWrite.FontWeight.ExtraLight;
				case FontWeight.Light:
					return SharpDX.DirectWrite.FontWeight.Light;
				case FontWeight.Medium:
					return SharpDX.DirectWrite.FontWeight.Medium;
				case FontWeight.SemiBold:
					return SharpDX.DirectWrite.FontWeight.SemiBold;
				case FontWeight.Bold:
					return SharpDX.DirectWrite.FontWeight.Bold;
				case FontWeight.ExtraBold:
					return SharpDX.DirectWrite.FontWeight.ExtraBold;
				case FontWeight.Heavy:
					return SharpDX.DirectWrite.FontWeight.Heavy;
				default:
					return SharpDX.DirectWrite.FontWeight.Normal;
			}
		}

		internal static SharpDX.DirectWrite.FontStyle ToDirectWrite(
			this FontStyle style)
		{
			switch(style)
			{
				case FontStyle.Regular:
					return SharpDX.DirectWrite.FontStyle.Normal;
				case FontStyle.Italic:
					return SharpDX.DirectWrite.FontStyle.Italic;
				case FontStyle.Oblique:
					return SharpDX.DirectWrite.FontStyle.Oblique;
				default:
					return SharpDX.DirectWrite.FontStyle.Normal;
			}
		}

		internal static SharpDX.DirectWrite.FontStretch ToDirectWrite(
			this FontStretch stretch)
		{
			switch(stretch)
			{
				case FontStretch.UltraCondensed:
					return SharpDX.DirectWrite.FontStretch.UltraCondensed;
				case FontStretch.ExtraCondensed:
					return SharpDX.DirectWrite.FontStretch.ExtraCondensed;
				case FontStretch.Condensed:
					return SharpDX.DirectWrite.FontStretch.Condensed;
				case FontStretch.SemiCondensed:
					return SharpDX.DirectWrite.FontStretch.SemiCondensed;
				case FontStretch.Regular:
					return SharpDX.DirectWrite.FontStretch.Normal;
				case FontStretch.SemiExpanded:
					return SharpDX.DirectWrite.FontStretch.SemiExpanded;
				case FontStretch.Expanded:
					return SharpDX.DirectWrite.FontStretch.Expanded;
				case FontStretch.ExtraExpanded:
					return SharpDX.DirectWrite.FontStretch.ExtraExpanded;
				case FontStretch.UltraExpanded:
					return SharpDX.DirectWrite.FontStretch.UltraExpanded;
				case FontStretch.Medium:
					return SharpDX.DirectWrite.FontStretch.Medium;
				default:
					return SharpDX.DirectWrite.FontStretch.Normal;
			}
		}
	}
}