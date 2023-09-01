/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_It
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_It
	{
		/********************************************************************/
		/// <summary>
		/// Any kind of Note Cut (SCx or ^^^) should stop the sample and not
		/// set its volume to 0. A subsequent volume command cannot continue
		/// the sample, but a note, even without an instrument number can do
		/// so. When played back correctly, the module should stay silent
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_NoteOffInstr()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "NoteOffInstr.it", "NoteOffInstr.data");
		}
	}
}
