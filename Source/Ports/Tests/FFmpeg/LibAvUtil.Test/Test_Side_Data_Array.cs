/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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
	public class Test_Side_Data_Array : TestBase
	{
		private class FrameSideDataSet
		{
			public CPointer<AvFrameSideData> Sd;
			public c_int Nb_Sd;
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Side_Data_Array_()
		{
			RunTest("side_data_array");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			FrameSideDataSet set = new FrameSideDataSet();

			Assert.IsNotNull(Side_Data.Av_Frame_Side_Data_New(ref set.Sd, ref set.Nb_Sd, AvFrameSideDataType.Content_Light_Level, sizeof(int64_t), AvFrameSideDataFlag.None));
			Assert.IsNotNull(Side_Data.Av_Frame_Side_Data_New(ref set.Sd, ref set.Nb_Sd, AvFrameSideDataType.Content_Light_Level, sizeof(int32_t), AvFrameSideDataFlag.Replace));

			// Test entries in the middle
			for (c_int value = 1; value < 4; value++)
			{
				AvFrameSideData sd = Side_Data.Av_Frame_Side_Data_New(ref set.Sd, ref set.Nb_Sd, AvFrameSideDataType.Sei_Unregistered, sizeof(int32_t), AvFrameSideDataFlag.None);

				Assert.IsNotNull(sd);

				((DataBufferContext)sd.Buf.Data).Data[0] = (uint8_t)value;
			}

			Assert.IsNotNull(Side_Data.Av_Frame_Side_Data_New(ref set.Sd, ref set.Nb_Sd, AvFrameSideDataType.Spherical, sizeof(int64_t), AvFrameSideDataFlag.None));
			Assert.IsNotNull(Side_Data.Av_Frame_Side_Data_New(ref set.Sd, ref set.Nb_Sd, AvFrameSideDataType.Spherical, sizeof(int32_t), AvFrameSideDataFlag.Replace));

			// Test entries at the end
			for (c_int value = 1; value < 4; value++)
			{
				AvFrameSideData sd = Side_Data.Av_Frame_Side_Data_New(ref set.Sd, ref set.Nb_Sd, AvFrameSideDataType.Sei_Unregistered, sizeof(int32_t), AvFrameSideDataFlag.None);

				Assert.IsNotNull(sd);

				((DataBufferContext)sd.Buf.Data).Data[0] = (uint8_t)(value + 3);
			}

			printf("Initial addition results with duplicates:\n");
			Print_Entries(set.Sd, set.Nb_Sd);

			{
				AvFrameSideData sd = Side_Data.Av_Frame_Side_Data_New(ref set.Sd, ref set.Nb_Sd, AvFrameSideDataType.Sei_Unregistered, sizeof(int32_t), AvFrameSideDataFlag.Unique);

				Assert.IsNotNull(sd);

				((DataBufferContext)sd.Buf.Data).Data[0] = 0x39;
				((DataBufferContext)sd.Buf.Data).Data[1] = 0x05;
			}

			printf("\nFinal state after a single 'no-duplicates' addition:\n");
			Print_Entries(set.Sd, set.Nb_Sd);

			Side_Data.Av_Frame_Side_Data_Free(ref set.Sd, ref set.Nb_Sd);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Print_Entries(CPointer<AvFrameSideData> sd, c_int nb_Sd)
		{
			for (c_int i = 0; i < nb_Sd; i++)
			{
				AvFrameSideData entry = sd[i];
				DataBufferContext dataBuffer = (DataBufferContext)entry.Data;

				printf("sd %d (size %lld), %s", i, dataBuffer.Size, Side_Data.Av_Frame_Side_Data_Name(entry.Type));

				if (entry.Type != AvFrameSideDataType.Sei_Unregistered)
				{
					printf("\n");
					continue;
				}

				printf(": %d\n", (dataBuffer.Data[1] << 8) | dataBuffer.Data[0]);
			}
		}
	}
}
