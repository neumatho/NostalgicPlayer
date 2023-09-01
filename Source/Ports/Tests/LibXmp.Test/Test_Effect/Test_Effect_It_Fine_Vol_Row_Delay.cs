/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// Similar to the OpenMPT test FineVolColSlide.it but this test
		/// makes sure multiple channels can volslide during a pattern delay.
		/// The prior handling only allowed the first channel to apply the
		/// volslide
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_It_Fine_Vol_Row_Delay()
		{
			Compare_Mixer_Data(dataDirectory, "FineVolRowDelayMultiple.it", "FineVolRowDelayMultiple.data");
		}
	}
}
