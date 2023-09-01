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
		/// In Impulse Tracker’s software mixer, ping-pong loops are
		/// shortened by one sample. This does not happen with the GUS
		/// hardware driver, but I assume that the software drivers were
		/// more popular due to the limitations of the GUS, so OpenMPT
		/// emulates this behaviour
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Bidi_Loops()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Bidi-Loops.it", "Bidi-Loops.data");
		}
	}
}
