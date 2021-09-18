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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindowForm));
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.infoLabel = new Krypton.Toolkit.KryptonLabel();
			this.infoGroup = new Krypton.Toolkit.KryptonGroup();
			this.moduleInfoButton = new Krypton.Toolkit.KryptonButton();
			this.masterVolumeTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.moduleListBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ModuleListControl();
			this.listButtonsGroup = new Krypton.Toolkit.KryptonGroup();
			this.diskButton = new Krypton.Toolkit.KryptonButton();
			this.listButton = new Krypton.Toolkit.KryptonButton();
			this.moveModulesDownButton = new Krypton.Toolkit.KryptonButton();
			this.moveModulesUpButton = new Krypton.Toolkit.KryptonButton();
			this.sortModulesButton = new Krypton.Toolkit.KryptonButton();
			this.swapModulesButton = new Krypton.Toolkit.KryptonButton();
			this.removeModuleButton = new Krypton.Toolkit.KryptonButton();
			this.addModuleButton = new Krypton.Toolkit.KryptonButton();
			this.listInfoGroup = new Krypton.Toolkit.KryptonGroup();
			this.totalLabel = new Krypton.Toolkit.KryptonLabel();
			this.timeLabel = new Krypton.Toolkit.KryptonLabel();
			this.positionTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.tapedeckGroup = new Krypton.Toolkit.KryptonGroup();
			this.pauseCheckButton = new Krypton.Toolkit.KryptonCheckButton();
			this.ejectButton = new Krypton.Toolkit.KryptonButton();
			this.nextModuleButton = new Krypton.Toolkit.KryptonButton();
			this.nextSongButton = new Krypton.Toolkit.KryptonButton();
			this.fastForwardButton = new Krypton.Toolkit.KryptonButton();
			this.playButton = new Krypton.Toolkit.KryptonButton();
			this.rewindButton = new Krypton.Toolkit.KryptonButton();
			this.previousSongButton = new Krypton.Toolkit.KryptonButton();
			this.previousModuleButton = new Krypton.Toolkit.KryptonButton();
			this.loopSampleGroup = new Krypton.Toolkit.KryptonGroup();
			this.loopCheckButton = new Krypton.Toolkit.KryptonCheckButton();
			this.sampleInfoButton = new Krypton.Toolkit.KryptonButton();
			this.muteCheckButton = new Krypton.Toolkit.KryptonCheckButton();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.clockTimer = new System.Windows.Forms.Timer(this.components);
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.sortContextMenu = new Krypton.Toolkit.KryptonContextMenu();
			this.listContextMenu = new Krypton.Toolkit.KryptonContextMenu();
			this.diskContextMenu = new Krypton.Toolkit.KryptonContextMenu();
			this.scrollTimer = new System.Windows.Forms.Timer(this.components);
			this.neverEndingTimer = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.infoGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.infoGroup.Panel)).BeginInit();
			this.infoGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listButtonsGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.listButtonsGroup.Panel)).BeginInit();
			this.listButtonsGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listInfoGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.listInfoGroup.Panel)).BeginInit();
			this.listInfoGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tapedeckGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tapedeckGroup.Panel)).BeginInit();
			this.tapedeckGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.loopSampleGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.loopSampleGroup.Panel)).BeginInit();
			this.loopSampleGroup.Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip
			// 
			this.menuStrip.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(384, 24);
			this.menuStrip.TabIndex = 0;
			this.menuStrip.Text = "menuStrip";
			// 
			// infoLabel
			// 
			this.infoLabel.AutoSize = false;
			this.infoLabel.Location = new System.Drawing.Point(3, 3);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(340, 16);
			this.infoLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.infoLabel.TabIndex = 0;
			this.infoLabel.Values.Text = "";
			// 
			// infoGroup
			// 
			this.infoGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.infoGroup.Location = new System.Drawing.Point(4, 28);
			this.infoGroup.Name = "infoGroup";
			// 
			// 
			// 
			this.infoGroup.Panel.Controls.Add(this.infoLabel);
			this.infoGroup.Size = new System.Drawing.Size(348, 24);
			this.infoGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.infoGroup.TabIndex = 1;
			// 
			// moduleInfoButton
			// 
			this.moduleInfoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.moduleInfoButton.Location = new System.Drawing.Point(356, 28);
			this.moduleInfoButton.Name = "moduleInfoButton";
			this.moduleInfoButton.Size = new System.Drawing.Size(24, 24);
			this.moduleInfoButton.TabIndex = 2;
			this.moduleInfoButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("moduleInfoButton.Values.Image")));
			this.moduleInfoButton.Values.Text = "";
			// 
			// masterVolumeTrackBar
			// 
			this.masterVolumeTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.masterVolumeTrackBar.DrawBackground = true;
			this.masterVolumeTrackBar.Location = new System.Drawing.Point(4, 84);
			this.masterVolumeTrackBar.Maximum = 256;
			this.masterVolumeTrackBar.Name = "masterVolumeTrackBar";
			this.masterVolumeTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.masterVolumeTrackBar.Size = new System.Drawing.Size(27, 88);
			this.masterVolumeTrackBar.TabIndex = 4;
			this.masterVolumeTrackBar.TickFrequency = 8;
			this.masterVolumeTrackBar.VolumeControl = true;
			// 
			// moduleListBox
			// 
			this.moduleListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.moduleListBox.Location = new System.Drawing.Point(35, 56);
			this.moduleListBox.Name = "moduleListBox";
			this.moduleListBox.ScrollAlwaysVisible = true;
			this.moduleListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.moduleListBox.Size = new System.Drawing.Size(345, 116);
			this.moduleListBox.StateCommon.Item.Content.LongText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleListBox.StateCommon.Item.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleListBox.TabIndex = 5;
			// 
			// listButtonsGroup
			// 
			this.listButtonsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.listButtonsGroup.Location = new System.Drawing.Point(4, 176);
			this.listButtonsGroup.Name = "listButtonsGroup";
			// 
			// 
			// 
			this.listButtonsGroup.Panel.Controls.Add(this.diskButton);
			this.listButtonsGroup.Panel.Controls.Add(this.listButton);
			this.listButtonsGroup.Panel.Controls.Add(this.moveModulesDownButton);
			this.listButtonsGroup.Panel.Controls.Add(this.moveModulesUpButton);
			this.listButtonsGroup.Panel.Controls.Add(this.sortModulesButton);
			this.listButtonsGroup.Panel.Controls.Add(this.swapModulesButton);
			this.listButtonsGroup.Panel.Controls.Add(this.removeModuleButton);
			this.listButtonsGroup.Panel.Controls.Add(this.addModuleButton);
			this.listButtonsGroup.Size = new System.Drawing.Size(230, 34);
			this.listButtonsGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.listButtonsGroup.TabIndex = 6;
			// 
			// diskButton
			// 
			this.diskButton.Location = new System.Drawing.Point(200, 4);
			this.diskButton.Name = "diskButton";
			this.diskButton.Size = new System.Drawing.Size(24, 24);
			this.diskButton.TabIndex = 7;
			this.diskButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("diskButton.Values.Image")));
			this.diskButton.Values.Text = "";
			// 
			// listButton
			// 
			this.listButton.Location = new System.Drawing.Point(172, 4);
			this.listButton.Name = "listButton";
			this.listButton.Size = new System.Drawing.Size(24, 24);
			this.listButton.TabIndex = 6;
			this.listButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("listButton.Values.Image")));
			this.listButton.Values.Text = "";
			// 
			// moveModulesDownButton
			// 
			this.moveModulesDownButton.Location = new System.Drawing.Point(144, 4);
			this.moveModulesDownButton.Name = "moveModulesDownButton";
			this.moveModulesDownButton.Size = new System.Drawing.Size(24, 24);
			this.moveModulesDownButton.TabIndex = 5;
			this.moveModulesDownButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("moveModulesDownButton.Values.Image")));
			this.moveModulesDownButton.Values.Text = "";
			// 
			// moveModulesUpButton
			// 
			this.moveModulesUpButton.Location = new System.Drawing.Point(116, 4);
			this.moveModulesUpButton.Name = "moveModulesUpButton";
			this.moveModulesUpButton.Size = new System.Drawing.Size(24, 24);
			this.moveModulesUpButton.TabIndex = 4;
			this.moveModulesUpButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("moveModulesUpButton.Values.Image")));
			this.moveModulesUpButton.Values.Text = "";
			// 
			// sortModulesButton
			// 
			this.sortModulesButton.Location = new System.Drawing.Point(88, 4);
			this.sortModulesButton.Name = "sortModulesButton";
			this.sortModulesButton.Size = new System.Drawing.Size(24, 24);
			this.sortModulesButton.TabIndex = 3;
			this.sortModulesButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("sortModulesButton.Values.Image")));
			this.sortModulesButton.Values.Text = "";
			// 
			// swapModulesButton
			// 
			this.swapModulesButton.Location = new System.Drawing.Point(60, 4);
			this.swapModulesButton.Name = "swapModulesButton";
			this.swapModulesButton.Size = new System.Drawing.Size(24, 24);
			this.swapModulesButton.TabIndex = 2;
			this.swapModulesButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("swapModulesButton.Values.Image")));
			this.swapModulesButton.Values.Text = "";
			// 
			// removeModuleButton
			// 
			this.removeModuleButton.Location = new System.Drawing.Point(32, 4);
			this.removeModuleButton.Name = "removeModuleButton";
			this.removeModuleButton.Size = new System.Drawing.Size(24, 24);
			this.removeModuleButton.TabIndex = 1;
			this.removeModuleButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("removeModuleButton.Values.Image")));
			this.removeModuleButton.Values.Text = "";
			// 
			// addModuleButton
			// 
			this.addModuleButton.Location = new System.Drawing.Point(4, 4);
			this.addModuleButton.Name = "addModuleButton";
			this.addModuleButton.Size = new System.Drawing.Size(24, 24);
			this.addModuleButton.TabIndex = 0;
			this.addModuleButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("addModuleButton.Values.Image")));
			this.addModuleButton.Values.Text = "";
			// 
			// listInfoGroup
			// 
			this.listInfoGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listInfoGroup.Location = new System.Drawing.Point(238, 176);
			this.listInfoGroup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.listInfoGroup.Name = "listInfoGroup";
			// 
			// 
			// 
			this.listInfoGroup.Panel.Controls.Add(this.totalLabel);
			this.listInfoGroup.Panel.Controls.Add(this.timeLabel);
			this.listInfoGroup.Size = new System.Drawing.Size(142, 34);
			this.listInfoGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.listInfoGroup.TabIndex = 7;
			// 
			// totalLabel
			// 
			this.totalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.totalLabel.Location = new System.Drawing.Point(106, 8);
			this.totalLabel.Name = "totalLabel";
			this.totalLabel.Size = new System.Drawing.Size(29, 18);
			this.totalLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.totalLabel.TabIndex = 1;
			this.totalLabel.Values.Text = "0/0";
			// 
			// timeLabel
			// 
			this.timeLabel.Location = new System.Drawing.Point(3, 8);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(64, 18);
			this.timeLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.timeLabel.TabIndex = 0;
			this.timeLabel.Values.Text = "0:00/0:00";
			// 
			// positionTrackBar
			// 
			this.positionTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.positionTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.SeparatorLowProfile;
			this.positionTrackBar.DrawBackground = true;
			this.positionTrackBar.Location = new System.Drawing.Point(4, 216);
			this.positionTrackBar.Maximum = 100;
			this.positionTrackBar.Name = "positionTrackBar";
			this.positionTrackBar.Size = new System.Drawing.Size(376, 21);
			this.positionTrackBar.TabIndex = 8;
			this.positionTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
			// 
			// tapedeckGroup
			// 
			this.tapedeckGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.tapedeckGroup.Location = new System.Drawing.Point(4, 243);
			this.tapedeckGroup.Name = "tapedeckGroup";
			// 
			// 
			// 
			this.tapedeckGroup.Panel.Controls.Add(this.pauseCheckButton);
			this.tapedeckGroup.Panel.Controls.Add(this.ejectButton);
			this.tapedeckGroup.Panel.Controls.Add(this.nextModuleButton);
			this.tapedeckGroup.Panel.Controls.Add(this.nextSongButton);
			this.tapedeckGroup.Panel.Controls.Add(this.fastForwardButton);
			this.tapedeckGroup.Panel.Controls.Add(this.playButton);
			this.tapedeckGroup.Panel.Controls.Add(this.rewindButton);
			this.tapedeckGroup.Panel.Controls.Add(this.previousSongButton);
			this.tapedeckGroup.Panel.Controls.Add(this.previousModuleButton);
			this.tapedeckGroup.Size = new System.Drawing.Size(258, 34);
			this.tapedeckGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.tapedeckGroup.TabIndex = 9;
			// 
			// pauseCheckButton
			// 
			this.pauseCheckButton.Location = new System.Drawing.Point(228, 4);
			this.pauseCheckButton.Name = "pauseCheckButton";
			this.pauseCheckButton.Size = new System.Drawing.Size(24, 24);
			this.pauseCheckButton.TabIndex = 8;
			this.pauseCheckButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("pauseCheckButton.Values.Image")));
			this.pauseCheckButton.Values.Text = "";
			// 
			// ejectButton
			// 
			this.ejectButton.Location = new System.Drawing.Point(200, 4);
			this.ejectButton.Name = "ejectButton";
			this.ejectButton.Size = new System.Drawing.Size(24, 24);
			this.ejectButton.TabIndex = 7;
			this.ejectButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("ejectButton.Values.Image")));
			this.ejectButton.Values.Text = "";
			// 
			// nextModuleButton
			// 
			this.nextModuleButton.Location = new System.Drawing.Point(172, 4);
			this.nextModuleButton.Name = "nextModuleButton";
			this.nextModuleButton.Size = new System.Drawing.Size(24, 24);
			this.nextModuleButton.TabIndex = 6;
			this.nextModuleButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("nextModuleButton.Values.Image")));
			this.nextModuleButton.Values.Text = "";
			// 
			// nextSongButton
			// 
			this.nextSongButton.Location = new System.Drawing.Point(144, 4);
			this.nextSongButton.Name = "nextSongButton";
			this.nextSongButton.Size = new System.Drawing.Size(24, 24);
			this.nextSongButton.TabIndex = 5;
			this.nextSongButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("nextSongButton.Values.Image")));
			this.nextSongButton.Values.Text = "";
			// 
			// fastForwardButton
			// 
			this.fastForwardButton.Location = new System.Drawing.Point(116, 4);
			this.fastForwardButton.Name = "fastForwardButton";
			this.fastForwardButton.Size = new System.Drawing.Size(24, 24);
			this.fastForwardButton.TabIndex = 4;
			this.fastForwardButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("fastForwardButton.Values.Image")));
			this.fastForwardButton.Values.Text = "";
			// 
			// playButton
			// 
			this.playButton.Location = new System.Drawing.Point(88, 4);
			this.playButton.Name = "playButton";
			this.playButton.Size = new System.Drawing.Size(24, 24);
			this.playButton.TabIndex = 3;
			this.playButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("playButton.Values.Image")));
			this.playButton.Values.Text = "";
			// 
			// rewindButton
			// 
			this.rewindButton.Location = new System.Drawing.Point(60, 4);
			this.rewindButton.Name = "rewindButton";
			this.rewindButton.Size = new System.Drawing.Size(24, 24);
			this.rewindButton.TabIndex = 2;
			this.rewindButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("rewindButton.Values.Image")));
			this.rewindButton.Values.Text = "";
			// 
			// previousSongButton
			// 
			this.previousSongButton.Location = new System.Drawing.Point(32, 4);
			this.previousSongButton.Name = "previousSongButton";
			this.previousSongButton.Size = new System.Drawing.Size(24, 24);
			this.previousSongButton.TabIndex = 1;
			this.previousSongButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("previousSongButton.Values.Image")));
			this.previousSongButton.Values.Text = "";
			// 
			// previousModuleButton
			// 
			this.previousModuleButton.Location = new System.Drawing.Point(4, 4);
			this.previousModuleButton.Name = "previousModuleButton";
			this.previousModuleButton.Size = new System.Drawing.Size(24, 24);
			this.previousModuleButton.TabIndex = 0;
			this.previousModuleButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("previousModuleButton.Values.Image")));
			this.previousModuleButton.Values.Text = "";
			// 
			// loopSampleGroup
			// 
			this.loopSampleGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.loopSampleGroup.Location = new System.Drawing.Point(318, 243);
			this.loopSampleGroup.Name = "loopSampleGroup";
			// 
			// 
			// 
			this.loopSampleGroup.Panel.Controls.Add(this.loopCheckButton);
			this.loopSampleGroup.Panel.Controls.Add(this.sampleInfoButton);
			this.loopSampleGroup.Size = new System.Drawing.Size(62, 34);
			this.loopSampleGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.loopSampleGroup.TabIndex = 10;
			// 
			// loopCheckButton
			// 
			this.loopCheckButton.Location = new System.Drawing.Point(4, 4);
			this.loopCheckButton.Name = "loopCheckButton";
			this.loopCheckButton.Size = new System.Drawing.Size(24, 24);
			this.loopCheckButton.TabIndex = 0;
			this.loopCheckButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("loopCheckButton.Values.Image")));
			this.loopCheckButton.Values.Text = "";
			// 
			// sampleInfoButton
			// 
			this.sampleInfoButton.Location = new System.Drawing.Point(32, 4);
			this.sampleInfoButton.Name = "sampleInfoButton";
			this.sampleInfoButton.Size = new System.Drawing.Size(24, 24);
			this.sampleInfoButton.TabIndex = 1;
			this.sampleInfoButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("sampleInfoButton.Values.Image")));
			this.sampleInfoButton.Values.Text = "";
			// 
			// muteCheckButton
			// 
			this.muteCheckButton.Location = new System.Drawing.Point(4, 56);
			this.muteCheckButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.muteCheckButton.Name = "muteCheckButton";
			this.muteCheckButton.Size = new System.Drawing.Size(27, 24);
			this.muteCheckButton.TabIndex = 3;
			this.muteCheckButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("muteCheckButton.Values.Image")));
			this.muteCheckButton.Values.Text = "";
			// 
			// clockTimer
			// 
			this.clockTimer.Interval = 995;
			// 
			// kryptonManager
			// 
			this.kryptonManager.GlobalPaletteMode = Krypton.Toolkit.PaletteModeManager.Office2010Blue;
			// 
			// scrollTimer
			// 
			this.scrollTimer.Interval = 300;
			// 
			// MainWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(384, 281);
			this.Controls.Add(this.muteCheckButton);
			this.Controls.Add(this.loopSampleGroup);
			this.Controls.Add(this.tapedeckGroup);
			this.Controls.Add(this.positionTrackBar);
			this.Controls.Add(this.listInfoGroup);
			this.Controls.Add(this.listButtonsGroup);
			this.Controls.Add(this.moduleListBox);
			this.Controls.Add(this.masterVolumeTrackBar);
			this.Controls.Add(this.moduleInfoButton);
			this.Controls.Add(this.infoGroup);
			this.Controls.Add(this.menuStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStrip;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MinimumSize = new System.Drawing.Size(400, 320);
			this.Name = "MainWindowForm";
			((System.ComponentModel.ISupportInitialize)(this.infoGroup.Panel)).EndInit();
			this.infoGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.infoGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.listButtonsGroup.Panel)).EndInit();
			this.listButtonsGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listButtonsGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.listInfoGroup.Panel)).EndInit();
			this.listInfoGroup.Panel.ResumeLayout(false);
			this.listInfoGroup.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.listInfoGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tapedeckGroup.Panel)).EndInit();
			this.tapedeckGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tapedeckGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.loopSampleGroup.Panel)).EndInit();
			this.loopSampleGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.loopSampleGroup)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private Krypton.Toolkit.KryptonLabel infoLabel;
		private Krypton.Toolkit.KryptonGroup infoGroup;
		private KryptonButton moduleInfoButton;
		private KryptonTrackBar masterVolumeTrackBar;
		private ModuleListControl moduleListBox;
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
		private KryptonGroup loopSampleGroup;
		private KryptonCheckButton loopCheckButton;
		private KryptonButton sampleInfoButton;
		private KryptonCheckButton muteCheckButton;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Timer clockTimer;
		private KryptonManager kryptonManager;
		private KryptonContextMenu sortContextMenu;
		private KryptonContextMenu listContextMenu;
		private KryptonContextMenu diskContextMenu;
		private System.Windows.Forms.Timer scrollTimer;
		private System.Windows.Forms.Timer neverEndingTimer;
	}
}

