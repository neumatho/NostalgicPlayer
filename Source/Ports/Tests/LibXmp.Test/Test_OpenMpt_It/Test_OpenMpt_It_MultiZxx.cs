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
		/// A MIDI macro can contain more than one MIDI message. In this
		/// case, the Z90 macro sets both the filter cutoff frequency and
		/// resonance, so if only the first MIDI message is considered in
		/// this macro, the module will no longer stay silent at row 8
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_MultiZxx()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "MultiZxx.it", "MultiZxx.data");
		}
	}
}
