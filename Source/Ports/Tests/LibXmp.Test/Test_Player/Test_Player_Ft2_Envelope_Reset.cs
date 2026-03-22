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
		/// More evidence towards the "K00 is literally just keyoff unless
		/// toneporta" theory. Outside of delay-related cases, envelope
		/// position reset only occurs when an instrument number is present,
		/// and it is blocked by keyoff and by K00-without-toneporta.
		///
		/// 00-07: none of the notes without an instrument reset the envelope
		///        positions. Envelope should not stop at the sustain point.
		/// 08-0B: instrument number resets positions.
		/// 0C-0F: instrument number (invalid) resets positions).
		/// 10-13: instrument number + toneporta resets positions.
		/// 14-17: instrument number (invalid) + toneporta resets positions.
		/// 18-1B: instrument number + toneporta + K00 resets positions.
		/// 1C-1F: instrument number (invalid) + toneporta + K00 resets
		///        positions.
		/// 20-23: instrument number + keyoff does not reset.
		/// 24-27: instrument number + K00 does not reset.
		/// 28-2B: instrument number + keyoff + K00 does not reset.
		/// 2C-2F: toneporta + keyoff does not reset.
		/// 30-33: toneporta + K00 does not reset.
		/// 34-37: instrument number + toneporta + keyoff does not reset.
		/// 38-3B: instrument number + toneporta + keyoff + K00 does not
		///        reset.
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Envelope_Reset()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Envelope_Reset.xm", "Ft2_Envelope_Reset.data");
		}
	}
}
