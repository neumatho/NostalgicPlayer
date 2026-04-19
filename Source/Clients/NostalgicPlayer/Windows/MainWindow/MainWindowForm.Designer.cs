using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow
{
	partial class MainWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindowForm));
			menuStrip = new System.Windows.Forms.MenuStrip();
			infoLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			bigFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			infoBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			equalizerButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			moduleInfoButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			masterVolumeTrackBar = new KryptonTrackBar();
			listButtonsBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			diskButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			listButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			moveModulesDownButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			moveModulesUpButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			sortModulesButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			swapModulesButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			removeModuleButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			addModuleButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			listInfoBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			totalLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			timeLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			positionTrackBar = new KryptonTrackBar();
			tapedeckBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			pauseCheckButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicToggleImageButton();
			ejectButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			nextModuleButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			nextSongButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			fastForwardButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			playButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			fastRewindButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			previousSongButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			previousModuleButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			functionsBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			loopCheckButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicToggleImageButton();
			favoritesButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			sampleInfoButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			muteCheckButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicToggleImageButton();
			toolTip = new System.Windows.Forms.ToolTip(components);
			sortContextMenu = new KryptonContextMenu();
			listContextMenu = new KryptonContextMenu();
			diskContextMenu = new KryptonContextMenu();
			neverEndingTimer = new System.Windows.Forms.Timer(components);
			addContextMenu = new KryptonContextMenu();
			moduleList = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicModuleList();
			searchPopupControl = new SearchPopupControl();
			infoBox.SuspendLayout();
			listButtonsBox.SuspendLayout();
			listInfoBox.SuspendLayout();
			tapedeckBox.SuspendLayout();
			functionsBox.SuspendLayout();
			SuspendLayout();
			// 
			// menuStrip
			// 
			menuStrip.Font = new System.Drawing.Font("Segoe UI", 9F);
			menuStrip.Location = new System.Drawing.Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.Size = new System.Drawing.Size(416, 24);
			menuStrip.TabIndex = 0;
			menuStrip.Text = "menuStrip";
			// 
			// infoLabel
			// 
			infoLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoLabel.Location = new System.Drawing.Point(3, 3);
			infoLabel.Name = "infoLabel";
			infoLabel.Size = new System.Drawing.Size(374, 16);
			infoLabel.TabIndex = 0;
			infoLabel.UseFont = bigFontConfiguration;
			// 
			// bigFontConfiguration
			// 
			bigFontConfiguration.FontSize = 1;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// infoBox
			// 
			infoBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoBox.Controls.Add(infoLabel);
			infoBox.Location = new System.Drawing.Point(4, 28);
			infoBox.Name = "infoBox";
			infoBox.Size = new System.Drawing.Size(380, 24);
			infoBox.TabIndex = 1;
			// 
			// equalizerButton
			// 
			equalizerButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			equalizerButton.ImageName = "Equalizer";
			equalizerButton.Location = new System.Drawing.Point(60, 4);
			equalizerButton.Name = "equalizerButton";
			equalizerButton.Size = new System.Drawing.Size(24, 24);
			equalizerButton.TabIndex = 2;
			// 
			// moduleInfoButton
			// 
			moduleInfoButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			moduleInfoButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			moduleInfoButton.ImageName = "Information";
			moduleInfoButton.Location = new System.Drawing.Point(388, 28);
			moduleInfoButton.Name = "moduleInfoButton";
			moduleInfoButton.Size = new System.Drawing.Size(24, 24);
			moduleInfoButton.TabIndex = 2;
			// 
			// masterVolumeTrackBar
			// 
			masterVolumeTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			masterVolumeTrackBar.Location = new System.Drawing.Point(4, 84);
			masterVolumeTrackBar.Maximum = 256;
			masterVolumeTrackBar.Name = "masterVolumeTrackBar";
			masterVolumeTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			masterVolumeTrackBar.Size = new System.Drawing.Size(27, 88);
			masterVolumeTrackBar.TabIndex = 4;
			masterVolumeTrackBar.TickFrequency = 8;
			masterVolumeTrackBar.VolumeControl = true;
			// 
			// listButtonsBox
			// 
			listButtonsBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			listButtonsBox.Controls.Add(diskButton);
			listButtonsBox.Controls.Add(listButton);
			listButtonsBox.Controls.Add(moveModulesDownButton);
			listButtonsBox.Controls.Add(moveModulesUpButton);
			listButtonsBox.Controls.Add(sortModulesButton);
			listButtonsBox.Controls.Add(swapModulesButton);
			listButtonsBox.Controls.Add(removeModuleButton);
			listButtonsBox.Controls.Add(addModuleButton);
			listButtonsBox.Location = new System.Drawing.Point(4, 176);
			listButtonsBox.Name = "listButtonsBox";
			listButtonsBox.Size = new System.Drawing.Size(230, 34);
			listButtonsBox.TabIndex = 6;
			// 
			// diskButton
			// 
			diskButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			diskButton.ImageName = "Disk";
			diskButton.Location = new System.Drawing.Point(200, 4);
			diskButton.Name = "diskButton";
			diskButton.Size = new System.Drawing.Size(24, 24);
			diskButton.TabIndex = 7;
			// 
			// listButton
			// 
			listButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			listButton.ImageName = "List";
			listButton.Location = new System.Drawing.Point(172, 4);
			listButton.Name = "listButton";
			listButton.Size = new System.Drawing.Size(24, 24);
			listButton.TabIndex = 6;
			// 
			// moveModulesDownButton
			// 
			moveModulesDownButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			moveModulesDownButton.ImageName = "MoveDown";
			moveModulesDownButton.Location = new System.Drawing.Point(144, 4);
			moveModulesDownButton.Name = "moveModulesDownButton";
			moveModulesDownButton.Size = new System.Drawing.Size(24, 24);
			moveModulesDownButton.TabIndex = 5;
			// 
			// moveModulesUpButton
			// 
			moveModulesUpButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			moveModulesUpButton.ImageName = "MoveUp";
			moveModulesUpButton.Location = new System.Drawing.Point(116, 4);
			moveModulesUpButton.Name = "moveModulesUpButton";
			moveModulesUpButton.Size = new System.Drawing.Size(24, 24);
			moveModulesUpButton.TabIndex = 4;
			// 
			// sortModulesButton
			// 
			sortModulesButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			sortModulesButton.ImageName = "Sort";
			sortModulesButton.Location = new System.Drawing.Point(88, 4);
			sortModulesButton.Name = "sortModulesButton";
			sortModulesButton.Size = new System.Drawing.Size(24, 24);
			sortModulesButton.TabIndex = 3;
			// 
			// swapModulesButton
			// 
			swapModulesButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			swapModulesButton.ImageName = "Swap";
			swapModulesButton.Location = new System.Drawing.Point(60, 4);
			swapModulesButton.Name = "swapModulesButton";
			swapModulesButton.Size = new System.Drawing.Size(24, 24);
			swapModulesButton.TabIndex = 2;
			// 
			// removeModuleButton
			// 
			removeModuleButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			removeModuleButton.ImageName = "Remove";
			removeModuleButton.Location = new System.Drawing.Point(32, 4);
			removeModuleButton.Name = "removeModuleButton";
			removeModuleButton.Size = new System.Drawing.Size(24, 24);
			removeModuleButton.TabIndex = 1;
			// 
			// addModuleButton
			// 
			addModuleButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			addModuleButton.ImageName = "Add";
			addModuleButton.Location = new System.Drawing.Point(4, 4);
			addModuleButton.Name = "addModuleButton";
			addModuleButton.Size = new System.Drawing.Size(24, 24);
			addModuleButton.TabIndex = 0;
			// 
			// listInfoBox
			// 
			listInfoBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			listInfoBox.Controls.Add(totalLabel);
			listInfoBox.Controls.Add(timeLabel);
			listInfoBox.Location = new System.Drawing.Point(238, 176);
			listInfoBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			listInfoBox.Name = "listInfoBox";
			listInfoBox.Size = new System.Drawing.Size(174, 34);
			listInfoBox.TabIndex = 7;
			// 
			// totalLabel
			// 
			totalLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			totalLabel.Location = new System.Drawing.Point(140, 8);
			totalLabel.Name = "totalLabel";
			totalLabel.Size = new System.Drawing.Size(28, 18);
			totalLabel.TabIndex = 1;
			totalLabel.Text = "0/0";
			totalLabel.UseFont = bigFontConfiguration;
			// 
			// timeLabel
			// 
			timeLabel.Location = new System.Drawing.Point(4, 8);
			timeLabel.Name = "timeLabel";
			timeLabel.Size = new System.Drawing.Size(62, 18);
			timeLabel.TabIndex = 0;
			timeLabel.Text = "0:00/0:00";
			timeLabel.UseFont = bigFontConfiguration;
			// 
			// positionTrackBar
			// 
			positionTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			positionTrackBar.BackStyle = PaletteBackStyle.SeparatorLowProfile;
			positionTrackBar.Location = new System.Drawing.Point(4, 216);
			positionTrackBar.Maximum = 100;
			positionTrackBar.Name = "positionTrackBar";
			positionTrackBar.Size = new System.Drawing.Size(408, 21);
			positionTrackBar.TabIndex = 8;
			positionTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
			// 
			// tapedeckBox
			// 
			tapedeckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			tapedeckBox.Controls.Add(pauseCheckButton);
			tapedeckBox.Controls.Add(ejectButton);
			tapedeckBox.Controls.Add(nextModuleButton);
			tapedeckBox.Controls.Add(nextSongButton);
			tapedeckBox.Controls.Add(fastForwardButton);
			tapedeckBox.Controls.Add(playButton);
			tapedeckBox.Controls.Add(fastRewindButton);
			tapedeckBox.Controls.Add(previousSongButton);
			tapedeckBox.Controls.Add(previousModuleButton);
			tapedeckBox.Location = new System.Drawing.Point(4, 243);
			tapedeckBox.Name = "tapedeckBox";
			tapedeckBox.Size = new System.Drawing.Size(258, 34);
			tapedeckBox.TabIndex = 9;
			// 
			// pauseCheckButton
			// 
			pauseCheckButton.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
			pauseCheckButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			pauseCheckButton.ImageName = "Pause";
			pauseCheckButton.Location = new System.Drawing.Point(228, 4);
			pauseCheckButton.Name = "pauseCheckButton";
			pauseCheckButton.Size = new System.Drawing.Size(24, 24);
			pauseCheckButton.TabIndex = 8;
			// 
			// ejectButton
			// 
			ejectButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			ejectButton.ImageName = "Eject";
			ejectButton.Location = new System.Drawing.Point(200, 4);
			ejectButton.Name = "ejectButton";
			ejectButton.Size = new System.Drawing.Size(24, 24);
			ejectButton.TabIndex = 7;
			// 
			// nextModuleButton
			// 
			nextModuleButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			nextModuleButton.ImageName = "NextModule";
			nextModuleButton.Location = new System.Drawing.Point(172, 4);
			nextModuleButton.Name = "nextModuleButton";
			nextModuleButton.Size = new System.Drawing.Size(24, 24);
			nextModuleButton.TabIndex = 6;
			// 
			// nextSongButton
			// 
			nextSongButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			nextSongButton.ImageName = "NextSong";
			nextSongButton.Location = new System.Drawing.Point(144, 4);
			nextSongButton.Name = "nextSongButton";
			nextSongButton.Size = new System.Drawing.Size(24, 24);
			nextSongButton.TabIndex = 5;
			// 
			// fastForwardButton
			// 
			fastForwardButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			fastForwardButton.ImageName = "FastForward";
			fastForwardButton.Location = new System.Drawing.Point(116, 4);
			fastForwardButton.Name = "fastForwardButton";
			fastForwardButton.Size = new System.Drawing.Size(24, 24);
			fastForwardButton.TabIndex = 4;
			// 
			// playButton
			// 
			playButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			playButton.ImageName = "Play";
			playButton.Location = new System.Drawing.Point(88, 4);
			playButton.Name = "playButton";
			playButton.Size = new System.Drawing.Size(24, 24);
			playButton.TabIndex = 3;
			// 
			// fastRewindButton
			// 
			fastRewindButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			fastRewindButton.ImageName = "FastRewind";
			fastRewindButton.Location = new System.Drawing.Point(60, 4);
			fastRewindButton.Name = "fastRewindButton";
			fastRewindButton.Size = new System.Drawing.Size(24, 24);
			fastRewindButton.TabIndex = 2;
			// 
			// previousSongButton
			// 
			previousSongButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			previousSongButton.ImageName = "PreviousSong";
			previousSongButton.Location = new System.Drawing.Point(32, 4);
			previousSongButton.Name = "previousSongButton";
			previousSongButton.Size = new System.Drawing.Size(24, 24);
			previousSongButton.TabIndex = 1;
			// 
			// previousModuleButton
			// 
			previousModuleButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			previousModuleButton.ImageName = "PreviousModule";
			previousModuleButton.Location = new System.Drawing.Point(4, 4);
			previousModuleButton.Name = "previousModuleButton";
			previousModuleButton.Size = new System.Drawing.Size(24, 24);
			previousModuleButton.TabIndex = 0;
			// 
			// functionsBox
			// 
			functionsBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			functionsBox.Controls.Add(loopCheckButton);
			functionsBox.Controls.Add(favoritesButton);
			functionsBox.Controls.Add(equalizerButton);
			functionsBox.Controls.Add(sampleInfoButton);
			functionsBox.Location = new System.Drawing.Point(294, 243);
			functionsBox.Name = "functionsBox";
			functionsBox.Size = new System.Drawing.Size(118, 34);
			functionsBox.TabIndex = 10;
			// 
			// loopCheckButton
			// 
			loopCheckButton.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
			loopCheckButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			loopCheckButton.ImageName = "Loop";
			loopCheckButton.Location = new System.Drawing.Point(4, 4);
			loopCheckButton.Name = "loopCheckButton";
			loopCheckButton.Size = new System.Drawing.Size(24, 24);
			loopCheckButton.TabIndex = 0;
			// 
			// favoritesButton
			// 
			favoritesButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			favoritesButton.ImageName = "Favorites";
			favoritesButton.Location = new System.Drawing.Point(32, 4);
			favoritesButton.Name = "favoritesButton";
			favoritesButton.Size = new System.Drawing.Size(24, 24);
			favoritesButton.TabIndex = 1;
			// 
			// sampleInfoButton
			// 
			sampleInfoButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			sampleInfoButton.ImageName = "Samples";
			sampleInfoButton.Location = new System.Drawing.Point(88, 4);
			sampleInfoButton.Name = "sampleInfoButton";
			sampleInfoButton.Size = new System.Drawing.Size(24, 24);
			sampleInfoButton.TabIndex = 3;
			// 
			// muteCheckButton
			// 
			muteCheckButton.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton;
			muteCheckButton.ImageArea = NostalgicPlayer.Controls.ImageBankArea.Main;
			muteCheckButton.ImageName = "Mute";
			muteCheckButton.Location = new System.Drawing.Point(4, 56);
			muteCheckButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			muteCheckButton.Name = "muteCheckButton";
			muteCheckButton.Size = new System.Drawing.Size(27, 24);
			muteCheckButton.TabIndex = 3;
			// 
			// sortContextMenu
			// 
			sortContextMenu.Palette = fontPalette;
			// 
			// listContextMenu
			// 
			listContextMenu.Palette = fontPalette;
			listContextMenu.Opening += ListContextMenu_Opening;
			// 
			// diskContextMenu
			// 
			diskContextMenu.Palette = fontPalette;
			// 
			// addContextMenu
			// 
			addContextMenu.Palette = fontPalette;
			// 
			// moduleList
			// 
			moduleList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			moduleList.Location = new System.Drawing.Point(35, 56);
			moduleList.Name = "moduleList";
			moduleList.Size = new System.Drawing.Size(377, 116);
			moduleList.TabIndex = 5;
			// 
			// searchPopupControl
			// 
			searchPopupControl.Location = new System.Drawing.Point(35, 56);
			searchPopupControl.Name = "searchPopupControl";
			searchPopupControl.Size = new System.Drawing.Size(377, 116);
			searchPopupControl.TabIndex = 100;
			searchPopupControl.Visible = false;
			// 
			// MainWindowForm
			// 
			ClientSize = new System.Drawing.Size(416, 281);
			Controls.Add(searchPopupControl);
			Controls.Add(moduleList);
			Controls.Add(muteCheckButton);
			Controls.Add(functionsBox);
			Controls.Add(tapedeckBox);
			Controls.Add(positionTrackBar);
			Controls.Add(listInfoBox);
			Controls.Add(listButtonsBox);
			Controls.Add(masterVolumeTrackBar);
			Controls.Add(moduleInfoButton);
			Controls.Add(infoBox);
			Controls.Add(menuStrip);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			MainMenuStrip = menuStrip;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MinimumSize = new System.Drawing.Size(432, 320);
			Name = "MainWindowForm";
			infoBox.ResumeLayout(false);
			listButtonsBox.ResumeLayout(false);
			listInfoBox.ResumeLayout(false);
			tapedeckBox.ResumeLayout(false);
			functionsBox.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel infoLabel;
		private NostalgicPlayer.Controls.Containers.NostalgicBox infoBox;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton equalizerButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton moduleInfoButton;
		private KryptonTrackBar masterVolumeTrackBar;
		private NostalgicPlayer.Controls.Containers.NostalgicBox listButtonsBox;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton addModuleButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton removeModuleButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton swapModulesButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton sortModulesButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton moveModulesDownButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton moveModulesUpButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton listButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton diskButton;
		private NostalgicPlayer.Controls.Containers.NostalgicBox listInfoBox;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel totalLabel;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel timeLabel;
		private KryptonTrackBar positionTrackBar;
		private NostalgicPlayer.Controls.Containers.NostalgicBox tapedeckBox;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton ejectButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton nextModuleButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton nextSongButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton fastForwardButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton playButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton fastRewindButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton previousSongButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton previousModuleButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicToggleImageButton pauseCheckButton;
		private NostalgicPlayer.Controls.Containers.NostalgicBox functionsBox;
		private NostalgicPlayer.Controls.Buttons.NostalgicToggleImageButton loopCheckButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton sampleInfoButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicToggleImageButton muteCheckButton;
		private System.Windows.Forms.ToolTip toolTip;
		private KryptonContextMenu sortContextMenu;
		private KryptonContextMenu listContextMenu;
		private KryptonContextMenu diskContextMenu;
		private System.Windows.Forms.Timer neverEndingTimer;
		private KryptonContextMenu addContextMenu;
		private NostalgicPlayer.Controls.Buttons.NostalgicImageButton favoritesButton;
		private Kit.Gui.Components.FontPalette fontPalette;
		private NostalgicPlayer.Controls.Lists.NostalgicModuleList moduleList;
		private SearchPopupControl searchPopupControl;
		private NostalgicPlayer.Controls.Components.FontConfiguration bigFontConfiguration;
	}
}

