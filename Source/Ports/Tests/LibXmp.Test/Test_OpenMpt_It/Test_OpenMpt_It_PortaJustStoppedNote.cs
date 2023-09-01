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
		/// Fade-Porta.it already tests the general case of portamento
		/// picking up a stopped note (portamento should just be ignored in
		/// this case), but there is an edge case when the note just stopped
		/// on the previous tick. In this case, OpenMPT did previously behave
		/// differently and still execute the portamento effect
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_PortaJustStoppedNote()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "PortaJustStoppedNote.it", "PortaJustStoppedNote.data");
		}
	}
}
