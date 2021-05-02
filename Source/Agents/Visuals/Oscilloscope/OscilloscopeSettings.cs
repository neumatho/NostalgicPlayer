/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display;

namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope
{
	/// <summary>
	/// This class holds all the settings
	/// </summary>
	internal class OscilloscopeSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OscilloscopeSettings()
		{
			settings = new Kit.Utility.Settings("Oscilloscope");
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
		public SpeakerOscilloscopeControl.ScopeType ScopeType
		{
			get
			{
				if (Enum.TryParse(settings.GetStringEntry("General", "ScopeType"), out SpeakerOscilloscopeControl.ScopeType type))
					return type;

				return SpeakerOscilloscopeControl.ScopeType.Filled;
			}

			set => settings.SetStringEntry("General", "ScopeType", value.ToString());
		}
	}
}
