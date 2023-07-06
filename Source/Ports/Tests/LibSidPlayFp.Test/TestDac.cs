/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.ReSidFp;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestDac
	{
		private const int DacBits = 8;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestDac6581()
		{
			Assert.AreEqual(false, IsDacLinear(ChipModel.MOS6581));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestDac8580()
		{
			Assert.AreEqual(true, IsDacLinear(ChipModel.MOS8580));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool IsDacLinear(ChipModel chipModel)
		{
			double[] dac = new double[1 << DacBits];
			BuildDac(dac, chipModel);

			for (int i = 1; i < (1 << DacBits); i++)
			{
				if (dac[i] <= dac[i - 1])
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BuildDac(double[] dac, ChipModel chipModel)
		{
			Dac dacBuilder = new Dac(DacBits);
			dacBuilder.KinkedDac(chipModel);

			for (uint i = 0; i < (1 << DacBits); i++)
				dac[i] = dacBuilder.GetOutput(i);
		}
		#endregion
	}
}