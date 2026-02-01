/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class ParseUtils
	{
		private const char Alpha_Sep = '@';

		#region Video size table
		private class VideoSizeAbbr
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public VideoSizeAbbr(string abbr, c_int width, c_int height)
			{
				Abbr = abbr;
				Width = width;
				Height = height;
			}



			/********************************************************************/
			/// <summary>
			/// A string representing the name of the size
			/// </summary>
			/********************************************************************/
			public string Abbr
			{
				get;
			}



			/********************************************************************/
			/// <summary>
			/// The width of the size
			/// </summary>
			/********************************************************************/
			public c_int Width
			{
				get;
			}



			/********************************************************************/
			/// <summary>
			/// The height of the size
			/// </summary>
			/********************************************************************/
			public c_int Height
			{
				get;
			}
		}

		private static readonly VideoSizeAbbr[] video_Size_Abbrs =
		[
			new VideoSizeAbbr("ntsc", 720, 480),
			new VideoSizeAbbr("pal", 720, 576),
			new VideoSizeAbbr("qntsc", 352, 240),	// VCD compliant NTSC
			new VideoSizeAbbr("qpal", 352, 288),		// VCD compliant PAL
			new VideoSizeAbbr("sntsc", 640, 480),	// Square pixel NTSC
			new VideoSizeAbbr("spal", 768, 576),		// Square pixel PAL
			new VideoSizeAbbr("film", 352, 240),
			new VideoSizeAbbr("ntsc-film", 352, 240),
			new VideoSizeAbbr("sqcif", 128,  96),
			new VideoSizeAbbr("qcif", 176, 144),
			new VideoSizeAbbr("cif", 352, 288),
			new VideoSizeAbbr("4cif", 704, 576),
			new VideoSizeAbbr("16cif", 1408,1152),
			new VideoSizeAbbr("qqvga", 160, 120),
			new VideoSizeAbbr("qvga", 320, 240),
			new VideoSizeAbbr("vga", 640, 480),
			new VideoSizeAbbr("svga", 800, 600),
			new VideoSizeAbbr("xga", 1024, 768),
			new VideoSizeAbbr("uxga", 1600,1200),
			new VideoSizeAbbr("qxga", 2048,1536),
			new VideoSizeAbbr("sxga", 1280,1024),
			new VideoSizeAbbr("qsxga", 2560,2048),
			new VideoSizeAbbr("hsxga", 5120,4096),
			new VideoSizeAbbr("wvga", 852, 480),
			new VideoSizeAbbr("wxga", 1366, 768),
			new VideoSizeAbbr("wsxga", 1600,1024),
			new VideoSizeAbbr("wuxga", 1920,1200),
			new VideoSizeAbbr("woxga", 2560,1600),
			new VideoSizeAbbr("wqhd", 2560,1440),
			new VideoSizeAbbr("wqsxga", 3200,2048),
			new VideoSizeAbbr("wquxga", 3840,2400),
			new VideoSizeAbbr("whsxga", 6400,4096),
			new VideoSizeAbbr("whuxga", 7680,4800),
			new VideoSizeAbbr("cga", 320, 200),
			new VideoSizeAbbr("ega", 640, 350),
			new VideoSizeAbbr("hd480", 852, 480),
			new VideoSizeAbbr("hd720", 1280, 720),
			new VideoSizeAbbr("hd1080", 1920, 1080),
			new VideoSizeAbbr("quadhd", 2560,1440),
			new VideoSizeAbbr("2k", 2048, 1080),		// Digital Cinema System Specification
			new VideoSizeAbbr("2kdci", 2048, 1080),
			new VideoSizeAbbr("2kflat", 1998, 1080),
			new VideoSizeAbbr("2kscope", 2048, 858),
			new VideoSizeAbbr("4k", 4096,2160),		// Digital Cinema System Specification
			new VideoSizeAbbr("4kdci", 4096,2160),
			new VideoSizeAbbr("4kflat", 3996,2160),
			new VideoSizeAbbr("4kscope", 4096,1716),
			new VideoSizeAbbr("nhd", 640,360  ),
			new VideoSizeAbbr("hqvga", 240,160  ),
			new VideoSizeAbbr("wqvga", 400,240  ),
			new VideoSizeAbbr("fwqvga", 432,240  ),
			new VideoSizeAbbr("hvga", 480,320  ),
			new VideoSizeAbbr("qhd", 960,540  ),
			new VideoSizeAbbr("uhd2160", 3840,2160 ),
			new VideoSizeAbbr("uhd4320", 7680,4320 ),
		];
		#endregion

		#region Video rate table
		private class VideoRateAbbr
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public VideoRateAbbr(string abbr, AvRational rate)
			{
				Abbr = abbr;
				Rate = rate;
			}



			/********************************************************************/
			/// <summary>
			/// A string representing the name of the size
			/// </summary>
			/********************************************************************/
			public string Abbr
			{
				get;
			}



			/********************************************************************/
			/// <summary>
			/// The rate
			/// </summary>
			/********************************************************************/
			public AvRational Rate
			{
				get;
			}
		}

		private static readonly VideoRateAbbr[] video_Rate_Abbrs =
		[
			new VideoRateAbbr("ntsc", new AvRational(30000, 1001)),
			new VideoRateAbbr("pal", new AvRational(25, 1)),
			new VideoRateAbbr("qntsc", new AvRational(30000, 1001)),		// VCD compliant NTSC
			new VideoRateAbbr("qpal", new AvRational(25, 1)),			// VCD compliant PAL
			new VideoRateAbbr("sntsc", new AvRational(30000, 1001)),		// Square pixel NTSC
			new VideoRateAbbr("spal", new AvRational(25, 1)),			// Square pixel PAL
			new VideoRateAbbr("film", new AvRational(24, 1)),
			new VideoRateAbbr("ntsc-film", new AvRational(24000, 1001))
		];
		#endregion

		#region Color table
		private class ColorEntry
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public ColorEntry(string name, uint8_t[] rgb_Color)
			{
				Name = name;
				Rgb_Color = rgb_Color;
			}



			/********************************************************************/
			/// <summary>
			/// A string representing the name of the color
			/// </summary>
			/********************************************************************/
			public string Name
			{
				get;
			}



			/********************************************************************/
			/// <summary>
			/// RGB values for the color
			/// </summary>
			/********************************************************************/
			public uint8_t[] Rgb_Color
			{
				get;
			}
		}

		private static readonly ColorEntry[] color_Table =
		[
			new ColorEntry("AliceBlue", [ 0xf0, 0xf8, 0xff ]),
			new ColorEntry("AntiqueWhite", [ 0xfa, 0xeb, 0xd7 ]),
			new ColorEntry("Aqua", [ 0x00, 0xff, 0xff ]),
			new ColorEntry("Aquamarine", [ 0x7f, 0xff, 0xd4 ]),
			new ColorEntry("Azure", [ 0xf0, 0xff, 0xff ]),
			new ColorEntry("Beige", [ 0xf5, 0xf5, 0xdc ]),
			new ColorEntry("Bisque", [ 0xff, 0xe4, 0xc4 ]),
			new ColorEntry("Black", [ 0x00, 0x00, 0x00 ]),
			new ColorEntry("BlanchedAlmond", [ 0xff, 0xeb, 0xcd ]),
			new ColorEntry("Blue", [ 0x00, 0x00, 0xff ]),
			new ColorEntry("BlueViolet", [ 0x8a, 0x2b, 0xe2 ]),
			new ColorEntry("Brown", [ 0xa5, 0x2a, 0x2a ]),
			new ColorEntry("BurlyWood", [ 0xde, 0xb8, 0x87 ]),
			new ColorEntry("CadetBlue", [ 0x5f, 0x9e, 0xa0 ]),
			new ColorEntry("Chartreuse", [ 0x7f, 0xff, 0x00 ]),
			new ColorEntry("Chocolate", [ 0xd2, 0x69, 0x1e ]),
			new ColorEntry("Coral", [ 0xff, 0x7f, 0x50 ]),
			new ColorEntry("CornflowerBlue", [ 0x64, 0x95, 0xed ]),
			new ColorEntry("Cornsilk", [ 0xff, 0xf8, 0xdc ]),
			new ColorEntry("Crimson", [ 0xdc, 0x14, 0x3c ]),
			new ColorEntry("Cyan", [ 0x00, 0xff, 0xff ]),
			new ColorEntry("DarkBlue", [ 0x00, 0x00, 0x8b ]),
			new ColorEntry("DarkCyan", [ 0x00, 0x8b, 0x8b ]),
			new ColorEntry("DarkGoldenRod", [ 0xb8, 0x86, 0x0b ]),
			new ColorEntry("DarkGray", [ 0xa9, 0xa9, 0xa9 ]),
			new ColorEntry("DarkGreen", [ 0x00, 0x64, 0x00 ]),
			new ColorEntry("DarkKhaki", [ 0xbd, 0xb7, 0x6b ]),
			new ColorEntry("DarkMagenta", [ 0x8b, 0x00, 0x8b ]),
			new ColorEntry("DarkOliveGreen", [ 0x55, 0x6b, 0x2f ]),
			new ColorEntry("Darkorange", [ 0xff, 0x8c, 0x00 ]),
			new ColorEntry("DarkOrchid", [ 0x99, 0x32, 0xcc ]),
			new ColorEntry("DarkRed", [ 0x8b, 0x00, 0x00 ]),
			new ColorEntry("DarkSalmon", [ 0xe9, 0x96, 0x7a ]),
			new ColorEntry("DarkSeaGreen", [ 0x8f, 0xbc, 0x8f ]),
			new ColorEntry("DarkSlateBlue", [ 0x48, 0x3d, 0x8b ]),
			new ColorEntry("DarkSlateGray", [ 0x2f, 0x4f, 0x4f ]),
			new ColorEntry("DarkTurquoise", [ 0x00, 0xce, 0xd1 ]),
			new ColorEntry("DarkViolet", [ 0x94, 0x00, 0xd3 ]),
			new ColorEntry("DeepPink", [ 0xff, 0x14, 0x93 ]),
			new ColorEntry("DeepSkyBlue", [ 0x00, 0xbf, 0xff ]),
			new ColorEntry("DimGray", [ 0x69, 0x69, 0x69 ]),
			new ColorEntry("DodgerBlue", [ 0x1e, 0x90, 0xff ]),
			new ColorEntry("FireBrick", [ 0xb2, 0x22, 0x22 ]),
			new ColorEntry("FloralWhite", [ 0xff, 0xfa, 0xf0 ]),
			new ColorEntry("ForestGreen", [ 0x22, 0x8b, 0x22 ]),
			new ColorEntry("Fuchsia", [ 0xff, 0x00, 0xff ]),
			new ColorEntry("Gainsboro", [ 0xdc, 0xdc, 0xdc ]),
			new ColorEntry("GhostWhite", [ 0xf8, 0xf8, 0xff ]),
			new ColorEntry("Gold", [ 0xff, 0xd7, 0x00 ]),
			new ColorEntry("GoldenRod", [ 0xda, 0xa5, 0x20 ]),
			new ColorEntry("Gray", [ 0x80, 0x80, 0x80 ]),
			new ColorEntry("Green", [ 0x00, 0x80, 0x00 ]),
			new ColorEntry("GreenYellow", [ 0xad, 0xff, 0x2f ]),
			new ColorEntry("HoneyDew", [ 0xf0, 0xff, 0xf0 ]),
			new ColorEntry("HotPink", [ 0xff, 0x69, 0xb4 ]),
			new ColorEntry("IndianRed", [ 0xcd, 0x5c, 0x5c ]),
			new ColorEntry("Indigo", [ 0x4b, 0x00, 0x82 ]),
			new ColorEntry("Ivory", [ 0xff, 0xff, 0xf0 ]),
			new ColorEntry("Khaki", [ 0xf0, 0xe6, 0x8c ]),
			new ColorEntry("Lavender", [ 0xe6, 0xe6, 0xfa ]),
			new ColorEntry("LavenderBlush", [ 0xff, 0xf0, 0xf5 ]),
			new ColorEntry("LawnGreen", [ 0x7c, 0xfc, 0x00 ]),
			new ColorEntry("LemonChiffon", [ 0xff, 0xfa, 0xcd ]),
			new ColorEntry("LightBlue", [ 0xad, 0xd8, 0xe6 ]),
			new ColorEntry("LightCoral", [ 0xf0, 0x80, 0x80 ]),
			new ColorEntry("LightCyan", [ 0xe0, 0xff, 0xff ]),
			new ColorEntry("LightGoldenRodYellow", [ 0xfa, 0xfa, 0xd2 ]),
			new ColorEntry("LightGreen", [ 0x90, 0xee, 0x90 ]),
			new ColorEntry("LightGrey", [ 0xd3, 0xd3, 0xd3 ]),
			new ColorEntry("LightPink", [ 0xff, 0xb6, 0xc1 ]),
			new ColorEntry("LightSalmon", [ 0xff, 0xa0, 0x7a ]),
			new ColorEntry("LightSeaGreen", [ 0x20, 0xb2, 0xaa ]),
			new ColorEntry("LightSkyBlue", [ 0x87, 0xce, 0xfa ]),
			new ColorEntry("LightSlateGray", [ 0x77, 0x88, 0x99 ]),
			new ColorEntry("LightSteelBlue", [ 0xb0, 0xc4, 0xde ]),
			new ColorEntry("LightYellow", [ 0xff, 0xff, 0xe0 ]),
			new ColorEntry("Lime", [ 0x00, 0xff, 0x00 ]),
			new ColorEntry("LimeGreen", [ 0x32, 0xcd, 0x32 ]),
			new ColorEntry("Linen", [ 0xfa, 0xf0, 0xe6 ]),
			new ColorEntry("Magenta", [ 0xff, 0x00, 0xff ]),
			new ColorEntry("Maroon", [ 0x80, 0x00, 0x00 ]),
			new ColorEntry("MediumAquaMarine", [ 0x66, 0xcd, 0xaa ]),
			new ColorEntry("MediumBlue", [ 0x00, 0x00, 0xcd ]),
			new ColorEntry("MediumOrchid", [ 0xba, 0x55, 0xd3 ]),
			new ColorEntry("MediumPurple", [ 0x93, 0x70, 0xd8 ]),
			new ColorEntry("MediumSeaGreen", [ 0x3c, 0xb3, 0x71 ]),
			new ColorEntry("MediumSlateBlue", [ 0x7b, 0x68, 0xee ]),
			new ColorEntry("MediumSpringGreen", [ 0x00, 0xfa, 0x9a ]),
			new ColorEntry("MediumTurquoise", [ 0x48, 0xd1, 0xcc ]),
			new ColorEntry("MediumVioletRed", [ 0xc7, 0x15, 0x85 ]),
			new ColorEntry("MidnightBlue", [ 0x19, 0x19, 0x70 ]),
			new ColorEntry("MintCream", [ 0xf5, 0xff, 0xfa ]),
			new ColorEntry("MistyRose", [ 0xff, 0xe4, 0xe1 ]),
			new ColorEntry("Moccasin", [ 0xff, 0xe4, 0xb5 ]),
			new ColorEntry("NavajoWhite", [ 0xff, 0xde, 0xad ]),
			new ColorEntry("Navy", [ 0x00, 0x00, 0x80 ]),
			new ColorEntry("OldLace", [ 0xfd, 0xf5, 0xe6 ]),
			new ColorEntry("Olive", [ 0x80, 0x80, 0x00 ]),
			new ColorEntry("OliveDrab", [ 0x6b, 0x8e, 0x23 ]),
			new ColorEntry("Orange", [ 0xff, 0xa5, 0x00 ]),
			new ColorEntry("OrangeRed", [ 0xff, 0x45, 0x00 ]),
			new ColorEntry("Orchid", [ 0xda, 0x70, 0xd6 ]),
			new ColorEntry("PaleGoldenRod", [ 0xee, 0xe8, 0xaa ]),
			new ColorEntry("PaleGreen", [ 0x98, 0xfb, 0x98 ]),
			new ColorEntry("PaleTurquoise", [ 0xaf, 0xee, 0xee ]),
			new ColorEntry("PaleVioletRed", [ 0xd8, 0x70, 0x93 ]),
			new ColorEntry("PapayaWhip", [ 0xff, 0xef, 0xd5 ]),
			new ColorEntry("PeachPuff", [ 0xff, 0xda, 0xb9 ]),
			new ColorEntry("Peru", [ 0xcd, 0x85, 0x3f ]),
			new ColorEntry("Pink", [ 0xff, 0xc0, 0xcb ]),
			new ColorEntry("Plum", [ 0xdd, 0xa0, 0xdd ]),
			new ColorEntry("PowderBlue", [ 0xb0, 0xe0, 0xe6 ]),
			new ColorEntry("Purple", [ 0x80, 0x00, 0x80 ]),
			new ColorEntry("Red", [ 0xff, 0x00, 0x00 ]),
			new ColorEntry("RosyBrown", [ 0xbc, 0x8f, 0x8f ]),
			new ColorEntry("RoyalBlue", [ 0x41, 0x69, 0xe1 ]),
			new ColorEntry("SaddleBrown", [ 0x8b, 0x45, 0x13 ]),
			new ColorEntry("Salmon", [ 0xfa, 0x80, 0x72 ]),
			new ColorEntry("SandyBrown", [ 0xf4, 0xa4, 0x60 ]),
			new ColorEntry("SeaGreen", [ 0x2e, 0x8b, 0x57 ]),
			new ColorEntry("SeaShell", [ 0xff, 0xf5, 0xee ]),
			new ColorEntry("Sienna", [ 0xa0, 0x52, 0x2d ]),
			new ColorEntry("Silver", [ 0xc0, 0xc0, 0xc0 ]),
			new ColorEntry("SkyBlue", [ 0x87, 0xce, 0xeb ]),
			new ColorEntry("SlateBlue", [ 0x6a, 0x5a, 0xcd ]),
			new ColorEntry("SlateGray", [ 0x70, 0x80, 0x90 ]),
			new ColorEntry("Snow", [ 0xff, 0xfa, 0xfa ]),
			new ColorEntry("SpringGreen", [ 0x00, 0xff, 0x7f ]),
			new ColorEntry("SteelBlue", [ 0x46, 0x82, 0xb4 ]),
			new ColorEntry("Tan", [ 0xd2, 0xb4, 0x8c ]),
			new ColorEntry("Teal", [ 0x00, 0x80, 0x80 ]),
			new ColorEntry("Thistle", [ 0xd8, 0xbf, 0xd8 ]),
			new ColorEntry("Tomato", [ 0xff, 0x63, 0x47 ]),
			new ColorEntry("Turquoise", [ 0x40, 0xe0, 0xd0 ]),
			new ColorEntry("Violet", [ 0xee, 0x82, 0xee ]),
			new ColorEntry("Wheat", [ 0xf5, 0xde, 0xb3 ]),
			new ColorEntry("White", [ 0xff, 0xff, 0xff ]),
			new ColorEntry("WhiteSmoke", [ 0xf5, 0xf5, 0xf5 ]),
			new ColorEntry("Yellow", [ 0xff, 0xff, 0x00 ]),
			new ColorEntry("YellowGreen", [ 0x9a, 0xcd, 0x32 ]),
		];
		#endregion

		private static readonly CPointer<char>[] months = 
		[
			"january".ToCharPointer(), "february".ToCharPointer(), "march".ToCharPointer(),
			"april".ToCharPointer(), "may".ToCharPointer(), "june".ToCharPointer(),
			"july".ToCharPointer(), "august".ToCharPointer(), "september".ToCharPointer(),
			"october".ToCharPointer(), "november".ToCharPointer(), "december".ToCharPointer()
		];

		private static readonly CPointer<char>[] date_Fmt =
		[
			"%Y - %m - %d".ToCharPointer(),
			"%Y%m%d".ToCharPointer()
		];

		private static readonly CPointer<char>[] time_Fmt =
		[
			"%H:%M:%S".ToCharPointer(),
			"%H%M%S".ToCharPointer()
		];

		private static readonly CPointer<char>[] tz_Fmt =
		[
			"%H:%M".ToCharPointer(),
			"%H%M".ToCharPointer(),
			"%H".ToCharPointer()
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_Parse_Ratio_Quiet(out AvRational q, CPointer<char> str, c_int max)
		{
			return Av_Parse_Ratio(out q, str, max, 0/*Av_Log_Max_Offset*/, null);
		}



		/********************************************************************/
		/// <summary>
		/// Parse str and store the parsed ratio in q.
		///
		/// Note that a ratio with infinite (1/0) or negative value is
		/// considered valid, so you should check on the returned value if
		/// you want to exclude those values.
		///
		/// The undefined value can be expressed using the "0:0" string
		/// </summary>
		/********************************************************************/
		public static c_int Av_Parse_Ratio(out AvRational q, CPointer<char> str, c_int max, c_int log_Offset, IContext log_Ctx)//XX 45
		{
			q = new AvRational(0, 0);

			CSScanF sscanF = new CSScanF();

			c_int ret = sscanF.Parse(new string(str.AsSpan()), "%d:%d%c");
			q = new AvRational(sscanF.Results.Count >= 1 ? (c_int)sscanF.Results[0] : 0, sscanF.Results.Count >= 2 ? (c_int)sscanF.Results[1] : 0);

			if (ret != 2)
			{
				ret = Eval.Av_Expr_Parse_And_Eval(out c_double d, str, null, null, null, null, null, null, null, log_Offset, log_Ctx);
				if (ret < 0)
					return ret;

				q = Rational.Av_D2Q(d, max);
			}
			else
				Rational.Av_Reduce(out q.Num, out q.Den, q.Num, q.Den, max);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse str and put in width_ptr and height_ptr the detected values
		/// </summary>
		/********************************************************************/
		public static c_int Av_Parse_Video_Size(out c_int width_Ptr, out c_int height_Ptr, CPointer<char> str)//XX 150
		{
			c_int i;
			c_int n = (c_int)Macros.FF_Array_Elems(video_Size_Abbrs);
			c_int width = 0, height = 0;

			width_Ptr = 0;
			height_Ptr = 0;

			for (i = 0; i < n; i++)
			{
				if (CString.strcmp(video_Size_Abbrs[i].Abbr.ToCharPointer(), str) == 0)
				{
					width = video_Size_Abbrs[i].Width;
					height = video_Size_Abbrs[i].Height;
					break;
				}
			}

			if (i == n)
			{
				width = (c_int)CString.strtol(str, out CPointer<char> p, 10, out bool _);

				if (p[0] != 0)
					p++;

				height = (c_int)CString.strtol(p, out p, 10, out bool _);

				// Trailing extraneous data detected, like in 123x345foobar
				if (p[0] != 0)
					return Error.EINVAL;
			}

			if ((width <= 0) || (height <= 0))
				return Error.EINVAL;

			width_Ptr = width;
			height_Ptr = height;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse str and store the detected values in *rate
		/// </summary>
		/********************************************************************/
		public static c_int Av_Parse_Video_Rate(out AvRational rate, CPointer<char> arg)//XX 181
		{
			c_int n = (c_int)Macros.FF_Array_Elems(video_Rate_Abbrs);

			// First, we check out abbreviation table
			for (c_int i = 0; i < n; i++)
			{
				if (CString.strcmp(video_Rate_Abbrs[i].Abbr.ToCharPointer(), arg) == 0)
				{
					rate = video_Rate_Abbrs[i].Rate;

					return 0;
				}
			}

			// Then we try to parse it as fraction
			c_int ret = Av_Parse_Ratio_Quiet(out rate, arg, 1001000);
			if (ret < 0)
				return ret;

			if ((rate.Num == 0) || (rate.Den == 0))
			{
				ret = Av_Parse_Ratio_Quiet(out rate, arg, c_int.MaxValue);
				if (ret < 0)
					return ret;
			}

			if ((rate.Num <= 0) || (rate.Den <= 0))
				return Error.EINVAL;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Put the RGBA values that correspond to color_string in rgba_color
		/// </summary>
		/********************************************************************/
		public static c_int Av_Parse_Color(CPointer<uint8_t> rgba_Color, CPointer<char> color_String, c_int sLen, IClass log_Ctx)//XX 359
		{
			CPointer<char> color_String2 = new CPointer<char>(128);
			c_int hex_Offset = 0;

			if (color_String[0] == '#')
				hex_Offset = 1;
			else if (CString.strncmp(color_String, "0x", 2) == 0)
				hex_Offset = 2;

			if (sLen < 0)
				sLen = color_String.Length;

			AvString.Av_Strlcpy(color_String2, color_String + hex_Offset, (size_t)Macros.FFMin(sLen - hex_Offset + 1, color_String2.Length));

			CPointer<char> tail = CString.strchr(color_String2, Alpha_Sep);
			if (tail.IsNotNull)
				tail[0, 1] = '\0';

			c_int len = (c_int)CString.strlen(color_String2);
			rgba_Color[3] = 255;

			if ((AvString.Av_Strcasecmp(color_String2, "random".ToCharPointer()) == 0) || (AvString.Av_Strcasecmp(color_String2, "bikeshed".ToCharPointer()) == 0))
			{
				c_int rgba = (c_int)(UnitTest.IsUnitTestEnabled() && (UnitTest.GetRandomMethod() != null) ? UnitTest.GetRandomMethod()() : Random_Seed.Av_Get_Random_Seed());
				rgba_Color[0] = (uint8_t)(rgba >> 24);
				rgba_Color[1] = (uint8_t)(rgba >> 16);
				rgba_Color[2] = (uint8_t)(rgba >> 8);
				rgba_Color[3] = (uint8_t)rgba;
			}
			else if ((hex_Offset != 0) || ((c_int)CString.strspn(color_String2, "0123456789ABCDEFabcdef") == len))
			{
				CPointer<char> tail1;
				c_uint rgba = (c_uint)CString.strtoul(color_String2, out tail1, 16, out bool _);

				if ((tail1[0] != '\0') || ((len != 6) && (len != 8)))
				{
					Log.Av_Log(log_Ctx, Log.Av_Log_Error, "Invalid 0xRRGGBB[AA] color string: '%s'\n", color_String2);

					return Error.EINVAL;
				}

				if (len == 8)
				{
					rgba_Color[3] = (uint8_t)rgba;
					rgba >>= 8;
				}

				rgba_Color[0] = (uint8_t)(rgba >> 16);
				rgba_Color[1] = (uint8_t)(rgba >> 8);
				rgba_Color[2] = (uint8_t)rgba;
			}
			else
			{
				CPointer<ColorEntry> entry = CArray.bsearch<ColorEntry, CPointer<char>>(color_String2, color_Table, Macros.FF_Array_Elems(color_Table), Color_Table_Compare);

				if (entry.IsNull)
				{
					Log.Av_Log(log_Ctx, Log.Av_Log_Error, "Cannot find color '%s'\n", color_String2);

					return Error.EINVAL;
				}

				CMemory.memcpy(rgba_Color, entry[0].Rgb_Color, 3);
			}

			if (tail.IsNotNull)
			{
				c_double alpha;
				CPointer<char> alpha_String = tail;

				if (CString.strncmp(alpha_String, "0x".ToCharPointer(), 2) == 0)
					alpha = CString.strtoul(alpha_String, out tail, 16, out bool _);
				else
				{
					c_double norm_Alpha = CString.strtod(alpha_String, out tail);

					if ((norm_Alpha < 0.0) || (norm_Alpha > 1.0))
						alpha = 256;
					else
						alpha = 255 * norm_Alpha;
				}

				if ((tail == alpha_String) || (tail[0] != '\0') || (alpha > 255) || (alpha < 0))
				{
					Log.Av_Log(log_Ctx, Log.Av_Log_Error, "Invalid alpha value specifier '%s' in '%s'\n", alpha_String, color_String);

					return Error.EINVAL;
				}

				rgba_Color[3] = (uint8_t)alpha;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the name of a color from the internal table of hard-coded
		/// named colors.
		///
		/// This function is meant to enumerate the color names recognized
		/// by av_parse_color()
		/// </summary>
		/********************************************************************/
		public static IEnumerable<(CPointer<char> Name, CPointer<uint8_t> rgba)> Av_Get_Known_Color_Name()//XX 439
		{
			foreach (ColorEntry color in color_Table)
				yield return new (color.Name.ToCharPointer(), new CPointer<uint8_t>([ color.Rgb_Color[0], color.Rgb_Color[1], color.Rgb_Color[2], 0 ]));
		}



		/********************************************************************/
		/// <summary>
		/// Simplified version of strptime
		///
		/// Parse the input string p according to the format string fmt and
		/// store its results in the structure dt.
		/// This implementation supports only a subset of the formats
		/// supported by the standard strptime().
		///
		/// The supported input field descriptors are listed below.
		/// - `%H`: the hour as a decimal number, using a 24-hour clock, in
		///   the range '00' through '23'
		/// - `%J`: hours as a decimal number, in the range '0' through
		///   INT_MAX
		/// - `%M`: the minute as a decimal number, using a 24-hour clock,
		///   in the range '00' through '59'
		/// - `%S`: the second as a decimal number, using a 24-hour clock,
		///   in the range '00' through '59'
		/// - `%Y`: the year as a decimal number, using the Gregorian
		///   calendar
		/// - `%m`: the month as a decimal number, in the range '1' through
		///   '12'
		/// - `%d`: the day of the month as a decimal number, in the range
		///   '1' through '31'
		/// - `%T`: alias for `%H:%M:%S`
		/// - `%%`: a literal `%`
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<char> Av_Small_Strptime(CPointer<char> p, string fmt, ref tm dt)
		{
			return Av_Small_Strptime(p, fmt.ToCharPointer(), ref dt);
		}



		/********************************************************************/
		/// <summary>
		/// Simplified version of strptime
		///
		/// Parse the input string p according to the format string fmt and
		/// store its results in the structure dt.
		/// This implementation supports only a subset of the formats
		/// supported by the standard strptime().
		///
		/// The supported input field descriptors are listed below.
		/// - `%H`: the hour as a decimal number, using a 24-hour clock, in
		///   the range '00' through '23'
		/// - `%J`: hours as a decimal number, in the range '0' through
		///   INT_MAX
		/// - `%M`: the minute as a decimal number, using a 24-hour clock,
		///   in the range '00' through '59'
		/// - `%S`: the second as a decimal number, using a 24-hour clock,
		///   in the range '00' through '59'
		/// - `%Y`: the year as a decimal number, using the Gregorian
		///   calendar
		/// - `%m`: the month as a decimal number, in the range '1' through
		///   '12'
		/// - `%d`: the day of the month as a decimal number, in the range
		///   '1' through '31'
		/// - `%T`: alias for `%H:%M:%S`
		/// - `%%`: a literal `%`
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Small_Strptime(CPointer<char> p, CPointer<char> fmt, ref tm dt)//XX 494
		{
			c_int c, val;

			while ((c = fmt[0, 1]) != 0)
			{
				if (c != '%')
				{
					if (AvString.Av_IsSpace(c))
					{
						for (; (p[0] != 0) && AvString.Av_IsSpace(p[0]); p++)
						{
						}
					}
					else if (p[0] != c)
						return null;
					else
						p++;

					continue;
				}

				c = fmt[0, 1];

				switch (c)
				{
					case 'H':
					case 'J':
					{
						val = Date_Get_Num(ref p, 0, c == 'H' ? 23 : c_int.MaxValue, c == 'H' ? 2 : 4);

						if (val == -1)
							return null;

						dt.tm_Hour = val;
						break;
					}

					case 'M':
					{
						val = Date_Get_Num(ref p, 0, 59, 2);

						if (val == -1)
							return null;

						dt.tm_Min = val;
						break;
					}

					case 'S':
					{
						val = Date_Get_Num(ref p, 0, 59, 2);

						if (val == -1)
							return null;

						dt.tm_Sec = val;
						break;
					}

					case 'Y':
					{
						val = Date_Get_Num(ref p, 0, 9999, 4);

						if (val == -1)
							return null;

						dt.tm_Year = val - 1900;
						break;
					}

					case 'm':
					{
						val = Date_Get_Num(ref p, 1, 12, 2);

						if (val == -1)
							return null;

						dt.tm_Mon = val - 1;
						break;
					}

					case 'd':
					{
						val = Date_Get_Num(ref p, 1, 31, 2);

						if (val == -1)
							return null;

						dt.tm_MDay = val;
						break;
					}

					case 'T':
					{
						p = Av_Small_Strptime(p, "%H:%M:%S", ref dt);

						if (p.IsNull)
							return null;

						break;
					}

					case 'b':
					case 'B':
					case 'h':
					{
						val = Date_Get_Month(ref p);

						if (val == -1)
							return null;

						dt.tm_Mon = val;
						break;
					}

					case '%':
					{
						if (p[0] != '%')
							return null;

						break;
					}

					default:
						return null;
				}
			}

			return p;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the decomposed UTC time in tm to a time_t value
		/// </summary>
		/********************************************************************/
		public static time_t Av_TimeGm(tm tm)//XX 573
		{
			c_int y = tm.tm_Year + 1900;
			c_int m = tm.tm_Mon + 1;
			c_int d = tm.tm_MDay;

			if (m < 3)
			{
				m += 12;
				y--;
			}

			time_t t = 86400 * (d + (((153 * m) - 457) / 5) + (365 * y) + (y / 4) - (y / 100) + (y / 400) - 719469);
			t += (3600 * tm.tm_Hour) + (60 * tm.tm_Min) + tm.tm_Sec;

			return t;
		}



		/********************************************************************/
		/// <summary>
		/// Parse timestr and return in *time a corresponding number of
		/// microseconds
		/// </summary>
		/********************************************************************/
		public static c_int Av_Parse_Time(out int64_t timeVal, CPointer<char> timeStr, c_int duration)//XX 592
		{
			int64_t t = 0;
			time_t now = 0;
			tm dt = new tm();
			c_int today = 0, negative = 0, microSeconds = 0, suffix = 1000000;

			CPointer<char> p = timeStr;
			CPointer<char> q = null;
			timeVal = int64_t.MinValue;

			if (duration == 0)
			{
				int64_t now64 = Time.Av_GetTime();
				now = now64 / 1000000;

				if (AvString.Av_Strcasecmp(timeStr, "now") == 0)
				{
					timeVal = now64;

					return 0;
				}

				// Parse the year-month-day part
				for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(date_Fmt); i++)
				{
					q = Av_Small_Strptime(p, date_Fmt[i], ref dt);
					if (q.IsNotNull)
						break;
				}

				// If the year-month-day part is missing, then take the
				// current year-month-day time
				if (q.IsNull)
				{
					today = 1;
					q = p;
				}

				p = q;

				if ((p[0] == 'T') || (p[0] == 't'))
					p++;
				else
				{
					while (AvString.Av_IsSpace(p[0]))
						p++;
				}

				// Parse the hour-minute-second part
				for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(time_Fmt); i++)
				{
					q = Av_Small_Strptime(p, time_Fmt[i], ref dt);
					if (q.IsNotNull)
						break;
				}
			}
			else
			{
				// Parse timestr as a duration
				if (p[0] == '-')
				{
					negative = 1;
					++p;
				}

				// Parse timestr as HH:MM:SS
				q = Av_Small_Strptime(p, "%J:%M:%S", ref dt);
				if (q.IsNull)
				{
					// Parse timestr as MM:SS
					q = Av_Small_Strptime(p, "%M:%S", ref dt);
					dt.tm_Hour = 0;
				}

				if (q.IsNull)
				{
					// Parse timestr as S+
					t = CString.strtoll(p, out CPointer<char> o, 10, out bool error);

					if (o == p)	// The parse didn't succeed
						return Error.EINVAL;

					if (error)
						return Error.ERANGE;

					q = o;
				}
				else
					t = (dt.tm_Hour * 3600) + (dt.tm_Min * 60) + dt.tm_Sec;
			}

			// Now we have all the fields that we can get
			if (q.IsNull)
				return Error.EINVAL;

			// Parse the .m... part
			if (q[0] == '.')
			{
				q++;

				for (c_int n = 100000; n >= 1; n /= 10, q++)
				{
					if (!AvString.Av_IsDigit(q[0]))
						break;

					microSeconds += n * (q[0] - '0');
				}

				while (AvString.Av_IsDigit(q[0]))
					q++;
			}

			if (duration != 0)
			{
				if ((q[0] == 'm') && (q[1] == 's'))
				{
					suffix = 1000;
					microSeconds /= 1000;
					q += 2;
				}
				else if ((q[0] == 'u') && (q[1] == 's'))
				{
					suffix = 1;
					microSeconds = 0;
					q += 2;
				}
				else if (q[0] == 's')
					q++;
			}
			else
			{
				c_int is_Utc = (q[0] == 'Z') || (q[0] == 'z') ? 1 : 0;
				c_int tzOffset = 0;

				q += is_Utc;

				if ((today == 0) && (is_Utc == 0) && ((q[0] == '+') || q[0] == '-'))
				{
					tm tz = new tm();
					c_int sign = q[0] == '+' ? -1 : 1;
					q++;
					p = q;

					for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(tz_Fmt); i++)
					{
						q = Av_Small_Strptime(p, tz_Fmt[i], ref tz);
						if (q.IsNotNull)
							break;
					}

					if (q.IsNull)
						return Error.EINVAL;

					tzOffset = sign * (tz.tm_Hour * 60 + tz.tm_Min) * 60;
					is_Utc = 1;
				}

				if (today != 0)		// Fill in today's date
				{
					tm dt2 = is_Utc != 0 ? CTime.gmtime_r(now, out _) : CTime.localtime_r(now, out _);

					dt2.tm_Hour = dt.tm_Hour;
					dt2.tm_Min = dt.tm_Min;
					dt2.tm_Sec = dt.tm_Sec;

					dt = dt2;
				}

				dt.tm_IsDst = is_Utc != 0 ? 0 : -1;
				t = is_Utc != 0 ? Av_TimeGm(dt) : CTime.mktime(dt);
				t += tzOffset;
			}

			// Check that we are at the end of the string
			if (q[0] != 0)
				return Error.EINVAL;

			if (((int64_t.MaxValue / suffix) < t) || (t < (int64_t.MinValue / suffix)))
				return Error.ERANGE;

			t *= suffix;

			if ((int64_t.MaxValue - microSeconds) < t)
				return Error.ERANGE;

			t += microSeconds;

			if ((t == int64_t.MinValue) && (negative != 0))
				return Error.ERANGE;

			timeVal = negative != 0 ? -t : t;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Attempt to find a specific tag in a URL.
		///
		/// syntax: '?tag1=val1＆tag2=val2...'. Little URL decoding is done.
		/// Return 1 if found
		/// </summary>
		/********************************************************************/
		public static c_int Av_Find_Info_Tag(CPointer<char> arg, c_int arg_Size, CPointer<char> tag1, CPointer<char> info)//XX 756
		{
			CPointer<char> tag = new CPointer<char>(128);

			CPointer<char> p = info;

			if (p[0] == '?')
				p++;

			for (;;)
			{
				CPointer<char> q = tag;

				while ((p[0] != '\0') && (p[0] != '=' && (p[0] != '&')))
				{
					if ((q - tag) < (tag.Length - 1))
						q[0, 1] = p[0];

					p++;
				}

				q[0] = '\0';

				q = arg;

				if (p[0] == '=')
				{
					p++;

					while ((p[0] != '&') && (p[0] != '\0'))
					{
						if ((q - arg) < (arg.Length - 1))
						{
							if (p[0] == '+')
								q[0, 1] = ' ';
							else
								q[0, 1] = p[0];
						}

						p++;
					}
				}

				q[0] = '\0';

				if (CString.strcmp(tag, tag1) == 0)
					return 1;

				if (p[0] != '&')
					break;

				p++;
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Color_Table_Compare(CPointer<char> lhs, ColorEntry rhs)
		{
			return AvString.Av_Strcasecmp(lhs, rhs.Name.ToCharPointer());
		}



		/********************************************************************/
		/// <summary>
		/// Get a positive number between n_min and n_max, for a maximum
		/// length of len_max. Return -1 if error
		/// </summary>
		/********************************************************************/
		private static c_int Date_Get_Num(ref CPointer<char> pp, c_int n_Min, c_int n_Max, c_int len_Max)//XX 455
		{
			CPointer<char> p = pp;
			c_int val = 0;

			for (c_int i = 0; i < len_Max; i++)
			{
				c_int c = p[0];

				if (!AvString.Av_IsDigit(c))
					break;

				val = (val * 10) + c - '0';
				p++;
			}

			// No number read?
			if (p == pp)
				return -1;

			if ((val < n_Min) || (val > n_Max))
				return -1;

			pp = p;

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Date_Get_Month(ref CPointer<char> pp)//XX 479
		{
			for (c_int i = 0; i < 12; i++)
			{
				if (AvString.Av_Strncasecmp(pp, months[i], 3) == 0)
				{
					CPointer<char> mo_Full = months[i] + 3;
					c_int len = (c_int)CString.strlen(mo_Full);
					pp += 3;

					if ((len > 0) && (AvString.Av_Strncasecmp(pp, mo_Full, (size_t)len) == 0))
						pp += len;

					return i;
				}
			}

			return -1;
		}
		#endregion
	}
}
