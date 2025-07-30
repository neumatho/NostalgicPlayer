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
		public byte Length { get; set; }

		public byte VibParam { get; set; }
		public byte VibDelay { get; set; }
		public byte VibWave { get; set; }				// Sine, triangle, rectangle
		public byte WaveSpeed { get; set; }
		public byte WaveLength { get; set; }
		public byte WaveLoopStart { get; set; }
		public byte WaveLoopLength { get; set; }
		public byte WaveLoopControl { get; set; }		// 0 = Normal, 1 = Backwards, 2 = Ping-pong
	}
}
