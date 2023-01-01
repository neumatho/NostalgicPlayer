/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// Extra synth information for His Master's Noise synth samples
	/// </summary>
	internal class HmnSynthData
	{
		public byte PatternNumber;
		public byte DataLoopStart;
		public byte DataLoopEnd;

		public sbyte[] WaveData;
		public byte[] Data;
		public byte[] VolumeData;
	}
}
