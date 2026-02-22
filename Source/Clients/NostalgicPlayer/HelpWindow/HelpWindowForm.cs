/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.HelpWindow
{
	/// <summary>
	/// This shows the help documentation
	/// </summary>
	public partial class HelpWindowForm : WindowFormBase
	{
		private readonly WebBrowser webBrowser;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HelpWindowForm()
		{
			InitializeComponent();

			// Add the browser control
			webBrowser = new WebBrowser();
			webBrowser.Dock = DockStyle.Fill;
			Controls.Add(webBrowser);

			if (!DesignMode)
			{
				InitializeWindow();

				// Load window settings
				LoadWindowSettings("HelpWindow");

				// Set the title of the window
				Text = Resources.IDS_HELP_TITLE;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Navigate to a specific page
		/// </summary>
		/********************************************************************/
		public void Navigate(string page)
		{
			// Load the version specific documentation
			webBrowser.Navigate($"https://nostalgicplayer.dk/appdoc/{Env.CurrentVersion}/{page}");
		}
	}
}
