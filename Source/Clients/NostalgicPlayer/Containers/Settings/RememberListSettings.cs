/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds some extra information needed when remember the module list
	/// </summary>
	public class RememberListSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public RememberListSettings()
		{
			settings = DependencyInjection.GetDefaultProvider().GetService<ISettings>();
			settings.LoadSettings("__RememberList");
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
