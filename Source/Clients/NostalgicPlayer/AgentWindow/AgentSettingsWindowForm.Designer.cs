
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AgentWindow
{
	partial class AgentSettingsWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AgentSettingsWindowForm));
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.settingsGroup = new Krypton.Toolkit.KryptonGroup();
			this.okButton = new Krypton.Toolkit.KryptonButton();
			this.cancelButton = new Krypton.Toolkit.KryptonButton();
			this.applyButton = new Krypton.Toolkit.KryptonButton();
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)(this.settingsGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.settingsGroup.Panel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			this.SuspendLayout();
			// 
			// settingsGroup
			// 
			this.settingsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.settingsGroup.Location = new System.Drawing.Point(8, 8);
			this.settingsGroup.Name = "settingsGroup";
			this.controlResource.SetResourceKey(this.settingsGroup, null);
			this.settingsGroup.Size = new System.Drawing.Size(286, 62);
			this.settingsGroup.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(8, 78);
			this.okButton.Name = "okButton";
			this.controlResource.SetResourceKey(this.okButton, "IDS_SETTINGS_OK");
			this.okButton.Size = new System.Drawing.Size(90, 25);
			this.okButton.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.okButton.TabIndex = 1;
			this.okButton.Values.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.Location = new System.Drawing.Point(106, 78);
			this.cancelButton.Name = "cancelButton";
			this.controlResource.SetResourceKey(this.cancelButton, "IDS_SETTINGS_CANCEL");
			this.cancelButton.Size = new System.Drawing.Size(90, 25);
			this.cancelButton.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Values.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// applyButton
			// 
			this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.applyButton.Location = new System.Drawing.Point(204, 78);
			this.applyButton.Name = "applyButton";
			this.controlResource.SetResourceKey(this.applyButton, "IDS_SETTINGS_APPLY");
			this.applyButton.Size = new System.Drawing.Size(90, 25);
			this.applyButton.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.applyButton.TabIndex = 3;
			this.applyButton.Values.Text = "Apply";
			this.applyButton.Click += new System.EventHandler(this.ApplyButton_Click);
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// AgentSettingsWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(302, 111);
			this.Controls.Add(this.applyButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.settingsGroup);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(318, 150);
			this.Name = "AgentSettingsWindowForm";
			this.controlResource.SetResourceKey(this, null);
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AgentSettingsWindowForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.settingsGroup.Panel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.settingsGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private Krypton.Toolkit.KryptonGroup settingsGroup;
		private Krypton.Toolkit.KryptonButton okButton;
		private Krypton.Toolkit.KryptonButton cancelButton;
		private Krypton.Toolkit.KryptonButton applyButton;
		private Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource controlResource;
	}
}