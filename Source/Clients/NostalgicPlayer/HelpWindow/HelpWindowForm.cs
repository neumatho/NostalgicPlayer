/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.HelpWindow
{
	/// <summary>
	/// This shows the help documentation
	/// </summary>
	public partial class HelpWindowForm : WindowFormBase
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HelpWindowForm(MainWindowForm mainWindow, OptionSettings optionSettings)
		{
			InitializeComponent();

			// Add the browser control
			WebBrowser webBrowser = new WebBrowser();
			webBrowser.Dock = DockStyle.Fill;
			Controls.Add(webBrowser);

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("HelpWindow");

				// Set the title of the window
				Text = Resources.IDS_HELP_TITLE;

				// Load the version specific documentation
				webBrowser.Navigate($"https://www.nostalgicplayer.dk/appdoc/{Env.CurrentVersion}/index.html");
			}
		}
	}
}
