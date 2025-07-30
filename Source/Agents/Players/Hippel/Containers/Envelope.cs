/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers
{
	/// <summary>
	/// Contain information about a single envelope
	/// </summary>
	internal class Envelope
	{
		public byte EnvelopeSpeed { get; set; }
		public byte FrequencyNumber { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoDepth { get; set; }
		public byte VibratoDelay { get; set; }
		public byte[] EnvelopeTable { get; set; }
	}
}
