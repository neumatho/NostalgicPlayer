/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Mpt.String
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mpt_String
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_String_Buffer()
		{
			{
				uint8[] buf = [ (uint8)'x', (uint8)'x', (uint8)'x', (uint8)'x' ];
				MptString.WriteAutoBuf(buf).Assign("foobar");
				Assert.AreEqual((uint8)'f', buf[0]);
				Assert.AreEqual((uint8)'o', buf[1]);
				Assert.AreEqual((uint8)'o', buf[2]);
				Assert.AreEqual(0, buf[3]);
			}

			//XX WriteTypedBuf
			{
			}

			//XX WriteTypedBuf
			{
			}

			//XX WriteTypedBuf
			{
			}

			{
				uint8[] buf = [ (uint8)'f', (uint8)'o', (uint8)'o', (uint8)'b' ];
				StdString foo = MptString.ReadAutoBuf(buf);
				Assert.AreEqual("foob", foo);
			}
		}
	}
}
