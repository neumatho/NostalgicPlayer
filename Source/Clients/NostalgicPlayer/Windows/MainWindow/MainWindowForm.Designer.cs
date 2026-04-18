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
			equalizerButton = new KryptonButton();
			moduleInfoButton = new KryptonButton();
			masterVolumeTrackBar = new KryptonTrackBar();
			listButtonsBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			diskButton = new KryptonButton();
			listButton = new KryptonButton();
			moveModulesDownButton = new KryptonButton();
			moveModulesUpButton = new KryptonButton();
			sortModulesButton = new KryptonButton();
			swapModulesButton = new KryptonButton();
			removeModuleButton = new KryptonButton();
			addModuleButton = new KryptonButton();
			listInfoBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			totalLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			timeLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			positionTrackBar = new KryptonTrackBar();
			tapedeckBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			pauseCheckButton = new KryptonCheckButton();
			ejectButton = new KryptonButton();
			nextModuleButton = new KryptonButton();
			nextSongButton = new KryptonButton();
			fastForwardButton = new KryptonButton();
			playButton = new KryptonButton();
			rewindButton = new KryptonButton();
			previousSongButton = new KryptonButton();
			previousModuleButton = new KryptonButton();
			functionsBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			loopCheckButton = new KryptonCheckButton();
			favoritesButton = new KryptonButton();
			sampleInfoButton = new KryptonButton();
			muteCheckButton = new KryptonCheckButton();
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
			equalizerButton.Location = new System.Drawing.Point(60, 4);
			equalizerButton.Name = "equalizerButton";
			equalizerButton.Size = new System.Drawing.Size(24, 24);
			equalizerButton.TabIndex = 2;
			equalizerButton.Values.Image = Resources.IDB_EQUALIZER;
			equalizerButton.Values.Text = "";
			// 
			// moduleInfoButton
			// 
			moduleInfoButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			moduleInfoButton.Location = new System.Drawing.Point(388, 28);
			moduleInfoButton.Name = "moduleInfoButton";
			moduleInfoButton.Size = new System.Drawing.Size(24, 24);
			moduleInfoButton.TabIndex = 2;
			moduleInfoButton.Values.Image = (System.Drawing.Image)resources.GetObject("moduleInfoButton.Values.Image");
			moduleInfoButton.Values.Text = "";
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
			diskButton.Location = new System.Drawing.Point(200, 4);
			diskButton.Name = "diskButton";
			diskButton.Size = new System.Drawing.Size(24, 24);
			diskButton.TabIndex = 7;
			diskButton.Values.Image = (System.Drawing.Image)resources.GetObject("diskButton.Values.Image");
			diskButton.Values.Text = "";
			// 
			// listButton
			// 
			listButton.Location = new System.Drawing.Point(172, 4);
			listButton.Name = "listButton";
			listButton.Size = new System.Drawing.Size(24, 24);
			listButton.TabIndex = 6;
			listButton.Values.Image = (System.Drawing.Image)resources.GetObject("listButton.Values.Image");
			listButton.Values.Text = "";
			// 
			// moveModulesDownButton
			// 
			moveModulesDownButton.Location = new System.Drawing.Point(144, 4);
			moveModulesDownButton.Name = "moveModulesDownButton";
			moveModulesDownButton.Size = new System.Drawing.Size(24, 24);
			moveModulesDownButton.TabIndex = 5;
			moveModulesDownButton.Values.Image = (System.Drawing.Image)resources.GetObject("moveModulesDownButton.Values.Image");
			moveModulesDownButton.Values.Text = "";
			// 
			// moveModulesUpButton
			// 
			moveModulesUpButton.Location = new System.Drawing.Point(116, 4);
			moveModulesUpButton.Name = "moveModulesUpButton";
			moveModulesUpButton.Size = new System.Drawing.Size(24, 24);
			moveModulesUpButton.TabIndex = 4;
			moveModulesUpButton.Values.Image = (System.Drawing.Image)resources.GetObject("moveModulesUpButton.Values.Image");
			moveModulesUpButton.Values.Text = "";
			// 
			// sortModulesButton
			// 
			sortModulesButton.Location = new System.Drawing.Point(88, 4);
			sortModulesButton.Name = "sortModulesButton";
			sortModulesButton.Size = new System.Drawing.Size(24, 24);
			sortModulesButton.TabIndex = 3;
			sortModulesButton.Values.Image = (System.Drawing.Image)resources.GetObject("sortModulesButton.Values.Image");
			sortModulesButton.Values.Text = "";
			// 
			// swapModulesButton
			// 
			swapModulesButton.Location = new System.Drawing.Point(60, 4);
			swapModulesButton.Name = "swapModulesButton";
			swapModulesButton.Size = new System.Drawing.Size(24, 24);
			swapModulesButton.TabIndex = 2;
			swapModulesButton.Values.Image = (System.Drawing.Image)resources.GetObject("swapModulesButton.Values.Image");
			swapModulesButton.Values.Text = "";
			// 
			// removeModuleButton
			// 
			removeModuleButton.Location = new System.Drawing.Point(32, 4);
			removeModuleButton.Name = "removeModuleButton";
			removeModuleButton.Size = new System.Drawing.Size(24, 24);
			removeModuleButton.TabIndex = 1;
			removeModuleButton.Values.Image = (System.Drawing.Image)resources.GetObject("removeModuleButton.Values.Image");
			removeModuleButton.Values.Text = "";
			// 
			// addModuleButton
			// 
			addModuleButton.Location = new System.Drawing.Point(4, 4);
			addModuleButton.Name = "addModuleButton";
			addModuleButton.Size = new System.Drawing.Size(24, 24);
			addModuleButton.TabIndex = 0;
			addModuleButton.Values.Image = (System.Drawing.Image)resources.GetObject("addModuleButton.Values.Image");
			addModuleButton.Values.Text = "";
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
			totalLabel.Location = new System.Drawing.Point(138, 7);
			totalLabel.Name = "totalLabel";
			totalLabel.Size = new System.Drawing.Size(28, 18);
			totalLabel.TabIndex = 1;
			totalLabel.Text = "0/0";
			totalLabel.UseFont = bigFontConfiguration;
			// 
			// timeLabel
			// 
			timeLabel.Location = new System.Drawing.Point(3, 7);
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
			tapedeckBox.Controls.Add(rewindButton);
			tapedeckBox.Controls.Add(previousSongButton);
			tapedeckBox.Controls.Add(previousModuleButton);
			tapedeckBox.Location = new System.Drawing.Point(4, 243);
			tapedeckBox.Name = "tapedeckBox";
			tapedeckBox.Size = new System.Drawing.Size(258, 34);
			tapedeckBox.TabIndex = 9;
			// 
			// pauseCheckButton
			// 
			pauseCheckButton.Location = new System.Drawing.Point(228, 4);
			pauseCheckButton.Name = "pauseCheckButton";
			pauseCheckButton.Size = new System.Drawing.Size(24, 24);
			pauseCheckButton.TabIndex = 8;
			pauseCheckButton.Values.Image = (System.Drawing.Image)resources.GetObject("pauseCheckButton.Values.Image");
			pauseCheckButton.Values.Text = "";
			// 
			// ejectButton
			// 
			ejectButton.Location = new System.Drawing.Point(200, 4);
			ejectButton.Name = "ejectButton";
			ejectButton.Size = new System.Drawing.Size(24, 24);
			ejectButton.TabIndex = 7;
			ejectButton.Values.Image = (System.Drawing.Image)resources.GetObject("ejectButton.Values.Image");
			ejectButton.Values.Text = "";
			// 
			// nextModuleButton
			// 
			nextModuleButton.Location = new System.Drawing.Point(172, 4);
			nextModuleButton.Name = "nextModuleButton";
			nextModuleButton.Size = new System.Drawing.Size(24, 24);
			nextModuleButton.TabIndex = 6;
			nextModuleButton.Values.Image = (System.Drawing.Image)resources.GetObject("nextModuleButton.Values.Image");
			nextModuleButton.Values.Text = "";
			// 
			// nextSongButton
			// 
			nextSongButton.Location = new System.Drawing.Point(144, 4);
			nextSongButton.Name = "nextSongButton";
			nextSongButton.Size = new System.Drawing.Size(24, 24);
			nextSongButton.TabIndex = 5;
			nextSongButton.Values.Image = (System.Drawing.Image)resources.GetObject("nextSongButton.Values.Image");
			nextSongButton.Values.Text = "";
			// 
			// fastForwardButton
			// 
			fastForwardButton.Location = new System.Drawing.Point(116, 4);
			fastForwardButton.Name = "fastForwardButton";
			fastForwardButton.Size = new System.Drawing.Size(24, 24);
			fastForwardButton.TabIndex = 4;
			fastForwardButton.Values.Image = (System.Drawing.Image)resources.GetObject("fastForwardButton.Values.Image");
			fastForwardButton.Values.Text = "";
			// 
			// playButton
			// 
			playButton.Location = new System.Drawing.Point(88, 4);
			playButton.Name = "playButton";
			playButton.Size = new System.Drawing.Size(24, 24);
			playButton.TabIndex = 3;
			playButton.Values.Image = (System.Drawing.Image)resources.GetObject("playButton.Values.Image");
			playButton.Values.Text = "";
			// 
			// rewindButton
			// 
			rewindButton.Location = new System.Drawing.Point(60, 4);
			rewindButton.Name = "rewindButton";
			rewindButton.Size = new System.Drawing.Size(24, 24);
			rewindButton.TabIndex = 2;
			rewindButton.Values.Image = (System.Drawing.Image)resources.GetObject("rewindButton.Values.Image");
			rewindButton.Values.Text = "";
			// 
			// previousSongButton
			// 
			previousSongButton.Location = new System.Drawing.Point(32, 4);
			previousSongButton.Name = "previousSongButton";
			previousSongButton.Size = new System.Drawing.Size(24, 24);
			previousSongButton.TabIndex = 1;
			previousSongButton.Values.Image = (System.Drawing.Image)resources.GetObject("previousSongButton.Values.Image");
			previousSongButton.Values.Text = "";
			// 
			// previousModuleButton
			// 
			previousModuleButton.Location = new System.Drawing.Point(4, 4);
			previousModuleButton.Name = "previousModuleButton";
			previousModuleButton.Size = new System.Drawing.Size(24, 24);
			previousModuleButton.TabIndex = 0;
			previousModuleButton.Values.Image = (System.Drawing.Image)resources.GetObject("previousModuleButton.Values.Image");
			previousModuleButton.Values.Text = "";
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
			loopCheckButton.Location = new System.Drawing.Point(4, 4);
			loopCheckButton.Name = "loopCheckButton";
			loopCheckButton.Size = new System.Drawing.Size(24, 24);
			loopCheckButton.TabIndex = 0;
			loopCheckButton.Values.Image = (System.Drawing.Image)resources.GetObject("loopCheckButton.Values.Image");
			loopCheckButton.Values.Text = "";
			// 
			// favoritesButton
			// 
			favoritesButton.Location = new System.Drawing.Point(32, 4);
			favoritesButton.Name = "favoritesButton";
			favoritesButton.Size = new System.Drawing.Size(24, 24);
			favoritesButton.TabIndex = 1;
			favoritesButton.Values.Image = Resources.IDB_FAVORITES;
			favoritesButton.Values.Text = "";
			// 
			// sampleInfoButton
			// 
			sampleInfoButton.Location = new System.Drawing.Point(88, 4);
			sampleInfoButton.Name = "sampleInfoButton";
			sampleInfoButton.Size = new System.Drawing.Size(24, 24);
			sampleInfoButton.TabIndex = 3;
			sampleInfoButton.Values.Image = (System.Drawing.Image)resources.GetObject("sampleInfoButton.Values.Image");
			sampleInfoButton.Values.Text = "";
			// 
			// muteCheckButton
			// 
			muteCheckButton.Location = new System.Drawing.Point(4, 56);
			muteCheckButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			muteCheckButton.Name = "muteCheckButton";
			muteCheckButton.Size = new System.Drawing.Size(27, 24);
			muteCheckButton.TabIndex = 3;
			muteCheckButton.Values.Image = (System.Drawing.Image)resources.GetObject("muteCheckButton.Values.Image");
			muteCheckButton.Values.Text = "";
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
		private KryptonButton equalizerButton;
		private KryptonButton moduleInfoButton;
		private KryptonTrackBar masterVolumeTrackBar;
		private NostalgicPlayer.Controls.Containers.NostalgicBox listButtonsBox;
		private KryptonButton addModuleButton;
		private KryptonButton removeModuleButton;
		private KryptonButton swapModulesButton;
		private KryptonButton sortModulesButton;
		private KryptonButton moveModulesDownButton;
		private KryptonButton moveModulesUpButton;
		private KryptonButton listButton;
		private KryptonButton diskButton;
		private NostalgicPlayer.Controls.Containers.NostalgicBox listInfoBox;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel totalLabel;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel timeLabel;
		private KryptonTrackBar positionTrackBar;
		private NostalgicPlayer.Controls.Containers.NostalgicBox tapedeckBox;
		private KryptonButton ejectButton;
		private KryptonButton nextModuleButton;
		private KryptonButton nextSongButton;
		private KryptonButton fastForwardButton;
		private KryptonButton playButton;
		private KryptonButton rewindButton;
		private KryptonButton previousSongButton;
		private KryptonButton previousModuleButton;
		private KryptonCheckButton pauseCheckButton;
		private NostalgicPlayer.Controls.Containers.NostalgicBox functionsBox;
		private KryptonCheckButton loopCheckButton;
		private KryptonButton sampleInfoButton;
		private KryptonCheckButton muteCheckButton;
		private System.Windows.Forms.ToolTip toolTip;
		private KryptonContextMenu sortContextMenu;
		private KryptonContextMenu listContextMenu;
		private KryptonContextMenu diskContextMenu;
		private System.Windows.Forms.Timer neverEndingTimer;
		private KryptonContextMenu addContextMenu;
		private KryptonButton favoritesButton;
		private Kit.Gui.Components.FontPalette fontPalette;
		private NostalgicPlayer.Controls.Lists.NostalgicModuleList moduleList;
		private SearchPopupControl searchPopupControl;
		private NostalgicPlayer.Controls.Components.FontConfiguration bigFontConfiguration;
	}
}

