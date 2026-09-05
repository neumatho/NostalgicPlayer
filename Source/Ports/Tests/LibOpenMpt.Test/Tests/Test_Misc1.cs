/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Tests
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Misc1()
		{
			Assert.IsFalse(ModCommand.IsPcNote(ModCommand.Note_Max));
			Assert.IsTrue(ModCommand.IsPcNote(ModCommand.Note_Pc));
			Assert.IsTrue(ModCommand.IsPcNote(ModCommand.Note_Pcs));

			//XX CModSpecifications::ExtensionToType
			//XX SampleFormat::FromInt
		}
	}
}
