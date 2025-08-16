/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
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
		public void Test_Sample_Load_Endian()
		{
			Xmp_Sample xxs = new Xmp_Sample();

			int8[] conv_R0 = [ 1, 0, 3, 2, 5, 4, -7, 6, -29, 8 ];
			int8[] conv_R1 = [ 0, 1, 2, 3, 4, 5, 6, -7, 8, -29 ];

			Module_Data m = new Module_Data();

			xxs.Len = 5;
			xxs.Flg = Xmp_Sample_Flag._16Bit;

			// Our input sample is big-endian
			Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad | Sample_Flag.BigEnd, xxs, MemoryMarshal.Cast<int8, uint8>(conv_R0).ToArray());

			if (Is_Big_Endian())
				Assert.AreEqual(0, CMemory.MemCmp(xxs.Data, MemoryMarshal.Cast<int8, uint8>(conv_R0).ToArray(), 10), "Invalid conversion from big-endian");
			else
				Assert.AreEqual(0, CMemory.MemCmp(xxs.Data, MemoryMarshal.Cast<int8, uint8>(conv_R1).ToArray(), 10), "Invalid conversion from big-endian");

			Sample.LibXmp_Free_Sample(xxs);

			// Now the sample is little-endian
			Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad, xxs, MemoryMarshal.Cast<int8, uint8>(conv_R0).ToArray());

			if (Is_Big_Endian())
				Assert.AreEqual(0, CMemory.MemCmp(xxs.Data, MemoryMarshal.Cast<int8, uint8>(conv_R1).ToArray(), 10), "Invalid conversion from little-endian");
			else
				Assert.AreEqual(0, CMemory.MemCmp(xxs.Data, MemoryMarshal.Cast<int8, uint8>(conv_R0).ToArray(), 10), "Invalid conversion from little-endian");

			Sample.LibXmp_Free_Sample(xxs);
		}
	}
}
