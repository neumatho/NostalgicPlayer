
namespace Polycode.NostalgicPlayer.Agent.Output.CoreAudioSettings
{
	partial class SettingsControl
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
			this.deviceLabel = new Krypton.Toolkit.KryptonLabel();
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.deviceComboBox = new Krypton.Toolkit.KryptonComboBox();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.deviceComboBox)).BeginInit();
			this.SuspendLayout();
			// 
			// kryptonManager
			// 
			this.kryptonManager.GlobalPaletteMode = Krypton.Toolkit.PaletteModeManager.Office2010Blue;
			// 
			// deviceLabel
			// 
			this.deviceLabel.Location = new System.Drawing.Point(0, 22);
			this.deviceLabel.Name = "deviceLabel";
			this.controlResource.SetResourceKey(this.deviceLabel, "IDS_SETTINGS_DEVICE");
			this.deviceLabel.Size = new System.Drawing.Size(80, 17);
			this.deviceLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.deviceLabel.TabIndex = 0;
			this.deviceLabel.Values.Text = "Output device";
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Agent.Output.CoreAudioSettings.Resources";
			// 
			// deviceComboBox
			// 
			this.deviceComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.deviceComboBox.DropDownWidth = 305;
			this.deviceComboBox.IntegralHeight = false;
			this.deviceComboBox.Location = new System.Drawing.Point(87, 21);
			this.deviceComboBox.Name = "deviceComboBox";
			this.controlResource.SetResourceKey(this.deviceComboBox, null);
			this.deviceComboBox.Size = new System.Drawing.Size(305, 19);
			this.deviceComboBox.StateCommon.ComboBox.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.deviceComboBox.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
			this.deviceComboBox.StateCommon.Item.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.deviceComboBox.TabIndex = 1;
			// 
			// SettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.deviceComboBox);
			this.Controls.Add(this.deviceLabel);
			this.MinimumSize = new System.Drawing.Size(400, 62);
			this.Name = "SettingsControl";
			this.controlResource.SetResourceKey(this, null);
			this.Size = new System.Drawing.Size(400, 62);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.deviceComboBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private Krypton.Toolkit.KryptonLabel deviceLabel;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonComboBox deviceComboBox;
	}
}
