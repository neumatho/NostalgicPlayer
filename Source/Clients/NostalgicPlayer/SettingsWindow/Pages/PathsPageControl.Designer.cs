
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class PathsPageControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PathsPageControl));
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			group = new Krypton.Toolkit.KryptonGroup();
			startScanButton = new Krypton.Toolkit.KryptonButton();
			startScanTextBox = new Krypton.Toolkit.KryptonTextBox();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			startScanLabel = new Krypton.Toolkit.KryptonLabel();
			moduleButton = new Krypton.Toolkit.KryptonButton();
			moduleTextBox = new Krypton.Toolkit.KryptonTextBox();
			moduleLabel = new Krypton.Toolkit.KryptonLabel();
			listButton = new Krypton.Toolkit.KryptonButton();
			listTextBox = new Krypton.Toolkit.KryptonTextBox();
			listLabel = new Krypton.Toolkit.KryptonLabel();
			modLibraryButton = new Krypton.Toolkit.KryptonButton();
			modLibraryTextBox = new Krypton.Toolkit.KryptonTextBox();
			modLibraryLabel = new Krypton.Toolkit.KryptonLabel();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)group).BeginInit();
			((System.ComponentModel.ISupportInitialize)group.Panel).BeginInit();
			group.Panel.SuspendLayout();
			SuspendLayout();
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// group
			// 
			group.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			group.Location = new System.Drawing.Point(8, 8);
			group.Name = "group";
			// 
			// 
			// 
			group.Panel.Controls.Add(startScanButton);
			group.Panel.Controls.Add(startScanTextBox);
			group.Panel.Controls.Add(startScanLabel);
			group.Panel.Controls.Add(moduleButton);
			group.Panel.Controls.Add(moduleTextBox);
			group.Panel.Controls.Add(moduleLabel);
			group.Panel.Controls.Add(listButton);
			group.Panel.Controls.Add(listTextBox);
			group.Panel.Controls.Add(listLabel);
			group.Panel.Controls.Add(modLibraryButton);
			group.Panel.Controls.Add(modLibraryTextBox);
			group.Panel.Controls.Add(modLibraryLabel);
			group.Size = new System.Drawing.Size(592, 340);
			group.TabIndex = 0;
			// 
			// startScanButton
			// 
			startScanButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			startScanButton.Location = new System.Drawing.Point(556, 37);
			startScanButton.Name = "startScanButton";
			startScanButton.Size = new System.Drawing.Size(22, 22);
			startScanButton.TabIndex = 2;
			startScanButton.Values.Image = (System.Drawing.Image)resources.GetObject("startScanButton.Values.Image");
			startScanButton.Values.Text = "";
			startScanButton.Click += StartScanButton_Click;
			// 
			// startScanTextBox
			// 
			startScanTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			startScanTextBox.Location = new System.Drawing.Point(100, 38);
			startScanTextBox.Name = "startScanTextBox";
			startScanTextBox.Palette = fontPalette;
			startScanTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			startScanTextBox.Size = new System.Drawing.Size(454, 20);
			startScanTextBox.TabIndex = 1;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// startScanLabel
			// 
			startScanLabel.Location = new System.Drawing.Point(4, 40);
			startScanLabel.Name = "startScanLabel";
			startScanLabel.Palette = fontPalette;
			startScanLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(startScanLabel, "IDS_SETTINGS_PATHS_STARTSCAN");
			startScanLabel.Size = new System.Drawing.Size(84, 16);
			startScanLabel.TabIndex = 0;
			startScanLabel.Values.Text = "Start scan path";
			// 
			// moduleButton
			// 
			moduleButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			moduleButton.Location = new System.Drawing.Point(556, 69);
			moduleButton.Name = "moduleButton";
			moduleButton.Size = new System.Drawing.Size(22, 22);
			moduleButton.TabIndex = 5;
			moduleButton.Values.Image = (System.Drawing.Image)resources.GetObject("moduleButton.Values.Image");
			moduleButton.Values.Text = "";
			moduleButton.Click += ModuleButton_Click;
			// 
			// moduleTextBox
			// 
			moduleTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			moduleTextBox.Location = new System.Drawing.Point(100, 70);
			moduleTextBox.Name = "moduleTextBox";
			moduleTextBox.Palette = fontPalette;
			moduleTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			moduleTextBox.Size = new System.Drawing.Size(454, 20);
			moduleTextBox.TabIndex = 4;
			// 
			// moduleLabel
			// 
			moduleLabel.Location = new System.Drawing.Point(4, 72);
			moduleLabel.Name = "moduleLabel";
			moduleLabel.Palette = fontPalette;
			moduleLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleLabel, "IDS_SETTINGS_PATHS_MODULE");
			moduleLabel.Size = new System.Drawing.Size(70, 16);
			moduleLabel.TabIndex = 3;
			moduleLabel.Values.Text = "Module path";
			// 
			// listButton
			// 
			listButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			listButton.Location = new System.Drawing.Point(556, 101);
			listButton.Name = "listButton";
			listButton.Size = new System.Drawing.Size(22, 22);
			listButton.TabIndex = 8;
			listButton.Values.Image = (System.Drawing.Image)resources.GetObject("listButton.Values.Image");
			listButton.Values.Text = "";
			listButton.Click += ListButton_Click;
			// 
			// listTextBox
			// 
			listTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			listTextBox.Location = new System.Drawing.Point(100, 102);
			listTextBox.Name = "listTextBox";
			listTextBox.Palette = fontPalette;
			listTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			listTextBox.Size = new System.Drawing.Size(454, 20);
			listTextBox.TabIndex = 7;
			// 
			// listLabel
			// 
			listLabel.Location = new System.Drawing.Point(4, 104);
			listLabel.Name = "listLabel";
			listLabel.Palette = fontPalette;
			listLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(listLabel, "IDS_SETTINGS_PATHS_LIST");
			listLabel.Size = new System.Drawing.Size(52, 16);
			listLabel.TabIndex = 6;
			listLabel.Values.Text = "List path";
			// 
			// modLibraryButton
			// 
			modLibraryButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			modLibraryButton.Location = new System.Drawing.Point(556, 133);
			modLibraryButton.Name = "modLibraryButton";
			modLibraryButton.Size = new System.Drawing.Size(22, 22);
			modLibraryButton.TabIndex = 11;
			modLibraryButton.Values.Image = (System.Drawing.Image)resources.GetObject("modLibraryButton.Values.Image");
			modLibraryButton.Values.Text = "";
			modLibraryButton.Click += ModLibraryButton_Click;
			// 
			// modLibraryTextBox
			// 
			modLibraryTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			modLibraryTextBox.Location = new System.Drawing.Point(100, 134);
			modLibraryTextBox.Name = "modLibraryTextBox";
			modLibraryTextBox.Palette = fontPalette;
			modLibraryTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			modLibraryTextBox.Size = new System.Drawing.Size(454, 20);
			modLibraryTextBox.TabIndex = 10;
			// 
			// modLibraryLabel
			// 
			modLibraryLabel.Location = new System.Drawing.Point(4, 136);
			modLibraryLabel.Name = "modLibraryLabel";
			modLibraryLabel.Palette = fontPalette;
			modLibraryLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(modLibraryLabel, "IDS_SETTINGS_PATHS_MODULELIBRARY");
			modLibraryLabel.Size = new System.Drawing.Size(79, 16);
			modLibraryLabel.TabIndex = 9;
			modLibraryLabel.Values.Text = "Module library";
			// 
			// PathsPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(group);
			Name = "PathsPageControl";
			Size = new System.Drawing.Size(608, 356);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)group.Panel).EndInit();
			group.Panel.ResumeLayout(false);
			group.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)group).EndInit();
			ResumeLayout(false);

		}

		#endregion
		private Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonGroup group;
		private Krypton.Toolkit.KryptonLabel startScanLabel;
		private Krypton.Toolkit.KryptonTextBox startScanTextBox;
		private Krypton.Toolkit.KryptonButton startScanButton;
		private Krypton.Toolkit.KryptonLabel moduleLabel;
		private Krypton.Toolkit.KryptonTextBox moduleTextBox;
		private Krypton.Toolkit.KryptonButton moduleButton;
		private Krypton.Toolkit.KryptonLabel listLabel;
		private Krypton.Toolkit.KryptonTextBox listTextBox;
		private Krypton.Toolkit.KryptonButton listButton;
		private Krypton.Toolkit.KryptonLabel modLibraryLabel;
		private Krypton.Toolkit.KryptonTextBox modLibraryTextBox;
		private Krypton.Toolkit.KryptonButton modLibraryButton;
		private Kit.Gui.Components.FontPalette fontPalette;
	}
}
