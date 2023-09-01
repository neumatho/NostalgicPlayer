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
		/// Unlike Fasttracker 2, Impulse Tracker ignores the portamento
		/// command if there is an portamento command next to an offset
		/// command. Even ModPlug Tracker 1.16 gets this test right
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Porta_Offset()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Porta-Offset.it", "Porta-Offset.data");
		}
	}
}
