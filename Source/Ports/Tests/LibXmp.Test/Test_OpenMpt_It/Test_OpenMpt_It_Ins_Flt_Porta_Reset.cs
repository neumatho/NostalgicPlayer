/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_It
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_It
	{
		/********************************************************************/
		/// <summary>
		/// Instrument filter settings should not be applied if there is a
		/// portamento effect
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Ins_Flt_Porta_Reset()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Ins-Flt-Porta-Reset.it", "Ins-Flt-Porta-Reset.data");
		}
	}
}
