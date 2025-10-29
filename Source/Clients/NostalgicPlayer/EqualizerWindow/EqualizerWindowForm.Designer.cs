namespace Polycode.NostalgicPlayer.Client.GuiPlayer.EqualizerWindow
{
	partial class EqualizerWindowForm
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
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			equalizerPanel = new System.Windows.Forms.Panel();
			SuspendLayout();

			//
			// fontPalette
			//
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;

			//
			// equalizerPanel
			//
			equalizerPanel.BackColor = System.Drawing.Color.White;
			equalizerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			equalizerPanel.Location = new System.Drawing.Point(8, 8);
			equalizerPanel.Name = "equalizerPanel";
			equalizerPanel.Size = new System.Drawing.Size(487, 218);
			equalizerPanel.TabIndex = 0;

			//
			// EqualizerWindowForm
			//
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.White;
			ClientSize = new System.Drawing.Size(503, 226);
			Controls.Add(equalizerPanel);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "EqualizerWindowForm";
			Padding = new System.Windows.Forms.Padding(8, 0, 8, 8);
			Palette = fontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Equalizer";
			FormClosed += EqualizerWindowForm_FormClosed;
			Shown += EqualizerWindowForm_Shown;
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette fontPalette;
		private System.Windows.Forms.Panel equalizerPanel;
	}
}
