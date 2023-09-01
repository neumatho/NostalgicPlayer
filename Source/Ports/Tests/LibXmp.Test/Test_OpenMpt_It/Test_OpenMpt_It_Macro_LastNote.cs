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
		/// A test of the MIDI macro letter “n”. This letter will always
		/// send the MIDI note value of the last triggered note, note cuts
		/// and similar “notes” are not considered. This module should remain
		/// silent as both channels should receive exactly the same cutoff
		/// values
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Macro_LastNote()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Macro-LastNote.it", "Macro-LastNote.data");
		}
	}
}
