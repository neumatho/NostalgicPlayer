/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Library.Sound.Equalizer
{
	/// <summary>
	///     Extension methods for EqualizerPreset enum
	/// </summary>
	public static class EqualizerPresetExtensions
	{
		/********************************************************************/
		/// <summary>
		/// Get display name for preset from resource strings
		/// </summary>
		/********************************************************************/
		public static string GetDisplayName(this EqualizerPreset preset)
		{
			return preset switch
			{
				EqualizerPreset.Custom => Resources.IDS_EQUALIZER_PRESET_CUSTOM,
				EqualizerPreset.Flat => Resources.IDS_EQUALIZER_PRESET_FLAT,
				EqualizerPreset.Rock => Resources.IDS_EQUALIZER_PRESET_ROCK,
				EqualizerPreset.Pop => Resources.IDS_EQUALIZER_PRESET_POP,
				EqualizerPreset.Classical => Resources.IDS_EQUALIZER_PRESET_CLASSICAL,
				EqualizerPreset.Jazz => Resources.IDS_EQUALIZER_PRESET_JAZZ,
				EqualizerPreset.Electronic => Resources.IDS_EQUALIZER_PRESET_ELECTRONIC,
				EqualizerPreset.Dance => Resources.IDS_EQUALIZER_PRESET_DANCE,
				EqualizerPreset.HipHop => Resources.IDS_EQUALIZER_PRESET_HIPHOP,
				EqualizerPreset.Metal => Resources.IDS_EQUALIZER_PRESET_METAL,
				EqualizerPreset.Acoustic => Resources.IDS_EQUALIZER_PRESET_ACOUSTIC,
				EqualizerPreset.BassBoost => Resources.IDS_EQUALIZER_PRESET_BASSBOOST,
				EqualizerPreset.TrebleBoost => Resources.IDS_EQUALIZER_PRESET_TREBLEBOOST,
				EqualizerPreset.VocalBoost => Resources.IDS_EQUALIZER_PRESET_VOCALBOOST,
				EqualizerPreset.FullBass => Resources.IDS_EQUALIZER_PRESET_FULLBASS,
				EqualizerPreset.FullTreble => Resources.IDS_EQUALIZER_PRESET_FULLTREBLE,
				EqualizerPreset.Soft => Resources.IDS_EQUALIZER_PRESET_SOFT,
				_ => preset.ToString()
			};
		}
	}
}
