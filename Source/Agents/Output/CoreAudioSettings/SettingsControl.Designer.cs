
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
			components = new System.ComponentModel.Container();
			deviceLabel = new Krypton.Toolkit.KryptonLabel();
			fontPalette = new Kit.Gui.Components.FontPalette(components);
			controlResource = new Kit.Gui.Designer.ControlResource();
			deviceComboBox = new Krypton.Toolkit.KryptonComboBox();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)deviceComboBox).BeginInit();
			SuspendLayout();
			// 
			// deviceLabel
			// 
			deviceLabel.Location = new System.Drawing.Point(0, 9);
			deviceLabel.Name = "deviceLabel";
			deviceLabel.Palette = fontPalette;
			deviceLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(deviceLabel, "IDS_SETTINGS_DEVICE");
			deviceLabel.Size = new System.Drawing.Size(78, 16);
			deviceLabel.TabIndex = 0;
			deviceLabel.Values.Text = "Output device";
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Agent.Output.CoreAudioSettings.Resources";
			// 
			// deviceComboBox
			// 
			deviceComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			deviceComboBox.DropDownWidth = 305;
			deviceComboBox.IntegralHeight = false;
			deviceComboBox.Location = new System.Drawing.Point(87, 8);
			deviceComboBox.Name = "deviceComboBox";
			deviceComboBox.Palette = fontPalette;
			deviceComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(deviceComboBox, null);
			deviceComboBox.Size = new System.Drawing.Size(305, 18);
			deviceComboBox.TabIndex = 1;
			// 
			// SettingsControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(deviceComboBox);
			Controls.Add(deviceLabel);
			MinimumSize = new System.Drawing.Size(400, 34);
			Name = "SettingsControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(400, 34);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)deviceComboBox).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private Krypton.Toolkit.KryptonLabel deviceLabel;
		private Kit.Gui.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonComboBox deviceComboBox;
		private Kit.Gui.Components.FontPalette fontPalette;
	}
}
