/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Holds sampled sound sample data
	/// </summary>
	internal class SampledSoundSampleData
	{
		public string SampleName { get; set; }

		// Information from sample file
		public ushort LengthOfOctaveOne { get; set; }
		public ushort LoopLengthOfOctaveOne { get; set; }
		public byte StartOctave { get; set; }
		public byte EndOctave { get; set; }
		public sbyte[] SampleData { get; set; }
	}
}
