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
		/// As mentioned already, filtering is only ever done in IT if either
		/// cutoff is not full or if resonance is set. When a Z7F command is
		/// found next to a note and no portamento is applied, it disables
		/// the filter, however in other cases this should not happen
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Filter_Reset()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Filter-Reset.it", "Filter-Reset.data");
		}
	}
}
