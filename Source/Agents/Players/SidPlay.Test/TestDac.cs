/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.Test
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