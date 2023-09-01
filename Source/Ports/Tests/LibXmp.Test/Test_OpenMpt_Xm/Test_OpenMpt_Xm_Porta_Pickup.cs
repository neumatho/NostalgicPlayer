/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Xm
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Xm
	{
		/********************************************************************/
		/// <summary>
		/// An instrument number should not reset the current portamento
		/// target. The portamento target is valid until a new target is
		/// specified by combining a note and a portamento effect
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_Porta_Pickup()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "Porta-Pickup.xm", "Porta-Pickup.data");
		}
	}
}
