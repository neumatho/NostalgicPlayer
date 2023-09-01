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
		/// Contrary to XM, the default instrument and sample panning should
		/// only be reset when a note is encountered, not when an instrument
		/// number (without note) is encountered. The two channels of this
		/// module should be panned identically
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_PanReset()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "PanReset.it", "PanReset.data");
		}
	}
}
