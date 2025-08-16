/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Sound.Mixer.Containers
{
	/// <summary>
	/// Holds standard information about a sample
	/// </summary>
	internal class VoiceSampleInfo
	{
		public SampleFlag Flags { get; set; }
		public VoiceSample Sample { get; } = new VoiceSample();
		public VoiceSample Loop { get; set; }
	}
}
