/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Forms;
using Polycode.NostalgicPlayer.Controls.Images;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SplashScreen
{
	/// <summary>
	/// Show a simple splash screen with a progress bar
	/// </summary>
	public partial class SplashScreenForm : NostalgicForm, ILoadProgressCallback
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
		/// Initialize the form
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeForm(INostalgicImageBank imageBank)
		{
			logoPictureBox.Image = new Bitmap(imageBank.General.Logo);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the form is closed
		/// </summary>
		/********************************************************************/
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);

			logoPictureBox.Image?.Dispose();
			logoPictureBox.Image = null;
		}

		#region ILoadProgressCallback implementation
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
			Application.DoEvents();
		}
		#endregion
	}
}
