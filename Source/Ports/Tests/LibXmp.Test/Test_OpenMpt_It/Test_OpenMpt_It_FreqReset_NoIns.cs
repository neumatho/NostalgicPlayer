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
		/// When using multisample instruments, even notes with no instrument
		/// number next to them can change the sample (based on the active
		/// instrument’s sample map). When switching between samples, you
		/// must not forget to update the C-5 frequency of the playing sample
		/// as well
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_FreqReset_NoIns()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "FreqReset-NoIns.it", "FreqReset-NoIns.data");
		}
	}
}
