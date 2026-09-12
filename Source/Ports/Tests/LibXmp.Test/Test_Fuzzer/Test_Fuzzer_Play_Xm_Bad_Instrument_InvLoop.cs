/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// This test relies on several fringe behaviors that might not be
		/// permanent:
		/// - Loading XMs does not filter out unsupported extended (Exx)
		///   effects, in this case, EFx (invert loop).
		/// - Loading XMs does not filter bad loop parameters when there is
		///   no sample data, as these samples will never be mixed.
		/// - When the player is set to Protracker 2 mode, EFx effects are
		///   interpreted as invert loop and can be used on junk samples
		///   attached to valid instruments via Protracker 2 instrument
		///   changes.
		///
		/// The invert loop handler correctly filtered NULL samples, but did
		/// not avoid signed integer overflow from bad loop parameters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Xm_Bad_Instrument_InvLoop()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Set_Player_Mode, (c_int)Xmp_Mode.ProTracker, 0),
				new Playback_Sequence(Playback_Action.Play_Frames, 4, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Xm_Bad_Instrument_InvLoop.xm", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
