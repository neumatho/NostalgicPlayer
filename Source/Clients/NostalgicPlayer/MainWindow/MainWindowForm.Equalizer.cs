/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.EqualizerWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// This part contains all Equalizer window related code
	/// </summary>
	public partial class MainWindowForm
	{
		private EqualizerWindowForm equalizerWindow = null;



		/********************************************************************/
		/// <summary>
		/// User selected the Equalizer menu item
		/// </summary>
		/********************************************************************/
		private void Menu_Window_Equalizer_Click(object sender, EventArgs e)
		{
			if (IsEqualizerWindowOpen())
			{
				if (equalizerWindow.WindowState == FormWindowState.Minimized)
					equalizerWindow.WindowState = FormWindowState.Normal;

				equalizerWindow.Activate();
			}
			else
			{
				equalizerWindow = new EqualizerWindowForm(moduleHandler, this, optionSettings, soundSettings);
				equalizerWindow.Disposed += (o, args) => { equalizerWindow = null; };
				equalizerWindow.Show();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Event handler for the equalizer button click
		/// </summary>
		/********************************************************************/
		private void EqualizerButton_Click(object sender, EventArgs e)
		{
			// Call the same method as the menu item
			Menu_Window_Equalizer_Click(sender, e);
		}



		/********************************************************************/
		/// <summary>
		/// Open the Equalizer window if configured to
		/// </summary>
		/********************************************************************/
		private void OpenEqualizerWindow()
		{
			if (mainWindowSettings.OpenEqualizerWindow)
			{
				equalizerWindow = new EqualizerWindowForm(moduleHandler, this, optionSettings, soundSettings);
				equalizerWindow.Show();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Close the Equalizer window
		/// </summary>
		/********************************************************************/
		private void CloseEqualizerWindow()
		{
			bool openAgain = IsEqualizerWindowOpen();
			if (openAgain)
				equalizerWindow.Close();

			equalizerWindow = null;
			mainWindowSettings.OpenEqualizerWindow = openAgain;
		}



		/********************************************************************/
		/// <summary>
		/// Enumerate Equalizer window if open
		/// </summary>
		/********************************************************************/
		private IEnumerable<Form> EnumerateEqualizerWindow()
		{
			if (IsEqualizerWindowOpen())
				yield return equalizerWindow;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if Equalizer window is open
		/// </summary>
		/********************************************************************/
		private bool IsEqualizerWindowOpen()
		{
			return (equalizerWindow != null) && !equalizerWindow.IsDisposed;
		}
	}
}
