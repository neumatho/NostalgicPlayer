/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers
{
	/// <summary>
	/// Holds all the information about the player state at a specific time
	/// </summary>
	internal class Snapshot : ISnapshot
	{
		public GlobalPlayingInfo PlayingInfo;
		public Instrument[] Instruments;
		public SampleNegateInfo[] SampleNegateInfo;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Snapshot(GlobalPlayingInfo playingInfo, Instrument[] instruments, SampleNegateInfo[] sampleNegateInfo)
		{
			PlayingInfo = playingInfo.MakeDeepClone();
			Instruments = ArrayHelper.CloneObjectArray(instruments);
			SampleNegateInfo = sampleNegateInfo;
		}
	}
}
