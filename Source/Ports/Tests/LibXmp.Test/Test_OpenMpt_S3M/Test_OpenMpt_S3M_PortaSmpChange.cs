/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_S3M
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_S3M
	{
		/********************************************************************/
		/// <summary>
		/// If the sample number next to a portamento effect differs from
		/// the previous number, the old sample should be kept, but the new
		/// sample's default volume should still be applied
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_S3M_PortaSmpChange()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "S3M"), "PortaSmpChange.s3m", "PortaSmpChange.data");
		}
	}
}
