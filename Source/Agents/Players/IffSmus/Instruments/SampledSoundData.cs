﻿/******************************************************************************/
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
		public ushort Volume;

		public ushort[] EnvelopeLevels = new ushort[4];
		public ushort[] EnvelopeRates = new ushort[4];

		public short VibratoDepth;
		public ushort VibratoSpeed;
		public ushort VibratoDelay;

		public SampledSoundSampleData SampleData;
	}
}
