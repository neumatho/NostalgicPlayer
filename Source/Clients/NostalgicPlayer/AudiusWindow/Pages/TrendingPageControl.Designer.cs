namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
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
			components = new System.ComponentModel.Container();
			timePanel = new System.Windows.Forms.Panel();
			timeAllRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			timeYearRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			timeMonthRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			timeWeekRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			typePanel = new System.Windows.Forms.Panel();
			typeUndergroundRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			typePlaylistsRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			typeTracksRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			genreComboBox = new Krypton.Toolkit.KryptonComboBox();
			audiusListControl = new AudiusListControl();
			timePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			typePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)genreComboBox).BeginInit();
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
			controlResource.SetResourceKey(timePanel, null);
			timePanel.Size = new System.Drawing.Size(302, 25);
			timePanel.TabIndex = 0;
			// 
			// timeAllRadioButton
			// 
			timeAllRadioButton.Location = new System.Drawing.Point(235, 3);
			timeAllRadioButton.Name = "timeAllRadioButton";
			timeAllRadioButton.LocalCustomPalette = fontPalette;
			timeAllRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(timeAllRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_ALL");
			timeAllRadioButton.Size = new System.Drawing.Size(58, 16);
			timeAllRadioButton.TabIndex = 3;
			timeAllRadioButton.Values.Text = "All time";
			timeAllRadioButton.CheckedChanged += TimeAll_CheckedChanged;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			// 
			// timeYearRadioButton
			// 
			timeYearRadioButton.Location = new System.Drawing.Point(162, 3);
			timeYearRadioButton.Name = "timeYearRadioButton";
			timeYearRadioButton.LocalCustomPalette = fontPalette;
			timeYearRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(timeYearRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_YEAR");
			timeYearRadioButton.Size = new System.Drawing.Size(67, 16);
			timeYearRadioButton.TabIndex = 2;
			timeYearRadioButton.Values.Text = "This year";
			timeYearRadioButton.CheckedChanged += TimeYear_CheckedChanged;
			// 
			// timeMonthRadioButton
			// 
			timeMonthRadioButton.Location = new System.Drawing.Point(80, 3);
			timeMonthRadioButton.Name = "timeMonthRadioButton";
			timeMonthRadioButton.LocalCustomPalette = fontPalette;
			timeMonthRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(timeMonthRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_MONTH");
			timeMonthRadioButton.Size = new System.Drawing.Size(76, 16);
			timeMonthRadioButton.TabIndex = 1;
			timeMonthRadioButton.Values.Text = "This month";
			timeMonthRadioButton.CheckedChanged += TimeMonth_CheckedChanged;
			// 
			// timeWeekRadioButton
			// 
			timeWeekRadioButton.Checked = true;
			timeWeekRadioButton.Location = new System.Drawing.Point(3, 3);
			timeWeekRadioButton.Name = "timeWeekRadioButton";
			timeWeekRadioButton.LocalCustomPalette = fontPalette;
			timeWeekRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(timeWeekRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_WEEK");
			timeWeekRadioButton.Size = new System.Drawing.Size(71, 16);
			timeWeekRadioButton.TabIndex = 0;
			timeWeekRadioButton.Values.Text = "This week";
			timeWeekRadioButton.CheckedChanged += TimeWeek_CheckedChanged;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// typePanel
			// 
			typePanel.Controls.Add(typeUndergroundRadioButton);
			typePanel.Controls.Add(typePlaylistsRadioButton);
			typePanel.Controls.Add(typeTracksRadioButton);
			typePanel.Location = new System.Drawing.Point(8, 37);
			typePanel.Name = "typePanel";
			controlResource.SetResourceKey(typePanel, null);
			typePanel.Size = new System.Drawing.Size(302, 25);
			typePanel.TabIndex = 1;
			// 
			// typeUndergroundRadioButton
			// 
			typeUndergroundRadioButton.Location = new System.Drawing.Point(137, 3);
			typeUndergroundRadioButton.Name = "typeUndergroundRadioButton";
			typeUndergroundRadioButton.LocalCustomPalette = fontPalette;
			typeUndergroundRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(typeUndergroundRadioButton, "IDS_AUDIUS_TAB_TRENDING_TYPE_UNDERGROUND");
			typeUndergroundRadioButton.Size = new System.Drawing.Size(86, 16);
			typeUndergroundRadioButton.TabIndex = 2;
			typeUndergroundRadioButton.Values.Text = "Underground";
			typeUndergroundRadioButton.CheckedChanged += TypeUnderground_CheckedChanged;
			// 
			// typePlaylistsRadioButton
			// 
			typePlaylistsRadioButton.Location = new System.Drawing.Point(66, 3);
			typePlaylistsRadioButton.Name = "typePlaylistsRadioButton";
			typePlaylistsRadioButton.LocalCustomPalette = fontPalette;
			typePlaylistsRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(typePlaylistsRadioButton, "IDS_AUDIUS_TAB_TRENDING_TYPE_PLAYLISTS");
			typePlaylistsRadioButton.Size = new System.Drawing.Size(63, 16);
			typePlaylistsRadioButton.TabIndex = 1;
			typePlaylistsRadioButton.Values.Text = "Playlists";
			typePlaylistsRadioButton.CheckedChanged += TypePlaylists_CheckedChanged;
			// 
			// typeTracksRadioButton
			// 
			typeTracksRadioButton.Checked = true;
			typeTracksRadioButton.Location = new System.Drawing.Point(3, 3);
			typeTracksRadioButton.Name = "typeTracksRadioButton";
			typeTracksRadioButton.LocalCustomPalette = fontPalette;
			typeTracksRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(typeTracksRadioButton, "IDS_AUDIUS_TAB_TRENDING_TYPE_TRACKS");
			typeTracksRadioButton.Size = new System.Drawing.Size(55, 16);
			typeTracksRadioButton.TabIndex = 0;
			typeTracksRadioButton.Values.Text = "Tracks";
			typeTracksRadioButton.CheckedChanged += TypeTracks_CheckedChanged;
			// 
			// genreComboBox
			// 
			genreComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			genreComboBox.DropDownWidth = 189;
			genreComboBox.IntegralHeight = false;
			genreComboBox.Location = new System.Drawing.Point(316, 8);
			genreComboBox.Name = "genreComboBox";
			genreComboBox.LocalCustomPalette = fontPalette;
			genreComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(genreComboBox, null);
			genreComboBox.Size = new System.Drawing.Size(189, 19);
			genreComboBox.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
			genreComboBox.TabIndex = 2;
			genreComboBox.SelectedIndexChanged += Genre_SelectedIndexChanged;
			// 
			// audiusListControl
			// 
			audiusListControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			audiusListControl.Location = new System.Drawing.Point(8, 68);
			audiusListControl.Name = "audiusListControl";
			controlResource.SetResourceKey(audiusListControl, null);
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
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(766, 368);
			timePanel.ResumeLayout(false);
			timePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			typePanel.ResumeLayout(false);
			typePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)genreComboBox).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel timePanel;
		private Krypton.Toolkit.KryptonRadioButton timeWeekRadioButton;
		private GuiKit.Designer.ControlResource controlResource;
		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonRadioButton timeMonthRadioButton;
		private Krypton.Toolkit.KryptonRadioButton timeYearRadioButton;
		private Krypton.Toolkit.KryptonRadioButton timeAllRadioButton;
		private System.Windows.Forms.Panel typePanel;
		private Krypton.Toolkit.KryptonRadioButton typeTracksRadioButton;
		private Krypton.Toolkit.KryptonRadioButton typePlaylistsRadioButton;
		private Krypton.Toolkit.KryptonComboBox genreComboBox;
		private AudiusListControl audiusListControl;
		private Krypton.Toolkit.KryptonRadioButton typeUndergroundRadioButton;
	}
}
