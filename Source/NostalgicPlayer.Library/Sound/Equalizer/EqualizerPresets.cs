/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Library.Sound.Equalizer
{
	/// <summary>
	/// Equalizer presets (Winamp-style)
	/// </summary>
	public static class EqualizerPresets
	{
		/********************************************************************/
		/// <summary>
		/// Try to find matching preset for given values (returns Custom if
		/// no match)
		/// </summary>
		/********************************************************************/
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



		/********************************************************************/
		/// <summary>
		/// Get all preset types
		/// </summary>
		/********************************************************************/
		public static EqualizerPreset[] GetAllPresets()
		{
			return (EqualizerPreset[])Enum.GetValues(typeof(EqualizerPreset));
		}



		/********************************************************************/
		/// <summary>
		/// Get preset values by enum (returns null for Custom)
		/// </summary>
		/********************************************************************/
		public static double[] GetPreset(EqualizerPreset preset)
		{
			// Bands: 60, 170, 310, 600, 1000, 3000, 6000, 12000, 14000, 16000 Hz
			// Note: Values shown are ±12dB UI range, internally scaled to
			// ±6dB in Equalizer class
			return preset switch
			{
				EqualizerPreset.Custom => null, // Custom has no predefined values
				EqualizerPreset.Flat => [ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ],
				EqualizerPreset.Rock => [ 5, 3, -3, -5, -2, 2, 5, 7, 7, 7 ],
				EqualizerPreset.Pop => [ -1, 3, 5, 5, 3, -1, -2, -2, -1, -1 ],
				EqualizerPreset.Classical => [ 0, 0, 0, 0, 0, 0, -5, -5, -5, -7 ],
				EqualizerPreset.Jazz => [ 4, 3, 1, 2, -2, -2, 0, 2, 4, 5 ],
				EqualizerPreset.Electronic => [ 4, 3, 1, 0, -2, 2, 1, 2, 4, 5 ],
				EqualizerPreset.Dance => [ 6, 4, 2, 0, 0, -3, -4, -4, 0, 0 ],
				EqualizerPreset.HipHop => [ 5, 4, 1, 3, -1, -1, 1, -1, 2, 3 ],
				EqualizerPreset.Metal => [ 6, 4, 1, -2, -2, 1, 4, 6, 7, 8 ],
				EqualizerPreset.Acoustic => [ 4, 4, 3, 1, 2, 2, 3, 3, 4, 3 ],
				EqualizerPreset.BassBoost => [ 8, 7, 6, 4, 2, 0, 0, 0, 0, 0 ],
				EqualizerPreset.TrebleBoost => [ 0, 0, 0, 0, 0, 2, 4, 6, 8, 10 ],
				EqualizerPreset.VocalBoost => [ -3, -1, 2, 4, 5, 4, 2, 0, -2, -3 ],
				EqualizerPreset.FullBass => [ 7, 6, 5, 4, 2, -3, -5, -6, -6, -6 ],
				EqualizerPreset.FullTreble => [ -7, -6, -5, -3, 0, 3, 6, 8, 9, 10 ],
				EqualizerPreset.Soft => [ 3, 2, 0, -2, 0, 2, 4, 5, 6, 7 ],
				_ => [ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ]
			};
		}
	}
}
