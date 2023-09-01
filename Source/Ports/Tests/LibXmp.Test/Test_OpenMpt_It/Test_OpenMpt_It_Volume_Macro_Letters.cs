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
		/// This test demonstrates how Zxx macros are parsed in Impulse
		/// Tracker:
		///   * Macros are evaluated only on the first tick.
		///   * They appear to be parsed after note / instrument information
		///     has been read from the pattern, but before any other effects
		///     (excluding "Set Volume").
		///   * 'u' and 'v' macros seem to emit at least a value of 1, not
		///     0 - v00 corresponds to Z01
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Volume_Macro_Letters()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Volume-Macro-Letters.it", "Volume-Macro-Letters.data");
		}
	}
}
