namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages
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
			typePanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
			typeProfilesRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			typePlaylistsRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			typeTracksRadioButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			searchPanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
			searchButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			searchTextBox = new Polycode.NostalgicPlayer.Controls.Inputs.NostalgicTextBox();
			audiusListControl = new AudiusListControl();
			controlPanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
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
			typePanel.Size = new System.Drawing.Size(206, 22);
			typePanel.TabIndex = 0;
			// 
			// typeProfilesRadioButton
			// 
			typeProfilesRadioButton.Location = new System.Drawing.Point(137, 4);
			typeProfilesRadioButton.Name = "typeProfilesRadioButton";
			controlResource.SetResourceKey(typeProfilesRadioButton, "IDS_AUDIUS_TAB_SEARCH_TYPE_PROFILES");
			typeProfilesRadioButton.Size = new System.Drawing.Size(59, 16);
			typeProfilesRadioButton.TabIndex = 2;
			typeProfilesRadioButton.Text = "Profiles";
			typeProfilesRadioButton.CheckedChanged += TypeProfiles_CheckedChanged;
			// 
			// typePlaylistsRadioButton
			// 
			typePlaylistsRadioButton.Location = new System.Drawing.Point(66, 4);
			typePlaylistsRadioButton.Name = "typePlaylistsRadioButton";
			controlResource.SetResourceKey(typePlaylistsRadioButton, "IDS_AUDIUS_TAB_SEARCH_TYPE_PLAYLISTS");
			typePlaylistsRadioButton.Size = new System.Drawing.Size(63, 16);
			typePlaylistsRadioButton.TabIndex = 1;
			typePlaylistsRadioButton.Text = "Playlists";
			typePlaylistsRadioButton.CheckedChanged += TypePlaylists_CheckedChanged;
			// 
			// typeTracksRadioButton
			// 
			typeTracksRadioButton.Checked = true;
			typeTracksRadioButton.Location = new System.Drawing.Point(3, 4);
			typeTracksRadioButton.Name = "typeTracksRadioButton";
			controlResource.SetResourceKey(typeTracksRadioButton, "IDS_AUDIUS_TAB_SEARCH_TYPE_TRACKS");
			typeTracksRadioButton.Size = new System.Drawing.Size(55, 16);
			typeTracksRadioButton.TabIndex = 0;
			typeTracksRadioButton.TabStop = true;
			typeTracksRadioButton.Text = "Tracks";
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
			searchPanel.Size = new System.Drawing.Size(536, 22);
			searchPanel.TabIndex = 1;
			// 
			// searchButton
			// 
			searchButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			searchButton.ImageArea = NostalgicPlayer.Controls.Types.ImageBankArea.General;
			searchButton.ImageName = "Search";
			searchButton.Location = new System.Drawing.Point(514, 0);
			searchButton.Name = "searchButton";
			searchButton.Size = new System.Drawing.Size(22, 22);
			searchButton.TabIndex = 1;
			searchButton.Click += Search_Click;
			// 
			// searchTextBox
			// 
			searchTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			searchTextBox.Location = new System.Drawing.Point(0, 1);
			searchTextBox.Name = "searchTextBox";
			searchTextBox.Size = new System.Drawing.Size(510, 21);
			searchTextBox.TabIndex = 0;
			searchTextBox.KeyDown += Search_KeyDown;
			// 
			// audiusListControl
			// 
			audiusListControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			audiusListControl.Location = new System.Drawing.Point(8, 36);
			audiusListControl.Name = "audiusListControl";
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
			controlPanel.Size = new System.Drawing.Size(766, 368);
			controlPanel.TabIndex = 0;
			// 
			// SearchPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(controlPanel);
			Name = "SearchPageControl";
			Size = new System.Drawing.Size(766, 368);
			typePanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			searchPanel.ResumeLayout(false);
			controlPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel typePanel;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton typeProfilesRadioButton;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton typePlaylistsRadioButton;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicRadioButton typeTracksRadioButton;
		private Kit.Gui.Designer.ControlResource controlResource;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel searchPanel;
		private Polycode.NostalgicPlayer.Controls.Inputs.NostalgicTextBox searchTextBox;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton searchButton;
		private AudiusListControl audiusListControl;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel controlPanel;
	}
}
