
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PathsPageControl));
			this.controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			this.group = new Krypton.Toolkit.KryptonGroup();
			this.startScanButton = new Krypton.Toolkit.KryptonButton();
			this.startScanTextBox = new Krypton.Toolkit.KryptonTextBox();
			this.startScanLabel = new Krypton.Toolkit.KryptonLabel();
			this.moduleButton = new Krypton.Toolkit.KryptonButton();
			this.moduleTextBox = new Krypton.Toolkit.KryptonTextBox();
			this.moduleLabel = new Krypton.Toolkit.KryptonLabel();
			this.listButton = new Krypton.Toolkit.KryptonButton();
			this.listTextBox = new Krypton.Toolkit.KryptonTextBox();
			this.listLabel = new Krypton.Toolkit.KryptonLabel();
			this.modLibraryButton = new Krypton.Toolkit.KryptonButton();
			this.modLibraryTextBox = new Krypton.Toolkit.KryptonTextBox();
			this.modLibraryLabel = new Krypton.Toolkit.KryptonLabel();
			this.fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(this.components);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.group)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.group.Panel)).BeginInit();
			this.group.Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// group
			// 
			this.group.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.group.Location = new System.Drawing.Point(8, 8);
			this.group.Name = "group";
			// 
			// 
			// 
			this.group.Panel.Controls.Add(this.startScanButton);
			this.group.Panel.Controls.Add(this.startScanTextBox);
			this.group.Panel.Controls.Add(this.startScanLabel);
			this.group.Panel.Controls.Add(this.moduleButton);
			this.group.Panel.Controls.Add(this.moduleTextBox);
			this.group.Panel.Controls.Add(this.moduleLabel);
			this.group.Panel.Controls.Add(this.listButton);
			this.group.Panel.Controls.Add(this.listTextBox);
			this.group.Panel.Controls.Add(this.listLabel);
			this.group.Panel.Controls.Add(this.modLibraryButton);
			this.group.Panel.Controls.Add(this.modLibraryTextBox);
			this.group.Panel.Controls.Add(this.modLibraryLabel);
			this.controlResource.SetResourceKey(this.group, null);
			this.group.Size = new System.Drawing.Size(592, 340);
			this.group.TabIndex = 0;
			// 
			// startScanButton
			// 
			this.startScanButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.startScanButton.Location = new System.Drawing.Point(556, 37);
			this.startScanButton.Name = "startScanButton";
			this.controlResource.SetResourceKey(this.startScanButton, null);
			this.startScanButton.Size = new System.Drawing.Size(22, 22);
			this.startScanButton.TabIndex = 2;
			this.startScanButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("startScanButton.Values.Image")));
			this.startScanButton.Values.Text = "";
			this.startScanButton.Click += new System.EventHandler(this.StartScanButton_Click);
			// 
			// startScanTextBox
			// 
			this.startScanTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.startScanTextBox.Location = new System.Drawing.Point(100, 38);
			this.startScanTextBox.Name = "startScanTextBox";
			this.startScanTextBox.Palette = this.fontPalette;
			this.startScanTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.startScanTextBox, null);
			this.startScanTextBox.Size = new System.Drawing.Size(454, 20);
			this.startScanTextBox.TabIndex = 1;
			// 
			// startScanLabel
			// 
			this.startScanLabel.Location = new System.Drawing.Point(4, 40);
			this.startScanLabel.Name = "startScanLabel";
			this.startScanLabel.Palette = this.fontPalette;
			this.startScanLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.startScanLabel, "IDS_SETTINGS_PATHS_STARTSCANPATH");
			this.startScanLabel.Size = new System.Drawing.Size(84, 16);
			this.startScanLabel.TabIndex = 0;
			this.startScanLabel.Values.Text = "Start scan path";
			// 
			// moduleButton
			// 
			this.moduleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.moduleButton.Location = new System.Drawing.Point(556, 69);
			this.moduleButton.Name = "moduleButton";
			this.controlResource.SetResourceKey(this.moduleButton, null);
			this.moduleButton.Size = new System.Drawing.Size(22, 22);
			this.moduleButton.TabIndex = 5;
			this.moduleButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("moduleButton.Values.Image")));
			this.moduleButton.Values.Text = "";
			this.moduleButton.Click += new System.EventHandler(this.ModuleButton_Click);
			// 
			// moduleTextBox
			// 
			this.moduleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.moduleTextBox.Location = new System.Drawing.Point(100, 70);
			this.moduleTextBox.Name = "moduleTextBox";
			this.moduleTextBox.Palette = this.fontPalette;
			this.moduleTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.moduleTextBox, null);
			this.moduleTextBox.Size = new System.Drawing.Size(454, 20);
			this.moduleTextBox.TabIndex = 4;
			// 
			// moduleLabel
			// 
			this.moduleLabel.Location = new System.Drawing.Point(4, 72);
			this.moduleLabel.Name = "moduleLabel";
			this.moduleLabel.Palette = this.fontPalette;
			this.moduleLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.moduleLabel, "IDS_SETTINGS_PATHS_MODULEPATH");
			this.moduleLabel.Size = new System.Drawing.Size(70, 16);
			this.moduleLabel.TabIndex = 3;
			this.moduleLabel.Values.Text = "Module path";
			// 
			// listButton
			// 
			this.listButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.listButton.Location = new System.Drawing.Point(556, 101);
			this.listButton.Name = "listButton";
			this.controlResource.SetResourceKey(this.listButton, null);
			this.listButton.Size = new System.Drawing.Size(22, 22);
			this.listButton.TabIndex = 8;
			this.listButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("listButton.Values.Image")));
			this.listButton.Values.Text = "";
			this.listButton.Click += new System.EventHandler(this.ListButton_Click);
			// 
			// listTextBox
			// 
			this.listTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listTextBox.Location = new System.Drawing.Point(100, 102);
			this.listTextBox.Name = "listTextBox";
			this.listTextBox.Palette = this.fontPalette;
			this.listTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.listTextBox, null);
			this.listTextBox.Size = new System.Drawing.Size(454, 20);
			this.listTextBox.TabIndex = 7;
			// 
			// listLabel
			//
			this.listLabel.Location = new System.Drawing.Point(4, 104);
			this.listLabel.Name = "listLabel";
			this.listLabel.Palette = this.fontPalette;
			this.listLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.listLabel, "IDS_SETTINGS_PATHS_LISTPATH");
			this.listLabel.Size = new System.Drawing.Size(52, 16);
			this.listLabel.TabIndex = 6;
			this.listLabel.Values.Text = "List path";
			//
			// modLibraryButton
			//
			this.modLibraryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.modLibraryButton.Location = new System.Drawing.Point(556, 133);
			this.modLibraryButton.Name = "modLibraryButton";
			this.controlResource.SetResourceKey(this.modLibraryButton, null);
			this.modLibraryButton.Size = new System.Drawing.Size(22, 22);
			this.modLibraryButton.TabIndex = 11;
			this.modLibraryButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("listButton.Values.Image")));
			this.modLibraryButton.Values.Text = "";
			this.modLibraryButton.Click += new System.EventHandler(this.ModLibraryButton_Click);
			//
			// modLibraryTextBox
			//
			this.modLibraryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.modLibraryTextBox.Location = new System.Drawing.Point(100, 134);
			this.modLibraryTextBox.Name = "modLibraryTextBox";
			this.modLibraryTextBox.Palette = this.fontPalette;
			this.modLibraryTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.modLibraryTextBox, null);
			this.modLibraryTextBox.Size = new System.Drawing.Size(454, 20);
			this.modLibraryTextBox.TabIndex = 10;
			//
			// modLibraryLabel
			//
			this.modLibraryLabel.Location = new System.Drawing.Point(4, 136);
			this.modLibraryLabel.Name = "modLibraryLabel";
			this.modLibraryLabel.Palette = this.fontPalette;
			this.modLibraryLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.modLibraryLabel, "IDS_SETTINGS_PATHS_MODLIBRARYPATH");
			this.modLibraryLabel.Size = new System.Drawing.Size(86, 16);
			this.modLibraryLabel.TabIndex = 9;
			this.modLibraryLabel.Values.Text = "Module Library";
			//
			// PathsPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.group);
			this.Name = "PathsPageControl";
			this.controlResource.SetResourceKey(this, null);
			this.Size = new System.Drawing.Size(608, 356);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.group.Panel)).EndInit();
			this.group.Panel.ResumeLayout(false);
			this.group.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.group)).EndInit();
			this.ResumeLayout(false);

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
