/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Read_File
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Read_File
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Read_File_24Bit_Big_Endian()
		{
			using (Stream f = OpenStream(dataDirectory, "Test.mmcmp"))
			{
				Assert.IsNotNull(f, "Can't open data file");

				uint32 x = DataIo.Read24B(f, out _);
				Assert.AreEqual(0x007a6952U, x, "Read error");
			}
		}
	}
}
