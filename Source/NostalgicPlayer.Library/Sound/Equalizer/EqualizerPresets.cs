using System;

namespace Polycode.NostalgicPlayer.Library.Sound.Equalizer
{
	/// <summary>
	///     Equalizer preset types
	/// </summary>
	public enum EqualizerPreset
	{
		/// <summary>
		/// User-defined custom values
		/// </summary>
		Custom,

		/// <summary>
		/// Flat response - no equalization
		/// </summary>
		Flat,

		/// <summary>
		/// Rock music preset
		/// </summary>
		Rock,

		/// <summary>
		/// Pop music preset
		/// </summary>
		Pop,

		/// <summary>
		/// Classical music preset
		/// </summary>
		Classical,

		/// <summary>
		/// Jazz music preset
		/// </summary>
		Jazz,

		/// <summary>
		/// Electronic music preset
		/// </summary>
		Electronic,

		/// <summary>
		/// Dance music preset
		/// </summary>
		Dance,

		/// <summary>
		/// Hip-Hop music preset
		/// </summary>
		HipHop,

		/// <summary>
		/// Metal music preset
		/// </summary>
		Metal,

		/// <summary>
		/// Acoustic music preset
		/// </summary>
		Acoustic,

		/// <summary>
		/// Bass boost preset
		/// </summary>
		BassBoost,

		/// <summary>
		/// Treble boost preset
		/// </summary>
		TrebleBoost,

		/// <summary>
		/// Vocal boost preset
		/// </summary>
		VocalBoost,

		/// <summary>
		/// Full bass emphasis preset
		/// </summary>
		FullBass,

		/// <summary>
		/// Full treble emphasis preset
		/// </summary>
		FullTreble,

		/// <summary>
		/// Soft/gentle sound preset
		/// </summary>
		Soft
	}

	/// <summary>
	///     Equalizer presets (Winamp-style)
	/// </summary>
	public static class EqualizerPresets
	{
		/// <summary>
		///     Try to find matching preset for given values (returns Custom if no match)
		/// </summary>
		public static EqualizerPreset FindMatchingPreset(double[] values)
		{
			if (values == null || values.Length != 10) return EqualizerPreset.Custom;

			foreach (EqualizerPreset preset in GetAllPresets())
			{
				if (preset == EqualizerPreset.Custom) continue; // Skip Custom preset

				double[] presetValues = GetPreset(preset);
				if (presetValues == null) continue;

				bool matches = true;
				for (int i = 0; i < 10; i++)
					if (Math.Abs(values[i] - presetValues[i]) > 0.1)
					{
						matches = false;
						break;
					}

				if (matches) return preset;
			}

			return EqualizerPreset.Custom; // No match found
		}


		/// <summary>
		///     Get all preset types
		/// </summary>
		public static EqualizerPreset[] GetAllPresets() => (EqualizerPreset[])Enum.GetValues(typeof(EqualizerPreset));

		/// <summary>
		///     Get preset values by enum (returns null for Custom)
		/// </summary>
		public static double[] GetPreset(EqualizerPreset preset) =>
			// Bands: 60, 170, 310, 600, 1000, 3000, 6000, 12000, 14000, 16000 Hz
			// Note: Values shown are ±12dB UI range, internally scaled to
			// ±6dB in Equalizer class
			preset switch
			{
				EqualizerPreset.Custom => null, // Custom has no predefined values
				EqualizerPreset.Flat => new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
				EqualizerPreset.Rock => new double[] {5, 3, -3, -5, -2, 2, 5, 7, 7, 7},
				EqualizerPreset.Pop => new double[] {-1, 3, 5, 5, 3, -1, -2, -2, -1, -1},
				EqualizerPreset.Classical => new double[] {0, 0, 0, 0, 0, 0, -5, -5, -5, -7},
				EqualizerPreset.Jazz => new double[] {4, 3, 1, 2, -2, -2, 0, 2, 4, 5},
				EqualizerPreset.Electronic => new double[] {4, 3, 1, 0, -2, 2, 1, 2, 4, 5},
				EqualizerPreset.Dance => new double[] {6, 4, 2, 0, 0, -3, -4, -4, 0, 0},
				EqualizerPreset.HipHop => new double[] {5, 4, 1, 3, -1, -1, 1, -1, 2, 3},
				EqualizerPreset.Metal => new double[] {6, 4, 1, -2, -2, 1, 4, 6, 7, 8},
				EqualizerPreset.Acoustic => new double[] {4, 4, 3, 1, 2, 2, 3, 3, 4, 3},
				EqualizerPreset.BassBoost => new double[] {8, 7, 6, 4, 2, 0, 0, 0, 0, 0},
				EqualizerPreset.TrebleBoost => new double[] {0, 0, 0, 0, 0, 2, 4, 6, 8, 10},
				EqualizerPreset.VocalBoost => new double[] {-3, -1, 2, 4, 5, 4, 2, 0, -2, -3},
				EqualizerPreset.FullBass => new double[] {7, 6, 5, 4, 2, -3, -5, -6, -6, -6},
				EqualizerPreset.FullTreble => new double[] {-7, -6, -5, -3, 0, 3, 6, 8, 9, 10},
				EqualizerPreset.Soft => new double[] {3, 2, 0, -2, 0, 2, 4, 5, 6, 7},
				_ => new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
			};
	}

	/// <summary>
	///     Extension methods for EqualizerPreset enum
	/// </summary>
	public static class EqualizerPresetExtensions
	{
		/// <summary>
		///     Get display name for preset from resource strings
		/// </summary>
		public static string GetDisplayName(this EqualizerPreset preset) =>
			preset switch
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