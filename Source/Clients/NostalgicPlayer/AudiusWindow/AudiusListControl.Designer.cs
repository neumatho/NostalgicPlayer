namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	partial class AudiusListControl
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
			flowLayoutPanel = new GuiKit.Controls.ImprovedFlowLayoutPanel();
			controlGroup = new Krypton.Toolkit.KryptonGroup();
			loadingLabel = new Krypton.Toolkit.KryptonLabel();
			bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)controlGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).BeginInit();
			controlGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
			// 
			// flowLayoutPanel
			// 
			flowLayoutPanel.AutoScroll = true;
			flowLayoutPanel.BackColor = System.Drawing.Color.Transparent;
			flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
			flowLayoutPanel.Name = "flowLayoutPanel";
			controlResource.SetResourceKey(flowLayoutPanel, null);
			flowLayoutPanel.Size = new System.Drawing.Size(274, 173);
			flowLayoutPanel.TabIndex = 0;
			flowLayoutPanel.WrapContents = false;
			flowLayoutPanel.Scroll += FlowLayout_Scroll;
			flowLayoutPanel.Resize += FlowLayout_Resize;
			// 
			// controlGroup
			// 
			controlGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			controlGroup.Location = new System.Drawing.Point(0, 0);
			controlGroup.Name = "controlGroup";
			// 
			// 
			// 
			controlGroup.Panel.Controls.Add(loadingLabel);
			controlGroup.Panel.Controls.Add(flowLayoutPanel);
			controlResource.SetResourceKey(controlGroup, null);
			controlGroup.Size = new System.Drawing.Size(276, 175);
			controlGroup.TabIndex = 1;
			// 
			// loadingLabel
			// 
			loadingLabel.Location = new System.Drawing.Point(44, 78);
			loadingLabel.Name = "loadingLabel";
			loadingLabel.Palette = bigFontPalette;
			loadingLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(loadingLabel, "IDS_AUDIUS_LOADING");
			loadingLabel.Size = new System.Drawing.Size(188, 18);
			loadingLabel.TabIndex = 1;
			loadingLabel.Values.Text = "Please wait while loading items…";
			loadingLabel.Visible = false;
			// 
			// bigFontPalette
			// 
			bigFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigFontPalette.BaseFontSize = 9F;
			bigFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			bigFontPalette.ThemeName = "";
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// AudiusListControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(controlGroup);
			Name = "AudiusListControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(276, 175);
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).EndInit();
			controlGroup.Panel.ResumeLayout(false);
			controlGroup.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)controlGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private GuiKit.Controls.ImprovedFlowLayoutPanel flowLayoutPanel;
		private Krypton.Toolkit.KryptonGroup controlGroup;
		private Krypton.Toolkit.KryptonLabel loadingLabel;
		private GuiKit.Designer.ControlResource controlResource;
		private GuiKit.Components.FontPalette bigFontPalette;
	}
}
