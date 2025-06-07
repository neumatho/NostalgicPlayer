/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// This shows the Audius window
	/// </summary>
	public partial class AudiusWindowForm : WindowFormBase
	{
		private MainWindowForm mainWindow;

		private const int Page_Trending = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusWindowForm(MainWindowForm mainWindow, OptionSettings optionSettings)
		{
			InitializeComponent();

			// Remember the arguments
			this.mainWindow = mainWindow;

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("AudiusWindow");

				// Set the title of the window
				Text = Resources.IDS_AUDIUS_TITLE;

				// Set the tab titles
				navigator.Pages[Page_Trending].Text = Resources.IDS_AUDIUS_TAB_TRENDING;
			}
		}
	}
}
