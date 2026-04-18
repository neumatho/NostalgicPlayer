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
	internal class SoundStreamFactory : ISoundStreamFactory
	{
		private readonly ISoundFactory soundFactory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SoundStreamFactory(ISoundFactory soundFactory)
		{
			this.soundFactory = soundFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Return a new instance of the mixer stream
		/// </summary>
		/********************************************************************/
		public MixerStream GetMixerStream()
		{
			return new MixerStream(soundFactory);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new instance of the resampler stream
		/// </summary>
		/********************************************************************/
		public ResamplerStream GetResamplerStream()
		{
			return new ResamplerStream(soundFactory);
		}
	}
}
