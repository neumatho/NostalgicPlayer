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
		public string SampleName;

		// Information from sample file
		public ushort LengthOfOctaveOne;
		public ushort LoopLengthOfOctaveOne;
		public byte StartOctave;
		public byte EndOctave;
		public sbyte[] SampleData;
	}
}
