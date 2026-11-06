/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Show download progress
	/// </summary>
	public partial class ModLibraryDownloadControl : UserControl
	{
		private int realMaximum = 0;
		private bool isCanceled = false;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryDownloadControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler Canceled;

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Initialize()
		{
			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Add a new batch to the current ones
		/// </summary>
		/********************************************************************/
		public void AddBatch(int numberOfItemsInBatch)
		{
			if (realMaximum == 0)
				Reset();

			cancelButton.Visible = true;
			progressBar.Width = Width - cancelButton.Width - 4;

			realMaximum += numberOfItemsInBatch;
			progressBar.Maximum = realMaximum;

			isCanceled = false;
		}



		/********************************************************************/
		/// <summary>
		/// Start download on a new module
		/// </summary>
		/********************************************************************/
		public void StartDownload(string name)
		{
			if (isCanceled)
				return;

			progressBar.Text = name;
		}



		/********************************************************************/
		/// <summary>
		/// Download done for a single module
		/// </summary>
		/********************************************************************/
		public void DownloadDone()
		{
			if (isCanceled)
				return;

			progressBar.Text = string.Empty;
			progressBar.Value++;
		}



		/********************************************************************/
		/// <summary>
		/// Done with all the downloads
		/// </summary>
		/********************************************************************/
		public void AllDownloadsCompleted(string message)
		{
			if (isCanceled)
				return;

			Reset();

			progressBar.Value = progressBar.Maximum;
			progressBar.Text = message;
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CancelButton_Click(object sender, EventArgs e)
		{
			Reset();

			progressBar.Text = Resources.IDS_MODLIBRARY_STATUS_DOWNLOAD_CANCELLED;
			isCanceled = true;

			Canceled?.Invoke(this, EventArgs.Empty);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Reset the controls to default
		/// </summary>
		/********************************************************************/
		private void Reset()
		{
			cancelButton.Visible = false;
			progressBar.Width = Width;
			progressBar.Text = string.Empty;

			realMaximum = 0;
			progressBar.Value = 0;
			progressBar.Maximum = 1;
		}
		#endregion
	}
}
