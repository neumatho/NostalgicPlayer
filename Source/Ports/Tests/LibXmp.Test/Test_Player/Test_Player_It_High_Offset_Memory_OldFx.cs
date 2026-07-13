/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// 1) High offset should NOT be applied unless Oxx is present.
		/// 2) Offsets past the end of a sample set the offset to the end in
		///    old FX mode
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_High_Offset_Memory_OldFx()
		{
			Compare_Mixer_Data(dataDirectory, "It_High_Offset_Memory_OldFx.it", "It_High_Offset_Memory_OldFx.data");
		}
	}
}
