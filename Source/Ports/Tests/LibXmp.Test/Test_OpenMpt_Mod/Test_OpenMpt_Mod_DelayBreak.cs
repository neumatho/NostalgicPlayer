/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Mod
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Mod
	{
		/********************************************************************/
		/// <summary>
		/// If there is a row delay (EEx) on the same row as a pattern break
		/// (Dxx), the target row of that jump is not played. Instead, the
		/// next row is played
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_DelayBreak()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "DelayBreak.mod", "DelayBreak.data");
		}
	}
}
