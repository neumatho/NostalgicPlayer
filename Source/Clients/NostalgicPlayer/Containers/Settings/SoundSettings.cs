/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the settings for sounds
	/// </summary>
	public class SoundSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SoundSettings(Kit.Utility.Settings userSettings)
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
	}
}
