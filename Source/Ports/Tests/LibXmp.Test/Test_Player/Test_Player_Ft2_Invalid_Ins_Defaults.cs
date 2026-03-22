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
		/// When an invalid instrument is active, it sets default volume and
		/// panning and probably other things, just like a valid instrument.
		/// It always sets the volume=0 and panning=0x80.
		///
		/// Referenced subinstruments seem to usually exist within an
		/// instrument, at least when saved by FT2 and clone. They will use
		/// whatever volume/panning they were saved with despite also cutting
		/// the current channel (same as invalid instruments).
		///
		/// This test exists to make it as obvious as possible that zeroing
		/// volume for invalid instruments is not connected to the cutting
		/// behavior; it is just normal default volume/pan
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Invalid_Ins_Defaults()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Invalid_Ins_Defaults.xm", "Ft2_Invalid_Ins_Defaults.data");
		}
	}
}
