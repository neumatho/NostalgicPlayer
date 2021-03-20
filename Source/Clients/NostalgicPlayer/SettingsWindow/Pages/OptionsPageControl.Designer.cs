
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class OptionsPageControl
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
			this.components = new System.ComponentModel.Container();
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.generalGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.addJumpCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.addToListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.rememberListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.rememberListPanel = new System.Windows.Forms.Panel();
			this.rememberModulePositionCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.rememberListPositionCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.tooltipsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.showNameInTitleCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.showListNumberCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.scanFilesCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.useDatabaseCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.loadingGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.doubleBufferingCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.doubleBufferingPanel = new System.Windows.Forms.Panel();
			this.earlyLoadLabel = new Krypton.Toolkit.KryptonLabel();
			this.doubleBufferingTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.moduleErrorLabel = new Krypton.Toolkit.KryptonLabel();
			this.moduleErrorComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.playingGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.moduleListEndComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.moduleListEndLabel = new Krypton.Toolkit.KryptonLabel();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox.Panel)).BeginInit();
			this.generalGroupBox.Panel.SuspendLayout();
			this.rememberListPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.loadingGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.loadingGroupBox.Panel)).BeginInit();
			this.loadingGroupBox.Panel.SuspendLayout();
			this.doubleBufferingPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.moduleErrorComboBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.playingGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.playingGroupBox.Panel)).BeginInit();
			this.playingGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.moduleListEndComboBox)).BeginInit();
			this.SuspendLayout();
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// generalGroupBox
			// 
			this.generalGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.generalGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.generalGroupBox.Location = new System.Drawing.Point(8, 4);
			this.generalGroupBox.Name = "generalGroupBox";
			// 
			// 
			// 
			this.generalGroupBox.Panel.Controls.Add(this.addJumpCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.addToListCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.rememberListCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.rememberListPanel);
			this.generalGroupBox.Panel.Controls.Add(this.tooltipsCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.showNameInTitleCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.showListNumberCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.scanFilesCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.useDatabaseCheckBox);
			this.controlResource.SetResourceKey(this.generalGroupBox, "IDS_SETTINGS_OPTIONS_GENERAL");
			this.generalGroupBox.Size = new System.Drawing.Size(592, 133);
			this.generalGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.generalGroupBox.TabIndex = 0;
			this.generalGroupBox.Values.Heading = "General";
			// 
			// addJumpCheckBox
			// 
			this.addJumpCheckBox.Location = new System.Drawing.Point(4, 5);
			this.addJumpCheckBox.Name = "addJumpCheckBox";
			this.controlResource.SetResourceKey(this.addJumpCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_ADDJUMP");
			this.addJumpCheckBox.Size = new System.Drawing.Size(138, 17);
			this.addJumpCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.addJumpCheckBox.TabIndex = 0;
			this.addJumpCheckBox.Values.Text = "Jump to added module";
			// 
			// addToListCheckBox
			// 
			this.addToListCheckBox.Location = new System.Drawing.Point(4, 26);
			this.addToListCheckBox.Name = "addToListCheckBox";
			this.controlResource.SetResourceKey(this.addToListCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_ADDTOLIST");
			this.addToListCheckBox.Size = new System.Drawing.Size(126, 17);
			this.addToListCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.addToListCheckBox.TabIndex = 1;
			this.addToListCheckBox.Values.Text = "Add to list as default";
			// 
			// rememberListCheckBox
			// 
			this.rememberListCheckBox.Location = new System.Drawing.Point(4, 47);
			this.rememberListCheckBox.Name = "rememberListCheckBox";
			this.controlResource.SetResourceKey(this.rememberListCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERLIST");
			this.rememberListCheckBox.Size = new System.Drawing.Size(133, 17);
			this.rememberListCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.rememberListCheckBox.TabIndex = 2;
			this.rememberListCheckBox.Values.Text = "Remember list on exit";
			this.rememberListCheckBox.CheckedChanged += new System.EventHandler(this.RememberListCheckBox_CheckedChanged);
			// 
			// rememberListPanel
			// 
			this.rememberListPanel.BackColor = System.Drawing.Color.Transparent;
			this.rememberListPanel.Controls.Add(this.rememberModulePositionCheckBox);
			this.rememberListPanel.Controls.Add(this.rememberListPositionCheckBox);
			this.rememberListPanel.Enabled = false;
			this.rememberListPanel.Location = new System.Drawing.Point(12, 68);
			this.rememberListPanel.Name = "rememberListPanel";
			this.controlResource.SetResourceKey(this.rememberListPanel, null);
			this.rememberListPanel.Size = new System.Drawing.Size(170, 38);
			this.rememberListPanel.TabIndex = 3;
			// 
			// rememberModulePositionCheckBox
			// 
			this.rememberModulePositionCheckBox.Location = new System.Drawing.Point(0, 21);
			this.rememberModulePositionCheckBox.Name = "rememberModulePositionCheckBox";
			this.controlResource.SetResourceKey(this.rememberModulePositionCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERMODULEPOSITION");
			this.rememberModulePositionCheckBox.Size = new System.Drawing.Size(160, 17);
			this.rememberModulePositionCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.rememberModulePositionCheckBox.TabIndex = 1;
			this.rememberModulePositionCheckBox.Values.Text = "Remember module position";
			// 
			// rememberListPositionCheckBox
			// 
			this.rememberListPositionCheckBox.Location = new System.Drawing.Point(0, 0);
			this.rememberListPositionCheckBox.Name = "rememberListPositionCheckBox";
			this.controlResource.SetResourceKey(this.rememberListPositionCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERLISTPOSITION");
			this.rememberListPositionCheckBox.Size = new System.Drawing.Size(157, 17);
			this.rememberListPositionCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.rememberListPositionCheckBox.TabIndex = 0;
			this.rememberListPositionCheckBox.Values.Text = "Remember playing module";
			this.rememberListPositionCheckBox.CheckedChanged += new System.EventHandler(this.RememberListPositionCheckBox_CheckedChanged);
			// 
			// tooltipsCheckBox
			// 
			this.tooltipsCheckBox.Location = new System.Drawing.Point(200, 5);
			this.tooltipsCheckBox.Name = "tooltipsCheckBox";
			this.controlResource.SetResourceKey(this.tooltipsCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_TOOLTIPS");
			this.tooltipsCheckBox.Size = new System.Drawing.Size(100, 17);
			this.tooltipsCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.tooltipsCheckBox.TabIndex = 4;
			this.tooltipsCheckBox.Values.Text = "Button tool tips";
			// 
			// showNameInTitleCheckBox
			// 
			this.showNameInTitleCheckBox.Location = new System.Drawing.Point(200, 26);
			this.showNameInTitleCheckBox.Name = "showNameInTitleCheckBox";
			this.controlResource.SetResourceKey(this.showNameInTitleCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWNAME");
			this.showNameInTitleCheckBox.Size = new System.Drawing.Size(173, 17);
			this.showNameInTitleCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.showNameInTitleCheckBox.TabIndex = 5;
			this.showNameInTitleCheckBox.Values.Text = "Show module name in titlebar";
			// 
			// showListNumberCheckBox
			// 
			this.showListNumberCheckBox.Location = new System.Drawing.Point(200, 47);
			this.showListNumberCheckBox.Name = "showListNumberCheckBox";
			this.controlResource.SetResourceKey(this.showListNumberCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWLISTNUMBER");
			this.showListNumberCheckBox.Size = new System.Drawing.Size(148, 17);
			this.showListNumberCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.showListNumberCheckBox.TabIndex = 6;
			this.showListNumberCheckBox.Values.Text = "Show item number in list";
			// 
			// scanFilesCheckBox
			// 
			this.scanFilesCheckBox.Location = new System.Drawing.Point(400, 5);
			this.scanFilesCheckBox.Name = "scanFilesCheckBox";
			this.controlResource.SetResourceKey(this.scanFilesCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SCANFILES");
			this.scanFilesCheckBox.Size = new System.Drawing.Size(105, 17);
			this.scanFilesCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.scanFilesCheckBox.TabIndex = 7;
			this.scanFilesCheckBox.Values.Text = "Scan added files";
			// 
			// useDatabaseCheckBox
			// 
			this.useDatabaseCheckBox.Location = new System.Drawing.Point(400, 26);
			this.useDatabaseCheckBox.Name = "useDatabaseCheckBox";
			this.controlResource.SetResourceKey(this.useDatabaseCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_USEDATABASE");
			this.useDatabaseCheckBox.Size = new System.Drawing.Size(173, 30);
			this.useDatabaseCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.useDatabaseCheckBox.TabIndex = 8;
			this.useDatabaseCheckBox.Values.Text = "Use database to store module\r\ninformation";
			// 
			// loadingGroupBox
			// 
			this.loadingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.loadingGroupBox.Location = new System.Drawing.Point(8, 160);
			this.loadingGroupBox.Name = "loadingGroupBox";
			// 
			// 
			// 
			this.loadingGroupBox.Panel.Controls.Add(this.doubleBufferingCheckBox);
			this.loadingGroupBox.Panel.Controls.Add(this.doubleBufferingPanel);
			this.loadingGroupBox.Panel.Controls.Add(this.moduleErrorLabel);
			this.loadingGroupBox.Panel.Controls.Add(this.moduleErrorComboBox);
			this.controlResource.SetResourceKey(this.loadingGroupBox, "IDS_SETTINGS_OPTIONS_LOADING");
			this.loadingGroupBox.Size = new System.Drawing.Size(592, 106);
			this.loadingGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.loadingGroupBox.TabIndex = 1;
			this.loadingGroupBox.Values.Heading = "Loading";
			// 
			// doubleBufferingCheckBox
			// 
			this.doubleBufferingCheckBox.Location = new System.Drawing.Point(4, 5);
			this.doubleBufferingCheckBox.Name = "doubleBufferingCheckBox";
			this.controlResource.SetResourceKey(this.doubleBufferingCheckBox, "IDS_SETTINGS_OPTIONS_LOADING_DOUBLEBUFFERING");
			this.doubleBufferingCheckBox.Size = new System.Drawing.Size(107, 17);
			this.doubleBufferingCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.doubleBufferingCheckBox.TabIndex = 0;
			this.doubleBufferingCheckBox.Values.Text = "Double buffering";
			this.doubleBufferingCheckBox.CheckedChanged += new System.EventHandler(this.DoubleBufferingCheckBox_CheckedChanged);
			// 
			// doubleBufferingPanel
			// 
			this.doubleBufferingPanel.BackColor = System.Drawing.Color.Transparent;
			this.doubleBufferingPanel.Controls.Add(this.earlyLoadLabel);
			this.doubleBufferingPanel.Controls.Add(this.doubleBufferingTrackBar);
			this.doubleBufferingPanel.Enabled = false;
			this.doubleBufferingPanel.Location = new System.Drawing.Point(8, 26);
			this.doubleBufferingPanel.Name = "doubleBufferingPanel";
			this.controlResource.SetResourceKey(this.doubleBufferingPanel, null);
			this.doubleBufferingPanel.Size = new System.Drawing.Size(574, 30);
			this.doubleBufferingPanel.TabIndex = 1;
			// 
			// earlyLoadLabel
			// 
			this.earlyLoadLabel.Location = new System.Drawing.Point(494, 5);
			this.earlyLoadLabel.Name = "earlyLoadLabel";
			this.controlResource.SetResourceKey(this.earlyLoadLabel, "IDS_SETTINGS_OPTIONS_LOADING_EARLYLOAD");
			this.earlyLoadLabel.Size = new System.Drawing.Size(59, 17);
			this.earlyLoadLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.earlyLoadLabel.TabIndex = 1;
			this.earlyLoadLabel.Values.Text = "Early load";
			// 
			// doubleBufferingTrackBar
			// 
			this.doubleBufferingTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			this.doubleBufferingTrackBar.DrawBackground = true;
			this.doubleBufferingTrackBar.Location = new System.Drawing.Point(0, 0);
			this.doubleBufferingTrackBar.Maximum = 8;
			this.doubleBufferingTrackBar.Name = "doubleBufferingTrackBar";
			this.controlResource.SetResourceKey(this.doubleBufferingTrackBar, null);
			this.doubleBufferingTrackBar.Size = new System.Drawing.Size(490, 27);
			this.doubleBufferingTrackBar.TabIndex = 0;
			// 
			// moduleErrorLabel
			// 
			this.moduleErrorLabel.Location = new System.Drawing.Point(4, 62);
			this.moduleErrorLabel.Name = "moduleErrorLabel";
			this.controlResource.SetResourceKey(this.moduleErrorLabel, "IDS_SETTINGS_OPTIONS_LOADING_MODULEERROR");
			this.moduleErrorLabel.Size = new System.Drawing.Size(171, 17);
			this.moduleErrorLabel.StateNormal.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleErrorLabel.TabIndex = 2;
			this.moduleErrorLabel.Values.Text = "When a module error is reached";
			// 
			// moduleErrorComboBox
			// 
			this.moduleErrorComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.moduleErrorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.moduleErrorComboBox.DropDownWidth = 121;
			this.moduleErrorComboBox.IntegralHeight = false;
			this.moduleErrorComboBox.Location = new System.Drawing.Point(180, 60);
			this.moduleErrorComboBox.Name = "moduleErrorComboBox";
			this.controlResource.SetResourceKey(this.moduleErrorComboBox, null);
			this.moduleErrorComboBox.Size = new System.Drawing.Size(180, 19);
			this.moduleErrorComboBox.StateCommon.ComboBox.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleErrorComboBox.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
			this.moduleErrorComboBox.StateCommon.Item.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleErrorComboBox.TabIndex = 3;
			// 
			// playingGroupBox
			// 
			this.playingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.playingGroupBox.Location = new System.Drawing.Point(8, 289);
			this.playingGroupBox.Name = "playingGroupBox";
			// 
			// 
			// 
			this.playingGroupBox.Panel.Controls.Add(this.moduleListEndComboBox);
			this.playingGroupBox.Panel.Controls.Add(this.moduleListEndLabel);
			this.controlResource.SetResourceKey(this.playingGroupBox, "IDS_SETTINGS_OPTIONS_PLAYING");
			this.playingGroupBox.Size = new System.Drawing.Size(592, 51);
			this.playingGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.playingGroupBox.TabIndex = 2;
			this.playingGroupBox.Values.Heading = "Playing";
			// 
			// moduleListEndComboBox
			// 
			this.moduleListEndComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.moduleListEndComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.moduleListEndComboBox.DropDownWidth = 121;
			this.moduleListEndComboBox.IntegralHeight = false;
			this.moduleListEndComboBox.Location = new System.Drawing.Point(124, 5);
			this.moduleListEndComboBox.Name = "moduleListEndComboBox";
			this.controlResource.SetResourceKey(this.moduleListEndComboBox, null);
			this.moduleListEndComboBox.Size = new System.Drawing.Size(100, 19);
			this.moduleListEndComboBox.StateCommon.ComboBox.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleListEndComboBox.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
			this.moduleListEndComboBox.StateCommon.Item.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleListEndComboBox.TabIndex = 1;
			// 
			// moduleListEndLabel
			// 
			this.moduleListEndLabel.Location = new System.Drawing.Point(4, 7);
			this.moduleListEndLabel.Name = "moduleListEndLabel";
			this.controlResource.SetResourceKey(this.moduleListEndLabel, "IDS_SETTINGS_OPTIONS_PLAYING_MODULELISTEND");
			this.moduleListEndLabel.Size = new System.Drawing.Size(114, 17);
			this.moduleListEndLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleListEndLabel.TabIndex = 0;
			this.moduleListEndLabel.Values.Text = "At end of module list";
			// 
			// OptionsPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.playingGroupBox);
			this.Controls.Add(this.loadingGroupBox);
			this.Controls.Add(this.generalGroupBox);
			this.Name = "OptionsPageControl";
			this.controlResource.SetResourceKey(this, null);
			this.Size = new System.Drawing.Size(608, 348);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox.Panel)).EndInit();
			this.generalGroupBox.Panel.ResumeLayout(false);
			this.generalGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox)).EndInit();
			this.rememberListPanel.ResumeLayout(false);
			this.rememberListPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.loadingGroupBox.Panel)).EndInit();
			this.loadingGroupBox.Panel.ResumeLayout(false);
			this.loadingGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.loadingGroupBox)).EndInit();
			this.doubleBufferingPanel.ResumeLayout(false);
			this.doubleBufferingPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.moduleErrorComboBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.playingGroupBox.Panel)).EndInit();
			this.playingGroupBox.Panel.ResumeLayout(false);
			this.playingGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.playingGroupBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.moduleListEndComboBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonGroupBox generalGroupBox;
		private Krypton.Toolkit.KryptonCheckBox addJumpCheckBox;
		private Krypton.Toolkit.KryptonCheckBox addToListCheckBox;
		private Krypton.Toolkit.KryptonCheckBox rememberListCheckBox;
		private System.Windows.Forms.Panel rememberListPanel;
		private Krypton.Toolkit.KryptonCheckBox rememberModulePositionCheckBox;
		private Krypton.Toolkit.KryptonCheckBox rememberListPositionCheckBox;
		private Krypton.Toolkit.KryptonCheckBox tooltipsCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showNameInTitleCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showListNumberCheckBox;
		private Krypton.Toolkit.KryptonCheckBox scanFilesCheckBox;
		private Krypton.Toolkit.KryptonCheckBox useDatabaseCheckBox;
		private Krypton.Toolkit.KryptonGroupBox loadingGroupBox;
		private Krypton.Toolkit.KryptonCheckBox doubleBufferingCheckBox;
		private System.Windows.Forms.Panel doubleBufferingPanel;
		private Krypton.Toolkit.KryptonLabel earlyLoadLabel;
		private Krypton.Toolkit.KryptonTrackBar doubleBufferingTrackBar;
		private Krypton.Toolkit.KryptonComboBox moduleErrorComboBox;
		private Krypton.Toolkit.KryptonLabel moduleErrorLabel;
		private Krypton.Toolkit.KryptonGroupBox playingGroupBox;
		private Krypton.Toolkit.KryptonLabel moduleListEndLabel;
		private Krypton.Toolkit.KryptonComboBox moduleListEndComboBox;
	}
}
