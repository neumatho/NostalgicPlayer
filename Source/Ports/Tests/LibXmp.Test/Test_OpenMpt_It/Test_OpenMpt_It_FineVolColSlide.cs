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
		/// Unlike fine volume slides in the effect column, fine volume
		/// slides in the volume column are only ever executed on the first
		/// tick — not on multiples of the first tick if there is a pattern
		/// delay. Thus, the left and right channel of this example should
		/// always have the same volume
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_FineVolColSlide()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "FineVolColSlide.it", "FineVolColSlide.data");
		}
	}
}
