/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Sound
{
	/// <summary>
	/// Creates different sound implementations
	/// </summary>
	internal interface ISoundFactory
	{
		/// <summary>
		/// Get a new instance of the mixer
		/// </summary>
		Mixer.Mixer GetMixer();

		/// <summary>
		/// Get a new instance of the resampler
		/// </summary>
		Resampler.Resampler GetResampler();
	}
}
