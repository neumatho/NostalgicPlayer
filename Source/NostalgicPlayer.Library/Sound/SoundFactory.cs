/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Sound
{
	/// <summary>
	/// Creates different sound implementations
	/// </summary>
	internal class SoundFactory : ISoundFactory
	{
		private readonly IApplicationContext applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SoundFactory(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Get a new instance of the mixer
		/// </summary>
		/********************************************************************/
		public Mixer.Mixer GetMixer()
		{
			return applicationContext.Container.GetInstance<Mixer.Mixer>();
		}



		/********************************************************************/
		/// <summary>
		/// Get a new instance of the resampler
		/// </summary>
		/********************************************************************/
		public Resampler.Resampler GetResampler()
		{
			return applicationContext.Container.GetInstance<Resampler.Resampler>();
		}
	}
}
