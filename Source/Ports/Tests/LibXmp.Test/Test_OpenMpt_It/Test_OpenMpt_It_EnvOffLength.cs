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
		/// If an envelope sustain loop happens to end on exactly the same
		/// tick as a note-off event occurs, the envelope is not yet
		/// released. It will be released whenever the loop end is being hit
		/// again
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_EnvOffLength()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "EnvOffLength.it", "EnvOffLength.data");
		}
	}
}
