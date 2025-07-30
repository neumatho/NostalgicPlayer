/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise.Containers
{
	/// <summary>
	/// Base class for all instrument types
	/// </summary>
	internal abstract class Instrument
	{
		public string Name { get; set; }
		public byte Volume { get; set; }
		public byte FineTune { get; set; }
		public byte WaveForm { get; set; }

		public byte EnvelopeStart { get; set; }
		public byte EnvelopeAdd { get; set; }
		public byte EnvelopeEnd { get; set; }
		public byte EnvelopeSub { get; set; }
	}
}
