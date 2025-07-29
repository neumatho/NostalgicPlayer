using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
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
			infoLabel = new KryptonLabel();
			bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			infoGroup = new KryptonGroup();
			moduleInfoButton = new KryptonButton();
			masterVolumeTrackBar = new KryptonTrackBar();
			listButtonsGroup = new KryptonGroup();
			diskButton = new KryptonButton();
			listButton = new KryptonButton();
			moveModulesDownButton = new KryptonButton();
			moveModulesUpButton = new KryptonButton();
			sortModulesButton = new KryptonButton();
			swapModulesButton = new KryptonButton();
			removeModuleButton = new KryptonButton();
			addModuleButton = new KryptonButton();
			listInfoGroup = new KryptonGroup();
			totalLabel = new KryptonLabel();
			timeLabel = new KryptonLabel();
			positionTrackBar = new KryptonTrackBar();
			tapedeckGroup = new KryptonGroup();
			pauseCheckButton = new KryptonCheckButton();
			ejectButton = new KryptonButton();
			nextModuleButton = new KryptonButton();
			nextSongButton = new KryptonButton();
			fastForwardButton = new KryptonButton();
			playButton = new KryptonButton();
			rewindButton = new KryptonButton();
			previousSongButton = new KryptonButton();
			previousModuleButton = new KryptonButton();
			functionsGroup = new KryptonGroup();
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
			moduleListControl = new ModuleListControl();
			((System.ComponentModel.ISupportInitialize)infoGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)infoGroup.Panel).BeginInit();
			infoGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)listButtonsGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)listButtonsGroup.Panel).BeginInit();
			listButtonsGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)listInfoGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)listInfoGroup.Panel).BeginInit();
			listInfoGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)tapedeckGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)tapedeckGroup.Panel).BeginInit();
			tapedeckGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)functionsGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)functionsGroup.Panel).BeginInit();
			functionsGroup.Panel.SuspendLayout();
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
			infoLabel.AutoSize = false;
			infoLabel.Location = new System.Drawing.Point(3, 3);
			infoLabel.Name = "infoLabel";
			infoLabel.Palette = bigFontPalette;
			infoLabel.PaletteMode = PaletteMode.Custom;
			infoLabel.Size = new System.Drawing.Size(374, 16);
			infoLabel.TabIndex = 0;
			infoLabel.Values.Text = "";
			// 
			// bigFontPalette
			// 
			bigFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigFontPalette.BaseFontSize = 9F;
			bigFontPalette.BasePaletteType = BasePaletteType.Custom;
			bigFontPalette.ThemeName = "";
			bigFontPalette.UseKryptonFileDialogs = true;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// infoGroup
			// 
			infoGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoGroup.Location = new System.Drawing.Point(4, 28);
			infoGroup.Name = "infoGroup";
			// 
			// 
			// 
			infoGroup.Panel.Controls.Add(infoLabel);
			infoGroup.Size = new System.Drawing.Size(380, 24);
			infoGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			infoGroup.TabIndex = 1;
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
			// listButtonsGroup
			// 
			listButtonsGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			listButtonsGroup.Location = new System.Drawing.Point(4, 176);
			listButtonsGroup.Name = "listButtonsGroup";
			// 
			// 
			// 
			listButtonsGroup.Panel.Controls.Add(diskButton);
			listButtonsGroup.Panel.Controls.Add(listButton);
			listButtonsGroup.Panel.Controls.Add(moveModulesDownButton);
			listButtonsGroup.Panel.Controls.Add(moveModulesUpButton);
			listButtonsGroup.Panel.Controls.Add(sortModulesButton);
			listButtonsGroup.Panel.Controls.Add(swapModulesButton);
			listButtonsGroup.Panel.Controls.Add(removeModuleButton);
			listButtonsGroup.Panel.Controls.Add(addModuleButton);
			listButtonsGroup.Size = new System.Drawing.Size(230, 34);
			listButtonsGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			listButtonsGroup.TabIndex = 6;
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
			// listInfoGroup
			// 
			listInfoGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			listInfoGroup.Location = new System.Drawing.Point(238, 176);
			listInfoGroup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			listInfoGroup.Name = "listInfoGroup";
			// 
			// 
			// 
			listInfoGroup.Panel.Controls.Add(totalLabel);
			listInfoGroup.Panel.Controls.Add(timeLabel);
			listInfoGroup.Size = new System.Drawing.Size(174, 34);
			listInfoGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			listInfoGroup.TabIndex = 7;
			// 
			// totalLabel
			// 
			totalLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			totalLabel.Location = new System.Drawing.Point(138, 7);
			totalLabel.Name = "totalLabel";
			totalLabel.Palette = bigFontPalette;
			totalLabel.PaletteMode = PaletteMode.Custom;
			totalLabel.Size = new System.Drawing.Size(28, 18);
			totalLabel.TabIndex = 1;
			totalLabel.Values.Text = "0/0";
			// 
			// timeLabel
			// 
			timeLabel.Location = new System.Drawing.Point(3, 7);
			timeLabel.Name = "timeLabel";
			timeLabel.Palette = bigFontPalette;
			timeLabel.PaletteMode = PaletteMode.Custom;
			timeLabel.Size = new System.Drawing.Size(62, 18);
			timeLabel.TabIndex = 0;
			timeLabel.Values.Text = "0:00/0:00";
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
			// tapedeckGroup
			// 
			tapedeckGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			tapedeckGroup.Location = new System.Drawing.Point(4, 243);
			tapedeckGroup.Name = "tapedeckGroup";
			// 
			// 
			// 
			tapedeckGroup.Panel.Controls.Add(pauseCheckButton);
			tapedeckGroup.Panel.Controls.Add(ejectButton);
			tapedeckGroup.Panel.Controls.Add(nextModuleButton);
			tapedeckGroup.Panel.Controls.Add(nextSongButton);
			tapedeckGroup.Panel.Controls.Add(fastForwardButton);
			tapedeckGroup.Panel.Controls.Add(playButton);
			tapedeckGroup.Panel.Controls.Add(rewindButton);
			tapedeckGroup.Panel.Controls.Add(previousSongButton);
			tapedeckGroup.Panel.Controls.Add(previousModuleButton);
			tapedeckGroup.Size = new System.Drawing.Size(258, 34);
			tapedeckGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			tapedeckGroup.TabIndex = 9;
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
			// functionsGroup
			// 
			functionsGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			functionsGroup.Location = new System.Drawing.Point(322, 243);
			functionsGroup.Name = "functionsGroup";
			// 
			// 
			// 
			functionsGroup.Panel.Controls.Add(loopCheckButton);
			functionsGroup.Panel.Controls.Add(favoritesButton);
			functionsGroup.Panel.Controls.Add(sampleInfoButton);
			functionsGroup.Size = new System.Drawing.Size(90, 34);
			functionsGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			functionsGroup.TabIndex = 10;
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
			sampleInfoButton.Location = new System.Drawing.Point(60, 4);
			sampleInfoButton.Name = "sampleInfoButton";
			sampleInfoButton.Size = new System.Drawing.Size(24, 24);
			sampleInfoButton.TabIndex = 2;
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
			// moduleListControl
			// 
			moduleListControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			moduleListControl.Location = new System.Drawing.Point(35, 56);
			moduleListControl.Name = "moduleListControl";
			moduleListControl.Size = new System.Drawing.Size(377, 116);
			moduleListControl.TabIndex = 5;
			// 
			// MainWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(416, 281);
			Controls.Add(moduleListControl);
			Controls.Add(muteCheckButton);
			Controls.Add(functionsGroup);
			Controls.Add(tapedeckGroup);
			Controls.Add(positionTrackBar);
			Controls.Add(listInfoGroup);
			Controls.Add(listButtonsGroup);
			Controls.Add(masterVolumeTrackBar);
			Controls.Add(moduleInfoButton);
			Controls.Add(infoGroup);
			Controls.Add(menuStrip);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			MainMenuStrip = menuStrip;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MinimumSize = new System.Drawing.Size(432, 320);
			Name = "MainWindowForm";
			Palette = fontPalette;
			PaletteMode = PaletteMode.Custom;
			((System.ComponentModel.ISupportInitialize)infoGroup.Panel).EndInit();
			infoGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)infoGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)listButtonsGroup.Panel).EndInit();
			listButtonsGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)listButtonsGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)listInfoGroup.Panel).EndInit();
			listInfoGroup.Panel.ResumeLayout(false);
			listInfoGroup.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)listInfoGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)tapedeckGroup.Panel).EndInit();
			tapedeckGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)tapedeckGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)functionsGroup.Panel).EndInit();
			functionsGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)functionsGroup).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private Krypton.Toolkit.KryptonLabel infoLabel;
		private Krypton.Toolkit.KryptonGroup infoGroup;
		private KryptonButton moduleInfoButton;
		private KryptonTrackBar masterVolumeTrackBar;
		private KryptonGroup listButtonsGroup;
		private KryptonButton addModuleButton;
		private KryptonButton removeModuleButton;
		private KryptonButton swapModulesButton;
		private KryptonButton sortModulesButton;
		private KryptonButton moveModulesDownButton;
		private KryptonButton moveModulesUpButton;
		private KryptonButton listButton;
		private KryptonButton diskButton;
		private KryptonGroup listInfoGroup;
		private KryptonLabel totalLabel;
		private KryptonLabel timeLabel;
		private KryptonTrackBar positionTrackBar;
		private KryptonGroup tapedeckGroup;
		private KryptonButton ejectButton;
		private KryptonButton nextModuleButton;
		private KryptonButton nextSongButton;
		private KryptonButton fastForwardButton;
		private KryptonButton playButton;
		private KryptonButton rewindButton;
		private KryptonButton previousSongButton;
		private KryptonButton previousModuleButton;
		private KryptonCheckButton pauseCheckButton;
		private KryptonGroup functionsGroup;
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
		private GuiKit.Components.FontPalette fontPalette;
		private GuiKit.Components.FontPalette bigFontPalette;
		private ModuleListControl moduleListControl;
	}
}

