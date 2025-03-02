/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility;

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
		/// Dolby Pro Logic surround
		/// </summary>
		/********************************************************************/
		public bool Surround
		{
			get => settings.GetBoolEntry("Sound", "Surround", false);

			set => settings.SetBoolEntry("Sound", "Surround", value);
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
	}
}
