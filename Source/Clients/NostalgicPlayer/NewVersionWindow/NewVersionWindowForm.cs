/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.External.Homepage.Interfaces;
using Polycode.NostalgicPlayer.External.Homepage.Models.VersionHistory;
using Polycode.NostalgicPlayer.Kit.Gui.Components;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.NewVersionWindow
{
	/// <summary>
	/// This shows the help documentation
	/// </summary>
	public partial class NewVersionWindowForm : KryptonForm
	{
		private readonly IVersionHistoryClient versionHistoryClient;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NewVersionWindowForm(string fromVersion, string toVersion)
		{
			InitializeComponent();

			if (!DesignMode)
			{
				// Set the title of the window
				Text = Resources.IDS_NEWVERSION_TITLE;

				versionHistoryClient = DependencyInjection.Container.GetInstance<IVersionHistoryClient>();

				// Retrieve and build the history list
				BuildHistoryList(fromVersion, toVersion);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Fill the rich text box with the list of changes
		/// </summary>
		/********************************************************************/
		private void BuildHistoryList(string fromVersion, string toVersion)
		{
			HistoriesModel histories = versionHistoryClient.GetHistories(fromVersion, toVersion);

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

			historyRichTextBox.SelectionBullet = true;
			historyRichTextBox.SelectionIndent = 4;
			historyRichTextBox.BulletIndent = 12;

			foreach (string bullet in history.Bullets)
				historyRichTextBox.SelectedText = bullet + "\n";

			historyRichTextBox.SelectionBullet = false;
			historyRichTextBox.SelectionIndent = 0;
		}
	}
}
