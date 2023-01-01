/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the module information window settings
	/// </summary>
	public class ModuleInfoWindowSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoWindowSettings(ISettings windowSettings)
		{
			settings = windowSettings;
		}



		/********************************************************************/
		/// <summary>
		/// The tab to auto select when new module is loaded
		/// </summary>
		/********************************************************************/
		public int AutoSelectTab
		{
			get => settings.GetIntEntry("Window", "AutoSelectTab", 0);

			set => settings.SetIntEntry("Window", "AutoSelectTab", value);
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
