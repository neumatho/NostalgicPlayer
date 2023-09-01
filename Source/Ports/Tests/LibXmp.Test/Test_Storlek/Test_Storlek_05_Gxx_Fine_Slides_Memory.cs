/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Storlek
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Storlek
	{
		/********************************************************************/
		/// <summary>
		/// 05 - Gxx, fine slides, effect memory
		///
		/// (Note: make sure the Compatible Gxx flag is handled correctly
		/// before performing this test.)
		///
		/// EFx and FFx are handled as fine slides even if the effect value
		/// was set by a Gxx effect. If this test is played correctly, the
		/// pitch should bend downward a full octave, and then up almost one
		/// semitone. If the note is bent way upward (and possibly out of
		/// control, causing the note to stop), the player is not handling
		/// the effect correctly
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_05_Gxx_Fine_Slides_Memory()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_05.it", "Storlek_05.data");
		}
	}
}
