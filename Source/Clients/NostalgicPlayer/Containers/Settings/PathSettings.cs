/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Helpers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the path settings
	/// </summary>
	public class PathSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PathSettings(ISettings userSettings)
		{
			settings = userSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Start scanning path
		/// </summary>
		/********************************************************************/
		public string StartScan
		{
			get => settings.GetStringEntry("Path", "StartScan");

			set => settings.SetStringEntry("Path", "StartScan", value);
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
