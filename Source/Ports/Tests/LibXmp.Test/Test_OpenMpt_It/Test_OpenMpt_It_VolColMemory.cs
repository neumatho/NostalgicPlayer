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
		/// Volume column commands a, b, c and d (volume slide) share one
		/// effect memory, but it should not be shared with Dxy in the effect
		/// column. Furthermore, there is no unified effect memory across
		/// different kinds of volume column effects (that's how OpenMPT used
		/// to handle it up to revision 1544)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_VolColMemory()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "VolColMemory.it", "VolColMemory.data");
		}
	}
}
