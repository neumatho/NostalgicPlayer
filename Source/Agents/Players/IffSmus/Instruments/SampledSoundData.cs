/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Holds data for Sampled Sound format
	/// </summary>
	internal class SampledSoundData
	{
		public ushort Volume { get; set; }

		public ushort[] EnvelopeLevels { get; } = new ushort[4];
		public ushort[] EnvelopeRates { get; } = new ushort[4];

		public short VibratoDepth { get; set; }
		public ushort VibratoSpeed { get; set; }
		public ushort VibratoDelay { get; set; }

		public SampledSoundSampleData SampleData { get; set; }
	}
}
