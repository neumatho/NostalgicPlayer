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
		public string Name;
		public byte Volume;
		public byte FineTune;
		public byte WaveForm;

		public byte EnvelopeStart;
		public byte EnvelopeAdd;
		public byte EnvelopeEnd;
		public byte EnvelopeSub;
	}
}
