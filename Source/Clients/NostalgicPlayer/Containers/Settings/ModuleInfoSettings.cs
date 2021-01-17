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
	/// This class holds all the module information window settings
	/// </summary>
	public class ModuleInfoSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoSettings(Kit.Utility.Settings windowSettings)
		{
			settings = windowSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Column 1 width
		/// </summary>
		/********************************************************************/
		public int Column1Width
		{
			get => settings.GetIntEntry("List", "Col1Width", 142);

			set => settings.SetIntEntry("List", "Col1Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column 2 width
		/// </summary>
		/********************************************************************/
		public int Column2Width
		{
			get => settings.GetIntEntry("List", "Col2Width", 141);

			set => settings.SetIntEntry("List", "Col2Width", value);
		}
	}
}
