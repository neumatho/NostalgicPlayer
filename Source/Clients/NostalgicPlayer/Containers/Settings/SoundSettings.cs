/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Helpers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the settings for sounds
	/// </summary>
	public class SoundSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SoundSettings(ISettings userSettings)
		{
			settings = userSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Default output agent
		/// </summary>
		/********************************************************************/
		public Guid DefaultOutputAgent => new Guid("b9cef7e4-c74c-4af0-b01d-802f0d1b4cc7"); // This is the ID of the CoreAudio output agent



		/********************************************************************/
		/// <summary>
		/// Output agent
		/// </summary>
		/********************************************************************/
		public Guid OutputAgent
		{
			get
			{
				if (Guid.TryParse(settings.GetStringEntry("Sound", "OutputAgent"), out Guid g))
					return g;

				return DefaultOutputAgent;
			}

			set => settings.SetStringEntry("Sound", "OutputAgent", value.ToString("D"));
		}



		/********************************************************************/
		/// <summary>
		/// Stereo separation
		/// </summary>
		/********************************************************************/
		public int StereoSeparation
		{
			get => settings.GetIntEntry("Sound", "StereoSep", 100);

			set => settings.SetIntEntry("Sound", "StereoSep", value);
		}



		/********************************************************************/
		/// <summary>
		/// Visuals latency
		/// </summary>
		/********************************************************************/
		public int VisualsLatency
		{
			get => settings.GetIntEntry("Sound", "VisualsLatency", 0);

			set => settings.SetIntEntry("Sound", "VisualsLatency", value);
		}



		/********************************************************************/
		/// <summary>
		/// Interpolation
		/// </summary>
		/********************************************************************/
		public bool Interpolation
		{
			get => settings.GetBoolEntry("Sound", "Interpolation", false);

			set => settings.SetBoolEntry("Sound", "Interpolation", value);
		}



		/********************************************************************/
		/// <summary>
		/// Swap left and right speakers
		/// </summary>
		/********************************************************************/
		public bool SwapSpeakers
		{
			get => settings.GetBoolEntry("Sound", "SwapSpeakers", false);

			set => settings.SetBoolEntry("Sound", "SwapSpeakers", value);
		}



		/********************************************************************/
		/// <summary>
		/// Which surround mode to use
		/// </summary>
		/********************************************************************/
		public SurroundMode SurroundMode
		{
			get => settings.GetEnumEntry("Sound", "SurroundMode", SurroundMode.None);

			set => settings.SetEnumEntry("Sound", "SurroundMode", value);
		}



		/********************************************************************/
		/// <summary>
		/// Disable center speaker
		/// </summary>
		/********************************************************************/
		public bool DisableCenterSpeaker
		{
			get => settings.GetBoolEntry("Sound", "DisableCenterSpeaker", false);

			set => settings.SetBoolEntry("Sound", "DisableCenterSpeaker", value);
		}



		/********************************************************************/
		/// <summary>
		/// Emulate Amiga LED filter
		/// </summary>
		/********************************************************************/
		public bool AmigaFilter
		{
			get => settings.GetBoolEntry("Sound", "AmigaFilter", true);

			set => settings.SetBoolEntry("Sound", "AmigaFilter", value);
		}



		/********************************************************************/
		/// <summary>
		/// Enable equalizer
		/// </summary>
		/********************************************************************/
		public bool EnableEqualizer
		{
			get => settings.GetBoolEntry("Sound", "EnableEqualizer", false);

			set => settings.SetBoolEntry("Sound", "EnableEqualizer", value);
		}



		/********************************************************************/
		/// <summary>
		/// Equalizer band values in dB (-12 to +12 dB)
		/// 10 bands: 60, 170, 310, 600, 1k, 3k, 6k, 12k, 14k, 16k Hz
		/// </summary>
		/********************************************************************/
		public double[] EqualizerBands
		{
			get
			{
				string value = settings.GetStringEntry("Sound", "EqualizerBands");
				if (string.IsNullOrEmpty(value))
					return new double[10]; // Flat response (0 dB on all bands)

				try
				{
					string[] parts = value.Split(',');
					if (parts.Length != 10)
						return new double[10];

					double[] result = new double[10];
					for (int i = 0; i < 10; i++)
					{
						if (!double.TryParse(parts[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result[i]))
							return new double[10];
					}

					return result;
				}
				catch
				{
					return new double[10];
				}
			}

			set
			{
				if ((value == null) || (value.Length != 10))
					return;

				string[] parts = new string[10];
				for (int i = 0; i < 10; i++)
					parts[i] = value[i].ToString("F1", System.Globalization.CultureInfo.InvariantCulture);

				settings.SetStringEntry("Sound", "EqualizerBands", string.Join(",", parts));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Equalizer pre-amp gain in dB (-12 to +12 dB)
		/// </summary>
		/********************************************************************/
		public double EqualizerPreAmp
		{
			get
			{
				string value = settings.GetStringEntry("Sound", "EqualizerPreAmp");
				if (string.IsNullOrEmpty(value))
					return 0.0; // Default: no attenuation/boost

				if (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double result))
					return 0.0;

				return Math.Max(-12.0, Math.Min(12.0, result));
			}

			set
			{
				double clamped = Math.Max(-12.0, Math.Min(12.0, value));
				settings.SetStringEntry("Sound", "EqualizerPreAmp", clamped.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
			}
		}
	}
}
