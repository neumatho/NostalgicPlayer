
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.NewVersionWindow
{
	partial class NewVersionWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewVersionWindowForm));
			this.controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			this.label = new Krypton.Toolkit.KryptonLabel();
			this.bigFontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(this.components);
			this.kryptonButton1 = new Krypton.Toolkit.KryptonButton();
			this.historyRichTextBox = new Krypton.Toolkit.KryptonRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			this.SuspendLayout();
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// label
			// 
			this.label.Location = new System.Drawing.Point(8, 8);
			this.label.Name = "label";
			this.label.Palette = this.bigFontPalette;
			this.label.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.label, "IDS_NEWVERSION_MESSAGE");
			this.label.Size = new System.Drawing.Size(451, 31);
			this.label.TabIndex = 0;
			this.label.Values.Text = "Congratulations! A new version of NostalgicPlayer has been installed. See below\r\n" +
    "what has changed since your previous version.";
			// 
			// bigFontPalette
			// 
			this.bigFontPalette.BaseFontSize = 9F;
			// 
			// kryptonButton1
			// 
			this.kryptonButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.kryptonButton1.Location = new System.Drawing.Point(369, 255);
			this.kryptonButton1.Name = "kryptonButton1";
			this.kryptonButton1.Palette = this.bigFontPalette;
			this.kryptonButton1.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.kryptonButton1, "IDS_BUT_OK");
			this.kryptonButton1.Size = new System.Drawing.Size(90, 25);
			this.kryptonButton1.TabIndex = 2;
			this.kryptonButton1.Values.Text = "Ok";
			// 
			// historyRichTextBox
			// 
			this.historyRichTextBox.DetectUrls = false;
			this.historyRichTextBox.Location = new System.Drawing.Point(8, 47);
			this.historyRichTextBox.Name = "historyRichTextBox";
			this.historyRichTextBox.ReadOnly = true;
			this.controlResource.SetResourceKey(this.historyRichTextBox, null);
			this.historyRichTextBox.Size = new System.Drawing.Size(451, 200);
			this.historyRichTextBox.TabIndex = 3;
			this.historyRichTextBox.Text = "";
			// 
			// NewVersionWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(467, 288);
			this.Controls.Add(this.historyRichTextBox);
			this.Controls.Add(this.kryptonButton1);
			this.Controls.Add(this.label);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NewVersionWindowForm";
			this.Palette = this.bigFontPalette;
			this.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this, null);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Kit.Gui.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonLabel label;
		private Krypton.Toolkit.KryptonButton kryptonButton1;
		private Krypton.Toolkit.KryptonRichTextBox historyRichTextBox;
		private Kit.Gui.Components.FontPalette bigFontPalette;
	}
}