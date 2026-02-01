/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_Channel_Layout : TestBase
	{
		private delegate c_int TestDelegate(CPointer<char> buf, size_t buf_Size);
		private delegate void TestBPrintDelegate(AVBPrint bp);

		private static readonly string[] channel_Order_Names = [ "UNSPEC", "NATIVE", "CUSTOM", "AMBI" ];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test()
		{
			RunTest("channel_layout");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			BPrint.Av_BPrint_Init(out AVBPrint bp, 64, BPrint.Av_BPrint_Size_Automatic);
			AvChannelLayout layout = new AvChannelLayout();

			Test_Av_Channel_Layout_Standard(bp);
			Test_Av_Channel_Name(bp);
			Test_Av_Channel_Description(bp);
			Test_Av_Channel_From_String();

			Test_Native_Layouts(bp, ref layout);
			Test_Custom_Layouts(bp, ref layout);
			Test_Ambisonic_Layouts(bp, ref layout);

			Channel_Layout.Av_Channel_Layout_Uninit(layout);

			Test_Av_Channel_Layout_Retype(bp, ref layout);

			BPrint.Av_BPrint_Finalize(bp, out _);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Channel_Layout_Standard(AVBPrint bp)
		{
			printf("Testing av_channel_layout_standard\n");

			foreach (AvChannelLayout playout in Channel_Layout.Av_Channel_Layout_Standard())
			{
				Channel_Layout.Av_Channel_Layout_Describe_BPrint(playout, bp);
				printf("%-14s ", bp.Str);

				BPrint.Av_BPrint_Clear(bp);

				for (c_int i = 0; i < 63; i++)
				{
					c_int idx = Channel_Layout.Av_Channel_Layout_Index_From_Channel(playout, (AvChannel)i);

					if (idx >= 0)
					{
						if (idx != 0)
							BPrint.Av_BPrintf(bp, "+");

						Channel_Layout.Av_Channel_Name_BPrint(bp, (AvChannel)i);
					}
				}

				printf("%s\n", bp.Str);

				BPrint.Av_BPrint_Clear(bp);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Channel_Name(AVBPrint bp)
		{
			printf("\nTesting av_channel_name\n");

			_Channel_Name(bp, AvChannel.Front_Left);
			_Channel_Name(bp, AvChannel.Front_Right);
			_Channel_Name(bp, (AvChannel)63);
			_Channel_Name(bp, AvChannel.Ambisonic_Base);
			_Channel_Name(bp, AvChannel.Ambisonic_End);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Channel_Description(AVBPrint bp)
		{
			printf("Testing av_channel_description\n");

			_Channel_Description(bp, AvChannel.Front_Left);
			_Channel_Description(bp, AvChannel.Front_Right);
			_Channel_Description(bp, (AvChannel)63);
			_Channel_Description(bp, AvChannel.Ambisonic_Base);
			_Channel_Description(bp, AvChannel.Ambisonic_End);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Channel_From_String()
		{
			printf("\nTesting av_channel_from_string\n");

			_Channel_From_String("FL");
			_Channel_From_String("FR");
			_Channel_From_String("USR63");
			_Channel_From_String("AMBI0");
			_Channel_From_String("AMBI1023");
			_Channel_From_String("AMBI1024");
			_Channel_From_String("Dummy");
			_Channel_From_String("FL@Foo");
			_Channel_From_String("Foo@FL");
			_Channel_From_String("@FL");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Native_Layouts(AVBPrint bp, ref AvChannelLayout layout)
		{
			printf("\n==Native layouts==\n");

			Test_Native_Av_Channel_Layout_From_String(bp, ref layout);
			Test_Native_Av_Channel_Layout_From_Mask(bp, layout);
			Test_Native_Av_Channel_Layout_Channel_From_Index(bp, layout);
			Test_Native_Av_Channel_Layout_Index_From_Channel(bp, layout);
			Test_Native_Av_Channel_Layout_Channel_From_String(bp, layout);
			Test_Native_Av_Channel_Layout_Index_From_String(bp, layout);
			Test_Native_Av_Channel_Layout_Subset(bp, layout);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Native_Av_Channel_Layout_From_String(AVBPrint bp, ref AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_from_string\n");

			_Channel_Layout_From_String(bp, ref layout, "0x3f");
			_Channel_Layout_From_String(bp, ref layout, "63");
			_Channel_Layout_From_String(bp, ref layout, "6c");
			_Channel_Layout_From_String(bp, ref layout, "6C");
			_Channel_Layout_From_String(bp, ref layout, "6 channels");
			_Channel_Layout_From_String(bp, ref layout, "6 channels (FL+FR+FC+LFE+BL+BR)");
			_Channel_Layout_From_String(bp, ref layout, "FL+FR+FC+LFE+BL+BR");
			_Channel_Layout_From_String(bp, ref layout, "5.1");
			_Channel_Layout_From_String(bp, ref layout, "FL+FR+USR63");
			_Channel_Layout_From_String(bp, ref layout, "FL+FR+FC+LFE+SL+SR");
			_Channel_Layout_From_String(bp, ref layout, "5.1(side)");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Native_Av_Channel_Layout_From_Mask(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_from_mask\n");

			_Channel_Layout_From_Mask(bp, layout, AvChannelMask.FivePointOne);
			printf("With AV_CH_LAYOUT_5POINT1: %25s\n", bp.Str);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Native_Av_Channel_Layout_Channel_From_Index(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_channel_from_index\n");

			_Channel_Layout_Channel_From_Index(layout, bp.Str, 0);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 1);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 2);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 3);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 4);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 5);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 6);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Native_Av_Channel_Layout_Index_From_Channel(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_index_from_channel\n");

			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Front_Left);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Front_Right);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Front_Center);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Low_Frequency);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Side_Left);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Side_Right);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Back_Center);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Native_Av_Channel_Layout_Channel_From_String(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_channel_from_string\n");

			_Channel_Layout_Channel_From_String(layout, bp.Str, "FL");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "FR");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "FC");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "LFE");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "SL");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "SR");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "BC");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "@");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "@Foo");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "FL@Foo");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Native_Av_Channel_Layout_Index_From_String(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_index_from_string\n");

			_Channel_Layout_Index_From_String(layout, bp.Str, "FL");
			_Channel_Layout_Index_From_String(layout, bp.Str, "FR");
			_Channel_Layout_Index_From_String(layout, bp.Str, "FC");
			_Channel_Layout_Index_From_String(layout, bp.Str, "LFE");
			_Channel_Layout_Index_From_String(layout, bp.Str, "SL");
			_Channel_Layout_Index_From_String(layout, bp.Str, "SR");
			_Channel_Layout_Index_From_String(layout, bp.Str, "BC");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Native_Av_Channel_Layout_Subset(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_subset\n");

			_Channel_Layout_Subset(layout, bp.Str, "AV_CH_LAYOUT_STEREO:", AvChannelMask.Stereo);
			_Channel_Layout_Subset(layout, bp.Str, "AV_CH_LAYOUT_2POINT1:", AvChannelMask.TwoPointOne);
			_Channel_Layout_Subset(layout, bp.Str, "AV_CH_LAYOUT_4POINT1:", AvChannelMask.FourPointOne);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Custom_Layouts(AVBPrint bp, ref AvChannelLayout layout)
		{
			printf("\n==Custom layouts==\n");

			Test_Custom_Av_Channel_Layout_From_String(bp, ref layout);
			Test_Custom_Av_Channel_Layout_Index_From_String(bp, layout);
			Test_Custom_Av_Channel_Layout_Channel_From_String(bp, layout);
			Test_Custom_Av_Channel_Layout_Index_From_Channel(bp, layout);
			Test_Custom_Av_Channel_Layout_Channel_From_Index(bp, layout);
			Test_Custom_Av_Channel_Layout_Subset(bp, layout);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Custom_Av_Channel_Layout_From_String(AVBPrint bp, ref AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_from_string\n");

			_Channel_Layout_From_String(bp, ref layout, "FL+FR+FC+BL+BR+LFE");
			_Channel_Layout_From_String(bp, ref layout, "2 channels (FR+FL)");
			_Channel_Layout_From_String(bp, ref layout, "2 channels (AMBI1023+FL)");
			_Channel_Layout_From_String(bp, ref layout, "3 channels (FR+FL)");
			_Channel_Layout_From_String(bp, ref layout, "-3 channels (FR+FL)");
			_Channel_Layout_From_String(bp, ref layout, "0 channels ()");
			_Channel_Layout_From_String(bp, ref layout, "2 channels (FL+FR");
			_Channel_Layout_From_String(bp, ref layout, "ambisonic 1+FR+FL");
			_Channel_Layout_From_String(bp, ref layout, "ambisonic 2+FC@Foo");
			_Channel_Layout_From_String(bp, ref layout, "FL@Foo+FR@Bar");
			_Channel_Layout_From_String(bp, ref layout, "FL+stereo");
			_Channel_Layout_From_String(bp, ref layout, "stereo+stereo");
			_Channel_Layout_From_String(bp, ref layout, "stereo@Boo");
			_Channel_Layout_From_String(bp, ref layout, string.Empty);
			_Channel_Layout_From_String(bp, ref layout, "@");
			_Channel_Layout_From_String(bp, ref layout, "@Dummy");
			_Channel_Layout_From_String(bp, ref layout, "@FL");
			_Channel_Layout_From_String(bp, ref layout, "Dummy");
			_Channel_Layout_From_String(bp, ref layout, "Dummy@FL");
			_Channel_Layout_From_String(bp, ref layout, "FR+Dummy");
			_Channel_Layout_From_String(bp, ref layout, "FR+Dummy@FL");
			_Channel_Layout_From_String(bp, ref layout, "UNK+UNSD");
			_Channel_Layout_From_String(bp, ref layout, "NONE");
			_Channel_Layout_From_String(bp, ref layout, "FR+@FL");
			_Channel_Layout_From_String(bp, ref layout, "FL+@");
			_Channel_Layout_From_String(bp, ref layout, "FR+FL@Foo+USR63@Foo");

			AvChannelLayout layout2 = new AvChannelLayout();

			c_int ret = Channel_Layout.Av_Channel_Layout_Copy(layout2, layout);

			if (ret < 0)
				printf("Copying channel layout \"FR+FL@Foo+USR63@Foo\" failed; ret %d\n", ret);

			ret = Channel_Layout.Av_Channel_Layout_Compare(layout, layout2);

			if (ret != 0)
				printf("Channel layout and its copy compare unequal; ret %d\n", ret);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Custom_Av_Channel_Layout_Index_From_String(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_index_from_string\n");

			_Channel_Layout_Index_From_String(layout, bp.Str, "FR");
			_Channel_Layout_Index_From_String(layout, bp.Str, "FL");
			_Channel_Layout_Index_From_String(layout, bp.Str, "USR63");
			_Channel_Layout_Index_From_String(layout, bp.Str, "Foo");
			_Channel_Layout_Index_From_String(layout, bp.Str, "@Foo");
			_Channel_Layout_Index_From_String(layout, bp.Str, "FR@Foo");
			_Channel_Layout_Index_From_String(layout, bp.Str, "FL@Foo");
			_Channel_Layout_Index_From_String(layout, bp.Str, "USR63@Foo");
			_Channel_Layout_Index_From_String(layout, bp.Str, "BC");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Custom_Av_Channel_Layout_Channel_From_String(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_channel_from_string\n");

			_Channel_Layout_Channel_From_String(layout, bp.Str, "FR");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "FL");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "USR63");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "Foo");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "@Foo");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "FR@Foo");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "FL@Foo");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "USR63@Foo");
			_Channel_Layout_Channel_From_String(layout, bp.Str, "BC");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Custom_Av_Channel_Layout_Index_From_Channel(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_index_from_channel\n");

			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Front_Right);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Front_Left);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, (AvChannel)63);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Back_Center);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Custom_Av_Channel_Layout_Channel_From_Index(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_channel_from_index\n");

			_Channel_Layout_Channel_From_Index(layout, bp.Str, 0);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 1);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 2);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 3);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Custom_Av_Channel_Layout_Subset(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_subset\n");

			_Channel_Layout_Subset(layout, bp.Str, "AV_CH_LAYOUT_STEREO:", AvChannelMask.Stereo);
			_Channel_Layout_Subset(layout, bp.Str, "AV_CH_LAYOUT_QUAD:", AvChannelMask.Quad);
		}






		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Ambisonic_Layouts(AVBPrint bp, ref AvChannelLayout layout)
		{
			printf("\n==Ambisonic layouts==\n");

			Test_Ambisonic_Av_Channel_Layout_From_String(bp, ref layout);
			Test_Ambisonic_Av_Channel_Layout_Index_From_Channel(bp, layout);
			Test_Ambisonic_Av_Channel_Layout_Channel_From_Index(bp, layout);
			Test_Ambisonic_Av_Channel_Layout_Subset(bp, layout);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Ambisonic_Av_Channel_Layout_From_String(AVBPrint bp, ref AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_from_string\n");

			_Channel_Layout_From_String(bp, ref layout, "ambisonic 1");
			_Channel_Layout_From_String(bp, ref layout, "ambisonic 2+stereo");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Ambisonic_Av_Channel_Layout_Index_From_Channel(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_index_from_channel\n");

			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Ambisonic_Base);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Front_Left);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Front_Right);
			_Channel_Layout_Index_From_Channel(layout, bp.Str, AvChannel.Back_Center);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Ambisonic_Av_Channel_Layout_Channel_From_Index(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_channel_from_index\n");

			_Channel_Layout_Channel_From_Index(layout, bp.Str, 0);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 9);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 10);
			_Channel_Layout_Channel_From_Index(layout, bp.Str, 11);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Ambisonic_Av_Channel_Layout_Subset(AVBPrint bp, AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_subset\n");

			_Channel_Layout_Subset(layout, bp.Str, "AV_CH_LAYOUT_STEREO:", AvChannelMask.Stereo);
			_Channel_Layout_Subset(layout, bp.Str, "AV_CH_LAYOUT_QUAD:", AvChannelMask.Quad);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Channel_Layout_Retype(AVBPrint bp, ref AvChannelLayout layout)
		{
			printf("\nTesting av_channel_layout_retype\n");

			string[] layouts =
			[
				"FL@Boo",
				"stereo",
				"FR+FL",
				"ambisonic 2+stereo",
				"2C",
				null
			];

			for (c_int i = 0; layouts[i] != null; i++)
				printf("With \"%s\": %s\n", layouts[i], Channel_Layout_Retype(ref layout, bp, layouts[i].ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Cmp_BPrint_And_NonBPrint(AVBPrint bp, string funcName, TestDelegate func, TestBPrintDelegate bprintFunc)
		{
			bprintFunc(bp);

			if (CString.strlen(bp.Str) != bp.Len)
			{
				printf($"strlen of AVBPrint-string returned by {funcName}_bprint differs from AVBPrint.len: %{UtilConstants.Size_Specifier} vs. %u\n", CString.strlen(bp.Str), bp.Len);
				return;
			}

			c_int size = func(null, 0);

			if (size <= 0)
			{
				printf($"{funcName} returned %d\n", size);
				return;
			}

			if (bp.Len != (size - 1))
			{
				printf($"Return value %d of {funcName} inconsistent with length %u obtained from corresponding bprint version\n", size, bp.Len);
				return;
			}

			CPointer<char> str = new CPointer<char>((size_t)size);

			if (str.IsNull)
			{
				printf("string of size %d could not be allocated.\n", size);
				return;
			}

			size = func(str, (size_t)size);

			if ((size <= 0) || (bp.Len != (size - 1)))
			{
				printf($"Return value %d of {funcName} inconsistent with length %d obtained in first pass.\n", size, bp.Len);

				Mem.Av_Free(str);
				return;
			}

			if (CString.strcmp(str, bp.Str) != 0)
			{
				printf($"Ordinary and _bprint versions of {funcName} disagree: '%s' vs. '%s'\n", str, bp.Str);

				Mem.Av_Free(str);
				return;
			}

			Mem.Av_Free(str);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Channel_Name(AVBPrint bp, AvChannel channel)
		{
			BPrint.Av_BPrint_Clear(bp);

			Cmp_BPrint_And_NonBPrint(bp, "av_channel_name",
			(buf, size) =>
			{
				 return Channel_Layout.Av_Channel_Name(buf, size, channel);
			},
			(buf =>
			{
				Channel_Layout.Av_Channel_Name_BPrint(buf, channel);
			}));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Channel_Description(AVBPrint bp, AvChannel channel)
		{
			BPrint.Av_BPrint_Clear(bp);

			Cmp_BPrint_And_NonBPrint(bp, "av_channel_description",
			(buf, size) =>
			{
				 return Channel_Layout.Av_Channel_Description(buf, size, channel);
			},
			(buf =>
			{
				Channel_Layout.Av_Channel_Description_BPrint(buf, channel);
			}));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Channel_Layout_From_Mask(AvChannelLayout layout, AVBPrint bp, AvChannelMask channel_Layout)
		{
			Channel_Layout.Av_Channel_Layout_Uninit(layout);
			BPrint.Av_BPrint_Clear(bp);

			if ((Channel_Layout.Av_Channel_Layout_From_Mask(layout, channel_Layout) == 0) && (Channel_Layout.Av_Channel_Layout_Check(layout) != 0))
			{
				Cmp_BPrint_And_NonBPrint(bp, "av_channel_layout_describe",
				(buf, size) =>
				{
					 return Channel_Layout.Av_Channel_Layout_Describe(layout, buf, size);
				},
				(buf =>
				{
					Channel_Layout.Av_Channel_Layout_Describe_BPrint(layout, buf);
				}));
			}
			else
				BPrint.Av_BPrintf(bp, "fail");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Channel_Layout_From_String(ref AvChannelLayout refLayout, AVBPrint bp, CPointer<char> channel_Layout)
		{
			AvChannelLayout layout = refLayout;

			Channel_Layout.Av_Channel_Layout_Uninit(layout);
			BPrint.Av_BPrint_Clear(bp);

			if ((Channel_Layout.Av_Channel_Layout_From_String(ref layout, channel_Layout) == 0) && (Channel_Layout.Av_Channel_Layout_Check(layout) != 0))
			{
				Cmp_BPrint_And_NonBPrint(bp, "av_channel_layout_describe",
				(buf, size) =>
				{
					 return Channel_Layout.Av_Channel_Layout_Describe(layout, buf, size);
				},
				(buf =>
				{
					Channel_Layout.Av_Channel_Layout_Describe_BPrint(layout, buf);
				}));
			}
			else
				BPrint.Av_BPrintf(bp, "fail");

			refLayout = layout;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Describe_Type(AVBPrint bp, AvChannelLayout layout)
		{
			if ((c_uint)layout.Order < Macros.FF_Array_Elems(channel_Order_Names))
			{
				BPrint.Av_BPrintf(bp, "%-6s (", channel_Order_Names[(c_int)layout.Order]);
				Channel_Layout.Av_Channel_Layout_Describe_BPrint(layout, bp);
				BPrint.Av_BPrintf(bp, ")");
			}
			else
				BPrint.Av_BPrintf(bp, "???");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private CPointer<char> Channel_Layout_Retype(ref AvChannelLayout layout, AVBPrint bp, CPointer<char> channel_Layout)
		{
			Channel_Layout.Av_Channel_Layout_Uninit(layout);
			BPrint.Av_BPrint_Clear(bp);

			if ((Channel_Layout.Av_Channel_Layout_From_String(ref layout, channel_Layout) == 0) && (Channel_Layout.Av_Channel_Layout_Check(layout) != 0))
			{
				Describe_Type(bp, layout);

				for (c_int i = 0; i < (c_int)AvChannelOrder.Nb; i++)
				{
					AvChannelLayout copy = new AvChannelLayout();
					BPrint.Av_BPrintf(bp, "\n ");

					if (Channel_Layout.Av_Channel_Layout_Copy(copy, layout) < 0)
						return "nomem".ToCharPointer();

					c_int ret = Channel_Layout.Av_Channel_Layout_Retype(copy, (AvChannelOrder)i, AvChannelLayoutRetypeFlag.None);

					if ((ret < 0) && ((copy.Order != layout.Order) || (Channel_Layout.Av_Channel_Layout_Compare(copy, layout) != 0)))
						BPrint.Av_BPrintf(bp, "failed to keep existing layout on failure");

					if ((ret >= 0) && (copy.Order != (AvChannelOrder)i))
						BPrint.Av_BPrintf(bp, "returned success but did not change order");

					if (ret == Error.ENOSYS)
						BPrint.Av_BPrintf(bp, " != %s", channel_Order_Names[i]);
					else if (ret < 0)
						BPrint.Av_BPrintf(bp, "FAIL");
					else
					{
						BPrint.Av_BPrintf(bp, " %s ", ret != 0 ? "~~" : "==");
						Describe_Type(bp, copy);
					}

					Channel_Layout.Av_Channel_Layout_Uninit(copy);
				}
			}
			else
				BPrint.Av_BPrintf(bp, "fail");

			return bp.Str;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Name(AVBPrint bp, AvChannel x)
		{
			Channel_Name(bp, x);
			printf("With %-32s %14s\n", (Enum.IsDefined(x) ? "AV_CHAN_" : string.Empty) + x.ToString().ToUpper() + ":", bp.Str);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Description(AVBPrint bp, AvChannel x)
		{
			Channel_Description(bp, x);
			printf("With %-23s %23s\n", (Enum.IsDefined(x) ? "AV_CHAN_" : string.Empty) + x.ToString().ToUpper() + ":", bp.Str);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_From_String(string x)
		{
			printf("With %-38s %8d\n", $"\"{x}\":", Channel_Layout.Av_Channel_From_String(x.ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Layout_From_Mask(AVBPrint bp, AvChannelLayout layout, AvChannelMask x)
		{
			Channel_Layout_From_Mask(layout, bp, x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Layout_From_String(AVBPrint bp, ref AvChannelLayout layout, string x)
		{
			CPointer<char> xPtr = x.ToCharPointer();

			Channel_Layout_From_String(ref layout, bp, xPtr);
			printf("With \"%s\":%*s %32s\n", x, CString.strlen(xPtr) > 32 ? 0 : 32 - CString.strlen(xPtr), string.Empty, bp.Str);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Layout_Channel_From_Index(AvChannelLayout layout, CPointer<char> l, c_uint x)
		{
			c_int ret = (c_int)Channel_Layout.Av_Channel_Layout_Channel_From_Index(layout, x);

			if (ret < 0)
				ret = -1;

			printf("On \"%s\" layout with %2d: %8d\n", l, x, ret);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Layout_Subset(AvChannelLayout layout, CPointer<char> l, string xStr, AvChannelMask x)
		{
			uint64_t mask = Channel_Layout.Av_Channel_Layout_Subset(layout, x);
			printf("On \"%s\" layout with %-22s 0x%llx\n", l, xStr, mask);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Layout_Index_From_Channel(AvChannelLayout layout, CPointer<char> l, AvChannel x)
		{
			c_int ret = Channel_Layout.Av_Channel_Layout_Index_From_Channel(layout, x);

			if (ret < 0)
				ret = -1;

			printf("On \"%s\" layout with %-23s %3d\n", l, (Enum.IsDefined(x) ? "AV_CHAN_" : string.Empty) + x.ToString().ToUpper() + ":", ret);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Layout_Channel_From_String(AvChannelLayout layout, CPointer<char> l, string x)
		{
			c_int ret = (c_int)Channel_Layout.Av_Channel_Layout_Channel_From_String(layout, x.ToCharPointer());

			if (ret < 0)
				ret = -1;

			printf("On \"%s\" layout with %-21s %3d\n", l, $"\"{x}\":", ret);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void _Channel_Layout_Index_From_String(AvChannelLayout layout, CPointer<char> l, string x)
		{
			c_int ret = Channel_Layout.Av_Channel_Layout_Index_From_String(layout, x.ToCharPointer());

			if (ret < 0)
				ret = -1;

			printf("On \"%s\" layout with %-20s %3d\n", l, $"\"{x}\":", ret);
		}
	}
}
