﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds some extra information needed when remember the module list
	/// </summary>
	public class RememberListSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public RememberListSettings()
		{
			settings = new Kit.Utility.Settings("__RememberList");
			settings.LoadSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Delete the settings
		/// </summary>
		/********************************************************************/
		public void DeleteSettings()
		{
			settings.DeleteSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Save the settings
		/// </summary>
		/********************************************************************/
		public void SaveSettings()
		{
			settings.SaveSettings();
		}



		/********************************************************************/
		/// <summary>
		/// List position
		/// </summary>
		/********************************************************************/
		public int ListPosition
		{
			get => settings.GetIntEntry("Remember", "ListPosition", -1);

			set => settings.SetIntEntry("Remember", "ListPosition", value);
		}



		/********************************************************************/
		/// <summary>
		/// Module position
		/// </summary>
		/********************************************************************/
		public int ModulePosition
		{
			get => settings.GetIntEntry("Remember", "ModulePosition", -1);

			set => settings.SetIntEntry("Remember", "ModulePosition", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sub song
		/// </summary>
		/********************************************************************/
		public int SubSong
		{
			get => settings.GetIntEntry("Remember", "SubSong", -1);

			set => settings.SetIntEntry("Remember", "SubSong", value);
		}
	}
}
