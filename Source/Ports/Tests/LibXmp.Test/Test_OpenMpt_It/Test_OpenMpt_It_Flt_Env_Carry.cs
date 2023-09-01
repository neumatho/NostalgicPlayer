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
		/// Envelope carry on the filter envelope. I think this is just a
		/// general test on how envelope carry is applied. It is possible
		/// that Impulse Tracker’s MMX drivers will play this in a different
		/// way from the WAV writer
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Flt_Env_Carry()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Flt-Env-Carry.it", "Flt-Env-Carry.data");
		}
	}
}
