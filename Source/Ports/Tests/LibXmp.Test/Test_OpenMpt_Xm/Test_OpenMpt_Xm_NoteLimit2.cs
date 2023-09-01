/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Xm
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Xm
	{
		/********************************************************************/
		/// <summary>
		/// This test is an addendum to the previous test (note-limit.xm)
		/// because I forgot that you first have to check if there is an
		/// instrument change happening before calculating the “real” note.
		/// I always took the previous transpose value when doing this check,
		/// so when switching from one instrument (with a high transpose
		/// value) to another one, it was possible that valid notes would get
		/// rejected
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_NoteLimit2()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "NoteLimit2.xm", "NoteLimit2.data");
		}
	}
}
