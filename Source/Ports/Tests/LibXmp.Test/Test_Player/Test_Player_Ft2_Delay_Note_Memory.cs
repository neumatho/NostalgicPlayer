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
		/// Fasttracker 2 note delay has a strange feature resembling "note
		/// memory" or "retrigger" where, when no note is set on a delayed
		/// row, Fasttracker 2 will pretend there's a note set anyway,
		/// reusing the last valid note.
		/// This note will then use the instrument number beside it, or
		/// instrument memory if there isn't one, to play a new note. This
		/// causes samples to restart from the beginning and activates new
		/// instruments (including invalid instruments). However, if there
		/// is no instrument number in the delayed row, this will NOT apply
		/// instrument defaults.
		///
		/// No envelope (for envelope versions, add 1C to the row numbers):
		///
		/// 00-07: normal non-delayed instrument numbers. They don't change
		///        instruments.
		/// 0A   : no note/no ins# + volume 40h + delay -> "retrig".
		/// 0C   : no note/ins# + delay -> retrig, new instrument, apply its
		///        defaults.
		/// 0F   : this delayed row sets the active instrument to an invalid
		///        instrument!
		/// 11   : this delayed row reactivates the channel.
		/// 12   : retrigger, but do not apply defaults.
		/// 15-16: keyoff -> do not reuse the old note, change instrument, or
		///        retrigger.
		/// 17   : new note -> do not reuse the old note.
		/// 19   : ED0, as always, does nothing.
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Delay_Note_Memory()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Delay_Note_Memory.xm", "Ft2_Delay_Note_Memory.data");
		}
	}
}
