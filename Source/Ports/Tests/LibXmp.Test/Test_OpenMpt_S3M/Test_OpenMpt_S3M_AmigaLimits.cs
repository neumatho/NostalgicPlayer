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
		/// If “Force Amiga Limits” is enabled, the limits should also be
		/// enforced on the stored period, not only on the computed period
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_S3M_AmigaLimits()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "S3M"), "AmigaLimits.s3m", "AmigaLimits.data");
		}
	}
}
