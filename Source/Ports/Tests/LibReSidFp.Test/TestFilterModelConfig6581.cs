/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibReSidFp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibReSidFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestFilterModelConfig6581
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestFilterCurve()
		{
			// Check that values in the filter curve range do not
			// trigger assertions
			c_ushort[] dac = FilterModelConfig6581.GetInstance().GetDac(0.0);

			dac = FilterModelConfig6581.GetInstance().GetDac(1.0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestFilterRange()
		{
			FilterModelConfig6581.GetInstance().SetFilterRange(0.0);

			FilterModelConfig6581.GetInstance().SetFilterRange(1.0);
		}
	}
}
