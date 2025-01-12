/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Mod
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Mod
	{
		/********************************************************************/
		/// <summary>
		/// Description: When a non-looped sample stopped playing, it is
		/// still possible to perform sample swapping to a looped sample. As
		/// a result, on the first and third row, a square wave should be
		/// heard, a drum on the second and fourth row, but no new sample
		/// should be triggered on the fifth row
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_PTStoppedSwap()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "PTStoppedSwap.mod", "PTStoppedSwap.data");
		}
	}
}
