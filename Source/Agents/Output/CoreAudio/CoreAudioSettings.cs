/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Output.CoreAudio
{
	/// <summary>
	/// This class holds all the settings
	/// </summary>
	public class CoreAudioSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CoreAudioSettings()
		{
			settings = new Kit.Utility.Settings("CoreAudio");
			settings.LoadSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Return the main settings object
		/// </summary>
		/********************************************************************/
		public Kit.Utility.Settings Settings => settings;



		/********************************************************************/
		/// <summary>
		/// Which output device to use
		/// </summary>
		/********************************************************************/
		public string OutputDevice
		{
			get => settings.GetStringEntry("General", "OutputDevice", string.Empty);

			set => settings.SetStringEntry("General", "OutputDevice", value);
		}
	}
}
