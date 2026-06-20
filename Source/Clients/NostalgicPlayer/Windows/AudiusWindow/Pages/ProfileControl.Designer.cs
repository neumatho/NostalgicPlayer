namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages
{
	partial class ProfileControl
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
			infoPanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
			closeButton = new System.Windows.Forms.PictureBox();
			handleLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			bigFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			nameLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			extraBigBoldFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			profilePictureBox = new System.Windows.Forms.PictureBox();
			tabControl = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTab();
			tabTracksPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			profileTracksPageControl = new ProfileTracksPageControl();
			tabPlaylistsPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			profilePlaylistsPageControl = new ProfilePlaylistsPageControl();
			infoPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)closeButton).BeginInit();
			((System.ComponentModel.ISupportInitialize)profilePictureBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)tabControl).BeginInit();
			tabControl.SuspendLayout();
			tabTracksPage.SuspendLayout();
			tabPlaylistsPage.SuspendLayout();
			SuspendLayout();
			// 
			// infoPanel
			// 
			infoPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoPanel.Controls.Add(closeButton);
			infoPanel.Controls.Add(handleLabel);
			infoPanel.Controls.Add(nameLabel);
			infoPanel.Controls.Add(profilePictureBox);
			infoPanel.Location = new System.Drawing.Point(0, 0);
			infoPanel.Name = "infoPanel";
			infoPanel.Size = new System.Drawing.Size(766, 224);
			infoPanel.TabIndex = 0;
			// 
			// closeButton
			// 
			closeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			closeButton.Location = new System.Drawing.Point(734, 8);
			closeButton.Name = "closeButton";
			closeButton.Size = new System.Drawing.Size(24, 24);
			closeButton.TabIndex = 3;
			closeButton.TabStop = false;
			closeButton.Click += Close_Click;
			closeButton.MouseEnter += Close_MouseEnter;
			closeButton.MouseLeave += Close_MouseLeave;
			// 
			// handleLabel
			// 
			handleLabel.Location = new System.Drawing.Point(144, 155);
			handleLabel.Name = "handleLabel";
			handleLabel.Size = new System.Drawing.Size(0, 17);
			handleLabel.TabIndex = 2;
			handleLabel.UseFont = bigFontConfiguration;
			// 
			// bigFontConfiguration
			// 
			bigFontConfiguration.FontSize = 2;
			// 
			// nameLabel
			// 
			nameLabel.Location = new System.Drawing.Point(144, 126);
			nameLabel.Name = "nameLabel";
			nameLabel.Size = new System.Drawing.Size(0, 26);
			nameLabel.TabIndex = 1;
			nameLabel.UseFont = extraBigBoldFontConfiguration;
			// 
			// extraBigBoldFontConfiguration
			// 
			extraBigBoldFontConfiguration.FontSize = 8;
			extraBigBoldFontConfiguration.FontStyle = System.Drawing.FontStyle.Bold;
			// 
			// profilePictureBox
			// 
			profilePictureBox.Location = new System.Drawing.Point(8, 88);
			profilePictureBox.Name = "profilePictureBox";
			profilePictureBox.Size = new System.Drawing.Size(128, 128);
			profilePictureBox.TabIndex = 0;
			profilePictureBox.TabStop = false;
			// 
			// tabControl
			// 
			tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tabControl.Controls.Add(tabTracksPage);
			tabControl.Controls.Add(tabPlaylistsPage);
			tabControl.Location = new System.Drawing.Point(8, 232);
			tabControl.Name = "tabControl";
			tabControl.SelectedIndex = 0;
			tabControl.Size = new System.Drawing.Size(750, 128);
			tabControl.TabIndex = 1;
			tabControl.SelectedIndexChanged += Tab_SelectedIndexChanged;
			// 
			// tabTracksPage
			// 
			tabTracksPage.Controls.Add(profileTracksPageControl);
			tabTracksPage.Location = new System.Drawing.Point(1, 24);
			tabTracksPage.MinimumSize = new System.Drawing.Size(150, 50);
			tabTracksPage.Name = "tabTracksPage";
			tabTracksPage.Size = new System.Drawing.Size(748, 101);
			tabTracksPage.TabIndex = 0;
			// 
			// profileTracksPageControl
			// 
			profileTracksPageControl.BackColor = System.Drawing.Color.Transparent;
			profileTracksPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			profileTracksPageControl.Location = new System.Drawing.Point(0, 0);
			profileTracksPageControl.Name = "profileTracksPageControl";
			profileTracksPageControl.Size = new System.Drawing.Size(748, 101);
			profileTracksPageControl.TabIndex = 2;
			// 
			// tabPlaylistsPage
			// 
			tabPlaylistsPage.Controls.Add(profilePlaylistsPageControl);
			tabPlaylistsPage.Location = new System.Drawing.Point(1, 24);
			tabPlaylistsPage.MinimumSize = new System.Drawing.Size(150, 50);
			tabPlaylistsPage.Name = "tabPlaylistsPage";
			tabPlaylistsPage.Size = new System.Drawing.Size(748, 101);
			tabPlaylistsPage.TabIndex = 1;
			// 
			// profilePlaylistsPageControl
			// 
			profilePlaylistsPageControl.BackColor = System.Drawing.Color.Transparent;
			profilePlaylistsPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			profilePlaylistsPageControl.Location = new System.Drawing.Point(0, 0);
			profilePlaylistsPageControl.Name = "profilePlaylistsPageControl";
			profilePlaylistsPageControl.Size = new System.Drawing.Size(748, 101);
			profilePlaylistsPageControl.TabIndex = 2;
			// 
			// ProfileControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(tabControl);
			Controls.Add(infoPanel);
			Name = "ProfileControl";
			Size = new System.Drawing.Size(766, 368);
			infoPanel.ResumeLayout(false);
			infoPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)closeButton).EndInit();
			((System.ComponentModel.ISupportInitialize)profilePictureBox).EndInit();
			((System.ComponentModel.ISupportInitialize)tabControl).EndInit();
			tabControl.ResumeLayout(false);
			tabTracksPage.ResumeLayout(false);
			tabPlaylistsPage.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel infoPanel;
		private System.Windows.Forms.PictureBox profilePictureBox;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel handleLabel;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel nameLabel;
		private System.Windows.Forms.PictureBox closeButton;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicTab tabControl;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage tabTracksPage;
		private ProfileTracksPageControl profileTracksPageControl;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage tabPlaylistsPage;
		private ProfilePlaylistsPageControl profilePlaylistsPageControl;
		private NostalgicPlayer.Controls.Components.FontConfiguration bigFontConfiguration;
		private NostalgicPlayer.Controls.Components.FontConfiguration extraBigBoldFontConfiguration;
	}
}
