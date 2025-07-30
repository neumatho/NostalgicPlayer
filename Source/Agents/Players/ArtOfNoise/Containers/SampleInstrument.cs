/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise.Containers
{
	/// <summary>
	/// A single instrument using a sample
	/// </summary>
	internal class SampleInstrument : Instrument
	{
		public uint StartOffset { get; set; }
		public uint Length { get; set; }
		public uint LoopStart { get; set; }
		public uint LoopLength { get; set; }
	}
}
