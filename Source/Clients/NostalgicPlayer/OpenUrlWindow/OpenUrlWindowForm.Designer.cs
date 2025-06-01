namespace Polycode.NostalgicPlayer.Client.GuiPlayer.OpenUrlWindow
{
	partial class OpenUrlWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenUrlWindowForm));
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			nameLabel = new Krypton.Toolkit.KryptonLabel();
			nameTextBox = new Krypton.Toolkit.KryptonTextBox();
			cancelButton = new Krypton.Toolkit.KryptonButton();
			playButton = new Krypton.Toolkit.KryptonButton();
			addButton = new Krypton.Toolkit.KryptonButton();
			urlTextBox = new Krypton.Toolkit.KryptonTextBox();
			urlLabel = new Krypton.Toolkit.KryptonLabel();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
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
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// nameLabel
			// 
			nameLabel.Location = new System.Drawing.Point(8, 8);
			nameLabel.Name = "nameLabel";
			nameLabel.Palette = fontPalette;
			nameLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(nameLabel, "IDS_OPENURL_NAME");
			nameLabel.Size = new System.Drawing.Size(232, 16);
			nameLabel.TabIndex = 0;
			nameLabel.Values.Text = "Enter the name to be shown in the module list";
			// 
			// nameTextBox
			// 
			nameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nameTextBox.Location = new System.Drawing.Point(8, 24);
			nameTextBox.Name = "nameTextBox";
			nameTextBox.Palette = fontPalette;
			nameTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(nameTextBox, null);
			nameTextBox.Size = new System.Drawing.Size(342, 20);
			nameTextBox.TabIndex = 1;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancelButton.Location = new System.Drawing.Point(260, 100);
			cancelButton.Name = "cancelButton";
			controlResource.SetResourceKey(cancelButton, "IDS_OPENURL_BUTTON_CANCEL");
			cancelButton.Size = new System.Drawing.Size(90, 25);
			cancelButton.TabIndex = 6;
			cancelButton.Values.Text = "Cancel";
			// 
			// playButton
			// 
			playButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			playButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			playButton.Location = new System.Drawing.Point(164, 100);
			playButton.Name = "playButton";
			controlResource.SetResourceKey(playButton, "IDS_OPENURL_BUTTON_PLAY");
			playButton.Size = new System.Drawing.Size(90, 25);
			playButton.TabIndex = 5;
			playButton.Values.Text = "Play";
			// 
			// addButton
			// 
			addButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			addButton.DialogResult = System.Windows.Forms.DialogResult.Continue;
			addButton.Location = new System.Drawing.Point(68, 100);
			addButton.Name = "addButton";
			controlResource.SetResourceKey(addButton, "IDS_OPENURL_BUTTON_ADD");
			addButton.Size = new System.Drawing.Size(90, 25);
			addButton.TabIndex = 4;
			addButton.Values.Text = "Add";
			// 
			// urlTextBox
			// 
			urlTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			urlTextBox.Location = new System.Drawing.Point(8, 72);
			urlTextBox.Name = "urlTextBox";
			urlTextBox.Palette = fontPalette;
			urlTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(urlTextBox, null);
			urlTextBox.Size = new System.Drawing.Size(342, 20);
			urlTextBox.TabIndex = 3;
			// 
			// urlLabel
			// 
			urlLabel.Location = new System.Drawing.Point(8, 56);
			urlLabel.Name = "urlLabel";
			urlLabel.Palette = fontPalette;
			urlLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(urlLabel, "IDS_OPENURL_URL");
			urlLabel.Size = new System.Drawing.Size(243, 16);
			urlLabel.TabIndex = 2;
			urlLabel.Values.Text = "Enter an URL to the stream you want to listen to";
			// 
			// OpenUrlWindowForm
			// 
			AcceptButton = addButton;
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			CancelButton = cancelButton;
			ClientSize = new System.Drawing.Size(362, 137);
			Controls.Add(urlTextBox);
			Controls.Add(urlLabel);
			Controls.Add(addButton);
			Controls.Add(playButton);
			Controls.Add(cancelButton);
			Controls.Add(nameTextBox);
			Controls.Add(nameLabel);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "OpenUrlWindowForm";
			Palette = fontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(this, null);
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private GuiKit.Components.FontPalette fontPalette;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonLabel nameLabel;
		private Krypton.Toolkit.KryptonTextBox nameTextBox;
		private Krypton.Toolkit.KryptonButton cancelButton;
		private Krypton.Toolkit.KryptonButton playButton;
		private Krypton.Toolkit.KryptonButton addButton;
		private Krypton.Toolkit.KryptonTextBox urlTextBox;
		private Krypton.Toolkit.KryptonLabel urlLabel;
	}
}