namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages
{
	partial class TrendingPageControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			timePanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
			timeAllRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			timeYearRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			timeMonthRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			timeWeekRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			typeUndergroundRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			typePlaylistsRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			typeTracksRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			typePanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
			genreComboBox = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicComboBox();
			audiusListControl = new AudiusListControl();
			timePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			typePanel.SuspendLayout();
			SuspendLayout();
			// 
			// timePanel
			// 
			timePanel.Controls.Add(timeAllRadioButton);
			timePanel.Controls.Add(timeYearRadioButton);
			timePanel.Controls.Add(timeMonthRadioButton);
			timePanel.Controls.Add(timeWeekRadioButton);
			timePanel.Location = new System.Drawing.Point(8, 8);
			timePanel.Name = "timePanel";
			timePanel.Size = new System.Drawing.Size(302, 25);
			timePanel.TabIndex = 0;
			// 
			// timeAllRadioButton
			// 
			timeAllRadioButton.Location = new System.Drawing.Point(235, 3);
			timeAllRadioButton.Name = "timeAllRadioButton";
			controlResource.SetResourceKey(timeAllRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_ALL");
			timeAllRadioButton.Size = new System.Drawing.Size(58, 16);
			timeAllRadioButton.TabIndex = 3;
			timeAllRadioButton.Text = "All time";
			timeAllRadioButton.CheckedChanged += TimeAll_CheckedChanged;
			// 
			// timeYearRadioButton
			// 
			timeYearRadioButton.Location = new System.Drawing.Point(162, 3);
			timeYearRadioButton.Name = "timeYearRadioButton";
			controlResource.SetResourceKey(timeYearRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_YEAR");
			timeYearRadioButton.Size = new System.Drawing.Size(67, 16);
			timeYearRadioButton.TabIndex = 2;
			timeYearRadioButton.Text = "This year";
			timeYearRadioButton.CheckedChanged += TimeYear_CheckedChanged;
			// 
			// timeMonthRadioButton
			// 
			timeMonthRadioButton.Location = new System.Drawing.Point(80, 3);
			timeMonthRadioButton.Name = "timeMonthRadioButton";
			controlResource.SetResourceKey(timeMonthRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_MONTH");
			timeMonthRadioButton.Size = new System.Drawing.Size(76, 16);
			timeMonthRadioButton.TabIndex = 1;
			timeMonthRadioButton.Text = "This month";
			timeMonthRadioButton.CheckedChanged += TimeMonth_CheckedChanged;
			// 
			// timeWeekRadioButton
			// 
			timeWeekRadioButton.Checked = true;
			timeWeekRadioButton.Location = new System.Drawing.Point(3, 3);
			timeWeekRadioButton.Name = "timeWeekRadioButton";
			controlResource.SetResourceKey(timeWeekRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_WEEK");
			timeWeekRadioButton.Size = new System.Drawing.Size(71, 16);
			timeWeekRadioButton.TabIndex = 0;
			timeWeekRadioButton.TabStop = true;
			timeWeekRadioButton.Text = "This week";
			timeWeekRadioButton.CheckedChanged += TimeWeek_CheckedChanged;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// typeUndergroundRadioButton
			// 
			typeUndergroundRadioButton.Location = new System.Drawing.Point(137, 3);
			typeUndergroundRadioButton.Name = "typeUndergroundRadioButton";
			controlResource.SetResourceKey(typeUndergroundRadioButton, "IDS_AUDIUS_TAB_TRENDING_TYPE_UNDERGROUND");
			typeUndergroundRadioButton.Size = new System.Drawing.Size(86, 16);
			typeUndergroundRadioButton.TabIndex = 2;
			typeUndergroundRadioButton.Text = "Underground";
			typeUndergroundRadioButton.CheckedChanged += TypeUnderground_CheckedChanged;
			// 
			// typePlaylistsRadioButton
			// 
			typePlaylistsRadioButton.Location = new System.Drawing.Point(66, 3);
			typePlaylistsRadioButton.Name = "typePlaylistsRadioButton";
			controlResource.SetResourceKey(typePlaylistsRadioButton, "IDS_AUDIUS_TAB_TRENDING_TYPE_PLAYLISTS");
			typePlaylistsRadioButton.Size = new System.Drawing.Size(63, 16);
			typePlaylistsRadioButton.TabIndex = 1;
			typePlaylistsRadioButton.Text = "Playlists";
			typePlaylistsRadioButton.CheckedChanged += TypePlaylists_CheckedChanged;
			// 
			// typeTracksRadioButton
			// 
			typeTracksRadioButton.Checked = true;
			typeTracksRadioButton.Location = new System.Drawing.Point(3, 3);
			typeTracksRadioButton.Name = "typeTracksRadioButton";
			controlResource.SetResourceKey(typeTracksRadioButton, "IDS_AUDIUS_TAB_TRENDING_TYPE_TRACKS");
			typeTracksRadioButton.Size = new System.Drawing.Size(55, 16);
			typeTracksRadioButton.TabIndex = 0;
			typeTracksRadioButton.TabStop = true;
			typeTracksRadioButton.Text = "Tracks";
			typeTracksRadioButton.CheckedChanged += TypeTracks_CheckedChanged;
			// 
			// typePanel
			// 
			typePanel.Controls.Add(typeUndergroundRadioButton);
			typePanel.Controls.Add(typePlaylistsRadioButton);
			typePanel.Controls.Add(typeTracksRadioButton);
			typePanel.Location = new System.Drawing.Point(8, 37);
			typePanel.Name = "typePanel";
			typePanel.Size = new System.Drawing.Size(302, 25);
			typePanel.TabIndex = 1;
			// 
			// genreComboBox
			// 
			genreComboBox.DropDownWidth = 189;
			genreComboBox.IntegralHeight = false;
			genreComboBox.Location = new System.Drawing.Point(316, 8);
			genreComboBox.Name = "genreComboBox";
			genreComboBox.Size = new System.Drawing.Size(189, 21);
			genreComboBox.TabIndex = 2;
			genreComboBox.SelectedIndexChanged += Genre_SelectedIndexChanged;
			// 
			// audiusListControl
			// 
			audiusListControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			audiusListControl.Location = new System.Drawing.Point(8, 68);
			audiusListControl.Name = "audiusListControl";
			audiusListControl.Size = new System.Drawing.Size(750, 292);
			audiusListControl.TabIndex = 3;
			// 
			// TrendingPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(audiusListControl);
			Controls.Add(genreComboBox);
			Controls.Add(typePanel);
			Controls.Add(timePanel);
			Name = "TrendingPageControl";
			Size = new System.Drawing.Size(766, 368);
			timePanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			typePanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel timePanel;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton timeWeekRadioButton;
		private Kit.Gui.Designer.ControlResource controlResource;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton timeMonthRadioButton;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton timeYearRadioButton;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton timeAllRadioButton;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel typePanel;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton typeTracksRadioButton;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton typePlaylistsRadioButton;
		private Polycode.NostalgicPlayer.Controls.Lists.NostalgicComboBox genreComboBox;
		private AudiusListControl audiusListControl;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton typeUndergroundRadioButton;
	}
}
