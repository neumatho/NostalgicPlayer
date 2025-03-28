﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Loaders;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Sample_Load
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Sample_Load
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Sample_Load_Signal()
		{
			Xmp_Sample xxs = new Xmp_Sample();

			int8[] buffer0 = [ 0, 1, 2, 3, 4, 5, 6, -7, 8, -29 ];
			uint8[] conv_R0 = [ 128, 129, 130, 131, 132, 133, 134, 121, 136, 99 ];

			// 16-bit input buffer is little-endian
			uint8[] buffer1 = [ 0, 0, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6, 0, 0xf9, 0xff, 8, 0, 0xe3, 0xff ];

			// 16-bit output buffer is native-endian
			uint16[] conv_R1 = [ 32768, 32769, 32770, 32771, 32772, 32773, 32774, 32761, 32776, 32739 ];

			Module_Data m = new Module_Data();

			xxs.Len = 10;
			Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad | Sample_Flag.Uns, xxs, MemoryMarshal.Cast<int8, uint8>(buffer0).ToArray());
			Assert.AreEqual(0, CMemory.MemCmp(xxs.Data, conv_R0, 10), "Invalid 8-bit conversion");
			Sample.LibXmp_Free_Sample(xxs);

			xxs.Flg = Xmp_Sample_Flag._16Bit;
			Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad | Sample_Flag.Uns, xxs, buffer1);
			Assert.AreEqual(0, CMemory.MemCmp(xxs.Data, MemoryMarshal.Cast<uint16, uint8>(conv_R1).ToArray(), 20), "Invalid 16-bit conversion");
			Sample.LibXmp_Free_Sample(xxs);
		}
	}
}
