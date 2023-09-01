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
		/// This is a very interesting test case, because it actually sounds
		/// different when using Impulse Tracker’s WAV writer and Sound
		/// Blaster (and probably other drivers) output. The main difference
		/// is that the Sound Blaster driver will only consider the envelope
		/// carry flag if the New Note Action is not “Note Cut”. The WAV
		/// writer does not check for the NNA and will always apply the carry
		/// flag if it’s set. OpenMPT goes after the WAV writer, while XMPlay
		/// (at the time of writing) uses the Sound Blaster behaviour for the
		/// volume and panning envelope, but the WAV writer’s behaviour for
		/// the filter envelope (because of a bug report some years ago).
		/// Obviously it is hard to consider which of the two behaviours is
		/// correct, so I would just say that both are :)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_CarryNna()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "CarryNna.it", "CarryNna.data");
		}
	}
}
