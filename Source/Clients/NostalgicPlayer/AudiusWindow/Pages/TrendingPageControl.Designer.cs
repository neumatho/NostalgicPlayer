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
			panel1 = new System.Windows.Forms.Panel();
			timeAllRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			timeYearRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			timeMonthRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			timeWeekRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			panel2 = new System.Windows.Forms.Panel();
			typePlaylistsRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			typeTracksRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			genreComboBox = new Krypton.Toolkit.KryptonComboBox();
			audiusListControl = new AudiusListControl();
			panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)genreComboBox).BeginInit();
			SuspendLayout();
			// 
			// panel1
			// 
			panel1.Controls.Add(timeAllRadioButton);
			panel1.Controls.Add(timeYearRadioButton);
			panel1.Controls.Add(timeMonthRadioButton);
			panel1.Controls.Add(timeWeekRadioButton);
			panel1.Location = new System.Drawing.Point(8, 8);
			panel1.Name = "panel1";
			controlResource.SetResourceKey(panel1, null);
			panel1.Size = new System.Drawing.Size(302, 25);
			panel1.TabIndex = 0;
			// 
			// timeAllRadioButton
			// 
			timeAllRadioButton.Location = new System.Drawing.Point(235, 3);
			timeAllRadioButton.Name = "timeAllRadioButton";
			timeAllRadioButton.Palette = fontPalette;
			timeAllRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(timeAllRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_ALL");
			timeAllRadioButton.Size = new System.Drawing.Size(58, 16);
			timeAllRadioButton.TabIndex = 3;
			timeAllRadioButton.Values.Text = "All time";
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
			timeYearRadioButton.Palette = fontPalette;
			timeYearRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(timeYearRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_YEAR");
			timeYearRadioButton.Size = new System.Drawing.Size(67, 16);
			timeYearRadioButton.TabIndex = 2;
			timeYearRadioButton.Values.Text = "This year";
			// 
			// timeMonthRadioButton
			// 
			timeMonthRadioButton.Location = new System.Drawing.Point(80, 3);
			timeMonthRadioButton.Name = "timeMonthRadioButton";
			timeMonthRadioButton.Palette = fontPalette;
			timeMonthRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(timeMonthRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_MONTH");
			timeMonthRadioButton.Size = new System.Drawing.Size(76, 16);
			timeMonthRadioButton.TabIndex = 1;
			timeMonthRadioButton.Values.Text = "This month";
			// 
			// timeWeekRadioButton
			// 
			timeWeekRadioButton.Checked = true;
			timeWeekRadioButton.Location = new System.Drawing.Point(3, 3);
			timeWeekRadioButton.Name = "timeWeekRadioButton";
			timeWeekRadioButton.Palette = fontPalette;
			timeWeekRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(timeWeekRadioButton, "IDS_AUDIUS_TAB_TRENDING_TIME_WEEK");
			timeWeekRadioButton.Size = new System.Drawing.Size(71, 16);
			timeWeekRadioButton.TabIndex = 0;
			timeWeekRadioButton.Values.Text = "This week";
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// panel2
			// 
			panel2.Controls.Add(typePlaylistsRadioButton);
			panel2.Controls.Add(typeTracksRadioButton);
			panel2.Location = new System.Drawing.Point(8, 37);
			panel2.Name = "panel2";
			controlResource.SetResourceKey(panel2, null);
			panel2.Size = new System.Drawing.Size(302, 25);
			panel2.TabIndex = 1;
			// 
			// typePlaylistsRadioButton
			// 
			typePlaylistsRadioButton.Location = new System.Drawing.Point(66, 3);
			typePlaylistsRadioButton.Name = "typePlaylistsRadioButton";
			typePlaylistsRadioButton.Palette = fontPalette;
			typePlaylistsRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(typePlaylistsRadioButton, "IDS_AUDIUS_TAB_TRENDING_TYPE_PLAYLISTS");
			typePlaylistsRadioButton.Size = new System.Drawing.Size(63, 16);
			typePlaylistsRadioButton.TabIndex = 1;
			typePlaylistsRadioButton.Values.Text = "Playlists";
			// 
			// typeTracksRadioButton
			// 
			typeTracksRadioButton.Checked = true;
			typeTracksRadioButton.Location = new System.Drawing.Point(3, 3);
			typeTracksRadioButton.Name = "typeTracksRadioButton";
			typeTracksRadioButton.Palette = fontPalette;
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
			genreComboBox.Palette = fontPalette;
			genreComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(genreComboBox, null);
			genreComboBox.Size = new System.Drawing.Size(189, 19);
			genreComboBox.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
			genreComboBox.TabIndex = 2;
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
			Controls.Add(panel2);
			Controls.Add(panel1);
			Name = "TrendingPageControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(766, 368);
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)genreComboBox).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private Krypton.Toolkit.KryptonRadioButton timeWeekRadioButton;
		private GuiKit.Designer.ControlResource controlResource;
		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonRadioButton timeMonthRadioButton;
		private Krypton.Toolkit.KryptonRadioButton timeYearRadioButton;
		private Krypton.Toolkit.KryptonRadioButton timeAllRadioButton;
		private System.Windows.Forms.Panel panel2;
		private Krypton.Toolkit.KryptonRadioButton typeTracksRadioButton;
		private Krypton.Toolkit.KryptonRadioButton typePlaylistsRadioButton;
		private Krypton.Toolkit.KryptonComboBox genreComboBox;
		private AudiusListControl audiusListControl;
	}
}
