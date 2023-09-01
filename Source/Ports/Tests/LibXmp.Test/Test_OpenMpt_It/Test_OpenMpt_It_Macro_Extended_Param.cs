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
		/// Even fixed macros (Z80-ZFF) can contain the letter “z”, which
		/// inserts the raw command parameter into the macro (i.e. a value
		/// in [80, FF[). In this file, macro ZF0 is used to insert byte F0
		/// into the string. This way, two MIDI messages to set both the
		/// filter cutoff and resonance to 60h are created, which are the
		/// same filter settings as used in instrument 2, so the module
		/// should stay silent
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Macro_Extended_Param()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "MacroExtendedParam.it", "MacroExtendedParam.data");
		}
	}
}
