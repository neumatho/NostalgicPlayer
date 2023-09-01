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
		/// Impulse Tracker does not retrigger notes that are shorter than
		/// the duration of a tick. One might argue that this is a bug in
		/// Impulse Tracker, but OpenMPT emulates it anyway
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Retrig_Short()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Retrig-Short.it", "Retrig-Short.data");
		}
	}
}
