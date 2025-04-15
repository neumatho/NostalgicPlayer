
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AgentSettingsWindowForm));
			settingsGroup = new Krypton.Toolkit.KryptonGroup();
			okButton = new Krypton.Toolkit.KryptonButton();
			bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			cancelButton = new Krypton.Toolkit.KryptonButton();
			applyButton = new Krypton.Toolkit.KryptonButton();
			controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)settingsGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)settingsGroup.Panel).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
			// 
			// settingsGroup
			// 
			settingsGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			settingsGroup.Location = new System.Drawing.Point(8, 8);
			settingsGroup.Name = "settingsGroup";
			controlResource.SetResourceKey(settingsGroup, null);
			settingsGroup.Size = new System.Drawing.Size(286, 62);
			settingsGroup.TabIndex = 0;
			// 
			// okButton
			// 
			okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			okButton.Location = new System.Drawing.Point(8, 78);
			okButton.Name = "okButton";
			okButton.Palette = bigFontPalette;
			okButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(okButton, "IDS_SETTINGS_BUTTON_OK");
			okButton.Size = new System.Drawing.Size(90, 25);
			okButton.TabIndex = 1;
			okButton.Values.Text = "";
			okButton.Click += OkButton_Click;
			// 
			// bigFontPalette
			// 
			bigFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigFontPalette.BaseFontSize = 9F;
			bigFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			bigFontPalette.ThemeName = "";
			bigFontPalette.UseKryptonFileDialogs = true;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			cancelButton.Location = new System.Drawing.Point(106, 78);
			cancelButton.Name = "cancelButton";
			cancelButton.Palette = bigFontPalette;
			cancelButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(cancelButton, "IDS_SETTINGS_BUTTON_CANCEL");
			cancelButton.Size = new System.Drawing.Size(90, 25);
			cancelButton.TabIndex = 2;
			cancelButton.Values.Text = "";
			cancelButton.Click += CancelButton_Click;
			// 
			// applyButton
			// 
			applyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			applyButton.Location = new System.Drawing.Point(204, 78);
			applyButton.Name = "applyButton";
			applyButton.LocalCustomPalette = bigFontPalette;
			applyButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(applyButton, "IDS_SETTINGS_BUTTON_APPLY");
			applyButton.Size = new System.Drawing.Size(90, 25);
			applyButton.TabIndex = 3;
			applyButton.Values.Text = "";
			applyButton.Click += ApplyButton_Click;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// AgentSettingsWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(302, 111);
			Controls.Add(applyButton);
			Controls.Add(cancelButton);
			Controls.Add(okButton);
			Controls.Add(settingsGroup);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(318, 150);
			Name = "AgentSettingsWindowForm";
			Palette = bigFontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(this, null);
			FormClosed += AgentSettingsWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)settingsGroup.Panel).EndInit();
			((System.ComponentModel.ISupportInitialize)settingsGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);

		}

		#endregion
		private Krypton.Toolkit.KryptonGroup settingsGroup;
		private Krypton.Toolkit.KryptonButton okButton;
		private Krypton.Toolkit.KryptonButton cancelButton;
		private Krypton.Toolkit.KryptonButton applyButton;
		private Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource controlResource;
		private GuiKit.Components.FontPalette bigFontPalette;
	}
}