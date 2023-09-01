/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Xm
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Xm
	{
		/********************************************************************/
		/// <summary>
		/// Fasttracker 2 tries to be inconsistent where possible, so you
		/// have to duplicate a lot of code or add conditions to behave
		/// exactly like Fasttracker. Yeah! Okay, seriously: Generally the
		/// vibrato and tremolo tables are identical to those that ProTracker
		/// uses, but the vibrato’s “ramp down” table is upside down. You
		/// will have to negate its sign for it to work as intended
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_VibratoWaveforms()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "VibratoWaveforms.xm", "VibratoWaveforms.data");
		}
	}
}
