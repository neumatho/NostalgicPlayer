﻿/******************************************************************************/
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
		/// Out-of-range global volume commands (V81...VFF) should not change
		/// the current global volume. This test module should remain
		/// completely silent
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_GlobalVol_Invalid()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "GlobalVol-Invalid.it", "GlobalVol-Invalid.data");
		}
	}
}
