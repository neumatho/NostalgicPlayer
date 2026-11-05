namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	partial class ModLibraryWindowForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModLibraryWindowForm));
			searchTextBox = new Polycode.NostalgicPlayer.Controls.Inputs.NostalgicTextBox();
			searchModeComboBox = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicComboBox();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			playImmediatelyCheckBox = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicCheckBox();
			searchButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			modeTabControl = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTab();
			tabOnlinePage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			onlinePageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Pages.OnlinePageControl();
			tabOfflinePage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			offlinePageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Pages.OfflinePageControl();
			downloadControl = new ModLibraryDownloadControl();
			searchDebounceTimer = new System.Windows.Forms.Timer(components);
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)modeTabControl).BeginInit();
			modeTabControl.SuspendLayout();
			tabOnlinePage.SuspendLayout();
			tabOfflinePage.SuspendLayout();
			SuspendLayout();
			// 
			// searchTextBox
			// 
			searchTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			searchTextBox.Location = new System.Drawing.Point(8, 8);
			searchTextBox.Name = "searchTextBox";
			searchTextBox.Size = new System.Drawing.Size(604, 21);
			searchTextBox.TabIndex = 0;
			searchTextBox.TextChanged += SearchTextBox_TextChanged;
			// 
			// searchModeComboBox
			// 
			searchModeComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			searchModeComboBox.FormattingEnabled = true;
			searchModeComboBox.Location = new System.Drawing.Point(642, 8);
			searchModeComboBox.Name = "searchModeComboBox";
			searchModeComboBox.Size = new System.Drawing.Size(250, 21);
			searchModeComboBox.TabIndex = 2;
			searchModeComboBox.SelectedIndexChanged += SearchModeComboBox_SelectedIndexChanged;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// playImmediatelyCheckBox
			// 
			playImmediatelyCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			playImmediatelyCheckBox.AutoSize = true;
			playImmediatelyCheckBox.Location = new System.Drawing.Point(8, 542);
			playImmediatelyCheckBox.Name = "playImmediatelyCheckBox";
			controlResource.SetResourceKey(playImmediatelyCheckBox, "IDS_MODLIBRARY_PLAYIMMEDIATELY_CHECKBOX");
			playImmediatelyCheckBox.Size = new System.Drawing.Size(278, 19);
			playImmediatelyCheckBox.TabIndex = 4;
			playImmediatelyCheckBox.Text = "Play immediately (otherwise just add to playlist)";
			playImmediatelyCheckBox.Click += PlayImmediatelyCheckBox_CheckedChanged;
			// 
			// searchButton
			// 
			searchButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			searchButton.ImageArea = NostalgicPlayer.Controls.Types.ImageBankArea.General;
			searchButton.ImageName = "Search";
			searchButton.Location = new System.Drawing.Point(616, 8);
			searchButton.Name = "searchButton";
			searchButton.Size = new System.Drawing.Size(22, 22);
			searchButton.TabIndex = 1;
			searchButton.Click += SearchButton_Click;
			// 
			// modeTabControl
			// 
			modeTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			modeTabControl.Controls.Add(tabOnlinePage);
			modeTabControl.Controls.Add(tabOfflinePage);
			modeTabControl.Location = new System.Drawing.Point(8, 37);
			modeTabControl.Name = "modeTabControl";
			modeTabControl.SelectedIndex = 0;
			modeTabControl.Size = new System.Drawing.Size(884, 497);
			modeTabControl.TabIndex = 3;
			modeTabControl.TabPlacement = NostalgicPlayer.Controls.Types.TabPlacement.Bottom;
			modeTabControl.SelectedIndexChanged += Tab_SelectedIndexChanged;
			// 
			// tabOnlinePage
			// 
			tabOnlinePage.Controls.Add(onlinePageControl);
			tabOnlinePage.Location = new System.Drawing.Point(1, 3);
			tabOnlinePage.Name = "tabOnlinePage";
			tabOnlinePage.Size = new System.Drawing.Size(882, 470);
			tabOnlinePage.TabIndex = 0;
			// 
			// onlinePageControl
			// 
			onlinePageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			onlinePageControl.Location = new System.Drawing.Point(0, 0);
			onlinePageControl.Name = "onlinePageControl";
			onlinePageControl.Size = new System.Drawing.Size(882, 470);
			onlinePageControl.TabIndex = 0;
			onlinePageControl.ModuleDownloaded += Online_ModuleDownloaded;
			// 
			// tabOfflinePage
			// 
			tabOfflinePage.Controls.Add(offlinePageControl);
			tabOfflinePage.Location = new System.Drawing.Point(1, 3);
			tabOfflinePage.Name = "tabOfflinePage";
			tabOfflinePage.Size = new System.Drawing.Size(882, 470);
			tabOfflinePage.TabIndex = 1;
			// 
			// offlinePageControl
			// 
			offlinePageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			offlinePageControl.Location = new System.Drawing.Point(0, 0);
			offlinePageControl.Name = "offlinePageControl";
			offlinePageControl.Size = new System.Drawing.Size(882, 470);
			offlinePageControl.TabIndex = 0;
			// 
			// downloadControl
			// 
			downloadControl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			downloadControl.Location = new System.Drawing.Point(8, 569);
			downloadControl.Name = "downloadControl";
			downloadControl.Size = new System.Drawing.Size(884, 23);
			downloadControl.TabIndex = 5;
			downloadControl.Canceled += Download_Canceled;
			// 
			// searchDebounceTimer
			// 
			searchDebounceTimer.Tick += SearchDebounceTimer_Tick;
			// 
			// ModLibraryWindowForm_New
			// 
			ClientSize = new System.Drawing.Size(900, 600);
			Controls.Add(downloadControl);
			Controls.Add(searchModeComboBox);
			Controls.Add(searchTextBox);
			Controls.Add(searchButton);
			Controls.Add(modeTabControl);
			Controls.Add(playImmediatelyCheckBox);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(600, 480);
			Name = "ModLibraryWindowForm_New";
			FormClosed += ModLibraryForm_FormClosed;
			Shown += ModLibraryForm_Shown;
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)modeTabControl).EndInit();
			modeTabControl.ResumeLayout(false);
			tabOnlinePage.ResumeLayout(false);
			tabOfflinePage.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private NostalgicPlayer.Controls.Inputs.NostalgicTextBox searchTextBox;
		private NostalgicPlayer.Controls.Lists.NostalgicComboBox searchModeComboBox;
		private Kit.Gui.Designer.ControlResource controlResource;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton searchButton;
		private NostalgicPlayer.Controls.Containers.NostalgicTab modeTabControl;
		private NostalgicPlayer.Controls.Containers.NostalgicTabPage tabOnlinePage;
		private NostalgicPlayer.Controls.Containers.NostalgicTabPage tabOfflinePage;
		private NostalgicPlayer.Controls.Buttons.NostalgicCheckBox playImmediatelyCheckBox;
		private Pages.OnlinePageControl onlinePageControl;
		private Pages.OfflinePageControl offlinePageControl;
		private ModLibraryDownloadControl downloadControl;
		private System.Windows.Forms.Timer searchDebounceTimer;
	}
}