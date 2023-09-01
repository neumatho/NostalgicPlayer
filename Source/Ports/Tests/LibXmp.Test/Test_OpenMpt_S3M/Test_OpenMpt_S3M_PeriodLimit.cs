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
		/// ScreamTracker 3 limits the final output period to be at least 64,
		/// i.e. when playing a note that is too high or when sliding the
		/// period lower than 64, the output period will simply be clamped
		/// to 64. However, when reaching a period of 0 through slides, the
		/// output on the channel should be stopped
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_S3M_PeriodLimit()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "S3M"), "PeriodLimit.s3m", "PeriodLimit.data");
		}
	}
}
