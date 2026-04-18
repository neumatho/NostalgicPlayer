/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Library.Sound.Mixer;
using Polycode.NostalgicPlayer.Library.Sound.Resampler;

namespace Polycode.NostalgicPlayer.Library.Sound
{
	/// <summary>
	/// Factory to SoundStream implementations
	/// </summary>
	internal interface ISoundStreamFactory
	{
		/// <summary>
		/// Return a new instance of the mixer stream
		/// </summary>
		MixerStream GetMixerStream();

		/// <summary>
		/// Return a new instance of the resampler stream
		/// </summary>
		ResamplerStream GetResamplerStream();
	}
}
