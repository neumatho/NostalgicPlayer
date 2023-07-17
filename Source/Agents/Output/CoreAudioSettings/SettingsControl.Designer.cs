
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
			fontPalette = new GuiKit.Components.FontPalette(components);
			controlResource = new GuiKit.Designer.ControlResource();
			deviceComboBox = new Krypton.Toolkit.KryptonComboBox();
			latencyTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			latencyLabel = new Krypton.Toolkit.KryptonLabel();
			latencyMsLabel = new Krypton.Toolkit.KryptonLabel();
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
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Agent.Output.CoreAudioSettings.Resources";
			// 
			// deviceComboBox
			// 
			deviceComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			deviceComboBox.CornerRoundingRadius = -1F;
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
			// latencyTrackBar
			// 
			latencyTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			latencyTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			latencyTrackBar.Location = new System.Drawing.Point(89, 34);
			latencyTrackBar.Maximum = 9;
			latencyTrackBar.Name = "latencyTrackBar";
			latencyTrackBar.Palette = fontPalette;
			latencyTrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(latencyTrackBar, null);
			latencyTrackBar.Size = new System.Drawing.Size(249, 27);
			latencyTrackBar.TabIndex = 3;
			latencyTrackBar.ValueChanged += LatencyTrackBar_ValueChanged;
			// 
			// latencyLabel
			// 
			latencyLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			latencyLabel.Location = new System.Drawing.Point(0, 39);
			latencyLabel.Name = "latencyLabel";
			latencyLabel.Palette = fontPalette;
			latencyLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(latencyLabel, "IDS_SETTINGS_LATENCY");
			latencyLabel.Size = new System.Drawing.Size(49, 16);
			latencyLabel.TabIndex = 2;
			latencyLabel.Values.Text = "Latency";
			// 
			// latencyMsLabel
			// 
			latencyMsLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			latencyMsLabel.Location = new System.Drawing.Point(346, 39);
			latencyMsLabel.Name = "latencyMsLabel";
			latencyMsLabel.Palette = fontPalette;
			latencyMsLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(latencyMsLabel, null);
			latencyMsLabel.Size = new System.Drawing.Size(40, 16);
			latencyMsLabel.TabIndex = 4;
			latencyMsLabel.Values.Text = "20 ms";
			// 
			// SettingsControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(latencyMsLabel);
			Controls.Add(latencyLabel);
			Controls.Add(latencyTrackBar);
			Controls.Add(deviceComboBox);
			Controls.Add(deviceLabel);
			MinimumSize = new System.Drawing.Size(400, 70);
			Name = "SettingsControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(400, 70);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)deviceComboBox).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private Krypton.Toolkit.KryptonLabel deviceLabel;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonComboBox deviceComboBox;
		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonTrackBar latencyTrackBar;
		private Krypton.Toolkit.KryptonLabel latencyLabel;
		private Krypton.Toolkit.KryptonLabel latencyMsLabel;
	}
}
