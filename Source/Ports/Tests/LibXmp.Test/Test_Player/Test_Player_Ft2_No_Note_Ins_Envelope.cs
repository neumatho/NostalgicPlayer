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
		/// Instrument numbers without a note do not update the current
		/// playing sample, but they ALSO don't update the current envelopes
		/// for the channel
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_No_Note_Ins_Envelope()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_No_Note_Ins_Envelope.xm", "Ft2_No_Note_Ins_Envelope.data");
		}
	}
}
