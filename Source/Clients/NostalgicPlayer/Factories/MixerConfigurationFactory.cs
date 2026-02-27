/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Factories
{
	/// <summary>
	/// Use this to create an instance of MixerConfiguration
	/// </summary>
	public class MixerConfigurationFactory : IMixerConfigurationFactory
	{
		private readonly SoundSettings soundSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MixerConfigurationFactory(SoundSettings soundSettings)
		{
			this.soundSettings = soundSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance based on the current settings
		/// </summary>
		/********************************************************************/
		public MixerConfiguration Create()
		{
			return new MixerConfiguration
			{
				StereoSeparator = soundSettings.StereoSeparation,
				VisualsLatency = soundSettings.VisualsLatency * 20,
				InterpolationMode = soundSettings.InterpolationMode,
				SwapSpeakers = soundSettings.SwapSpeakers,
				EnableAmigaFilter = soundSettings.AmigaFilter,
				EnableEqualizer = soundSettings.EnableEqualizer,
				EqualizerBands = soundSettings.EqualizerBands,
				EqualizerPreAmp = soundSettings.EqualizerPreAmp
			};
		}
	}
}
