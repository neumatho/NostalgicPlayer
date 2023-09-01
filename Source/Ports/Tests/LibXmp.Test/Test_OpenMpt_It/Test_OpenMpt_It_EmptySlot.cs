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
		/// I think, Impulse Tracker treats instruments like an additional
		/// layer of abstraction and first replaces the note and instrument
		/// in the pattern by the sample and note assignments from the sample
		/// map table before further evaluating the pattern. That would
		/// explain why for example the empty sample map slots do nothing in
		/// this module
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_EmptySlot()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "EmptySlot.it", "EmptySlot.data");
		}
	}
}
