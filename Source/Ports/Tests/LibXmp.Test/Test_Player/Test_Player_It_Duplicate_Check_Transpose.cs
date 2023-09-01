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
		/// Duplicate note check is based on the key used to play the note,
		/// not the note played after transpose. Only the exact same key
		/// should trigger the duplicate check action in this module; keys
		/// transposed to the same note should not
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Duplicate_Check_Transpose()
		{
			Compare_Mixer_Data(dataDirectory, "Duplicate_Check_Transpose.it", "Duplicate_Check_Transpose.data");
		}
	}
}
