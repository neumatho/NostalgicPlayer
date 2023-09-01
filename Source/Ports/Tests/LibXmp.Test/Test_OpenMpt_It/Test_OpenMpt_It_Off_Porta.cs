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
		/// When "Compatible Gxx" is disabled, the key-off flag should only
		/// be removed when triggering new notes, but not when continuing a
		/// note using a portamento command (row 2, 4). However, you should
		/// keep in mind that the portamento flag is still set even if there
		/// is an offset command next to the portamento command (row 4),
		/// which would normally nullify the portamento effect (see
		/// porta-offset.it)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Off_Porta()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Off-Porta.it", "Off-Porta.data");
		}
	}
}
