// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost
{
	public struct Color : IEquatable<Color>
	{
		/// <summary>
		///   This field contains the color AliceBlue, having the ARGB value of #FFF0F8FF.
		/// </summary>
		public static readonly Color AliceBlue = new Color(0.9411765f, 0.972549f, 1f);

		/// <summary>
		///   This field contains the color AntiqueWhite, having the ARGB value of #FFFAEBD7.
		/// </summary>
		public static readonly Color AntiqueWhite = new Color(
			0.9803922f, 0.9215686f, 0.8431373f);

		/// <summary>
		///   This field contains the color Aqua, having the ARGB value of #FF00FFFF.
		/// </summary>
		public static readonly Color Aqua = new Color(0f, 1f, 1f);

		/// <summary>
		///   This field contains the color Aquamarine, having the ARGB value of #FF7FFFD4.
		/// </summary>
		public static readonly Color Aquamarine = new Color(
			0.4980392f, 1f, 0.8313726f);

		/// <summary>
		///   This field contains the color Azure, having the ARGB value of #FFF0FFFF.
		/// </summary>
		public static readonly Color Azure = new Color(0.9411765f, 1f, 1f);

		/// <summary>
		///   This field contains the color Beige, having the ARGB value of #FFF5F5DC.
		/// </summary>
		public static readonly Color Beige = new Color(
			0.9607843f, 0.9607843f, 0.8627451f);

		/// <summary>
		///   This field contains the color Bisque, having the ARGB value of #FFFFE4C4.
		/// </summary>
		public static readonly Color Bisque = new Color(1f, 0.8941177f, 0.7686275f);

		/// <summary>
		///   This field contains the color Black, having the ARGB value of #FF000000.
		/// </summary>
		public static readonly Color Black = new Color(0f, 0f, 0f);

		/// <summary>
		///   This field contains the color BlanchedAlmond, having the ARGB value of #FFFFEBCD.
		/// </summary>
		public static readonly Color BlanchedAlmond = new Color(
			1f, 0.9215686f, 0.8039216f);

		/// <summary>
		///   This field contains the color Blue, having the ARGB value of #FF0000FF.
		/// </summary>
		public static readonly Color Blue = new Color(0f, 0f, 1f);

		/// <summary>
		///   This field contains the color BlueViolet, having the ARGB value of #FF8A2BE2.
		/// </summary>
		public static readonly Color BlueViolet = new Color(
			0.5411765f, 0.1686275f, 0.8862745f);

		/// <summary>
		///   This field contains the color Brown, having the ARGB value of #FFA52A2A.
		/// </summary>
		public static readonly Color Brown = new Color(
			0.6470588f, 0.1647059f, 0.1647059f);

		/// <summary>
		///   This field contains the color BurlyWood, having the ARGB value of #FFDEB887.
		/// </summary>
		public static readonly Color BurlyWood = new Color(
			0.8705882f, 0.7215686f, 0.5294118f);

		/// <summary>
		///   This field contains the color CadetBlue, having the ARGB value of #FF5F9EA0.
		/// </summary>
		public static readonly Color CadetBlue = new Color(
			0.372549f, 0.6196079f, 0.627451f);

		/// <summary>
		///   This field contains the color Chartreuse, having the ARGB value of #FF7FFF00.
		/// </summary>
		public static readonly Color Chartreuse = new Color(0.4980392f, 1f, 0f);

		/// <summary>
		///   This field contains the color Chocolate, having the ARGB value of #FFD2691E.
		/// </summary>
		public static readonly Color Chocolate = new Color(
			0.8235294f, 0.4117647f, 0.1176471f);

		/// <summary>
		///   This field contains the color Coral, having the ARGB value of #FFFF7F50.
		/// </summary>
		public static readonly Color Coral = new Color(1f, 0.4980392f, 0.3137255f);

		/// <summary>
		///   This field contains the color CornflowerBlue, having the ARGB value of #FF6495ED.
		/// </summary>
		public static readonly Color CornflowerBlue = new Color(
			0.3921569f, 0.5843138f, 0.9294118f);

		/// <summary>
		///   This field contains the color Cornsilk, having the ARGB value of #FFFFF8DC.
		/// </summary>
		public static readonly Color Cornsilk = new Color(1f, 0.972549f, 0.8627451f);

		/// <summary>
		///   This field contains the color Crimson, having the ARGB value of #FFDC143C.
		/// </summary>
		public static readonly Color Crimson = new Color(
			0.8627451f, 0.07843138f, 0.2352941f);

		/// <summary>
		///   This field contains the color Cyan, having the ARGB value of #FF00FFFF.
		/// </summary>
		public static readonly Color Cyan = new Color(0f, 1f, 1f);

		/// <summary>
		///   This field contains the color DarkBlue, having the ARGB value of #FF00008B.
		/// </summary>
		public static readonly Color DarkBlue = new Color(0f, 0f, 0.5450981f);

		/// <summary>
		///   This field contains the color DarkCyan, having the ARGB value of #FF008B8B.
		/// </summary>
		public static readonly Color DarkCyan = new Color(0f, 0.5450981f, 0.5450981f);

		/// <summary>
		///   This field contains the color DarkGoldenrod, having the ARGB value of #FFB8860B.
		/// </summary>
		public static readonly Color DarkGoldenrod = new Color(
			0.7215686f, 0.5254902f, 0.04313726f);

		/// <summary>
		///   This field contains the color DarkGray, having the ARGB value of #FFA9A9A9.
		/// </summary>
		public static readonly Color DarkGray = new Color(
			0.6627451f, 0.6627451f, 0.6627451f);

		/// <summary>
		///   This field contains the color DarkGreen, having the ARGB value of #FF006400.
		/// </summary>
		public static readonly Color DarkGreen = new Color(0f, 0.3921569f, 0f);

		/// <summary>
		///   This field contains the color DarkKhaki, having the ARGB value of #FFBDB76B.
		/// </summary>
		public static readonly Color DarkKhaki = new Color(
			0.7411765f, 0.7176471f, 0.4196078f);

		/// <summary>
		///   This field contains the color DarkMagenta, having the ARGB value of #FF8B008B.
		/// </summary>
		public static readonly Color DarkMagenta = new Color(
			0.5450981f, 0f, 0.5450981f);

		/// <summary>
		///   This field contains the color DarkOliveGreen, having the ARGB value of #FF556B2F.
		/// </summary>
		public static readonly Color DarkOliveGreen = new Color(
			0.3333333f, 0.4196078f, 0.1843137f);

		/// <summary>
		///   This field contains the color DarkOrange, having the ARGB value of #FFFF8C00.
		/// </summary>
		public static readonly Color DarkOrange = new Color(1f, 0.5490196f, 0f);

		/// <summary>
		///   This field contains the color DarkOrchid, having the ARGB value of #FF9932CC.
		/// </summary>
		public static readonly Color DarkOrchid = new Color(0.6f, 0.1960784f, 0.8f);

		/// <summary>
		///   This field contains the color DarkRed, having the ARGB value of #FF8B0000.
		/// </summary>
		public static readonly Color DarkRed = new Color(0.5450981f, 0f, 0f);

		/// <summary>
		///   This field contains the color DarkSalmon, having the ARGB value of #FFE9967A.
		/// </summary>
		public static readonly Color DarkSalmon = new Color(
			0.9137255f, 0.5882353f, 0.4784314f);

		/// <summary>
		///   This field contains the color DarkSeaGreen, having the ARGB value of #FF8FBC8F.
		/// </summary>
		public static readonly Color DarkSeaGreen = new Color(
			0.5607843f, 0.7372549f, 0.5607843f);

		/// <summary>
		///   This field contains the color DarkSlateBlue, having the ARGB value of #FF483D8B.
		/// </summary>
		public static readonly Color DarkSlateBlue = new Color(
			0.282353f, 0.2392157f, 0.5450981f);

		/// <summary>
		///   This field contains the color DarkSlateGray, having the ARGB value of #FF2F4F4F.
		/// </summary>
		public static readonly Color DarkSlateGray = new Color(
			0.1843137f, 0.3098039f, 0.3098039f);

		/// <summary>
		///   This field contains the color DarkTurquoise, having the ARGB value of #FF00CED1.
		/// </summary>
		public static readonly Color DarkTurquoise = new Color(
			0f, 0.8078431f, 0.8196079f);

		/// <summary>
		///   This field contains the color DarkViolet, having the ARGB value of #FF9400D3.
		/// </summary>
		public static readonly Color DarkViolet = new Color(0.5803922f, 0f, 0.827451f);

		/// <summary>
		///   This field contains the color DeepPink, having the ARGB value of #FFFF1493.
		/// </summary>
		public static readonly Color DeepPink = new Color(1f, 0.07843138f, 0.5764706f);

		/// <summary>
		///   This field contains the color DeepSkyBlue, having the ARGB value of #FF00BFFF.
		/// </summary>
		public static readonly Color DeepSkyBlue = new Color(0f, 0.7490196f, 1f);

		/// <summary>
		///   This field contains the color DimGray, having the ARGB value of #FF696969.
		/// </summary>
		public static readonly Color DimGray = new Color(
			0.4117647f, 0.4117647f, 0.4117647f);

		/// <summary>
		///   This field contains the color DodgerBlue, having the ARGB value of #FF1E90FF.
		/// </summary>
		public static readonly Color DodgerBlue = new Color(
			0.1176471f, 0.5647059f, 1f);

		/// <summary>
		///   This field contains the color Firebrick, having the ARGB value of #FFB22222.
		/// </summary>
		public static readonly Color Firebrick = new Color(
			0.6980392f, 0.1333333f, 0.1333333f);

		/// <summary>
		///   This field contains the color FloralWhite, having the ARGB value of #FFFFFAF0.
		/// </summary>
		public static readonly Color FloralWhite = new Color(
			1f, 0.9803922f, 0.9411765f);

		/// <summary>
		///   This field contains the color ForestGreen, having the ARGB value of #FF228B22.
		/// </summary>
		public static readonly Color ForestGreen = new Color(
			0.1333333f, 0.5450981f, 0.1333333f);

		/// <summary>
		///   This field contains the color Fuchsia, having the ARGB value of #FFFF00FF.
		/// </summary>
		public static readonly Color Fuchsia = new Color(1f, 0f, 1f);

		/// <summary>
		///   This field contains the color Gainsboro, having the ARGB value of #FFDCDCDC.
		/// </summary>
		public static readonly Color Gainsboro = new Color(
			0.8627451f, 0.8627451f, 0.8627451f);

		/// <summary>
		///   This field contains the color GhostWhite, having the ARGB value of #FFF8F8FF.
		/// </summary>
		public static readonly Color GhostWhite = new Color(0.972549f, 0.972549f, 1f);

		/// <summary>
		///   This field contains the color Gold, having the ARGB value of #FFFFD700.
		/// </summary>
		public static readonly Color Gold = new Color(1f, 0.8431373f, 0f);

		/// <summary>
		///   This field contains the color Goldenrod, having the ARGB value of #FFDAA520.
		/// </summary>
		public static readonly Color Goldenrod = new Color(
			0.854902f, 0.6470588f, 0.1254902f);

		/// <summary>
		///   This field contains the color Gray, having the ARGB value of #FF808080.
		/// </summary>
		public static readonly Color Gray = new Color(
			0.5019608f, 0.5019608f, 0.5019608f);

		/// <summary>
		///   This field contains the color Green, having the ARGB value of #FF008000.
		/// </summary>
		public static readonly Color Green = new Color(0f, 0.5019608f, 0f);

		/// <summary>
		///   This field contains the color GreenYellow, having the ARGB value of #FFADFF2F.
		/// </summary>
		public static readonly Color GreenYellow = new Color(
			0.6784314f, 1f, 0.1843137f);

		/// <summary>
		///   This field contains the color Honeydew, having the ARGB value of #FFF0FFF0.
		/// </summary>
		public static readonly Color Honeydew = new Color(0.9411765f, 1f, 0.9411765f);

		/// <summary>
		///   This field contains the color HotPink, having the ARGB value of #FFFF69B4.
		/// </summary>
		public static readonly Color HotPink = new Color(1f, 0.4117647f, 0.7058824f);

		/// <summary>
		///   This field contains the color IndianRed, having the ARGB value of #FFCD5C5C.
		/// </summary>
		public static readonly Color IndianRed = new Color(
			0.8039216f, 0.3607843f, 0.3607843f);

		/// <summary>
		///   This field contains the color Indigo, having the ARGB value of #FF4B0082.
		/// </summary>
		public static readonly Color Indigo = new Color(0.2941177f, 0f, 0.509804f);

		/// <summary>
		///   This field contains the color Ivory, having the ARGB value of #FFFFFFF0.
		/// </summary>
		public static readonly Color Ivory = new Color(1f, 1f, 0.9411765f);

		/// <summary>
		///   This field contains the color Khaki, having the ARGB value of #FFF0E68C.
		/// </summary>
		public static readonly Color Khaki = new Color(
			0.9411765f, 0.9019608f, 0.5490196f);

		/// <summary>
		///   This field contains the color Lavender, having the ARGB value of #FFE6E6FA.
		/// </summary>
		public static readonly Color Lavender = new Color(
			0.9019608f, 0.9019608f, 0.9803922f);

		/// <summary>
		///   This field contains the color LavenderBlush, having the ARGB value of #FFFFF0F5.
		/// </summary>
		public static readonly Color LavenderBlush = new Color(
			1f, 0.9411765f, 0.9607843f);

		/// <summary>
		///   This field contains the color LawnGreen, having the ARGB value of #FF7CFC00.
		/// </summary>
		public static readonly Color LawnGreen = new Color(0.4862745f, 0.9882353f, 0f);

		/// <summary>
		///   This field contains the color LemonChiffon, having the ARGB value of #FFFFFACD.
		/// </summary>
		public static readonly Color LemonChiffon = new Color(
			1f, 0.9803922f, 0.8039216f);

		/// <summary>
		///   This field contains the color LightBlue, having the ARGB value of #FFADD8E6.
		/// </summary>
		public static readonly Color LightBlue = new Color(
			0.6784314f, 0.8470588f, 0.9019608f);

		/// <summary>
		///   This field contains the color LightCoral, having the ARGB value of #FFF08080.
		/// </summary>
		public static readonly Color LightCoral = new Color(
			0.9411765f, 0.5019608f, 0.5019608f);

		/// <summary>
		///   This field contains the color LightCyan, having the ARGB value of #FFE0FFFF.
		/// </summary>
		public static readonly Color LightCyan = new Color(0.8784314f, 1f, 1f);

		/// <summary>
		///   This field contains the color LightGoldenrodYellow, having the ARGB value of #FFFAFAD2.
		/// </summary>
		public static readonly Color LightGoldenrodYellow = new Color(
			0.9803922f, 0.9803922f, 0.8235294f);

		/// <summary>
		///   This field contains the color LightGray, having the ARGB value of #FFD3D3D3.
		/// </summary>
		public static readonly Color LightGray = new Color(
			0.827451f, 0.827451f, 0.827451f);

		/// <summary>
		///   This field contains the color LightGreen, having the ARGB value of #FF90EE90.
		/// </summary>
		public static readonly Color LightGreen = new Color(
			0.5647059f, 0.9333333f, 0.5647059f);

		/// <summary>
		///   This field contains the color LightPink, having the ARGB value of #FFFFB6C1.
		/// </summary>
		public static readonly Color LightPink = new Color(1f, 0.7137255f, 0.7568628f);

		/// <summary>
		///   This field contains the color LightSalmon, having the ARGB value of #FFFFA07A.
		/// </summary>
		public static readonly Color LightSalmon = new Color(
			1f, 0.627451f, 0.4784314f);

		/// <summary>
		///   This field contains the color LightSeaGreen, having the ARGB value of #FF20B2AA.
		/// </summary>
		public static readonly Color LightSeaGreen = new Color(
			0.1254902f, 0.6980392f, 0.6666667f);

		/// <summary>
		///   This field contains the color LightSkyBlue, having the ARGB value of #FF87CEFA.
		/// </summary>
		public static readonly Color LightSkyBlue = new Color(
			0.5294118f, 0.8078431f, 0.9803922f);

		/// <summary>
		///   This field contains the color LightSlateGray, having the ARGB value of #FF778899.
		/// </summary>
		public static readonly Color LightSlateGray = new Color(
			0.4666667f, 0.5333334f, 0.6f);

		/// <summary>
		///   This field contains the color LightSteelBlue, having the ARGB value of #FFB0C4DE.
		/// </summary>
		public static readonly Color LightSteelBlue = new Color(
			0.6901961f, 0.7686275f, 0.8705882f);

		/// <summary>
		///   This field contains the color LightYellow, having the ARGB value of #FFFFFFE0.
		/// </summary>
		public static readonly Color LightYellow = new Color(1f, 1f, 0.8784314f);

		/// <summary>
		///   This field contains the color Lime, having the ARGB value of #FF00FF00.
		/// </summary>
		public static readonly Color Lime = new Color(0f, 1f, 0f);

		/// <summary>
		///   This field contains the color LimeGreen, having the ARGB value of #FF32CD32.
		/// </summary>
		public static readonly Color LimeGreen = new Color(
			0.1960784f, 0.8039216f, 0.1960784f);

		/// <summary>
		///   This field contains the color Linen, having the ARGB value of #FFFAF0E6.
		/// </summary>
		public static readonly Color Linen = new Color(
			0.9803922f, 0.9411765f, 0.9019608f);

		/// <summary>
		///   This field contains the color Magenta, having the ARGB value of #FFFF00FF.
		/// </summary>
		public static readonly Color Magenta = new Color(1f, 0f, 1f);

		/// <summary>
		///   This field contains the color Maroon, having the ARGB value of #FF800000.
		/// </summary>
		public static readonly Color Maroon = new Color(0.5019608f, 0f, 0f);

		/// <summary>
		///   This field contains the color MediumAquamarine, having the ARGB value of #FF66CDAA.
		/// </summary>
		public static readonly Color MediumAquamarine = new Color(
			0.4f, 0.8039216f, 0.6666667f);

		/// <summary>
		///   This field contains the color MediumBlue, having the ARGB value of #FF0000CD.
		/// </summary>
		public static readonly Color MediumBlue = new Color(0f, 0f, 0.8039216f);

		/// <summary>
		///   This field contains the color MediumOrchid, having the ARGB value of #FFBA55D3.
		/// </summary>
		public static readonly Color MediumOrchid = new Color(
			0.7294118f, 0.3333333f, 0.827451f);

		/// <summary>
		///   This field contains the color MediumPurple, having the ARGB value of #FF9370DB.
		/// </summary>
		public static readonly Color MediumPurple = new Color(
			0.5764706f, 0.4392157f, 0.8588235f);

		/// <summary>
		///   This field contains the color MediumSeaGreen, having the ARGB value of #FF3CB371.
		/// </summary>
		public static readonly Color MediumSeaGreen = new Color(
			0.2352941f, 0.7019608f, 0.4431373f);

		/// <summary>
		///   This field contains the color MediumSlateBlue, having the ARGB value of #FF7B68EE.
		/// </summary>
		public static readonly Color MediumSlateBlue = new Color(
			0.4823529f, 0.4078431f, 0.9333333f);

		/// <summary>
		///   This field contains the color MediumSpringGreen, having the ARGB value of #FF00FA9A.
		/// </summary>
		public static readonly Color MediumSpringGreen = new Color(
			0f, 0.9803922f, 0.6039216f);

		/// <summary>
		///   This field contains the color MediumTurquoise, having the ARGB value of #FF48D1CC.
		/// </summary>
		public static readonly Color MediumTurquoise = new Color(
			0.282353f, 0.8196079f, 0.8f);

		/// <summary>
		///   This field contains the color MediumVioletRed, having the ARGB value of #FFC71585.
		/// </summary>
		public static readonly Color MediumVioletRed = new Color(
			0.7803922f, 0.08235294f, 0.5215687f);

		/// <summary>
		///   This field contains the color MidnightBlue, having the ARGB value of #FF191970.
		/// </summary>
		public static readonly Color MidnightBlue = new Color(
			0.09803922f, 0.09803922f, 0.4392157f);

		/// <summary>
		///   This field contains the color MintCream, having the ARGB value of #FFF5FFFA.
		/// </summary>
		public static readonly Color MintCream = new Color(0.9607843f, 1f, 0.9803922f);

		/// <summary>
		///   This field contains the color MistyRose, having the ARGB value of #FFFFE4E1.
		/// </summary>
		public static readonly Color MistyRose = new Color(1f, 0.8941177f, 0.8823529f);

		/// <summary>
		///   This field contains the color Moccasin, having the ARGB value of #FFFFE4B5.
		/// </summary>
		public static readonly Color Moccasin = new Color(1f, 0.8941177f, 0.7098039f);

		/// <summary>
		///   This field contains the color NavajoWhite, having the ARGB value of #FFFFDEAD.
		/// </summary>
		public static readonly Color NavajoWhite = new Color(
			1f, 0.8705882f, 0.6784314f);

		/// <summary>
		///   This field contains the color Navy, having the ARGB value of #FF000080.
		/// </summary>
		public static readonly Color Navy = new Color(0f, 0f, 0.5019608f);

		/// <summary>
		///   This field contains the color OldLace, having the ARGB value of #FFFDF5E6.
		/// </summary>
		public static readonly Color OldLace = new Color(
			0.9921569f, 0.9607843f, 0.9019608f);

		/// <summary>
		///   This field contains the color Olive, having the ARGB value of #FF808000.
		/// </summary>
		public static readonly Color Olive = new Color(0.5019608f, 0.5019608f, 0f);

		/// <summary>
		///   This field contains the color OliveDrab, having the ARGB value of #FF6B8E23.
		/// </summary>
		public static readonly Color OliveDrab = new Color(
			0.4196078f, 0.5568628f, 0.1372549f);

		/// <summary>
		///   This field contains the color Orange, having the ARGB value of #FFFFA500.
		/// </summary>
		public static readonly Color Orange = new Color(1f, 0.6470588f, 0f);

		/// <summary>
		///   This field contains the color OrangeRed, having the ARGB value of #FFFF4500.
		/// </summary>
		public static readonly Color OrangeRed = new Color(1f, 0.2705882f, 0f);

		/// <summary>
		///   This field contains the color Orchid, having the ARGB value of #FFDA70D6.
		/// </summary>
		public static readonly Color Orchid = new Color(
			0.854902f, 0.4392157f, 0.8392157f);

		/// <summary>
		///   This field contains the color PaleGoldenrod, having the ARGB value of #FFEEE8AA.
		/// </summary>
		public static readonly Color PaleGoldenrod = new Color(
			0.9333333f, 0.9098039f, 0.6666667f);

		/// <summary>
		///   This field contains the color PaleGreen, having the ARGB value of #FF98FB98.
		/// </summary>
		public static readonly Color PaleGreen = new Color(
			0.5960785f, 0.9843137f, 0.5960785f);

		/// <summary>
		///   This field contains the color PaleTurquoise, having the ARGB value of #FFAFEEEE.
		/// </summary>
		public static readonly Color PaleTurquoise = new Color(
			0.6862745f, 0.9333333f, 0.9333333f);

		/// <summary>
		///   This field contains the color PaleVioletRed, having the ARGB value of #FFDB7093.
		/// </summary>
		public static readonly Color PaleVioletRed = new Color(
			0.8588235f, 0.4392157f, 0.5764706f);

		/// <summary>
		///   This field contains the color PapayaWhip, having the ARGB value of #FFFFEFD5.
		/// </summary>
		public static readonly Color PapayaWhip = new Color(
			1f, 0.9372549f, 0.8352941f);

		/// <summary>
		///   This field contains the color PeachPuff, having the ARGB value of #FFFFDAB9.
		/// </summary>
		public static readonly Color PeachPuff = new Color(1f, 0.854902f, 0.7254902f);

		/// <summary>
		///   This field contains the color Peru, having the ARGB value of #FFCD853F.
		/// </summary>
		public static readonly Color Peru = new Color(
			0.8039216f, 0.5215687f, 0.2470588f);

		/// <summary>
		///   This field contains the color Pink, having the ARGB value of #FFFFC0CB.
		/// </summary>
		public static readonly Color Pink = new Color(1f, 0.7529412f, 0.7960784f);

		/// <summary>
		///   This field contains the color Plum, having the ARGB value of #FFDDA0DD.
		/// </summary>
		public static readonly Color Plum = new Color(
			0.8666667f, 0.627451f, 0.8666667f);

		/// <summary>
		///   This field contains the color PowderBlue, having the ARGB value of #FFB0E0E6.
		/// </summary>
		public static readonly Color PowderBlue = new Color(
			0.6901961f, 0.8784314f, 0.9019608f);

		/// <summary>
		///   This field contains the color Purple, having the ARGB value of #FF800080.
		/// </summary>
		public static readonly Color Purple = new Color(0.5019608f, 0f, 0.5019608f);

		/// <summary>
		///   This field contains the color Red, having the ARGB value of #FFFF0000.
		/// </summary>
		public static readonly Color Red = new Color(1f, 0f, 0f);

		/// <summary>
		///   This field contains the color RosyBrown, having the ARGB value of #FFBC8F8F.
		/// </summary>
		public static readonly Color RosyBrown = new Color(
			0.7372549f, 0.5607843f, 0.5607843f);

		/// <summary>
		///   This field contains the color RoyalBlue, having the ARGB value of #FF4169E1.
		/// </summary>
		public static readonly Color RoyalBlue = new Color(
			0.254902f, 0.4117647f, 0.8823529f);

		/// <summary>
		///   This field contains the color SaddleBrown, having the ARGB value of #FF8B4513.
		/// </summary>
		public static readonly Color SaddleBrown = new Color(
			0.5450981f, 0.2705882f, 0.07450981f);

		/// <summary>
		///   This field contains the color Salmon, having the ARGB value of #FFFA8072.
		/// </summary>
		public static readonly Color Salmon = new Color(
			0.9803922f, 0.5019608f, 0.4470588f);

		/// <summary>
		///   This field contains the color SandyBrown, having the ARGB value of #FFF4A460.
		/// </summary>
		public static readonly Color SandyBrown = new Color(
			0.9568627f, 0.6431373f, 0.3764706f);

		/// <summary>
		///   This field contains the color SeaGreen, having the ARGB value of #FF2E8B57.
		/// </summary>
		public static readonly Color SeaGreen = new Color(
			0.1803922f, 0.5450981f, 0.3411765f);

		/// <summary>
		///   This field contains the color SeaShell, having the ARGB value of #FFFFF5EE.
		/// </summary>
		public static readonly Color SeaShell = new Color(1f, 0.9607843f, 0.9333333f);

		/// <summary>
		///   This field contains the color Sienna, having the ARGB value of #FFA0522D.
		/// </summary>
		public static readonly Color Sienna = new Color(
			0.627451f, 0.3215686f, 0.1764706f);

		/// <summary>
		///   This field contains the color Silver, having the ARGB value of #FFC0C0C0.
		/// </summary>
		public static readonly Color Silver = new Color(
			0.7529412f, 0.7529412f, 0.7529412f);

		/// <summary>
		///   This field contains the color SkyBlue, having the ARGB value of #FF87CEEB.
		/// </summary>
		public static readonly Color SkyBlue = new Color(
			0.5294118f, 0.8078431f, 0.9215686f);

		/// <summary>
		///   This field contains the color SlateBlue, having the ARGB value of #FF6A5ACD.
		/// </summary>
		public static readonly Color SlateBlue = new Color(
			0.4156863f, 0.3529412f, 0.8039216f);

		/// <summary>
		///   This field contains the color SlateGray, having the ARGB value of #FF708090.
		/// </summary>
		public static readonly Color SlateGray = new Color(
			0.4392157f, 0.5019608f, 0.5647059f);

		/// <summary>
		///   This field contains the color Snow, having the ARGB value of #FFFFFAFA.
		/// </summary>
		public static readonly Color Snow = new Color(1f, 0.9803922f, 0.9803922f);

		/// <summary>
		///   This field contains the color SpringGreen, having the ARGB value of #FF00FF7F.
		/// </summary>
		public static readonly Color SpringGreen = new Color(0f, 1f, 0.4980392f);

		/// <summary>
		///   This field contains the color SteelBlue, having the ARGB value of #FF4682B4.
		/// </summary>
		public static readonly Color SteelBlue = new Color(
			0.2745098f, 0.509804f, 0.7058824f);

		/// <summary>
		///   This field contains the color Tan, having the ARGB value of #FFD2B48C.
		/// </summary>
		public static readonly Color Tan = new Color(
			0.8235294f, 0.7058824f, 0.5490196f);

		/// <summary>
		///   This field contains the color Teal, having the ARGB value of #FF008080.
		/// </summary>
		public static readonly Color Teal = new Color(0f, 0.5019608f, 0.5019608f);

		/// <summary>
		///   This field contains the color Thistle, having the ARGB value of #FFD8BFD8.
		/// </summary>
		public static readonly Color Thistle = new Color(
			0.8470588f, 0.7490196f, 0.8470588f);

		/// <summary>
		///   This field contains the color Tomato, having the ARGB value of #FFFF6347.
		/// </summary>
		public static readonly Color Tomato = new Color(1f, 0.3882353f, 0.2784314f);

		/// <summary>
		///   This field contains the color Turquoise, having the ARGB value of #FF40E0D0.
		/// </summary>
		public static readonly Color Turquoise = new Color(
			0.2509804f, 0.8784314f, 0.8156863f);

		/// <summary>
		///   This field contains the color Violet, having the ARGB value of #FFEE82EE.
		/// </summary>
		public static readonly Color Violet = new Color(
			0.9333333f, 0.509804f, 0.9333333f);

		/// <summary>
		///   This field contains the color Wheat, having the ARGB value of #FFF5DEB3.
		/// </summary>
		public static readonly Color Wheat = new Color(
			0.9607843f, 0.8705882f, 0.7019608f);

		/// <summary>
		///   This field contains the color White, having the ARGB value of #FFFFFFFF.
		/// </summary>
		public static readonly Color White = new Color(1f, 1f, 1f);

		/// <summary>
		///   This field contains the color WhiteSmoke, having the ARGB value of #FFF5F5F5.
		/// </summary>
		public static readonly Color WhiteSmoke = new Color(
			0.9607843f, 0.9607843f, 0.9607843f);

		/// <summary>
		///   This field contains the color Yellow, having the ARGB value of #FFFFFF00.
		/// </summary>
		public static readonly Color Yellow = new Color(1f, 1f, 0f);

		/// <summary>
		///   This field contains the color YellowGreen, having the ARGB value of #FF9ACD32.
		/// </summary>
		public static readonly Color YellowGreen = new Color(
			0.6039216f, 0.8039216f, 0.1960784f);

		/// <summary>
		///   This field contains the color Transparent, having the ARGB value of #00000000.
		/// </summary>
		public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);

		private readonly float _A;
		private readonly float _B;
		private readonly float _G;
		private readonly float _R;

		public Color(float red, float green, float blue, float alpha = 1.0f)
		{
			Trace.Assert(Check.IsNormalized(green));
			Trace.Assert(Check.IsNormalized(blue));
			Trace.Assert(Check.IsNormalized(alpha));
			Trace.Assert(Check.IsNormalized(red));

			this._R = red;
			this._G = green;
			this._B = blue;
			this._A = alpha;

			Contract.Assert(R.Equals(red));
			Contract.Assert(G.Equals(green));
			Contract.Assert(B.Equals(blue));
			Contract.Assert(A.Equals(alpha));
		}

		public float R
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._R));

				return this._R;
			}
		}

		public float G
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._G));

				return this._G;
			}
		}

		public float B
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._B));

				return this._B;
			}
		}

		public float A
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._A));

				return this._A;
			}
		}

		public bool Equals(Color other)
		{
			return other._A.Equals(this._A) && other._B.Equals(this._B) &&
			       other._G.Equals(this._G) && other._R.Equals(this._R);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Color && Equals((Color)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = this._A.GetHashCode();
				result = (result * 397) ^ this._B.GetHashCode();
				result = (result * 397) ^ this._G.GetHashCode();
				result = (result * 397) ^ this._R.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"R: {1}, G: {2}, B: {3}, A: {0}", this._R, this._G, this._B, this._A);
		}

		public static implicit operator Color(RGBColor color)
		{
			return color.ToColor();
		}

		public static implicit operator Color(HSVColor color)
		{
			return color.ToColor();
		}

		public static bool operator ==(Color left, Color right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Color left, Color right)
		{
			return !left.Equals(right);
		}

		#if(UNIT_TESTING)

		[Fact] internal void Test0()
		{
			Assert.Equal(0, new Color(0, 0.25f, 0.5f).R);
			Assert.Equal(0.25f, new Color(0, 0.25f, 0.5f).G);
			Assert.Equal(0.5f, new Color(0, 0.25f, 0.5f).B);
			Assert.Equal(1, new Color(0, 0.25f, 0.5f).A);

			Assert.TestObject(Red, Blue);

			Assert.Equal<Color>(new Color(1, 0, 0), new HSVColor(000, 100, 100));
			Assert.Equal<Color>(new Color(1, 1, 0), new HSVColor(060, 100, 100));
			Assert.Equal<Color>(new Color(0, 1, 0), new HSVColor(120, 100, 100));
			Assert.Equal<Color>(new Color(0, 1, 1), new HSVColor(180, 100, 100));
			Assert.Equal<Color>(new Color(0, 0, 1), new HSVColor(240, 100, 100));
			Assert.Equal<Color>(new Color(1, 0, 1), new HSVColor(360, 100, 100));
			Assert.Equal<Color>(new Color(1, 1, 1), new HSVColor(180, 000, 100));

			Assert.Equal<Color>(new Color(0, 0, 0), new RGBColor(000, 000, 000));
			Assert.Equal<Color>(new Color(1, 1, 1), new RGBColor(255, 255, 255));
		}

		#endif
	}
}