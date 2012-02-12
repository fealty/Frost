// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost
{
	// ATTRIBUTION: HSV transformations are derived from code by Michael
	// Jackson: <http://mjijackson.com/2008/02/rgb-to-hsl-and-rgb-to-hsv
	// -color-model-conversion-algorithms-in-javascript>.
	public struct Color : IEquatable<Color>
	{
		/// <summary>
		///   This field contains the color AliceBlue, having the ARGB value of #FFF0F8FF.
		/// </summary>
		public static readonly Color AliceBlue = new Color(
			240,
			248,
			255,
			255);

		/// <summary>
		///   This field contains the color AntiqueWhite, having the ARGB value of #FFFAEBD7.
		/// </summary>
		public static readonly Color AntiqueWhite = new Color(
			250,
			235,
			215,
			255);

		/// <summary>
		///   This field contains the color Aqua, having the ARGB value of #FF00FFFF.
		/// </summary>
		public static readonly Color Aqua = new Color(
			0,
			255,
			255,
			255);

		/// <summary>
		///   This field contains the color Aquamarine, having the ARGB value of #FF7FFFD4.
		/// </summary>
		public static readonly Color Aquamarine = new Color(
			127,
			255,
			212,
			255);

		/// <summary>
		///   This field contains the color Azure, having the ARGB value of #FFF0FFFF.
		/// </summary>
		public static readonly Color Azure = new Color(
			240,
			255,
			255,
			255);

		/// <summary>
		///   This field contains the color Beige, having the ARGB value of #FFF5F5DC.
		/// </summary>
		public static readonly Color Beige = new Color(
			245,
			245,
			220,
			255);

		/// <summary>
		///   This field contains the color Bisque, having the ARGB value of #FFFFE4C4.
		/// </summary>
		public static readonly Color Bisque = new Color(
			255,
			228,
			196,
			255);

		/// <summary>
		///   This field contains the color Black, having the ARGB value of #FF000000.
		/// </summary>
		public static readonly Color Black = new Color(
			0,
			0,
			0,
			255);

		/// <summary>
		///   This field contains the color BlanchedAlmond, having the ARGB value of #FFFFEBCD.
		/// </summary>
		public static readonly Color BlanchedAlmond = new Color(
			255,
			235,
			205,
			255);

		/// <summary>
		///   This field contains the color Blue, having the ARGB value of #FF0000FF.
		/// </summary>
		public static readonly Color Blue = new Color(
			0,
			0,
			255,
			255);

		/// <summary>
		///   This field contains the color BlueViolet, having the ARGB value of #FF8A2BE2.
		/// </summary>
		public static readonly Color BlueViolet = new Color(
			138,
			43,
			226,
			255);

		/// <summary>
		///   This field contains the color Brown, having the ARGB value of #FFA52A2A.
		/// </summary>
		public static readonly Color Brown = new Color(
			165,
			42,
			42,
			255);

		/// <summary>
		///   This field contains the color BurlyWood, having the ARGB value of #FFDEB887.
		/// </summary>
		public static readonly Color BurlyWood = new Color(
			222,
			184,
			135,
			255);

		/// <summary>
		///   This field contains the color CadetBlue, having the ARGB value of #FF5F9EA0.
		/// </summary>
		public static readonly Color CadetBlue = new Color(
			95,
			158,
			160,
			255);

		/// <summary>
		///   This field contains the color Chartreuse, having the ARGB value of #FF7FFF00.
		/// </summary>
		public static readonly Color Chartreuse = new Color(
			127,
			255,
			0,
			255);

		/// <summary>
		///   This field contains the color Chocolate, having the ARGB value of #FFD2691E.
		/// </summary>
		public static readonly Color Chocolate = new Color(
			210,
			105,
			30,
			255);

		/// <summary>
		///   This field contains the color Coral, having the ARGB value of #FFFF7F50.
		/// </summary>
		public static readonly Color Coral = new Color(
			255,
			127,
			80,
			255);

		/// <summary>
		///   This field contains the color CornflowerBlue, having the ARGB value of #FF6495ED.
		/// </summary>
		public static readonly Color CornflowerBlue = new Color(
			100,
			149,
			237,
			255);

		/// <summary>
		///   This field contains the color Cornsilk, having the ARGB value of #FFFFF8DC.
		/// </summary>
		public static readonly Color Cornsilk = new Color(
			255,
			248,
			220,
			255);

		/// <summary>
		///   This field contains the color Crimson, having the ARGB value of #FFDC143C.
		/// </summary>
		public static readonly Color Crimson = new Color(
			220,
			20,
			60,
			255);

		/// <summary>
		///   This field contains the color Cyan, having the ARGB value of #FF00FFFF.
		/// </summary>
		public static readonly Color Cyan = new Color(
			0,
			255,
			255,
			255);

		/// <summary>
		///   This field contains the color DarkBlue, having the ARGB value of #FF00008B.
		/// </summary>
		public static readonly Color DarkBlue = new Color(
			0,
			0,
			139,
			255);

		/// <summary>
		///   This field contains the color DarkCyan, having the ARGB value of #FF008B8B.
		/// </summary>
		public static readonly Color DarkCyan = new Color(
			0,
			139,
			139,
			255);

		/// <summary>
		///   This field contains the color DarkGoldenrod, having the ARGB value of #FFB8860B.
		/// </summary>
		public static readonly Color DarkGoldenrod = new Color(
			184,
			134,
			11,
			255);

		/// <summary>
		///   This field contains the color DarkGray, having the ARGB value of #FFA9A9A9.
		/// </summary>
		public static readonly Color DarkGray = new Color(
			169,
			169,
			169,
			255);

		/// <summary>
		///   This field contains the color DarkGreen, having the ARGB value of #FF006400.
		/// </summary>
		public static readonly Color DarkGreen = new Color(
			0,
			100,
			0,
			255);

		/// <summary>
		///   This field contains the color DarkKhaki, having the ARGB value of #FFBDB76B.
		/// </summary>
		public static readonly Color DarkKhaki = new Color(
			189,
			183,
			107,
			255);

		/// <summary>
		///   This field contains the color DarkMagenta, having the ARGB value of #FF8B008B.
		/// </summary>
		public static readonly Color DarkMagenta = new Color(
			139,
			0,
			139,
			255);

		/// <summary>
		///   This field contains the color DarkOliveGreen, having the ARGB value of #FF556B2F.
		/// </summary>
		public static readonly Color DarkOliveGreen = new Color(
			85,
			107,
			47,
			255);

		/// <summary>
		///   This field contains the color DarkOrange, having the ARGB value of #FFFF8C00.
		/// </summary>
		public static readonly Color DarkOrange = new Color(
			255,
			140,
			0,
			255);

		/// <summary>
		///   This field contains the color DarkOrchid, having the ARGB value of #FF9932CC.
		/// </summary>
		public static readonly Color DarkOrchid = new Color(
			153,
			50,
			204,
			255);

		/// <summary>
		///   This field contains the color DarkRed, having the ARGB value of #FF8B0000.
		/// </summary>
		public static readonly Color DarkRed = new Color(
			139,
			0,
			0,
			255);

		/// <summary>
		///   This field contains the color DarkSalmon, having the ARGB value of #FFE9967A.
		/// </summary>
		public static readonly Color DarkSalmon = new Color(
			233,
			150,
			122,
			255);

		/// <summary>
		///   This field contains the color DarkSeaGreen, having the ARGB value of #FF8FBC8F.
		/// </summary>
		public static readonly Color DarkSeaGreen = new Color(
			143,
			188,
			143,
			255);

		/// <summary>
		///   This field contains the color DarkSlateBlue, having the ARGB value of #FF483D8B.
		/// </summary>
		public static readonly Color DarkSlateBlue = new Color(
			72,
			61,
			139,
			255);

		/// <summary>
		///   This field contains the color DarkSlateGray, having the ARGB value of #FF2F4F4F.
		/// </summary>
		public static readonly Color DarkSlateGray = new Color(
			47,
			79,
			79,
			255);

		/// <summary>
		///   This field contains the color DarkTurquoise, having the ARGB value of #FF00CED1.
		/// </summary>
		public static readonly Color DarkTurquoise = new Color(
			0,
			206,
			209,
			255);

		/// <summary>
		///   This field contains the color DarkViolet, having the ARGB value of #FF9400D3.
		/// </summary>
		public static readonly Color DarkViolet = new Color(
			148,
			0,
			211,
			255);

		/// <summary>
		///   This field contains the color DeepPink, having the ARGB value of #FFFF1493.
		/// </summary>
		public static readonly Color DeepPink = new Color(
			255,
			20,
			147,
			255);

		/// <summary>
		///   This field contains the color DeepSkyBlue, having the ARGB value of #FF00BFFF.
		/// </summary>
		public static readonly Color DeepSkyBlue = new Color(
			0,
			191,
			255,
			255);

		/// <summary>
		///   This field contains the color DimGray, having the ARGB value of #FF696969.
		/// </summary>
		public static readonly Color DimGray = new Color(
			105,
			105,
			105,
			255);

		/// <summary>
		///   This field contains the color DodgerBlue, having the ARGB value of #FF1E90FF.
		/// </summary>
		public static readonly Color DodgerBlue = new Color(
			30,
			144,
			255,
			255);

		/// <summary>
		///   This field contains the color Firebrick, having the ARGB value of #FFB22222.
		/// </summary>
		public static readonly Color Firebrick = new Color(
			178,
			34,
			34,
			255);

		/// <summary>
		///   This field contains the color FloralWhite, having the ARGB value of #FFFFFAF0.
		/// </summary>
		public static readonly Color FloralWhite = new Color(
			255,
			250,
			240,
			255);

		/// <summary>
		///   This field contains the color ForestGreen, having the ARGB value of #FF228B22.
		/// </summary>
		public static readonly Color ForestGreen = new Color(
			34,
			139,
			34,
			255);

		/// <summary>
		///   This field contains the color Fuchsia, having the ARGB value of #FFFF00FF.
		/// </summary>
		public static readonly Color Fuchsia = new Color(
			255,
			0,
			255,
			255);

		/// <summary>
		///   This field contains the color Gainsboro, having the ARGB value of #FFDCDCDC.
		/// </summary>
		public static readonly Color Gainsboro = new Color(
			220,
			220,
			220,
			255);

		/// <summary>
		///   This field contains the color GhostWhite, having the ARGB value of #FFF8F8FF.
		/// </summary>
		public static readonly Color GhostWhite = new Color(
			248,
			248,
			255,
			255);

		/// <summary>
		///   This field contains the color Gold, having the ARGB value of #FFFFD700.
		/// </summary>
		public static readonly Color Gold = new Color(
			255,
			215,
			0,
			255);

		/// <summary>
		///   This field contains the color Goldenrod, having the ARGB value of #FFDAA520.
		/// </summary>
		public static readonly Color Goldenrod = new Color(
			218,
			165,
			32,
			255);

		/// <summary>
		///   This field contains the color Gray, having the ARGB value of #FF808080.
		/// </summary>
		public static readonly Color Gray = new Color(
			128,
			128,
			128,
			255);

		/// <summary>
		///   This field contains the color Green, having the ARGB value of #FF008000.
		/// </summary>
		public static readonly Color Green = new Color(
			0,
			128,
			0,
			255);

		/// <summary>
		///   This field contains the color GreenYellow, having the ARGB value of #FFADFF2F.
		/// </summary>
		public static readonly Color GreenYellow = new Color(
			173,
			255,
			47,
			255);

		/// <summary>
		///   This field contains the color Honeydew, having the ARGB value of #FFF0FFF0.
		/// </summary>
		public static readonly Color Honeydew = new Color(
			240,
			255,
			240,
			255);

		/// <summary>
		///   This field contains the color HotPink, having the ARGB value of #FFFF69B4.
		/// </summary>
		public static readonly Color HotPink = new Color(
			255,
			105,
			180,
			255);

		/// <summary>
		///   This field contains the color IndianRed, having the ARGB value of #FFCD5C5C.
		/// </summary>
		public static readonly Color IndianRed = new Color(
			205,
			92,
			92,
			255);

		/// <summary>
		///   This field contains the color Indigo, having the ARGB value of #FF4B0082.
		/// </summary>
		public static readonly Color Indigo = new Color(
			75,
			0,
			130,
			255);

		/// <summary>
		///   This field contains the color Ivory, having the ARGB value of #FFFFFFF0.
		/// </summary>
		public static readonly Color Ivory = new Color(
			255,
			255,
			240,
			255);

		/// <summary>
		///   This field contains the color Khaki, having the ARGB value of #FFF0E68C.
		/// </summary>
		public static readonly Color Khaki = new Color(
			240,
			230,
			140,
			255);

		/// <summary>
		///   This field contains the color Lavender, having the ARGB value of #FFE6E6FA.
		/// </summary>
		public static readonly Color Lavender = new Color(
			230,
			230,
			250,
			255);

		/// <summary>
		///   This field contains the color LavenderBlush, having the ARGB value of #FFFFF0F5.
		/// </summary>
		public static readonly Color LavenderBlush = new Color(
			255,
			240,
			245,
			255);

		/// <summary>
		///   This field contains the color LawnGreen, having the ARGB value of #FF7CFC00.
		/// </summary>
		public static readonly Color LawnGreen = new Color(
			124,
			252,
			0,
			255);

		/// <summary>
		///   This field contains the color LemonChiffon, having the ARGB value of #FFFFFACD.
		/// </summary>
		public static readonly Color LemonChiffon = new Color(
			255,
			250,
			205,
			255);

		/// <summary>
		///   This field contains the color LightBlue, having the ARGB value of #FFADD8E6.
		/// </summary>
		public static readonly Color LightBlue = new Color(
			173,
			216,
			230,
			255);

		/// <summary>
		///   This field contains the color LightCoral, having the ARGB value of #FFF08080.
		/// </summary>
		public static readonly Color LightCoral = new Color(
			240,
			128,
			128,
			255);

		/// <summary>
		///   This field contains the color LightCyan, having the ARGB value of #FFE0FFFF.
		/// </summary>
		public static readonly Color LightCyan = new Color(
			224,
			255,
			255,
			255);

		/// <summary>
		///   This field contains the color LightGoldenrodYellow, having the ARGB value of #FFFAFAD2.
		/// </summary>
		public static readonly Color LightGoldenrodYellow = new Color(
			250,
			250,
			210,
			255);

		/// <summary>
		///   This field contains the color LightGray, having the ARGB value of #FFD3D3D3.
		/// </summary>
		public static readonly Color LightGray = new Color(
			211,
			211,
			211,
			255);

		/// <summary>
		///   This field contains the color LightGreen, having the ARGB value of #FF90EE90.
		/// </summary>
		public static readonly Color LightGreen = new Color(
			144,
			238,
			144,
			255);

		/// <summary>
		///   This field contains the color LightPink, having the ARGB value of #FFFFB6C1.
		/// </summary>
		public static readonly Color LightPink = new Color(
			255,
			182,
			193,
			255);

		/// <summary>
		///   This field contains the color LightSalmon, having the ARGB value of #FFFFA07A.
		/// </summary>
		public static readonly Color LightSalmon = new Color(
			255,
			160,
			122,
			255);

		/// <summary>
		///   This field contains the color LightSeaGreen, having the ARGB value of #FF20B2AA.
		/// </summary>
		public static readonly Color LightSeaGreen = new Color(
			32,
			178,
			170,
			255);

		/// <summary>
		///   This field contains the color LightSkyBlue, having the ARGB value of #FF87CEFA.
		/// </summary>
		public static readonly Color LightSkyBlue = new Color(
			135,
			206,
			250,
			255);

		/// <summary>
		///   This field contains the color LightSlateGray, having the ARGB value of #FF778899.
		/// </summary>
		public static readonly Color LightSlateGray = new Color(
			119,
			136,
			153,
			255);

		/// <summary>
		///   This field contains the color LightSteelBlue, having the ARGB value of #FFB0C4DE.
		/// </summary>
		public static readonly Color LightSteelBlue = new Color(
			176,
			196,
			222,
			255);

		/// <summary>
		///   This field contains the color LightYellow, having the ARGB value of #FFFFFFE0.
		/// </summary>
		public static readonly Color LightYellow = new Color(
			255,
			255,
			224,
			255);

		/// <summary>
		///   This field contains the color Lime, having the ARGB value of #FF00FF00.
		/// </summary>
		public static readonly Color Lime = new Color(
			0,
			255,
			0,
			255);

		/// <summary>
		///   This field contains the color LimeGreen, having the ARGB value of #FF32CD32.
		/// </summary>
		public static readonly Color LimeGreen = new Color(
			50,
			205,
			50,
			255);

		/// <summary>
		///   This field contains the color Linen, having the ARGB value of #FFFAF0E6.
		/// </summary>
		public static readonly Color Linen = new Color(
			250,
			240,
			230,
			255);

		/// <summary>
		///   This field contains the color Magenta, having the ARGB value of #FFFF00FF.
		/// </summary>
		public static readonly Color Magenta = new Color(
			255,
			0,
			255,
			255);

		/// <summary>
		///   This field contains the color Maroon, having the ARGB value of #FF800000.
		/// </summary>
		public static readonly Color Maroon = new Color(
			128,
			0,
			0,
			255);

		/// <summary>
		///   This field contains the color MediumAquamarine, having the ARGB value of #FF66CDAA.
		/// </summary>
		public static readonly Color MediumAquamarine = new Color(
			102,
			205,
			170,
			255);

		/// <summary>
		///   This field contains the color MediumBlue, having the ARGB value of #FF0000CD.
		/// </summary>
		public static readonly Color MediumBlue = new Color(
			0,
			0,
			205,
			255);

		/// <summary>
		///   This field contains the color MediumOrchid, having the ARGB value of #FFBA55D3.
		/// </summary>
		public static readonly Color MediumOrchid = new Color(
			186,
			85,
			211,
			255);

		/// <summary>
		///   This field contains the color MediumPurple, having the ARGB value of #FF9370DB.
		/// </summary>
		public static readonly Color MediumPurple = new Color(
			147,
			112,
			219,
			255);

		/// <summary>
		///   This field contains the color MediumSeaGreen, having the ARGB value of #FF3CB371.
		/// </summary>
		public static readonly Color MediumSeaGreen = new Color(
			60,
			179,
			113,
			255);

		/// <summary>
		///   This field contains the color MediumSlateBlue, having the ARGB value of #FF7B68EE.
		/// </summary>
		public static readonly Color MediumSlateBlue = new Color(
			123,
			104,
			238,
			255);

		/// <summary>
		///   This field contains the color MediumSpringGreen, having the ARGB value of #FF00FA9A.
		/// </summary>
		public static readonly Color MediumSpringGreen = new Color(
			0,
			250,
			154,
			255);

		/// <summary>
		///   This field contains the color MediumTurquoise, having the ARGB value of #FF48D1CC.
		/// </summary>
		public static readonly Color MediumTurquoise = new Color(
			72,
			209,
			204,
			255);

		/// <summary>
		///   This field contains the color MediumVioletRed, having the ARGB value of #FFC71585.
		/// </summary>
		public static readonly Color MediumVioletRed = new Color(
			199,
			21,
			133,
			255);

		/// <summary>
		///   This field contains the color MidnightBlue, having the ARGB value of #FF191970.
		/// </summary>
		public static readonly Color MidnightBlue = new Color(
			25,
			25,
			112,
			255);

		/// <summary>
		///   This field contains the color MintCream, having the ARGB value of #FFF5FFFA.
		/// </summary>
		public static readonly Color MintCream = new Color(
			245,
			255,
			250,
			255);

		/// <summary>
		///   This field contains the color MistyRose, having the ARGB value of #FFFFE4E1.
		/// </summary>
		public static readonly Color MistyRose = new Color(
			255,
			228,
			225,
			255);

		/// <summary>
		///   This field contains the color Moccasin, having the ARGB value of #FFFFE4B5.
		/// </summary>
		public static readonly Color Moccasin = new Color(
			255,
			228,
			181,
			255);

		/// <summary>
		///   This field contains the color NavajoWhite, having the ARGB value of #FFFFDEAD.
		/// </summary>
		public static readonly Color NavajoWhite = new Color(
			255,
			222,
			173,
			255);

		/// <summary>
		///   This field contains the color Navy, having the ARGB value of #FF000080.
		/// </summary>
		public static readonly Color Navy = new Color(
			0,
			0,
			128,
			255);

		/// <summary>
		///   This field contains the color OldLace, having the ARGB value of #FFFDF5E6.
		/// </summary>
		public static readonly Color OldLace = new Color(
			253,
			245,
			230,
			255);

		/// <summary>
		///   This field contains the color Olive, having the ARGB value of #FF808000.
		/// </summary>
		public static readonly Color Olive = new Color(
			128,
			128,
			0,
			255);

		/// <summary>
		///   This field contains the color OliveDrab, having the ARGB value of #FF6B8E23.
		/// </summary>
		public static readonly Color OliveDrab = new Color(
			107,
			142,
			35,
			255);

		/// <summary>
		///   This field contains the color Orange, having the ARGB value of #FFFFA500.
		/// </summary>
		public static readonly Color Orange = new Color(
			255,
			165,
			0,
			255);

		/// <summary>
		///   This field contains the color OrangeRed, having the ARGB value of #FFFF4500.
		/// </summary>
		public static readonly Color OrangeRed = new Color(
			255,
			69,
			0,
			255);

		/// <summary>
		///   This field contains the color Orchid, having the ARGB value of #FFDA70D6.
		/// </summary>
		public static readonly Color Orchid = new Color(
			218,
			112,
			214,
			255);

		/// <summary>
		///   This field contains the color PaleGoldenrod, having the ARGB value of #FFEEE8AA.
		/// </summary>
		public static readonly Color PaleGoldenrod = new Color(
			238,
			232,
			170,
			255);

		/// <summary>
		///   This field contains the color PaleGreen, having the ARGB value of #FF98FB98.
		/// </summary>
		public static readonly Color PaleGreen = new Color(
			152,
			251,
			152,
			255);

		/// <summary>
		///   This field contains the color PaleTurquoise, having the ARGB value of #FFAFEEEE.
		/// </summary>
		public static readonly Color PaleTurquoise = new Color(
			175,
			238,
			238,
			255);

		/// <summary>
		///   This field contains the color PaleVioletRed, having the ARGB value of #FFDB7093.
		/// </summary>
		public static readonly Color PaleVioletRed = new Color(
			219,
			112,
			147,
			255);

		/// <summary>
		///   This field contains the color PapayaWhip, having the ARGB value of #FFFFEFD5.
		/// </summary>
		public static readonly Color PapayaWhip = new Color(
			255,
			239,
			213,
			255);

		/// <summary>
		///   This field contains the color PeachPuff, having the ARGB value of #FFFFDAB9.
		/// </summary>
		public static readonly Color PeachPuff = new Color(
			255,
			218,
			185,
			255);

		/// <summary>
		///   This field contains the color Peru, having the ARGB value of #FFCD853F.
		/// </summary>
		public static readonly Color Peru = new Color(
			205,
			133,
			63,
			255);

		/// <summary>
		///   This field contains the color Pink, having the ARGB value of #FFFFC0CB.
		/// </summary>
		public static readonly Color Pink = new Color(
			255,
			192,
			203,
			255);

		/// <summary>
		///   This field contains the color Plum, having the ARGB value of #FFDDA0DD.
		/// </summary>
		public static readonly Color Plum = new Color(
			221,
			160,
			221,
			255);

		/// <summary>
		///   This field contains the color PowderBlue, having the ARGB value of #FFB0E0E6.
		/// </summary>
		public static readonly Color PowderBlue = new Color(
			176,
			224,
			230,
			255);

		/// <summary>
		///   This field contains the color Purple, having the ARGB value of #FF800080.
		/// </summary>
		public static readonly Color Purple = new Color(
			128,
			0,
			128,
			255);

		/// <summary>
		///   This field contains the color Red, having the ARGB value of #FFFF0000.
		/// </summary>
		public static readonly Color Red = new Color(
			255,
			0,
			0,
			255);

		/// <summary>
		///   This field contains the color RosyBrown, having the ARGB value of #FFBC8F8F.
		/// </summary>
		public static readonly Color RosyBrown = new Color(
			188,
			143,
			143,
			255);

		/// <summary>
		///   This field contains the color RoyalBlue, having the ARGB value of #FF4169E1.
		/// </summary>
		public static readonly Color RoyalBlue = new Color(
			65,
			105,
			225,
			255);

		/// <summary>
		///   This field contains the color SaddleBrown, having the ARGB value of #FF8B4513.
		/// </summary>
		public static readonly Color SaddleBrown = new Color(
			139,
			69,
			19,
			255);

		/// <summary>
		///   This field contains the color Salmon, having the ARGB value of #FFFA8072.
		/// </summary>
		public static readonly Color Salmon = new Color(
			250,
			128,
			114,
			255);

		/// <summary>
		///   This field contains the color SandyBrown, having the ARGB value of #FFF4A460.
		/// </summary>
		public static readonly Color SandyBrown = new Color(
			244,
			164,
			96,
			255);

		/// <summary>
		///   This field contains the color SeaGreen, having the ARGB value of #FF2E8B57.
		/// </summary>
		public static readonly Color SeaGreen = new Color(
			46,
			139,
			87,
			255);

		/// <summary>
		///   This field contains the color SeaShell, having the ARGB value of #FFFFF5EE.
		/// </summary>
		public static readonly Color SeaShell = new Color(
			255,
			245,
			238,
			255);

		/// <summary>
		///   This field contains the color Sienna, having the ARGB value of #FFA0522D.
		/// </summary>
		public static readonly Color Sienna = new Color(
			160,
			82,
			45,
			255);

		/// <summary>
		///   This field contains the color Silver, having the ARGB value of #FFC0C0C0.
		/// </summary>
		public static readonly Color Silver = new Color(
			192,
			192,
			192,
			255);

		/// <summary>
		///   This field contains the color SkyBlue, having the ARGB value of #FF87CEEB.
		/// </summary>
		public static readonly Color SkyBlue = new Color(
			135,
			206,
			235,
			255);

		/// <summary>
		///   This field contains the color SlateBlue, having the ARGB value of #FF6A5ACD.
		/// </summary>
		public static readonly Color SlateBlue = new Color(
			106,
			90,
			205,
			255);

		/// <summary>
		///   This field contains the color SlateGray, having the ARGB value of #FF708090.
		/// </summary>
		public static readonly Color SlateGray = new Color(
			112,
			128,
			144,
			255);

		/// <summary>
		///   This field contains the color Snow, having the ARGB value of #FFFFFAFA.
		/// </summary>
		public static readonly Color Snow = new Color(
			255,
			250,
			250,
			255);

		/// <summary>
		///   This field contains the color SpringGreen, having the ARGB value of #FF00FF7F.
		/// </summary>
		public static readonly Color SpringGreen = new Color(
			0,
			255,
			127,
			255);

		/// <summary>
		///   This field contains the color SteelBlue, having the ARGB value of #FF4682B4.
		/// </summary>
		public static readonly Color SteelBlue = new Color(
			70,
			130,
			180,
			255);

		/// <summary>
		///   This field contains the color Tan, having the ARGB value of #FFD2B48C.
		/// </summary>
		public static readonly Color Tan = new Color(
			210,
			180,
			140,
			255);

		/// <summary>
		///   This field contains the color Teal, having the ARGB value of #FF008080.
		/// </summary>
		public static readonly Color Teal = new Color(
			0,
			128,
			128,
			255);

		/// <summary>
		///   This field contains the color Thistle, having the ARGB value of #FFD8BFD8.
		/// </summary>
		public static readonly Color Thistle = new Color(
			216,
			191,
			216,
			255);

		/// <summary>
		///   This field contains the color Tomato, having the ARGB value of #FFFF6347.
		/// </summary>
		public static readonly Color Tomato = new Color(
			255,
			99,
			71,
			255);

		/// <summary>
		///   This field contains the color Turquoise, having the ARGB value of #FF40E0D0.
		/// </summary>
		public static readonly Color Turquoise = new Color(
			64,
			224,
			208,
			255);

		/// <summary>
		///   This field contains the color Violet, having the ARGB value of #FFEE82EE.
		/// </summary>
		public static readonly Color Violet = new Color(
			238,
			130,
			238,
			255);

		/// <summary>
		///   This field contains the color Wheat, having the ARGB value of #FFF5DEB3.
		/// </summary>
		public static readonly Color Wheat = new Color(
			245,
			222,
			179,
			255);

		/// <summary>
		///   This field contains the color White, having the ARGB value of #FFFFFFFF.
		/// </summary>
		public static readonly Color White = new Color(
			255,
			255,
			255,
			255);

		/// <summary>
		///   This field contains the color WhiteSmoke, having the ARGB value of #FFF5F5F5.
		/// </summary>
		public static readonly Color WhiteSmoke = new Color(
			245,
			245,
			245,
			255);

		/// <summary>
		///   This field contains the color Yellow, having the ARGB value of #FFFFFF00.
		/// </summary>
		public static readonly Color Yellow = new Color(
			255,
			255,
			0,
			255);

		/// <summary>
		///   This field contains the color YellowGreen, having the ARGB value of #FF9ACD32.
		/// </summary>
		public static readonly Color YellowGreen = new Color(
			154,
			205,
			50,
			255);

		/// <summary>
		///   This field contains the color Transparent, having the ARGB value of #00000000.
		/// </summary>
		public static readonly Color Transparent = new Color(
			0,
			0,
			0,
			0);

		private readonly float _A;
		private readonly float _B;
		private readonly float _G;
		private readonly float _R;

		public Color(
			float rh,
			float gs,
			float bv,
			float aa = 1.0f,
			ColorSpace spectrum = ColorSpace.RGB)
		{
			Trace.Assert(Check.IsNormalized(gs));
			Trace.Assert(Check.IsNormalized(bv));
			Trace.Assert(Check.IsNormalized(aa));

			if(spectrum == ColorSpace.HSV)
			{
				Trace.Assert(Check.IsDegrees(rh));

				ToRGB(
					rh / 360.0f,
					gs,
					bv,
					out this._R,
					out this._G,
					out this._B);

				this._A = aa;
			}
			else
			{
				Trace.Assert(Check.IsNormalized(rh));

				this._R = rh;
				this._G = gs;
				this._B = bv;
				this._A = aa;
			}
		}

		public Color(
			byte r,
			byte g,
			byte b,
			byte a = 255)
		{
			this._R = r / 255.0f;
			this._G = g / 255.0f;
			this._B = b / 255.0f;
			this._A = a / 255.0f;
		}

		public Color(
			byte r,
			byte g,
			byte b,
			float a = 1.0f)
		{
			Trace.Assert(Check.IsNormalized(a));

			this._R = r / 255.0f;
			this._G = g / 255.0f;
			this._B = b / 255.0f;
			this._A = a;
		}

		public float R
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));
				
				return this._R;
			}
		}

		public float G
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));
				
				return this._G;
			}
		}

		public float B
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));
				
				return this._B;
			}
		}

		public float A
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));
				
				return this._A;
			}
		}

		public float H
		{
			get
			{
				Contract.Ensures(Check.IsDegrees(Contract.Result<float>()));

				float result;

				float max = V;
				float min = Math.Min(
					this._R,
					Math.Min(
						this._G,
						this._B));

				if(max.Equals(min))
				{
					return 0.0f;
				}

				float d = max - min;

				if(this._R.Equals(max))
				{
					result = (this._G - this._B) / d + (this._G < this._B ? 6.0f : 0.0f);
				}
				else if(this._G.Equals(max))
				{
					result = (this._B - this._R) / d + 2.0f;
				}
				else
				{
					result = (this._R - this._G) / d + 4.0f;
				}

				return (result / 6.0f) * 360.0f;
			}
		}

		public float S
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));

				float max = V;
				float min = Math.Min(
					this._R,
					Math.Min(
						this._G,
						this._B));

				return max.Equals(0.0) ? 0.0f : (max - min) / max;
			}
		}

		public float V
		{
			get
			{
				Contract.Ensures(Check.IsNormalized(Contract.Result<float>()));

				return Math.Max(
					this._R,
					Math.Max(
						this._G,
						this._B));
			}
		}

		public bool Equals(Color other)
		{
			return other._A.Equals(this._A) && other._B.Equals(this._B) &&
			       other._G.Equals(this._G) && other._R.Equals(this._R);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(
				null,
				obj))
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
				"R: {1}, G: {2}, B: {3}, A: {0}",
				this._R,
				this._G,
				this._B,
				this._A);
		}

		private static void ToRGB(
			float h,
			float s,
			float v,
			out float r,
			out float g,
			out float b)
		{
			int i = (int)Math.Floor(h * 6);

			float f = h * 6 - i;
			float p = v * (1.0f - s);
			float q = v * (1.0f - f * s);
			float t = v * (1.0f - (1.0f - f) * s);

			switch(i % 6)
			{
				case 0:
					r = v;
					g = t;
					b = p;
					break;
				case 1:
					r = q;
					g = v;
					b = p;
					break;
				case 2:
					r = p;
					g = v;
					b = t;
					break;
				case 3:
					r = p;
					g = q;
					b = v;
					break;
				case 4:
					r = t;
					g = p;
					b = v;
					break;
				case 5:
					r = v;
					g = p;
					b = q;
					break;
				default:
					r = 0;
					g = 0;
					b = 0;
					break;
			}
		}

		public static bool operator ==(Color left,
		                               Color right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Color left,
		                               Color right)
		{
			return !left.Equals(right);
		}
	}
}