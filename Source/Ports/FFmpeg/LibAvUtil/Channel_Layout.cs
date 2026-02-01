/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Audio channel layout utility functions
	/// </summary>
	public static class Channel_Layout
	{
		/// <summary></summary>
		public static readonly AvChannelLayout Mono = Av_Channel_Layout_Mask(1, AvChannelMask.Mono);
		/// <summary></summary>
		public static readonly AvChannelLayout Stereo = Av_Channel_Layout_Mask(2, AvChannelMask.Stereo);
		/// <summary></summary>
		public static readonly AvChannelLayout TwoPointOne = Av_Channel_Layout_Mask(3, AvChannelMask.TwoPointOne);
		/// <summary></summary>
		public static readonly AvChannelLayout Two_One = Av_Channel_Layout_Mask(3, AvChannelMask.Two_One);
		/// <summary></summary>
		public static readonly AvChannelLayout Surround = Av_Channel_Layout_Mask(3, AvChannelMask.Surround);
		/// <summary></summary>
		public static readonly AvChannelLayout ThreePointOne = Av_Channel_Layout_Mask(4, AvChannelMask.ThreePointOne);
		/// <summary></summary>
		public static readonly AvChannelLayout FourPointZero = Av_Channel_Layout_Mask(4, AvChannelMask.FourPointZero);
		/// <summary></summary>
		public static readonly AvChannelLayout FourPointOne = Av_Channel_Layout_Mask(5, AvChannelMask.FourPointOne);
		/// <summary></summary>
		public static readonly AvChannelLayout Two_Two = Av_Channel_Layout_Mask(4, AvChannelMask.Two_Two);
		/// <summary></summary>
		public static readonly AvChannelLayout Quad = Av_Channel_Layout_Mask(4, AvChannelMask.Quad);
		/// <summary></summary>
		public static readonly AvChannelLayout FivePointZero = Av_Channel_Layout_Mask(5, AvChannelMask.FivePointZero);
		/// <summary></summary>
		public static readonly AvChannelLayout FivePointOne = Av_Channel_Layout_Mask(6, AvChannelMask.FivePointOne);
		/// <summary></summary>
		public static readonly AvChannelLayout FivePointZero_Back = Av_Channel_Layout_Mask(5, AvChannelMask.FivePointZero_Back);
		/// <summary></summary>
		public static readonly AvChannelLayout FivePointOne_Back = Av_Channel_Layout_Mask(6, AvChannelMask.FivePointOne_Back);
		/// <summary></summary>
		public static readonly AvChannelLayout SixPointZero = Av_Channel_Layout_Mask(6, AvChannelMask.SixPointZero);
		/// <summary></summary>
		public static readonly AvChannelLayout SixPointZero_Front = Av_Channel_Layout_Mask(6, AvChannelMask.SixPointZero_Front);
		/// <summary></summary>
		public static readonly AvChannelLayout ThreePointOnePointTwo = Av_Channel_Layout_Mask(6, AvChannelMask.ThreePointOnePointTwo);
		/// <summary></summary>
		public static readonly AvChannelLayout Hexagonal = Av_Channel_Layout_Mask(6, AvChannelMask.Hexagonal);
		/// <summary></summary>
		public static readonly AvChannelLayout SixPointOne = Av_Channel_Layout_Mask(7, AvChannelMask.SixPointOne);
		/// <summary></summary>
		public static readonly AvChannelLayout SixPointOne_Back = Av_Channel_Layout_Mask(7, AvChannelMask.SixPointOne_Back);
		/// <summary></summary>
		public static readonly AvChannelLayout SixPointOne_Front = Av_Channel_Layout_Mask(7, AvChannelMask.SixPointOne_Front);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointZero = Av_Channel_Layout_Mask(7, AvChannelMask.SevenPointZero);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointZero_Front = Av_Channel_Layout_Mask(7, AvChannelMask.SevenPointZero_Front);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointOne = Av_Channel_Layout_Mask(8, AvChannelMask.SevenPointOne);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointOne_Wide = Av_Channel_Layout_Mask(8, AvChannelMask.SevenPointOne_Wide);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointOne_Wide_Back = Av_Channel_Layout_Mask(8, AvChannelMask.SevenPointOne_Wide_Back);
		/// <summary></summary>
		public static readonly AvChannelLayout FivePointOnePointTwo = Av_Channel_Layout_Mask(8, AvChannelMask.FivePointOnePointTwo);
		/// <summary></summary>
		public static readonly AvChannelLayout FivePointOnePointTwo_Back = Av_Channel_Layout_Mask(8, AvChannelMask.FivePointOnePointTwo_Back);
		/// <summary></summary>
		public static readonly AvChannelLayout Octagonal = Av_Channel_Layout_Mask(8, AvChannelMask.Octagonal);
		/// <summary></summary>
		public static readonly AvChannelLayout Cube = Av_Channel_Layout_Mask(8, AvChannelMask.Cube);
		/// <summary></summary>
		public static readonly AvChannelLayout FivePointOnePointFour_Back = Av_Channel_Layout_Mask(10, AvChannelMask.FivePointOnePointFour_Back);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointOnePointTwo = Av_Channel_Layout_Mask(10, AvChannelMask.SevenPointOnePointTwo);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointOnePointFour_Back = Av_Channel_Layout_Mask(12, AvChannelMask.SevenPointOnePointFour_Back);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointTwoPointThree = Av_Channel_Layout_Mask(12, AvChannelMask.SevenPointTwoPointThree);
		/// <summary></summary>
		/// <summary></summary>
		public static readonly AvChannelLayout NinePointOnePointFour_Back = Av_Channel_Layout_Mask(14, AvChannelMask.NinePointOnePointFour_Back);
		/// <summary></summary>
		public static readonly AvChannelLayout NinePointOnePointSix = Av_Channel_Layout_Mask(16, AvChannelMask.NinePointOnePointSix);
		/// <summary></summary>
		public static readonly AvChannelLayout Hexadecagonal = Av_Channel_Layout_Mask(16, AvChannelMask.Hexadecagonal);
		/// <summary></summary>
		public static readonly AvChannelLayout Binaural = Av_Channel_Layout_Mask(2, AvChannelMask.Binaural);
		/// <summary></summary>
		public static readonly AvChannelLayout Stereo_Downmix = Av_Channel_Layout_Mask(2, AvChannelMask.Stereo_Downmix);
		/// <summary></summary>
		public static readonly AvChannelLayout TwentyTwoPointTwo = Av_Channel_Layout_Mask(24, AvChannelMask.TwentyTwoPointTwo);
		/// <summary></summary>
		public static readonly AvChannelLayout SevenPointOne_Top_Back = FivePointOnePointTwo_Back;

		private static readonly Channel_Name[] channel_Names = BuildNames();

		private static readonly Channel_Layout_Name[] channel_Layout_Map =
		[
			new Channel_Layout_Name("mono", Mono),
			new Channel_Layout_Name("stereo", Stereo),
			new Channel_Layout_Name("2.1", TwoPointOne),
			new Channel_Layout_Name("3.0", Surround),
			new Channel_Layout_Name("3.0(back)", Two_One),
			new Channel_Layout_Name("4.0", FourPointZero),
			new Channel_Layout_Name("quad", Quad),
			new Channel_Layout_Name("quad(side)", Two_Two),
			new Channel_Layout_Name("3.1", ThreePointOne),
			new Channel_Layout_Name("5.0", FivePointZero_Back),
			new Channel_Layout_Name("5.0(side)", FivePointZero),
			new Channel_Layout_Name("4.1", FourPointOne),
			new Channel_Layout_Name("5.1", FivePointOne_Back),
			new Channel_Layout_Name("5.1(side)", FivePointOne),
			new Channel_Layout_Name("6.0", SixPointZero),
			new Channel_Layout_Name("6.0(front)", SixPointZero_Front),
			new Channel_Layout_Name("3.1.2", ThreePointOnePointTwo),
			new Channel_Layout_Name("hexagonal", Hexagonal),
			new Channel_Layout_Name("6.1", SixPointOne),
			new Channel_Layout_Name("6.1(back)", SixPointOne_Back),
			new Channel_Layout_Name("6.1(front)", SixPointOne_Front),
			new Channel_Layout_Name("7.0", SevenPointZero),
			new Channel_Layout_Name("7.0(front)", SevenPointZero_Front),
			new Channel_Layout_Name("7.1", SevenPointOne),
			new Channel_Layout_Name("7.1(wide)", SevenPointOne_Wide_Back),
			new Channel_Layout_Name("7.1(wide-side)", SevenPointOne_Wide),
			new Channel_Layout_Name("5.1.2", FivePointOnePointTwo),
			new Channel_Layout_Name("5.1.2(back)", FivePointOnePointTwo_Back),
			new Channel_Layout_Name("octagonal", Octagonal),
			new Channel_Layout_Name("cube", Cube),
			new Channel_Layout_Name("5.1.4", FivePointOnePointFour_Back),
			new Channel_Layout_Name("7.1.2", SevenPointOnePointTwo),
			new Channel_Layout_Name("7.1.4", SevenPointOnePointFour_Back),
			new Channel_Layout_Name("7.2.3", SevenPointTwoPointThree),
			new Channel_Layout_Name("9.1.4", NinePointOnePointFour_Back),
			new Channel_Layout_Name("9.1.6", NinePointOnePointSix),
			new Channel_Layout_Name("hexadecagonal", Hexadecagonal),
			new Channel_Layout_Name("binaural", Binaural),
			new Channel_Layout_Name("downmix", Stereo_Downmix),
			new Channel_Layout_Name("22.2", TwentyTwoPointTwo)
		];

		#region Build names
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Channel_Name[] BuildNames()
		{
			List<Channel_Name> list = new List<Channel_Name>();

			void AddAndGrow(AvChannel channel, Channel_Name channel_Name)
			{
				while (list.Count <= (c_int)channel)
					list.Add(new Channel_Name());

				list[(c_int)channel] = channel_Name;
			}

			AddAndGrow(AvChannel.Front_Left, new Channel_Name("FL", "front left"));
			AddAndGrow(AvChannel.Front_Right, new Channel_Name("FR", "front right"));
			AddAndGrow(AvChannel.Front_Center, new Channel_Name("FC", "front center"));
			AddAndGrow(AvChannel.Low_Frequency, new Channel_Name("LFE", "low frequency"));
			AddAndGrow(AvChannel.Back_Left, new Channel_Name("BL", "back left"));
			AddAndGrow(AvChannel.Back_Right, new Channel_Name("BR", "back right"));
			AddAndGrow(AvChannel.Front_Left_Of_Center, new Channel_Name("FLC", "front left-of-center"));
			AddAndGrow(AvChannel.Front_Right_Of_Center, new Channel_Name("FRC", "front right-of-center"));
			AddAndGrow(AvChannel.Back_Center, new Channel_Name("BC", "back center"));
			AddAndGrow(AvChannel.Side_Left, new Channel_Name("SL", "side left"));
			AddAndGrow(AvChannel.Side_Right, new Channel_Name("SR", "side right"));
			AddAndGrow(AvChannel.Top_Center, new Channel_Name("TC", "top center"));
			AddAndGrow(AvChannel.Top_Front_Left, new Channel_Name("TFL", "top front left"));
			AddAndGrow(AvChannel.Top_Front_Center, new Channel_Name("TFC", "top front center"));
			AddAndGrow(AvChannel.Top_Front_Right, new Channel_Name("TFR", "top front right"));
			AddAndGrow(AvChannel.Top_Back_Left, new Channel_Name("TBL", "top back left"));
			AddAndGrow(AvChannel.Top_Back_Center, new Channel_Name("TBC", "top back center"));
			AddAndGrow(AvChannel.Top_Back_Right, new Channel_Name("TBR", "top back right"));
			AddAndGrow(AvChannel.Stereo_Left, new Channel_Name("DL", "downmix left"));
			AddAndGrow(AvChannel.Stereo_Right, new Channel_Name("DR", "downmix right"));
			AddAndGrow(AvChannel.Wide_Left, new Channel_Name("WL", "wide left"));
			AddAndGrow(AvChannel.Wide_Right, new Channel_Name("WR", "wide right"));
			AddAndGrow(AvChannel.Surround_Direct_Left, new Channel_Name("SDL", "surround direct left"));
			AddAndGrow(AvChannel.Surround_Direct_Right, new Channel_Name("SDR", "surround direct right"));
			AddAndGrow(AvChannel.Low_Frequency_2, new Channel_Name("LFE2", "low frequency 2"));
			AddAndGrow(AvChannel.Top_Side_Left, new Channel_Name("TSL", "top side left"));
			AddAndGrow(AvChannel.Top_Side_Right, new Channel_Name("TSR", "top side right"));
			AddAndGrow(AvChannel.Bottom_Front_Center, new Channel_Name("BFC", "bottom front center"));
			AddAndGrow(AvChannel.Bottom_Front_Left, new Channel_Name("BFL", "bottom front left"));
			AddAndGrow(AvChannel.Bottom_Front_Right, new Channel_Name("BFR", "bottom front right"));
			AddAndGrow(AvChannel.Side_Surround_Left, new Channel_Name("SSL", "side surround left"));
			AddAndGrow(AvChannel.Side_Surround_Right, new Channel_Name("SSR", "side surround right"));
			AddAndGrow(AvChannel.Top_Surround_Left, new Channel_Name("TTL", "top surround left"));
			AddAndGrow(AvChannel.Top_Surround_Right, new Channel_Name("TTR", "top surround right"));
			AddAndGrow(AvChannel.Binaural_Left, new Channel_Name("BIL", "binaural left"));
			AddAndGrow(AvChannel.Binaural_Right, new Channel_Name("BIR", "binaural right"));

			return list.ToArray();
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// BPrint variant of av_channel_name().
		///
		/// Note the string will be appended to the bprint buffer
		/// </summary>
		/********************************************************************/
		public static void Av_Channel_Name_BPrint(AVBPrint bp, AvChannel channel_Id)//XX 86
		{
			if ((channel_Id >= AvChannel.Ambisonic_Base) && (channel_Id <= AvChannel.Ambisonic_End))
				BPrint.Av_BPrintf(bp, "AMBI%d", channel_Id - AvChannel.Ambisonic_Base);
			else if (((c_uint)channel_Id < Macros.FF_Array_Elems(channel_Names)) && channel_Names[(c_int)channel_Id].Name.IsNotNull)
				BPrint.Av_BPrintf(bp, "%s", channel_Names[(c_int)channel_Id].Name);
			else if (channel_Id == AvChannel.None)
				BPrint.Av_BPrintf(bp, "NONE");
			else if (channel_Id == AvChannel.Unknown)
				BPrint.Av_BPrintf(bp, "UNK");
			else if (channel_Id == AvChannel.Unused)
				BPrint.Av_BPrintf(bp, "UNSD");
			else
				BPrint.Av_BPrintf(bp, "USR%d", (c_int)channel_Id);
		}



		/********************************************************************/
		/// <summary>
		/// Get a human readable string in an abbreviated form describing a
		/// given channel.
		/// This is the inverse function of av_channel_from_string()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Name(CPointer<char> buf, size_t buf_Size, AvChannel channel_Id)//XX 104
		{
			if (buf.IsNull && (buf_Size != 0))
				return Error.EINVAL;

			BPrint.Av_BPrint_Init_For_Buffer(out AVBPrint bp, buf, (c_uint)buf_Size);
			Av_Channel_Name_BPrint(bp, channel_Id);

			if (bp.Len >= c_int.MaxValue)
				return Error.ERANGE;

			return (c_int)(bp.Len + 1);
		}



		/********************************************************************/
		/// <summary>
		/// BPrint variant of av_channel_description().
		///
		/// Note the string will be appended to the bprint buffer
		/// </summary>
		/********************************************************************/
		public static void Av_Channel_Description_BPrint(AVBPrint bp, AvChannel channel_Id)//XX 119
		{
			if ((channel_Id >= AvChannel.Ambisonic_Base) && (channel_Id <= AvChannel.Ambisonic_End))
				BPrint.Av_BPrintf(bp, "ambisonic ACN %d", channel_Id - AvChannel.Ambisonic_Base);
			else if (((c_uint)channel_Id < Macros.FF_Array_Elems(channel_Names)) && channel_Names[(c_int)channel_Id].Description.IsNotNull)
				BPrint.Av_BPrintf(bp, "%s", channel_Names[(c_int)channel_Id].Description);
			else if (channel_Id == AvChannel.None)
				BPrint.Av_BPrintf(bp, "none");
			else if (channel_Id == AvChannel.Unknown)
				BPrint.Av_BPrintf(bp, "unknown");
			else if (channel_Id == AvChannel.Unused)
				BPrint.Av_BPrintf(bp, "unused");
			else
				BPrint.Av_BPrintf(bp, "user %d", (c_int)channel_Id);
		}



		/********************************************************************/
		/// <summary>
		/// Get a human readable string describing a given channel
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Description(CPointer<char> buf, size_t buf_Size, AvChannel channel_Id)//XX 137
		{
			if (buf.IsNull && (buf_Size != 0))
				return Error.EINVAL;

			BPrint.Av_BPrint_Init_For_Buffer(out AVBPrint bp, buf, (c_uint)buf_Size);
			Av_Channel_Description_BPrint(bp, channel_Id);

			if (bp.Len >= c_int.MaxValue)
				return Error.ERANGE;

			return (c_int)(bp.Len + 1);
		}



		/********************************************************************/
		/// <summary>
		/// This is the inverse function of av_channel_name()
		/// </summary>
		/********************************************************************/
		public static AvChannel Av_Channel_From_String(CPointer<char> str)//XX 152
		{
			c_int i;
			CPointer<char> endPtr = str;
			AvChannel id = AvChannel.None;

			if (CString.strncmp(str, "AMBI", 4) == 0)
			{
				i = (c_int)CString.strtol(str + 4, out _, 0, out _);

				if ((i < 0) || (i > (AvChannel.Ambisonic_End - AvChannel.Ambisonic_Base)))
					return AvChannel.None;

				return AvChannel.Ambisonic_Base + i;
			}

			for (i = 0; i < (c_int)Macros.FF_Array_Elems(channel_Names); i++)
			{
				if (channel_Names[i].Name.IsNotNull && (CString.strcmp(str, channel_Names[i].Name) == 0))
					return (AvChannel)i;
			}

			if (CString.strcmp(str, "UNK") == 0)
				return AvChannel.Unknown;

			if (CString.strcmp(str, "UNSD") == 0)
				return AvChannel.Unused;

			if (CString.strncmp(str, "USR", 3) == 0)
			{
				CPointer<char> p = str + 3;
				id = (AvChannel)CString.strtol(p, out endPtr, 0, out _);
			}

			if ((id >= AvChannel.Front_Left) && (endPtr[0] == 0))
				return id;

			return AvChannel.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Custom_Init(AvChannelLayout channel_Layout, c_int nb_Channels)//XX 232
		{
			if (nb_Channels <= 0)
				return Error.EINVAL;

			CPointer<AvChannelCustom> map = Mem.Av_CAllocObj<AvChannelCustom>((size_t)nb_Channels);
			if (map.IsNull)
				return Error.ENOMEM;

			for (c_int i = 0; i < nb_Channels; i++)
				map[i].Id = AvChannel.Unknown;

			channel_Layout.Order = AvChannelOrder.Custom;
			channel_Layout.Nb_Channels = nb_Channels;
			channel_Layout.U.Map = map;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a native channel layout from a bitmask indicating
		/// which channels are present
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_From_Mask(AvChannelLayout channel_Layout, AvChannelMask mask)//XX 252
		{
			if (mask == AvChannelMask.None)
				return Error.EINVAL;

			channel_Layout.Order = AvChannelOrder.Native;
			channel_Layout.Nb_Channels = Common.Av_PopCount64((uint64_t)mask);
			channel_Layout.U.Mask = mask;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a channel layout from a given string description.
		/// The input string can be represented by:
		///  - the formal channel layout name (returned by
		///    av_channel_layout_describe())
		///  - single or multiple channel names (returned by
		///    av_channel_name(), eg. "FL", or concatenated with "+", each
		///    optionally containing a custom name after a "@", eg.
		///    "FL@Left+FR@Right+LFE")
		///  - a decimal or hexadecimal value of a native channel layout
		///    (eg. "4" or "0x4")
		///  - the number of channels with default layout (eg. "4c")
		///  - the number of unordered channels (eg. "4C" or "4 channels")
		///  - the ambisonic order followed by optional non-diegetic channels
		///    (eg. "ambisonic 2+stereo")
		/// On error, the channel layout will remain uninitialized, but not
		/// necessarily untouched
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_From_String(ref AvChannelLayout channel_Layout, CPointer<char> str)//XX 312
		{
			c_int ret;
			c_int channels = 0, nb_Channels = 0;
			CPointer<char> end;
			AvChannelMask mask = 0;

			// Channel layout names
			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(channel_Layout_Map); i++)
			{
				if (channel_Layout_Map[i].Name.IsNotNull && (CString.strcmp(str, channel_Layout_Map[i].Name) == 0))
				{
					channel_Layout = channel_Layout_Map[i].Layout.MakeDeepClone();

					return 0;
				}
			}

			// This function is a channel layout initializer, so we have to
			// zero-initialize before we start setting fields individually
			channel_Layout.Clear();

			// Ambisonic
			if (CString.strncmp(str, "ambisonic ", 10) == 0)
			{
				CPointer<char> p = str + 10;
				AvChannelLayout extra = new AvChannelLayout();

				c_int order = (c_int)CString.strtol(p, out CPointer<char> endPtr, 0, out _);

				if ((order < 0) || ((order + 1) > (c_int.MaxValue / (order + 1))) || ((endPtr[0] != 0) && (endPtr[0] != '+')))
					return Error.EINVAL;

				channel_Layout.Order = AvChannelOrder.Ambisonic;
				channel_Layout.Nb_Channels = (order + 1) * (order + 1);

				if (endPtr[0] != 0)
				{
					ret = Av_Channel_Layout_From_String(ref extra, endPtr + 1);
					if (ret < 0)
						return ret;

					if (extra.Nb_Channels >= (c_int.MaxValue - channel_Layout.Nb_Channels))
					{
						Av_Channel_Layout_Uninit(extra);

						return Error.EINVAL;
					}

					if (extra.Order == AvChannelOrder.Native)
						channel_Layout.U.Mask = extra.U.Mask;
					else
					{
						channel_Layout.Order = AvChannelOrder.Custom;
						channel_Layout.U.Map = Mem.Av_CAllocObj<AvChannelCustom>((size_t)(channel_Layout.Nb_Channels + extra.Nb_Channels));

						if (channel_Layout.U.Map.IsNull)
						{
							Av_Channel_Layout_Uninit(extra);

							return Error.ENOMEM;
						}

						for (c_int i = 0; i < channel_Layout.Nb_Channels; i++)
							channel_Layout.U.Map[i].Id = AvChannel.Ambisonic_Base + i;

						for (c_int i = 0; i < extra.Nb_Channels; i++)
						{
							AvChannel ch = Av_Channel_Layout_Channel_From_Index(extra, (c_uint)i);

							if (Chan_Is_Ambi(ch))
							{
								Av_Channel_Layout_Uninit(channel_Layout);
								Av_Channel_Layout_Uninit(extra);

								return Error.EINVAL;
							}

							channel_Layout.U.Map[channel_Layout.Nb_Channels + i].Id = ch;

							if ((extra.Order == AvChannelOrder.Custom) && (extra.U.Map[i].Name[0] != 0))
								AvString.Av_Strlcpy(channel_Layout.U.Map[channel_Layout.Nb_Channels + i].Name, extra.U.Map[i].Name, (size_t)channel_Layout.U.Map[channel_Layout.Nb_Channels + i].Name.Length);
						}
					}

					channel_Layout.Nb_Channels += extra.Nb_Channels;

					Av_Channel_Layout_Uninit(extra);
				}

				return 0;
			}

			CPointer<char> chList = Mem.Av_StrDup(str);

			if (chList.IsNull)
				return Error.ENOMEM;

			// Channel names
			object[] results = [ nb_Channels, chList ];
			c_int matches = AvSScanF.Av_SScanF(str, "%d channels (%[^)]", results);
			nb_Channels = (c_int)results[0];
			chList = (CPointer<char>)results[1];

			ret = Parse_Channel_List(channel_Layout, chList);
			Mem.Av_FreeP(ref chList);

			if ((ret < 0) && (ret != Error.EINVAL))
				return ret;

			if (ret >= 0)
			{
				end = CString.strchr(str, ')');

				if ((matches == 2) && ((nb_Channels != channel_Layout.Nb_Channels) || end.IsNull || (end[1, 1] != 0)))
				{
					Av_Channel_Layout_Uninit(channel_Layout);

					return Error.EINVAL;
				}

				return 0;
			}

			mask = (AvChannelMask)CString.strtoull(str, out end, 0, out bool error);

			// Channel layout mask
			if (!error && (end[0] == 0) && CString.strchr(str, '-').IsNull && (mask != AvChannelMask.None))
			{
				Av_Channel_Layout_From_Mask(channel_Layout, mask);

				return 0;
			}

			channels = (c_int)CString.strtol(str, out end, 10, out error);

			// Number of channels
			if (!error && (CString.strcmp(end, "c") == 0) && (channels > 0))
			{
				Av_Channel_Layout_Default(ref channel_Layout, channels);

				if (channel_Layout.Order == AvChannelOrder.Native)
					return 0;
			}

			// Number of unordered channels
			if (!error && ((CString.strcmp(end, "C") == 0) || (CString.strcmp(end, " channels") == 0)) && (channels > 0))
			{
				channel_Layout.Order = AvChannelOrder.Unspec;
				channel_Layout.Nb_Channels = channels;

				return 0;
			}

			return Error.EINVAL;
		}



		/********************************************************************/
		/// <summary>
		/// Free any allocated data in the channel layout and reset the
		/// channel count to 0
		/// </summary>
		/********************************************************************/
		public static void Av_Channel_Layout_Uninit(AvChannelLayout channel_Layout)//XX 442
		{
			if (channel_Layout.Order == AvChannelOrder.Custom)
				Mem.Av_FreeP(ref channel_Layout.U.Map);

			channel_Layout.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Make a copy of a channel layout. This differs from just assigning
		/// src to dst in that it allocates and copies the map for
		/// AV_CHANNEL_ORDER_CUSTOM.
		///
		/// Note the destination channel_layout will be always uninitialized
		/// before copy
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Copy(AvChannelLayout dst, AvChannelLayout src)//XX 449
		{
			Av_Channel_Layout_Uninit(dst);

			src.CopyTo(dst);

			if (src.Order == AvChannelOrder.Custom)
			{
				dst.U.Map = Mem.Av_MAlloc_ArrayObj<AvChannelCustom>((size_t)src.Nb_Channels);
				if (dst.U.Map.IsNull)
					return Error.ENOMEM;

				for (c_int i = 0; i < src.Nb_Channels; i++)
					dst.U.Map[i] = src.U.Map[i].MakeDeepClone();
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Ambisonic_Order(AvChannelLayout channel_Layout)//XX 485
		{
			if ((channel_Layout.Order != AvChannelOrder.Ambisonic) && (channel_Layout.Order != AvChannelOrder.Custom))
				return Error.EINVAL;

			c_int highest_Ambi = -1;

			if (channel_Layout.Order == AvChannelOrder.Ambisonic)
				highest_Ambi = channel_Layout.Nb_Channels - Common.Av_PopCount64((uint64_t)channel_Layout.U.Mask) - 1;
			else
			{
				CPointer<AvChannelCustom> map = channel_Layout.U.Map;

				for (c_int i = 0; i < channel_Layout.Nb_Channels; i++)
				{
					bool is_Ambi = Chan_Is_Ambi(map[i].Id);

					// Ambisonic following non-ambisonic
					if ((i > 0) && is_Ambi && !Chan_Is_Ambi(map[i - 1].Id))
						return Error.EINVAL;

					// Non-default ordering
					if (is_Ambi && (map[i].Id - AvChannel.Ambisonic_Base != i))
						return Error.EINVAL;

					if (Chan_Is_Ambi(map[i].Id))
						highest_Ambi = i;
				}
			}

			// No ambisonic channels
			if (highest_Ambi < 0)
				return Error.EINVAL;

			c_int order = (c_int)CMath.floor(CMath.sqrt(highest_Ambi));

			// Incomplete order - some harmonics are missing
			if (((order + 1) * (order + 1)) != (highest_Ambi + 1))
				return Error.EINVAL;

			return order;
		}



		/********************************************************************/
		/// <summary>
		/// bprint variant of av_channel_layout_describe()
		///
		/// Note the string will be appended to the bprint buffer
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Describe_BPrint(AvChannelLayout channel_Layout, AVBPrint bp)//XX 599
		{
			switch (channel_Layout.Order)
			{
				case AvChannelOrder.Native:
				{
					for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(channel_Layout_Map); i++)
					{
						if (channel_Layout.U.Mask == channel_Layout_Map[i].Layout.U.Mask)
						{
							BPrint.Av_BPrintf(bp, "%s", channel_Layout_Map[i].Name);
							return 0;
						}
					}

					goto case AvChannelOrder.Custom;
				}

				case AvChannelOrder.Custom:
				{
					if (channel_Layout.Order == AvChannelOrder.Custom)
					{
						int64_t mask;

						c_int res = Try_Describe_Ambisonic(bp, channel_Layout);

						if (res >= 0)
							return 0;

						if ((Has_Channel_Names(channel_Layout) == 0) && ((mask = Masked_Description(channel_Layout, 0)) > 0))
						{
							AvChannelLayout native = new AvChannelLayout
							{
								Order = AvChannelOrder.Native,
								Nb_Channels = Common.Av_PopCount64((uint64_t)mask),
							};
							native.U.Mask = (AvChannelMask)mask;

							return Av_Channel_Layout_Describe_BPrint(native, bp);
						}
					}

					if (channel_Layout.Nb_Channels != 0)
						BPrint.Av_BPrintf(bp, "%d channels (", channel_Layout.Nb_Channels);

					for (c_int i = 0; i < channel_Layout.Nb_Channels; i++)
					{
						AvChannel ch = Av_Channel_Layout_Channel_From_Index(channel_Layout, (c_uint)i);

						if (i != 0)
							BPrint.Av_BPrintf(bp, "+");

						Av_Channel_Name_BPrint(bp, ch);

						if ((channel_Layout.Order == AvChannelOrder.Custom) && (channel_Layout.U.Map[i].Name[0] != 0))
							BPrint.Av_BPrintf(bp, "@%s", channel_Layout.U.Map[i].Name);
					}

					if (channel_Layout.Nb_Channels != 0)
					{
						BPrint.Av_BPrintf(bp, ")");

						return 0;
					}

					goto case AvChannelOrder.Unspec;
				}

				case AvChannelOrder.Unspec:
				{
					BPrint.Av_BPrintf(bp, "%d channels", channel_Layout.Nb_Channels);

					return 0;
				}

				case AvChannelOrder.Ambisonic:
					return Try_Describe_Ambisonic(bp, channel_Layout);

				default:
					return Error.EINVAL;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get a human-readable string describing the channel layout
		/// properties. The string will be in the same format that is
		/// accepted by av_channel_layout_from_string(), allowing to rebuild
		/// the same channel layout, except for opaque pointers
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Describe(AvChannelLayout channel_Layout, CPointer<char> buf, size_t buf_Size)//XX 653
		{
			if (buf.IsNull && (buf_Size != 0))
				return Error.EINVAL;

			BPrint.Av_BPrint_Init_For_Buffer(out AVBPrint bp, buf, (c_uint)buf_Size);
			c_int ret = Av_Channel_Layout_Describe_BPrint(channel_Layout, bp);

			if (ret < 0)
				return ret;

			if (bp.Len >= c_int.MaxValue)
				return Error.ERANGE;

			return (c_int)(bp.Len + 1);
		}



		/********************************************************************/
		/// <summary>
		/// Get the channel with the given index in a channel layout
		/// </summary>
		/********************************************************************/
		public static AvChannel Av_Channel_Layout_Channel_From_Index(AvChannelLayout channel_Layout, c_uint idx)//XX 673
		{
			if (idx >= channel_Layout.Nb_Channels)
				return AvChannel.None;

			switch (channel_Layout.Order)
			{
				case AvChannelOrder.Custom:
					return channel_Layout.U.Map[idx].Id;

				case AvChannelOrder.Ambisonic:
				{
					c_int ambi_Channels = channel_Layout.Nb_Channels - Common.Av_PopCount64((uint64_t)channel_Layout.U.Mask);

					if (idx < ambi_Channels)
						return (AvChannel)((c_uint)AvChannel.Ambisonic_Base + idx);

					idx -= (c_uint)ambi_Channels;
					goto case AvChannelOrder.Native;
				}

				case AvChannelOrder.Native:
				{
					for (c_int i = 0; i < 64; i++)
					{
						if ((((AvChannelMask)(1U << i) & channel_Layout.U.Mask) != 0) && (idx-- == 0))
							return (AvChannel)i;
					}

					break;
				}
			}

			return AvChannel.None;
		}



		/********************************************************************/
		/// <summary>
		/// Get a channel described by the given string.
		///
		/// This function accepts channel names in the same format as
		/// av_channel_from_string()
		/// </summary>
		/********************************************************************/
		public static AvChannel Av_Channel_Layout_Channel_From_String(AvChannelLayout channel_Layout, CPointer<char> str)//XX 702
		{
			c_int index = Av_Channel_Layout_Index_From_String(channel_Layout, str);

			if (index < 0)
				return AvChannel.None;

			return Av_Channel_Layout_Channel_From_Index(channel_Layout, (c_uint)index);
		}



		/********************************************************************/
		/// <summary>
		/// Get the index of a given channel in a channel layout. In case
		/// multiple channels are found, only the first match will be
		/// returned
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Index_From_Channel(AvChannelLayout channel_Layout, AvChannel channel)//XX 713
		{
			if (channel == AvChannel.None)
				return Error.EINVAL;

			switch (channel_Layout.Order)
			{
				case AvChannelOrder.Custom:
				{
					for (c_int i = 0; i < channel_Layout.Nb_Channels; i++)
					{
						if (channel_Layout.U.Map[i].Id == channel)
							return i;
					}

					return Error.EINVAL;
				}

				case AvChannelOrder.Ambisonic:
				case AvChannelOrder.Native:
				{
					uint64_t mask = (uint64_t)channel_Layout.U.Mask;
					c_int ambi_Channels = channel_Layout.Nb_Channels - Common.Av_PopCount64(mask);

					if ((channel_Layout.Order == AvChannelOrder.Ambisonic) && (channel >= AvChannel.Ambisonic_Base))
					{
						if ((channel - AvChannel.Ambisonic_Base) >= ambi_Channels)
							return Error.EINVAL;

						return channel - AvChannel.Ambisonic_Base;
					}

					if (((c_uint)channel > 63) || ((mask & (1UL << (c_int)channel)) == 0))
						return Error.EINVAL;

					mask &= (1UL << (c_int)channel) - 1;

					return Common.Av_PopCount64(mask) + ambi_Channels;
				}

				default:
					return Error.EINVAL;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get the index in a channel layout of a channel described by the
		/// given string. In case multiple channels are found, only the first
		/// match will be returned.
		///
		/// This function accepts channel names in the same format as
		/// av_channel_from_string()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Index_From_String(AvChannelLayout channel_Layout, CPointer<char> str)//XX 747
		{
			CPointer<char> chName;
			AvChannel ch = AvChannel.None;

			switch (channel_Layout.Order)
			{
				case AvChannelOrder.Custom:
				{
					chName = CString.strstr(str, "@");

					if (chName.IsNotNull)
					{
						CPointer<char> buf = new CPointer<char>(16);

						chName++;
						AvString.Av_Strlcpy(buf, str, (size_t)Macros.FFMin(buf.Length, chName - str));

						if (chName[0] == 0)
							chName.SetToNull();

						ch = Av_Channel_From_String(buf);

						if ((ch == AvChannel.None) && (buf[0] != 0))
							return Error.EINVAL;
					}

					for (c_int i = 0; chName.IsNotNull && (i < channel_Layout.Nb_Channels); i++)
					{
						if ((CString.strcmp(chName, channel_Layout.U.Map[i].Name) == 0) && ((ch == AvChannel.None) || (ch == channel_Layout.U.Map[i].Id)))
							return i;
					}

					goto case AvChannelOrder.Native;
				}

				case AvChannelOrder.Ambisonic:
				case AvChannelOrder.Native:
				{
					ch = Av_Channel_From_String(str);

					if (ch == AvChannel.None)
						return Error.EINVAL;

					return Av_Channel_Layout_Index_From_Channel(channel_Layout, ch);
				}
			}

			return Error.EINVAL;
		}



		/********************************************************************/
		/// <summary>
		/// Check whether a channel layout is valid, i.e. can possibly
		/// describe audio data
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Check(AvChannelLayout channel_Layout)//XX 783
		{
			if (channel_Layout.Nb_Channels <= 0)
				return 0;

			switch (channel_Layout.Order)
			{
				case AvChannelOrder.Native:
					return Common.Av_PopCount64((uint64_t)channel_Layout.U.Mask) == channel_Layout.Nb_Channels ? 1 : 0;

				case AvChannelOrder.Custom:
				{
					if (channel_Layout.U.Map.IsNull)
						return 0;

					for (c_int i = 0; i < channel_Layout.Nb_Channels; i++)
					{
						if (channel_Layout.U.Map[i].Id == AvChannel.None)
							return 0;
					}

					return 1;
				}

				case AvChannelOrder.Ambisonic:
				{
					// If non-diegetic channels are present, ensure they are taken into account
					return Common.Av_PopCount64((uint64_t)channel_Layout.U.Mask) < channel_Layout.Nb_Channels ? 1 : 0;
				}

				case AvChannelOrder.Unspec:
					return 1;

				default:
					return 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check whether two channel layouts are semantically the same,
		/// i.e. the same channels are present on the same positions in
		/// both.
		///
		/// If one of the channel layouts is AV_CHANNEL_ORDER_UNSPEC, while
		/// the other is not, they are considered to be unequal. If both are
		/// AV_CHANNEL_ORDER_UNSPEC, they are considered equal iff the
		/// channel counts are the same in both
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Compare(AvChannelLayout chl, AvChannelLayout chl1)//XX 809
		{
			// Different channel counts -> not equal
			if (chl.Nb_Channels != chl1.Nb_Channels)
				return 1;

			// If only one is unspecified -> not equal
			if ((chl.Order == AvChannelOrder.Unspec) != (chl1.Order == AvChannelOrder.Unspec))
				return 1;
			// Both are unspecified -> equal
			else if (chl.Order == AvChannelOrder.Unspec)
				return 0;

			// Can compare masks directly
			if (((chl.Order == AvChannelOrder.Native) || (chl.Order == AvChannelOrder.Ambisonic)) && (chl.Order == chl1.Order))
				return chl.U.Mask != chl1.U.Mask ? 1 : 0;

			// Compare channel by channel
			for (c_int i = 0; i < chl.Nb_Channels; i++)
			{
				if (Av_Channel_Layout_Channel_From_Index(chl, (c_uint)i) != Av_Channel_Layout_Channel_From_Index(chl1, (c_uint)i))
					return 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the default channel layout for a given number of channels
		/// </summary>
		/********************************************************************/
		public static void Av_Channel_Layout_Default(ref AvChannelLayout ch_Layout, c_int nb_Channels)//XX 839
		{
			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(channel_Layout_Map); i++)
			{
				if (nb_Channels == channel_Layout_Map[i].Layout.Nb_Channels)
				{
					ch_Layout = channel_Layout_Map[i].Layout.MakeDeepClone();
					return;
				}
			}

			ch_Layout.Order = AvChannelOrder.Unspec;
			ch_Layout.Nb_Channels = nb_Channels;
		}



		/********************************************************************/
		/// <summary>
		/// Iterate over all standard channel layouts
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvChannelLayout> Av_Channel_Layout_Standard()//XX 852
		{
			foreach (Channel_Layout_Name ch_Layout in channel_Layout_Map)
				yield return ch_Layout.Layout;
		}



		/********************************************************************/
		/// <summary>
		/// Find out what channels from a given set are present in a channel
		/// layout, without regard for their positions
		/// </summary>
		/********************************************************************/
		public static uint64_t Av_Channel_Layout_Subset(AvChannelLayout channel_Layout, AvChannelMask mask)//XX 865
		{
			uint64_t ret = 0;

			switch (channel_Layout.Order)
			{
				case AvChannelOrder.Native:
				case AvChannelOrder.Ambisonic:
					return (uint64_t)(channel_Layout.U.Mask & mask);

				case AvChannelOrder.Custom:
				{
					for (c_int i = 0; i < 64; i++)
					{
						if ((((uint64_t)mask & (1UL << i)) != 0) && (Av_Channel_Layout_Index_From_Channel(channel_Layout, (AvChannel)i) >= 0))
							ret |= (1UL << i);
					}

					break;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Change the AVChannelOrder of a channel layout.
		///
		/// Change of AVChannelOrder can be either lossless or lossy. In case
		/// of a lossless conversion all the channel designations and the
		/// associated channel names (if any) are kept. On a lossy conversion
		/// the channel names and channel designations might be lost
		/// depending on the capabilities of the desired AVChannelOrder. Note
		/// that some conversions are simply not possible in which case this
		/// function returns AVERROR(ENOSYS).
		///
		/// The following conversions are supported:
		///
		/// Any       -> Custom     : Always possible, always lossless
		/// Any       -> Unspecified: Always possible, lossless if channel
		///   designations are all unknown and channel names are not used,
		///   lossy otherwise
		/// Custom    -> Ambisonic  : Possible if it contains ambisonic
		///   channels with optional non-diegetic channels in the end. Lossy
		///   if the channels have custom names, lossless otherwise
		/// Custom    -> Native     : Possible if it contains native channels
		///   in native order. Lossy if the channels have custom names,
		///   lossless otherwise
		///
		/// On error this function keeps the original channel layout
		/// untouched
		/// </summary>
		/********************************************************************/
		public static c_int Av_Channel_Layout_Retype(AvChannelLayout channel_Layout, AvChannelOrder order, AvChannelLayoutRetypeFlag flags)//XX 885
		{
			c_int allow_Lossy = (flags & AvChannelLayoutRetypeFlag.Lossless) == 0 ? 1 : 0;
			c_int lossy;

			if (Av_Channel_Layout_Check(channel_Layout) == 0)
				return Error.EINVAL;

			if ((flags & AvChannelLayoutRetypeFlag.Canonical) != 0)
				order = Canonical_Order(channel_Layout);

			if (channel_Layout.Order == order)
				return 0;

			switch (order)
			{
				case AvChannelOrder.Unspec:
				{
					c_int nb_Channels = channel_Layout.Nb_Channels;

					if (channel_Layout.Order == AvChannelOrder.Custom)
					{
						lossy = 0;

						for (c_int i = 0; i < nb_Channels; i++)
						{
							if ((channel_Layout.U.Map[i].Id != AvChannel.Unknown) || (channel_Layout.U.Map[i].Name[0] != 0))
							{
								lossy = 1;
								break;
							}
						}
					}
					else
						lossy = 1;

					if ((lossy == 0) || (allow_Lossy != 0))
					{
						IOpaque opaque = channel_Layout.Opaque;

						Av_Channel_Layout_Uninit(channel_Layout);

						channel_Layout.Order = AvChannelOrder.Unspec;
						channel_Layout.Nb_Channels = nb_Channels;
						channel_Layout.Opaque = opaque;

						return lossy;
					}

					return Error.ENOSYS;
				}

				case AvChannelOrder.Native:
				{
					if (channel_Layout.Order == AvChannelOrder.Custom)
					{
						int64_t mask = Masked_Description(channel_Layout, 0);

						if (mask < 0)
							return Error.ENOSYS;

						lossy = Has_Channel_Names(channel_Layout);

						if ((lossy == 0) || (allow_Lossy != 0))
						{
							IOpaque opaque = channel_Layout.Opaque;

							Av_Channel_Layout_Uninit(channel_Layout);

							Av_Channel_Layout_From_Mask(channel_Layout, (AvChannelMask)mask);
							channel_Layout.Opaque = opaque;

							return lossy;
						}
					}

					return Error.ENOSYS;
				}

				case AvChannelOrder.Custom:
				{
					AvChannelLayout custom = new AvChannelLayout();

					c_int ret = Av_Channel_Layout_Custom_Init(custom, channel_Layout.Nb_Channels);
					IOpaque opaque = channel_Layout.Opaque;

					if (ret < 0)
						return ret;

					if (channel_Layout.Order != AvChannelOrder.Unspec)
					{
						for (c_int i = 0; i < channel_Layout.Nb_Channels; i++)
							custom.U.Map[i].Id = Av_Channel_Layout_Channel_From_Index(channel_Layout, (c_uint)i);
					}

					Av_Channel_Layout_Uninit(channel_Layout);
					custom.CopyTo(channel_Layout);
					channel_Layout.Opaque = opaque;

					return 0;
				}

				case AvChannelOrder.Ambisonic:
				{
					if (channel_Layout.Order == AvChannelOrder.Custom)
					{
						c_int nb_Channels = channel_Layout.Nb_Channels;
						c_int order2 = Av_Channel_Layout_Ambisonic_Order(channel_Layout);

						if (order2 < 0)
							return Error.ENOSYS;

						int64_t mask = Masked_Description(channel_Layout, (order2 + 1) * (order2 + 1));

						if (mask < 0)
							return Error.ENOSYS;

						lossy = Has_Channel_Names(channel_Layout);

						if ((lossy == 0) || (allow_Lossy != 0))
						{
							IOpaque opaque = channel_Layout.Opaque;

							Av_Channel_Layout_Uninit(channel_Layout);

							channel_Layout.Order = AvChannelOrder.Ambisonic;
							channel_Layout.Nb_Channels = nb_Channels;
							channel_Layout.U.Mask = (AvChannelMask)mask;
							channel_Layout.Opaque = opaque;

							return lossy;
						}
					}

					return Error.ENOSYS;
				}

				default:
					return Error.EINVAL;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static AvChannelLayout Av_Channel_Layout_Mask(c_int nb, AvChannelMask m)
		{
			AvChannelLayout channel_Layout = new AvChannelLayout
			{
				Order = AvChannelOrder.Native,
				Nb_Channels = nb,
				Opaque = null
			};
			channel_Layout.U.Mask = m;

			return channel_Layout;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Chan_Is_Ambi(AvChannel x)//XX 39
		{
			return (x >= AvChannel.Ambisonic_Base) && (x <= AvChannel.Ambisonic_End);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Channel_List(AvChannelLayout ch_Layout, CPointer<char> str)//XX 265
		{
			c_int ret;
			c_int nb_Channels = 0;
			CPointer<AvChannelCustom> map = null;
			AvChannelCustom custom = new AvChannelCustom();

			while (str[0] != 0)
			{
				ret = Opt.Av_Opt_Get_Key_Value(ref str, "@".ToCharPointer(), "+".ToCharPointer(), AvOptFlag2.Implicit_Key, out CPointer<char> channel, out CPointer<char> chName);

				if (ret < 0)
				{
					Mem.Av_FreeP(ref map);

					return ret;
				}

				if (str[0] != 0)
					str++;	// Skip separator

				if (channel.IsNull)
				{
					channel = chName;
					chName.SetToNull();
				}

				AvString.Av_Strlcpy(custom.Name, chName.IsNotNull ? chName : CString.Empty, (size_t)custom.Name.Length);
				custom.Id = Av_Channel_From_String(channel);

				Mem.Av_Free(channel);
				Mem.Av_Free(chName);

				if (custom.Id == AvChannel.None)
				{
					Mem.Av_FreeP(ref map);

					return Error.EINVAL;
				}

				Mem.Av_DynArray2_AddObj(ref map, ref nb_Channels, custom);

				if (map == null)
					return Error.ENOMEM;
			}

			if (nb_Channels == 0)
				return Error.EINVAL;

			ch_Layout.Order = AvChannelOrder.Custom;
			ch_Layout.U.Map = map;
			ch_Layout.Nb_Channels = nb_Channels;

			ret = Av_Channel_Layout_Retype(ch_Layout, AvChannelOrder.Unspec, AvChannelLayoutRetypeFlag.Canonical);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int64_t Masked_Description(AvChannelLayout channel_Layout, c_int start_Channel)//XX 462
		{
			uint64_t mask = 0;

			for (c_int i = start_Channel; i < channel_Layout.Nb_Channels; i++)
			{
				AvChannel ch = channel_Layout.U.Map[i].Id;

				if ((ch >= AvChannel.Front_Left) && (ch < (AvChannel.Binaural_Right + 1)) && (mask < (1UL << (c_int)ch)))
					mask |= (1UL << (c_int)ch);
				else
					return Error.EINVAL;
			}

			return (int64_t)mask;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Has_Channel_Names(AvChannelLayout channel_Layout)//XX 475
		{
			if (channel_Layout.Order != AvChannelOrder.Custom)
				return 0;

			for (c_int i = 0; i < channel_Layout.Nb_Channels; i++)
			{
				if (channel_Layout.U.Map[i].Name[0] != 0)
					return 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvChannelOrder Canonical_Order(AvChannelLayout channel_Layout)//XX 527
		{
			c_int has_Known_Channel = 0;

			if (channel_Layout.Order != AvChannelOrder.Custom)
				return channel_Layout.Order;

			if (Has_Channel_Names(channel_Layout) != 0)
				return AvChannelOrder.Custom;

			for (c_int i = 0; (i < channel_Layout.Nb_Channels) && (has_Known_Channel == 0); i++)
			{
				if (channel_Layout.U.Map[i].Id != AvChannel.Unknown)
					has_Known_Channel = 1;
			}

			if (has_Known_Channel == 0)
				return AvChannelOrder.Unspec;

			if (Masked_Description(channel_Layout, 0) > 0)
				return AvChannelOrder.Native;

			c_int order = Av_Channel_Layout_Ambisonic_Order(channel_Layout);

			if ((order >= 0) && (Masked_Description(channel_Layout, (order + 1) * (order + 1))) >= 0)
				return AvChannelOrder.Ambisonic;

			return AvChannelOrder.Custom;
		}



		/********************************************************************/
		/// <summary>
		/// If the custom layout is n-th order standard-order ambisonic,
		/// with optional extra non-diegetic channels at the end, write its
		/// string description in bp.
		/// Return a negative error code otherwise
		/// </summary>
		/********************************************************************/
		private static c_int Try_Describe_Ambisonic(AVBPrint bp, AvChannelLayout channel_Layout)//XX 559
		{
			c_int order = Av_Channel_Layout_Ambisonic_Order(channel_Layout);

			if (order < 0)
				return order;

			BPrint.Av_BPrintf(bp, "ambisonic %d", order);

			// Extra channels present
			c_int nb_Ambi_Channels = (order + 1) * (order + 1);

			if (nb_Ambi_Channels < channel_Layout.Nb_Channels)
			{
				AvChannelLayout extra = new AvChannelLayout();

				if (channel_Layout.Order == AvChannelOrder.Ambisonic)
				{
					extra.Order = AvChannelOrder.Native;
					extra.Nb_Channels = Common.Av_PopCount64((uint64_t)channel_Layout.U.Mask);
					extra.U.Mask = channel_Layout.U.Mask;
				}
				else
				{
					int64_t mask;

					if ((Has_Channel_Names(channel_Layout) == 0) && ((mask = Masked_Description(channel_Layout, nb_Ambi_Channels)) > 0))
					{
						extra.Order = AvChannelOrder.Native;
						extra.Nb_Channels = Common.Av_PopCount64((uint64_t)mask);
						extra.U.Mask = (AvChannelMask)mask;
					}
					else
					{
						extra.Order = AvChannelOrder.Custom;
						extra.Nb_Channels = channel_Layout.Nb_Channels - nb_Ambi_Channels;
						extra.U.Map = channel_Layout.U.Map + nb_Ambi_Channels;
					}
				}

				BPrint.Av_BPrint_Chars(bp, '+', 1);
				Av_Channel_Layout_Describe_BPrint(extra, bp);

				// Not calling uninit here on extra because we don't own the u.map pointer
			}

			return 0;
		}
		#endregion
	}
}
