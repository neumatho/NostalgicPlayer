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
		/// The sample changes on rows 4 and 20, but not on rows 8 and 24.
		/// 
		/// Claudio's note: don't use compare_mixer_data, we have random
		/// pan here
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_NoteOff2()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "NoteOff2.it", "NoteOff2.data");
		}
	}
}
