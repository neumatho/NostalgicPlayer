/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise.Containers
{
	/// <summary>
	/// Holds information about a single sample
	/// </summary>
	internal class Sample
	{
		public SampleType Type { get; set; }
		public byte WaveformSpeed { get; set; }
		public ushort PeriodMultiplier { get; set; }
		public uint DataOffset { get; set; }
		public ushort Length { get; set; }
		public ushort LoopStart { get; set; }
		public ushort EnvelopeOffset { get; set; }
		public byte Volume { get; set; }
		public byte EnvelopeSpeed { get; set; }
		public ushort VibratoOffset { get; set; }
		public byte VibratoDelay { get; set; }
		public byte VibratoSpeed { get; set; }
		public ushort ArpeggioOffset { get; set; }
		public byte ArpeggioSpeed { get; set; }
	}
}
