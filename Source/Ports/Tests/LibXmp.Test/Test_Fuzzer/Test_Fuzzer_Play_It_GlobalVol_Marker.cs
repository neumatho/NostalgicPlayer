/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// This module is based on fuzzer files that found the following
		/// bugs:
		///  - When there are 256 orders in a module with markers, libxmp
		///    would read beyond the order array to look for an end marker
		///  - When a module repeats into a marker, libxmp would derive the
		///    new global volume from invalid data on the marker
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_GlobalVol_Marker()
		{
			Compare_Mixer_Data_Loops(Path.Combine(dataDirectory, "F"), "Play_It_GlobalVol_Marker.it", "Play_It_GlobalVol_Marker.data", 3);
		}
	}
}
