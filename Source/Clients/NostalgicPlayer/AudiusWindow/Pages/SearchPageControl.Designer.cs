namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	partial class SearchPageControl
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
			typePanel = new System.Windows.Forms.Panel();
			typeProfilesRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			typePlaylistsRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			typeTracksRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			searchPanel = new System.Windows.Forms.Panel();
			searchButton = new Krypton.Toolkit.KryptonButton();
			searchTextBox = new Krypton.Toolkit.KryptonTextBox();
			audiusListControl = new AudiusListControl();
			controlPanel = new System.Windows.Forms.Panel();
			typePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			searchPanel.SuspendLayout();
			controlPanel.SuspendLayout();
			SuspendLayout();
			// 
			// typePanel
			// 
			typePanel.Controls.Add(typeProfilesRadioButton);
			typePanel.Controls.Add(typePlaylistsRadioButton);
			typePanel.Controls.Add(typeTracksRadioButton);
			typePanel.Location = new System.Drawing.Point(8, 8);
			typePanel.Name = "typePanel";
			controlResource.SetResourceKey(typePanel, null);
			typePanel.Size = new System.Drawing.Size(206, 22);
			typePanel.TabIndex = 0;
			// 
			// typeProfilesRadioButton
			// 
			typeProfilesRadioButton.Location = new System.Drawing.Point(137, 4);
			typeProfilesRadioButton.Name = "typeProfilesRadioButton";
			typeProfilesRadioButton.Palette = fontPalette;
			typeProfilesRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(typeProfilesRadioButton, "IDS_AUDIUS_TAB_SEARCH_TYPE_PROFILES");
			typeProfilesRadioButton.Size = new System.Drawing.Size(59, 16);
			typeProfilesRadioButton.TabIndex = 2;
			typeProfilesRadioButton.Values.Text = "Profiles";
			typeProfilesRadioButton.CheckedChanged += TypeProfiles_CheckedChanged;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// typePlaylistsRadioButton
			// 
			typePlaylistsRadioButton.Location = new System.Drawing.Point(66, 4);
			typePlaylistsRadioButton.Name = "typePlaylistsRadioButton";
			typePlaylistsRadioButton.Palette = fontPalette;
			typePlaylistsRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(typePlaylistsRadioButton, "IDS_AUDIUS_TAB_SEARCH_TYPE_PLAYLISTS");
			typePlaylistsRadioButton.Size = new System.Drawing.Size(63, 16);
			typePlaylistsRadioButton.TabIndex = 1;
			typePlaylistsRadioButton.Values.Text = "Playlists";
			typePlaylistsRadioButton.CheckedChanged += TypePlaylists_CheckedChanged;
			// 
			// typeTracksRadioButton
			// 
			typeTracksRadioButton.Checked = true;
			typeTracksRadioButton.Location = new System.Drawing.Point(3, 4);
			typeTracksRadioButton.Name = "typeTracksRadioButton";
			typeTracksRadioButton.Palette = fontPalette;
			typeTracksRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(typeTracksRadioButton, "IDS_AUDIUS_TAB_SEARCH_TYPE_TRACKS");
			typeTracksRadioButton.Size = new System.Drawing.Size(55, 16);
			typeTracksRadioButton.TabIndex = 0;
			typeTracksRadioButton.Values.Text = "Tracks";
			typeTracksRadioButton.CheckedChanged += TypeTracks_CheckedChanged;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// searchPanel
			// 
			searchPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			searchPanel.Controls.Add(searchButton);
			searchPanel.Controls.Add(searchTextBox);
			searchPanel.Location = new System.Drawing.Point(222, 8);
			searchPanel.Name = "searchPanel";
			controlResource.SetResourceKey(searchPanel, null);
			searchPanel.Size = new System.Drawing.Size(536, 22);
			searchPanel.TabIndex = 1;
			// 
			// searchButton
			// 
			searchButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			searchButton.Location = new System.Drawing.Point(514, 0);
			searchButton.Name = "searchButton";
			controlResource.SetResourceKey(searchButton, null);
			searchButton.Size = new System.Drawing.Size(22, 22);
			searchButton.TabIndex = 1;
			searchButton.Values.Image = Resources.IDB_SEARCH;
			searchButton.Values.Text = "";
			searchButton.Click += Search_Click;
			// 
			// searchTextBox
			// 
			searchTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			searchTextBox.Location = new System.Drawing.Point(0, 1);
			searchTextBox.Name = "searchTextBox";
			searchTextBox.Palette = fontPalette;
			searchTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(searchTextBox, null);
			searchTextBox.Size = new System.Drawing.Size(510, 20);
			searchTextBox.TabIndex = 0;
			searchTextBox.KeyDown += Search_KeyDown;
			// 
			// audiusListControl
			// 
			audiusListControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			audiusListControl.Location = new System.Drawing.Point(8, 36);
			audiusListControl.Name = "audiusListControl";
			controlResource.SetResourceKey(audiusListControl, null);
			audiusListControl.Size = new System.Drawing.Size(750, 324);
			audiusListControl.TabIndex = 2;
			audiusListControl.ShowProfile += AudiusList_ShowProfile;
			// 
			// controlPanel
			// 
			controlPanel.Controls.Add(audiusListControl);
			controlPanel.Controls.Add(searchPanel);
			controlPanel.Controls.Add(typePanel);
			controlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			controlPanel.Location = new System.Drawing.Point(0, 0);
			controlPanel.Name = "controlPanel";
			controlResource.SetResourceKey(controlPanel, null);
			controlPanel.Size = new System.Drawing.Size(766, 368);
			controlPanel.TabIndex = 0;
			// 
			// SearchPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(controlPanel);
			Name = "SearchPageControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(766, 368);
			typePanel.ResumeLayout(false);
			typePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			searchPanel.ResumeLayout(false);
			searchPanel.PerformLayout();
			controlPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel typePanel;
		private Krypton.Toolkit.KryptonRadioButton typeProfilesRadioButton;
		private Krypton.Toolkit.KryptonRadioButton typePlaylistsRadioButton;
		private Krypton.Toolkit.KryptonRadioButton typeTracksRadioButton;
		private Kit.Gui.Designer.ControlResource controlResource;
		private Kit.Gui.Components.FontPalette fontPalette;
		private System.Windows.Forms.Panel searchPanel;
		private Krypton.Toolkit.KryptonTextBox searchTextBox;
		private Krypton.Toolkit.KryptonButton searchButton;
		private AudiusListControl audiusListControl;
		private System.Windows.Forms.Panel controlPanel;
	}
}
