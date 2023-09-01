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
		/// If resonant filters are rendered with integer arithmetic, they
		/// may produce scratching noises in some edge cases. You should not
		/// hear any scratches or other weird noises when playing this
		/// example
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Extreme_Filter_Test_1()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Extreme-Filter-Test-1.it", "Extreme-Filter-Test-1.data");
		}
	}
}
