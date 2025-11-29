/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Modules
{
	/// <summary>
	/// Use this to create an instance of MixerConfiguration
	/// </summary>
	public static class MixerConfigurationFactory
	{
		/********************************************************************/
		/// <summary>
		/// Create a new instance based on the current settings
		/// </summary>
		/********************************************************************/
		public static MixerConfiguration Create(SoundSettings soundSettings)
		{
			return new MixerConfiguration
			{
				StereoSeparator = soundSettings.StereoSeparation,
				VisualsLatency = soundSettings.VisualsLatency * 20,
				EnableInterpolation = soundSettings.Interpolation,
				SwapSpeakers = soundSettings.SwapSpeakers,
				EnableAmigaFilter = soundSettings.AmigaFilter,
				EnableEqualizer = soundSettings.EnableEqualizer,
				EqualizerBands = soundSettings.EqualizerBands,
				EqualizerPreAmp = soundSettings.EqualizerPreAmp
			};
		}
	}
}
