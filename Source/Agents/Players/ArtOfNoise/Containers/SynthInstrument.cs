/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise.Containers
{
	/// <summary>
	/// A single instrument using synthesis sound
	/// </summary>
	internal class SynthInstrument : Instrument
	{
		public byte Length;

		public byte VibParam;
		public byte VibDelay;
		public byte VibWave;				// Sine, triangle, rectangle
		public byte WaveSpeed;
		public byte WaveLength;
		public byte WaveLoopStart;
		public byte WaveLoopLength;
		public byte WaveLoopControl;		// 0 = Normal, 1 = Backwards, 2 = Ping-pong
	}
}
