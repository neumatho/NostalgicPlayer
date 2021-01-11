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
			this.moduleListBox = new Krypton.Toolkit.KryptonListBox();
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
			this.controlGroup = new Krypton.Toolkit.KryptonGroup();
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
			this.showSamplesButton = new Krypton.Toolkit.KryptonButton();
			this.muteCheckButton = new Krypton.Toolkit.KryptonCheckButton();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.clockTimer = new System.Windows.Forms.Timer(this.components);
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			((System.ComponentModel.ISupportInitialize)(this.infoGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.infoGroup.Panel)).BeginInit();
			this.infoGroup.Panel.SuspendLayout();
			this.infoGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listButtonsGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.listButtonsGroup.Panel)).BeginInit();
			this.listButtonsGroup.Panel.SuspendLayout();
			this.listButtonsGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listInfoGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.listInfoGroup.Panel)).BeginInit();
			this.listInfoGroup.Panel.SuspendLayout();
			this.listInfoGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.controlGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.controlGroup.Panel)).BeginInit();
			this.controlGroup.Panel.SuspendLayout();
			this.controlGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.loopSampleGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.loopSampleGroup.Panel)).BeginInit();
			this.loopSampleGroup.Panel.SuspendLayout();
			this.loopSampleGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip
			// 
			this.menuStrip.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(384, 24);
			this.menuStrip.TabIndex = 0;
			this.menuStrip.Text = "menuStrip";
			// 
			// infoLabel
			// 
			this.infoLabel.LabelStyle = Krypton.Toolkit.LabelStyle.NormalPanel;
			this.infoLabel.Location = new System.Drawing.Point(3, 3);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(6, 2);
			this.infoLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Rockwell", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.infoLabel.TabIndex = 4;
			this.infoLabel.Values.Text = "";
			// 
			// infoGroup
			// 
			this.infoGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.infoGroup.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelCustom1;
			this.infoGroup.GroupBorderStyle = Krypton.Toolkit.PaletteBorderStyle.ButtonStandalone;
			this.infoGroup.Location = new System.Drawing.Point(4, 28);
			this.infoGroup.Name = "infoGroup";
			// 
			// infoGroup.Panel
			// 
			this.infoGroup.Panel.Controls.Add(this.infoLabel);
			this.infoGroup.Size = new System.Drawing.Size(348, 24);
			this.infoGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.infoGroup.TabIndex = 6;
			// 
			// moduleInfoButton
			// 
			this.moduleInfoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.moduleInfoButton.Enabled = false;
			this.moduleInfoButton.Location = new System.Drawing.Point(356, 28);
			this.moduleInfoButton.Name = "moduleInfoButton";
			this.moduleInfoButton.Size = new System.Drawing.Size(24, 24);
			this.moduleInfoButton.TabIndex = 7;
			this.moduleInfoButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Information;
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
			this.masterVolumeTrackBar.TabIndex = 9;
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
			this.moduleListBox.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.moduleListBox.ScrollAlwaysVisible = true;
			this.moduleListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.moduleListBox.Size = new System.Drawing.Size(345, 116);
			this.moduleListBox.StateCommon.Item.Content.LongText.Font = new System.Drawing.Font("Rockwell", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.moduleListBox.StateCommon.Item.Content.ShortText.Font = new System.Drawing.Font("Rockwell", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.moduleListBox.TabIndex = 10;
			// 
			// listButtonsGroup
			// 
			this.listButtonsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.listButtonsGroup.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelCustom1;
			this.listButtonsGroup.GroupBorderStyle = Krypton.Toolkit.PaletteBorderStyle.ButtonStandalone;
			this.listButtonsGroup.Location = new System.Drawing.Point(4, 176);
			this.listButtonsGroup.Name = "listButtonsGroup";
			// 
			// listButtonsGroup.Panel
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
			this.listButtonsGroup.TabIndex = 11;
			// 
			// diskButton
			// 
			this.diskButton.Location = new System.Drawing.Point(199, 3);
			this.diskButton.Name = "diskButton";
			this.diskButton.Size = new System.Drawing.Size(24, 24);
			this.diskButton.TabIndex = 7;
			this.diskButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Disk;
			this.diskButton.Values.Text = "";
			// 
			// listButton
			// 
			this.listButton.Location = new System.Drawing.Point(171, 3);
			this.listButton.Name = "listButton";
			this.listButton.Size = new System.Drawing.Size(24, 24);
			this.listButton.TabIndex = 6;
			this.listButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.List;
			this.listButton.Values.Text = "";
			// 
			// moveModulesDownButton
			// 
			this.moveModulesDownButton.Location = new System.Drawing.Point(143, 3);
			this.moveModulesDownButton.Name = "moveModulesDownButton";
			this.moveModulesDownButton.Size = new System.Drawing.Size(24, 24);
			this.moveModulesDownButton.TabIndex = 5;
			this.moveModulesDownButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.MoveDown;
			this.moveModulesDownButton.Values.Text = "";
			// 
			// moveModulesUpButton
			// 
			this.moveModulesUpButton.Location = new System.Drawing.Point(115, 3);
			this.moveModulesUpButton.Name = "moveModulesUpButton";
			this.moveModulesUpButton.Size = new System.Drawing.Size(24, 24);
			this.moveModulesUpButton.TabIndex = 4;
			this.moveModulesUpButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.MoveUp;
			this.moveModulesUpButton.Values.Text = "";
			// 
			// sortModulesButton
			// 
			this.sortModulesButton.Location = new System.Drawing.Point(87, 3);
			this.sortModulesButton.Name = "sortModulesButton";
			this.sortModulesButton.Size = new System.Drawing.Size(24, 24);
			this.sortModulesButton.TabIndex = 3;
			this.sortModulesButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Sort;
			this.sortModulesButton.Values.Text = "";
			// 
			// swapModulesButton
			// 
			this.swapModulesButton.Location = new System.Drawing.Point(59, 3);
			this.swapModulesButton.Name = "swapModulesButton";
			this.swapModulesButton.Size = new System.Drawing.Size(24, 24);
			this.swapModulesButton.TabIndex = 2;
			this.swapModulesButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Swap;
			this.swapModulesButton.Values.Text = "";
			// 
			// removeModuleButton
			// 
			this.removeModuleButton.Location = new System.Drawing.Point(31, 3);
			this.removeModuleButton.Name = "removeModuleButton";
			this.removeModuleButton.Size = new System.Drawing.Size(24, 24);
			this.removeModuleButton.TabIndex = 1;
			this.removeModuleButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Remove;
			this.removeModuleButton.Values.Text = "";
			// 
			// addModuleButton
			// 
			this.addModuleButton.Location = new System.Drawing.Point(3, 3);
			this.addModuleButton.Name = "addModuleButton";
			this.addModuleButton.Size = new System.Drawing.Size(24, 24);
			this.addModuleButton.TabIndex = 0;
			this.addModuleButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Add;
			this.addModuleButton.Values.Text = "";
			// 
			// listInfoGroup
			// 
			this.listInfoGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listInfoGroup.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelCustom1;
			this.listInfoGroup.GroupBorderStyle = Krypton.Toolkit.PaletteBorderStyle.ButtonStandalone;
			this.listInfoGroup.Location = new System.Drawing.Point(238, 176);
			this.listInfoGroup.Name = "listInfoGroup";
			// 
			// listInfoGroup.Panel
			// 
			this.listInfoGroup.Panel.Controls.Add(this.totalLabel);
			this.listInfoGroup.Panel.Controls.Add(this.timeLabel);
			this.listInfoGroup.Size = new System.Drawing.Size(142, 34);
			this.listInfoGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.listInfoGroup.TabIndex = 12;
			// 
			// totalLabel
			// 
			this.totalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.totalLabel.LabelStyle = Krypton.Toolkit.LabelStyle.NormalPanel;
			this.totalLabel.Location = new System.Drawing.Point(108, 7);
			this.totalLabel.Name = "totalLabel";
			this.totalLabel.Size = new System.Drawing.Size(27, 17);
			this.totalLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Rockwell", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.totalLabel.TabIndex = 5;
			this.totalLabel.Values.Text = "0/0";
			// 
			// timeLabel
			// 
			this.timeLabel.LabelStyle = Krypton.Toolkit.LabelStyle.NormalPanel;
			this.timeLabel.Location = new System.Drawing.Point(3, 7);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(59, 17);
			this.timeLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Rockwell", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.timeLabel.TabIndex = 4;
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
			this.positionTrackBar.TabIndex = 13;
			this.positionTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
			// 
			// controlGroup
			// 
			this.controlGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.controlGroup.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelCustom1;
			this.controlGroup.GroupBorderStyle = Krypton.Toolkit.PaletteBorderStyle.ButtonStandalone;
			this.controlGroup.Location = new System.Drawing.Point(4, 243);
			this.controlGroup.Name = "controlGroup";
			// 
			// controlGroup.Panel
			// 
			this.controlGroup.Panel.Controls.Add(this.pauseCheckButton);
			this.controlGroup.Panel.Controls.Add(this.ejectButton);
			this.controlGroup.Panel.Controls.Add(this.nextModuleButton);
			this.controlGroup.Panel.Controls.Add(this.nextSongButton);
			this.controlGroup.Panel.Controls.Add(this.fastForwardButton);
			this.controlGroup.Panel.Controls.Add(this.playButton);
			this.controlGroup.Panel.Controls.Add(this.rewindButton);
			this.controlGroup.Panel.Controls.Add(this.previousSongButton);
			this.controlGroup.Panel.Controls.Add(this.previousModuleButton);
			this.controlGroup.Size = new System.Drawing.Size(258, 34);
			this.controlGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.controlGroup.TabIndex = 14;
			// 
			// pauseCheckButton
			// 
			this.pauseCheckButton.Location = new System.Drawing.Point(227, 3);
			this.pauseCheckButton.Name = "pauseCheckButton";
			this.pauseCheckButton.Size = new System.Drawing.Size(24, 24);
			this.pauseCheckButton.TabIndex = 8;
			this.pauseCheckButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Pause;
			this.pauseCheckButton.Values.Text = "";
			// 
			// ejectButton
			// 
			this.ejectButton.Location = new System.Drawing.Point(199, 3);
			this.ejectButton.Name = "ejectButton";
			this.ejectButton.Size = new System.Drawing.Size(24, 24);
			this.ejectButton.TabIndex = 7;
			this.ejectButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Eject;
			this.ejectButton.Values.Text = "";
			// 
			// nextModuleButton
			// 
			this.nextModuleButton.Location = new System.Drawing.Point(171, 3);
			this.nextModuleButton.Name = "nextModuleButton";
			this.nextModuleButton.Size = new System.Drawing.Size(24, 24);
			this.nextModuleButton.TabIndex = 6;
			this.nextModuleButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.NextModule;
			this.nextModuleButton.Values.Text = "";
			// 
			// nextSongButton
			// 
			this.nextSongButton.Location = new System.Drawing.Point(143, 3);
			this.nextSongButton.Name = "nextSongButton";
			this.nextSongButton.Size = new System.Drawing.Size(24, 24);
			this.nextSongButton.TabIndex = 5;
			this.nextSongButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.NextSong;
			this.nextSongButton.Values.Text = "";
			// 
			// fastForwardButton
			// 
			this.fastForwardButton.Location = new System.Drawing.Point(115, 3);
			this.fastForwardButton.Name = "fastForwardButton";
			this.fastForwardButton.Size = new System.Drawing.Size(24, 24);
			this.fastForwardButton.TabIndex = 4;
			this.fastForwardButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.FastForward;
			this.fastForwardButton.Values.Text = "";
			// 
			// playButton
			// 
			this.playButton.Location = new System.Drawing.Point(87, 3);
			this.playButton.Name = "playButton";
			this.playButton.Size = new System.Drawing.Size(24, 24);
			this.playButton.TabIndex = 3;
			this.playButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Play;
			this.playButton.Values.Text = "";
			// 
			// rewindButton
			// 
			this.rewindButton.Location = new System.Drawing.Point(59, 3);
			this.rewindButton.Name = "rewindButton";
			this.rewindButton.Size = new System.Drawing.Size(24, 24);
			this.rewindButton.TabIndex = 2;
			this.rewindButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Rewind;
			this.rewindButton.Values.Text = "";
			// 
			// previousSongButton
			// 
			this.previousSongButton.Location = new System.Drawing.Point(31, 3);
			this.previousSongButton.Name = "previousSongButton";
			this.previousSongButton.Size = new System.Drawing.Size(24, 24);
			this.previousSongButton.TabIndex = 1;
			this.previousSongButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.PreviousSong;
			this.previousSongButton.Values.Text = "";
			// 
			// previousModuleButton
			// 
			this.previousModuleButton.Location = new System.Drawing.Point(3, 3);
			this.previousModuleButton.Name = "previousModuleButton";
			this.previousModuleButton.Size = new System.Drawing.Size(24, 24);
			this.previousModuleButton.TabIndex = 0;
			this.previousModuleButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.PreviousModule;
			this.previousModuleButton.Values.Text = "";
			// 
			// loopSampleGroup
			// 
			this.loopSampleGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.loopSampleGroup.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.PanelCustom1;
			this.loopSampleGroup.GroupBorderStyle = Krypton.Toolkit.PaletteBorderStyle.ButtonStandalone;
			this.loopSampleGroup.Location = new System.Drawing.Point(318, 243);
			this.loopSampleGroup.Name = "loopSampleGroup";
			// 
			// loopSampleGroup.Panel
			// 
			this.loopSampleGroup.Panel.Controls.Add(this.loopCheckButton);
			this.loopSampleGroup.Panel.Controls.Add(this.showSamplesButton);
			this.loopSampleGroup.Size = new System.Drawing.Size(62, 34);
			this.loopSampleGroup.StateNormal.Back.Color1 = System.Drawing.SystemColors.Control;
			this.loopSampleGroup.TabIndex = 15;
			// 
			// loopCheckButton
			// 
			this.loopCheckButton.Location = new System.Drawing.Point(3, 3);
			this.loopCheckButton.Name = "loopCheckButton";
			this.loopCheckButton.Size = new System.Drawing.Size(24, 24);
			this.loopCheckButton.TabIndex = 8;
			this.loopCheckButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Loop;
			this.loopCheckButton.Values.Text = "";
			// 
			// showSamplesButton
			// 
			this.showSamplesButton.Enabled = false;
			this.showSamplesButton.Location = new System.Drawing.Point(31, 3);
			this.showSamplesButton.Name = "showSamplesButton";
			this.showSamplesButton.Size = new System.Drawing.Size(24, 24);
			this.showSamplesButton.TabIndex = 1;
			this.showSamplesButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Samples;
			this.showSamplesButton.Values.Text = "";
			// 
			// muteCheckButton
			// 
			this.muteCheckButton.Location = new System.Drawing.Point(4, 56);
			this.muteCheckButton.Name = "muteCheckButton";
			this.muteCheckButton.Size = new System.Drawing.Size(27, 24);
			this.muteCheckButton.TabIndex = 16;
			this.muteCheckButton.Values.Image = global::Polycode.NostalgicPlayer.Client.GuiPlayer.Properties.Resources.Mute;
			this.muteCheckButton.Values.Text = "";
			// 
			// clockTimer
			// 
			this.clockTimer.Interval = 995;
			this.clockTimer.Tick += new System.EventHandler(this.ClockTimer_Tick);
			// 
			// MainWindowForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 281);
			this.Controls.Add(this.muteCheckButton);
			this.Controls.Add(this.loopSampleGroup);
			this.Controls.Add(this.controlGroup);
			this.Controls.Add(this.positionTrackBar);
			this.Controls.Add(this.listInfoGroup);
			this.Controls.Add(this.listButtonsGroup);
			this.Controls.Add(this.moduleListBox);
			this.Controls.Add(this.masterVolumeTrackBar);
			this.Controls.Add(this.moduleInfoButton);
			this.Controls.Add(this.infoGroup);
			this.Controls.Add(this.menuStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip;
			this.MinimumSize = new System.Drawing.Size(400, 320);
			this.Name = "MainWindowForm";
			((System.ComponentModel.ISupportInitialize)(this.infoGroup.Panel)).EndInit();
			this.infoGroup.Panel.ResumeLayout(false);
			this.infoGroup.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.infoGroup)).EndInit();
			this.infoGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listButtonsGroup.Panel)).EndInit();
			this.listButtonsGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listButtonsGroup)).EndInit();
			this.listButtonsGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listInfoGroup.Panel)).EndInit();
			this.listInfoGroup.Panel.ResumeLayout(false);
			this.listInfoGroup.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.listInfoGroup)).EndInit();
			this.listInfoGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.controlGroup.Panel)).EndInit();
			this.controlGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.controlGroup)).EndInit();
			this.controlGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.loopSampleGroup.Panel)).EndInit();
			this.loopSampleGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.loopSampleGroup)).EndInit();
			this.loopSampleGroup.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private Krypton.Toolkit.KryptonLabel infoLabel;
		private Krypton.Toolkit.KryptonGroup infoGroup;
		private KryptonButton moduleInfoButton;
		private KryptonTrackBar masterVolumeTrackBar;
		private KryptonListBox moduleListBox;
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
		private KryptonGroup controlGroup;
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
		private KryptonButton showSamplesButton;
		private KryptonCheckButton muteCheckButton;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Timer clockTimer;
		private KryptonManager kryptonManager;
	}
}

