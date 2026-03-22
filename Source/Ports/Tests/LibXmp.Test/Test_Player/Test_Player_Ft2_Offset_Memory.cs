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
		/// FT2 only applies the offset effect for events with note + 9xx +
		/// !toneporta. Unusually, however, it also only sets its MEMORY
		/// when offset is applied. When the offset is past the end of the
		/// sample, the sample also cuts.
		///
		/// 00-0C: various cases of note + !toneporta + 9xx, including cuts.
		/// 0D   : do not apply offset (no note).
		/// 0E-15: do not set memory with toneporta present.
		/// 16-1F: more cases where toneporta memory should not be updated.
		///        Notably: DO update it when playing a note with an invalid
		///        instrument.
		///        (Memory is FF after this point.)
		/// 22-24: ›A#9, as usual, acts like there's no note i.e. for the
		///        purposes of offset, it does not set offset or update
		///        memory.
		///        (Memory is still FF after this point.)
		/// 25-28: B-(-1), as usual, acts like whatever the last note was
		///        i.e. for the purposes of offset, it sets offset and
		///        updates memory.
		///        (Memory is overwritten with 01 here.)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Offset_Memory()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Offset_Memory.xm", "Ft2_Offset_Memory.data");
		}
	}
}
