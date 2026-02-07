/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.External.Homepage.Interfaces;
using Polycode.NostalgicPlayer.External.Homepage.Models.VersionHistory;
using Polycode.NostalgicPlayer.Kit.Gui.Components;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.NewVersionWindow
{
	/// <summary>
	/// This shows the help documentation
	/// </summary>
	public partial class NewVersionWindowForm : KryptonForm
	{
		private IVersionHistoryClient _versionHistoryClient;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NewVersionWindowForm()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the form
		/// </summary>
		/********************************************************************/
		public void InitializeForm(IVersionHistoryClient versionHistoryClient)
		{
			_versionHistoryClient = versionHistoryClient;

			// Set the title of the window
			Text = Resources.IDS_NEWVERSION_TITLE;
		}



		/********************************************************************/
		/// <summary>
		/// Fill the rich text box with the list of changes
		/// </summary>
		/********************************************************************/
		public void BuildHistoryList(string fromVersion, string toVersion)
		{
			HistoriesModel histories = _versionHistoryClient.GetHistories(fromVersion, toVersion);

			historyRichTextBox.Clear();

			if (histories.Histories != null)
			{
				AddHistory(histories.Histories[0]);

				for (int i = 1; i < histories.Histories.Length; i++)
				{
					historyRichTextBox.SelectedText = "\n";
					AddHistory(histories.Histories[i]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add all changes for the given version
		/// </summary>
		/********************************************************************/
		private void AddHistory(HistoryModel history)
		{
			historyRichTextBox.SelectionFont = FontPalette.GetRegularFont(12.0f);
			historyRichTextBox.SelectedText = history.Version + "\n";
			historyRichTextBox.SelectionFont = FontPalette.GetRegularFont(9.0f);
			historyRichTextBox.SelectedText = "\n";

			AddHistoryBullets(history.Bullets, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Add all the bullets
		/// </summary>
		/********************************************************************/
		private void AddHistoryBullets(HistoryBulletModel[] bullets, int index)
		{
			historyRichTextBox.SelectionIndent = index * 8;
			historyRichTextBox.SelectionHangingIndent = 12;

			string bulletChar = index switch
			{
				0 => "•",
				1 => "◦",
				2 => "▪",
				_ => "‣"
			};

			bool insertEmptyLine = false;

			foreach (HistoryBulletModel bullet in bullets)
			{
				if (insertEmptyLine)
				{
					historyRichTextBox.SelectedText = "\n";
					insertEmptyLine = false;
				}

				historyRichTextBox.SelectedText = $"{bulletChar} {bullet.BulletText}\n";

				if (bullet.SubBullets != null)
				{
					AddHistoryBullets(bullet.SubBullets, index + 1);
					insertEmptyLine = true;
				}
			}

			historyRichTextBox.SelectionIndent = 0;
			historyRichTextBox.SelectionHangingIndent = 0;
		}
	}
}
