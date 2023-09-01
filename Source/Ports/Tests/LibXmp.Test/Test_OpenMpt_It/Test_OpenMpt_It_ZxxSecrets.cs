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
		/// Impulse Tracker supports three MIDI macro characters which are
		/// not documented in MIDI.TXT:
		///   * h: Host channel, i.e. the pattern channel in which the Zxx
		///     command is encountered (0-based).
		///   * o: The last used sample offset value. The high offset (SAx)
		///     is not taken into account. Note that offsets above 80h are
		///     not clamped, i.e. they generate MIDI command bytes (e.g. O90
		///     would cause a note-on command to be emitted).
		///   * m: This command sends the current MIDI note if the channel is
		///     a MIDI channel, but on sample channels the current loop
		///     direction (forward = 0, backward = 1) of the sample is stored
		///     in the same memory location, so the macro evaluates to that
		///     instead of a note number.
		/// 
		/// In addition, the MIDI messages FA (start song), FC (stop song)
		/// and FF (reset) reset the resonant filter parameters
		/// (cutoff = 127, resonance = 0) for all channels, but do not
		/// immediately update the filter coefficients.
		/// 
		/// FIXME: libxmp gets the mixer test data correct but the audio
		/// test (libxmp's output and the IT WAV sample should cancel out)
		/// does not work, probably due to libxmp right shifting the cutoff
		/// by the filter envelope range instead of deriving coefficients off
		/// of the product
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_ZxxSecrets()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "ZxxSecrets.it", "ZxxSecrets.data");
		}
	}
}
