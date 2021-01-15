/******************************************************************************/
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
	/// This class holds all the path settings
	/// </summary>
	public class PathSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PathSettings(Kit.Utility.Settings userSettings)
		{
			settings = userSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Module start path
		/// </summary>
		/********************************************************************/
		public string Modules
		{
			get => settings.GetStringEntry("Path", "Modules");

			set => settings.SetStringEntry("Path", "Modules", value);
		}



		/********************************************************************/
		/// <summary>
		/// Module list start path
		/// </summary>
		/********************************************************************/
		public string ModuleList
		{
			get => settings.GetStringEntry("Path", "List");

			set => settings.SetStringEntry("Path", "List", value);
		}
	}
}
