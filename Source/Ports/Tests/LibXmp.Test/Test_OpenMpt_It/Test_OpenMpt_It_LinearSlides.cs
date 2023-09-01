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
		/// Impulse Tracker internally uses actual frequency values, while
		/// Fasttracker 2 still uses exponentially scaled fine Amiga periods.
		/// When doing fine slides, errors from using periods instead of
		/// frequency can add up very quickly, and thus the two channel's
		/// frequency in this test will converge noticeably
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_LinearSlides()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "LinearSlides.it", "LinearSlides.data");
		}
	}
}
