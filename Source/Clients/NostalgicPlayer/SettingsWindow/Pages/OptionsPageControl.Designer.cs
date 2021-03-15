
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
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox.Panel)).BeginInit();
			this.generalGroupBox.Panel.SuspendLayout();
			this.rememberListPanel.SuspendLayout();
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
			this.controlResource.SetResourceKey(this.addJumpCheckBox, "IDS_SETTINGS_OPTIONS_ADDJUMP");
			this.addJumpCheckBox.Size = new System.Drawing.Size(138, 17);
			this.addJumpCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.addJumpCheckBox.TabIndex = 0;
			this.addJumpCheckBox.Values.Text = "Jump to added module";
			// 
			// addToListCheckBox
			// 
			this.addToListCheckBox.Location = new System.Drawing.Point(4, 26);
			this.addToListCheckBox.Name = "addToListCheckBox";
			this.controlResource.SetResourceKey(this.addToListCheckBox, "IDS_SETTINGS_OPTIONS_ADDTOLIST");
			this.addToListCheckBox.Size = new System.Drawing.Size(126, 17);
			this.addToListCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.addToListCheckBox.TabIndex = 1;
			this.addToListCheckBox.Values.Text = "Add to list as default";
			// 
			// rememberListCheckBox
			// 
			this.rememberListCheckBox.Location = new System.Drawing.Point(4, 47);
			this.rememberListCheckBox.Name = "rememberListCheckBox";
			this.controlResource.SetResourceKey(this.rememberListCheckBox, "IDS_SETTINGS_OPTIONS_REMEMBERLIST");
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
			this.controlResource.SetResourceKey(this.rememberModulePositionCheckBox, "IDS_SETTINGS_OPTIONS_REMEMBERMODULEPOSITION");
			this.rememberModulePositionCheckBox.Size = new System.Drawing.Size(160, 17);
			this.rememberModulePositionCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.rememberModulePositionCheckBox.TabIndex = 1;
			this.rememberModulePositionCheckBox.Values.Text = "Remember module position";
			// 
			// rememberListPositionCheckBox
			// 
			this.rememberListPositionCheckBox.Location = new System.Drawing.Point(0, 0);
			this.rememberListPositionCheckBox.Name = "rememberListPositionCheckBox";
			this.controlResource.SetResourceKey(this.rememberListPositionCheckBox, "IDS_SETTINGS_OPTIONS_REMEMBERLISTPOSITION");
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
			this.controlResource.SetResourceKey(this.tooltipsCheckBox, "IDS_SETTINGS_OPTIONS_TOOLTIPS");
			this.tooltipsCheckBox.Size = new System.Drawing.Size(100, 17);
			this.tooltipsCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.tooltipsCheckBox.TabIndex = 4;
			this.tooltipsCheckBox.Values.Text = "Button tool tips";
			// 
			// showNameInTitleCheckBox
			// 
			this.showNameInTitleCheckBox.Location = new System.Drawing.Point(200, 26);
			this.showNameInTitleCheckBox.Name = "showNameInTitleCheckBox";
			this.controlResource.SetResourceKey(this.showNameInTitleCheckBox, "IDS_SETTINGS_OPTIONS_SHOWNAME");
			this.showNameInTitleCheckBox.Size = new System.Drawing.Size(173, 17);
			this.showNameInTitleCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.showNameInTitleCheckBox.TabIndex = 5;
			this.showNameInTitleCheckBox.Values.Text = "Show module name in titlebar";
			// 
			// showListNumberCheckBox
			// 
			this.showListNumberCheckBox.Location = new System.Drawing.Point(200, 47);
			this.showListNumberCheckBox.Name = "showListNumberCheckBox";
			this.controlResource.SetResourceKey(this.showListNumberCheckBox, "IDS_SETTINGS_OPTIONS_SHOWLISTNUMBER");
			this.showListNumberCheckBox.Size = new System.Drawing.Size(148, 17);
			this.showListNumberCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.showListNumberCheckBox.TabIndex = 6;
			this.showListNumberCheckBox.Values.Text = "Show item number in list";
			// 
			// OptionsPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
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
	}
}
