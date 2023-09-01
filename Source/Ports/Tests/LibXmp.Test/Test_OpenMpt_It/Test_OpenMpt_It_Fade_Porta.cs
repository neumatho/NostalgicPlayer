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
		/// After a note has been stopped in some way (for example through
		/// fade-out or note cut), tone portamento effects on the following
		/// note are ignored, i.e. there is no portamento from the stopped
		/// note to the new note
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Fade_Porta()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Fade-Porta.it", "Fade-Porta.data");
		}
	}
}
