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
		/// Saga Musix reports:
		///
		/// "More frequency trouble!
		/// After the discovery of how frequency slides work in 669, I
		/// present some new findings about the MDL format: Frequency slides
		/// are not linear, but they are also independent of the middle-C
		/// frequency.
		///
		/// Regardless of what the middle-C frequency is, portamentos and
		/// vibratos behave as if the middle-C frequency was 8363 Hz (i.e.
		/// like normal Amiga slides)."
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Mdl_Period()
		{
			Compare_Mixer_Data(dataDirectory, "Period.mdl", "Period_Mdl.data");
		}
	}
}
