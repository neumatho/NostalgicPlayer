/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SplashScreen
{
	/// <summary>
	/// Show a simple splash screen with a progress bar
	/// </summary>
	public partial class SplashScreenForm : Form
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SplashScreenForm()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Update the progress bar
		/// </summary>
		/********************************************************************/
		public void UpdateProgress(int loaded, int total)
		{
			progressBar.Maximum = total;
			progressBar.Value = loaded;

			// Force immediate repaint
			progressBar.Refresh();
		}
	}
}
