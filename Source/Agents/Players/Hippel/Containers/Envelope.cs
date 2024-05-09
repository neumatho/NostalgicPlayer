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
		public byte EnvelopeSpeed;
		public byte FrequencyNumber;
		public byte VibratoSpeed;
		public byte VibratoDepth;
		public byte VibratoDelay;
		public byte[] EnvelopeTable = new byte[64 - 5];
	}
}
