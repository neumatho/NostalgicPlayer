/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Xm
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Xm
	{
		/********************************************************************/
		/// <summary>
		/// Tone portamento is actually the only volume column command with
		/// an effect memory (as it is shared with the other effect column).
		/// Another nice bug demonstrated in this module is the combination
		/// of both portamento commands (Mx and 3xx) in the same cell: The
		/// 3xx parameter is ignored completely, and the Mx parameter is
		/// doubled, i.e. M2 3FF is the same as M4 000
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_TonePortamentoMemory()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "TonePortamentoMemory.xm", "TonePortamentoMemory.data");
		}
	}
}
