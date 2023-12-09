/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// The MTM tempo effect has the weird property of resetting the
		/// speed to 6 if the tempo is set, and resetting the tempo to 125
		/// if the speed is set. For a real module that relies on this, see
		/// absolve.mtm orders 22 and 23.
		/// 
		/// A lot of MTM modules rely on DMP's timing instead, which emulates
		/// Protracker. When both a speed and tempo effect are found on the
		/// same row, this timing generally should be used
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Mtm_Tempo()
		{
			Compare_Mixer_Data(dataDirectory, "Tempo.mtm", "Tempo_Mtm.data");
			Compare_Mixer_Data(dataDirectory, "Tempo2.mtm", "Tempo2_Mtm.data");
		}
	}
}
