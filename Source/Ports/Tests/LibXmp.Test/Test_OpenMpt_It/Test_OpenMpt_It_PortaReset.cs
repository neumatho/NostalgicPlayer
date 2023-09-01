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
		/// Impulse Tracker completely resets the portamento target on every
		/// new non-portamento note, i.e. a follow-up Gxx effect should not
		/// slide back to the previously triggered note
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_PortaReset()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "PortaReset.it", "PortaReset.data");
		}
	}
}
