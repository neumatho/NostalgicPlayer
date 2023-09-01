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
		/// Scream Tracker 3 stops playback if the period is too low (the
		/// frequency is too high), but there is no maximum threshold above
		/// which playback is cut
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_S3M_FreqLimits()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "S3M"), "FreqLimits.s3m", "FreqLimits.data");
		}
	}
}
