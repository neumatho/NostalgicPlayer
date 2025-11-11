using System;
using System.ComponentModel;

namespace Polycode.NostalgicPlayer.Library.Sound.Mixer
{
	/// <summary>
	/// Equalizer preset types
	/// </summary>
	public enum EqualizerPreset
	{
		Custom,		// User-defined values
		Flat,
		Rock,
		Pop,
		Classical,
		Jazz,
		Electronic,
		Dance,
		[Description("Hip-Hop")]
		HipHop,
		Metal,
		Acoustic,
		[Description("Bass Boost")]
		BassBoost,
		[Description("Treble Boost")]
		TrebleBoost,
		[Description("Vocal Boost")]
		VocalBoost,
		[Description("Full Bass")]
		FullBass,
		[Description("Full Treble")]
		FullTreble,
		Soft
	}

	/// <summary>
	/// Equalizer presets (Winamp-style)
	/// </summary>
	public static class EqualizerPresets
	{
		/// <summary>
		/// Get preset values by enum (returns null for Custom)
		/// </summary>
		public static double[] GetPreset(EqualizerPreset preset)
		{
			// Bands: 60, 170, 310, 600, 1000, 3000, 6000, 12000, 14000, 16000 Hz
			// Note: Values shown are ±12dB UI range, internally scaled to ±6dB in Equalizer class
			return preset switch
			{
				EqualizerPreset.Custom => null, // Custom has no predefined values
				EqualizerPreset.Flat => new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
				EqualizerPreset.Rock => new double[] { 5, 3, -3, -5, -2, 2, 5, 7, 7, 7 },
				EqualizerPreset.Pop => new double[] { -1, 3, 5, 5, 3, -1, -2, -2, -1, -1 },
				EqualizerPreset.Classical => new double[] { 0, 0, 0, 0, 0, 0, -5, -5, -5, -7 },
				EqualizerPreset.Jazz => new double[] { 4, 3, 1, 2, -2, -2, 0, 2, 4, 5 },
				EqualizerPreset.Electronic => new double[] { 4, 3, 1, 0, -2, 2, 1, 2, 4, 5 },
				EqualizerPreset.Dance => new double[] { 6, 4, 2, 0, 0, -3, -4, -4, 0, 0 },
				EqualizerPreset.HipHop => new double[] { 5, 4, 1, 3, -1, -1, 1, -1, 2, 3 },
				EqualizerPreset.Metal => new double[] { 6, 4, 1, -2, -2, 1, 4, 6, 7, 8 },
				EqualizerPreset.Acoustic => new double[] { 4, 4, 3, 1, 2, 2, 3, 3, 4, 3 },
				EqualizerPreset.BassBoost => new double[] { 8, 7, 6, 4, 2, 0, 0, 0, 0, 0 },
				EqualizerPreset.TrebleBoost => new double[] { 0, 0, 0, 0, 0, 2, 4, 6, 8, 10 },
				EqualizerPreset.VocalBoost => new double[] { -3, -1, 2, 4, 5, 4, 2, 0, -2, -3 },
				EqualizerPreset.FullBass => new double[] { 7, 6, 5, 4, 2, -3, -5, -6, -6, -6 },
				EqualizerPreset.FullTreble => new double[] { -7, -6, -5, -3, 0, 3, 6, 8, 9, 10 },
				EqualizerPreset.Soft => new double[] { 3, 2, 0, -2, 0, 2, 4, 5, 6, 7 },
				_ => new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
			};
		}


		/// <summary>
		/// Get all preset types
		/// </summary>
		public static EqualizerPreset[] GetAllPresets()
		{
			return (EqualizerPreset[])Enum.GetValues(typeof(EqualizerPreset));
		}

		/// <summary>
		/// Try to find matching preset for given values (returns Custom if no match)
		/// </summary>
		public static EqualizerPreset FindMatchingPreset(double[] values)
		{
			if (values == null || values.Length != 10)
				return EqualizerPreset.Custom;

			foreach (EqualizerPreset preset in GetAllPresets())
			{
				if (preset == EqualizerPreset.Custom)
					continue; // Skip Custom preset

				double[] presetValues = GetPreset(preset);
				if (presetValues == null)
					continue;

				bool matches = true;
				for (int i = 0; i < 10; i++)
				{
					if (Math.Abs(values[i] - presetValues[i]) > 0.1)
					{
						matches = false;
						break;
					}
				}

				if (matches)
					return preset;
			}

			return EqualizerPreset.Custom; // No match found
		}
	}

	/// <summary>
	/// Extension methods for EqualizerPreset enum
	/// </summary>
	public static class EqualizerPresetExtensions
	{
		/// <summary>
		/// Get display name for preset (from Description attribute or ToString)
		/// </summary>
		public static string GetDisplayName(this EqualizerPreset preset)
		{
			var field = preset.GetType().GetField(preset.ToString());
			var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
			return attribute?.Description ?? preset.ToString();
		}
	}
}
