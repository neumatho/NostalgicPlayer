
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class AgentsListUserControl
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
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.agentsDataGridView = new Krypton.Toolkit.KryptonDataGridView();
			this.descriptionDataGridView = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.DescriptionListControl();
			this.settingsButton = new Krypton.Toolkit.KryptonButton();
			this.displayButton = new Krypton.Toolkit.KryptonButton();
			this.agentsGroup = new Krypton.Toolkit.KryptonGroup();
			this.descriptionGroup = new Krypton.Toolkit.KryptonGroup();
			this.fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(this.components);
			this.bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(this.components);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.agentsDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.descriptionDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.agentsGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.agentsGroup.Panel)).BeginInit();
			this.agentsGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.descriptionGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.descriptionGroup.Panel)).BeginInit();
			this.descriptionGroup.Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// agentsDataGridView
			// 
			this.agentsDataGridView.AllowUserToAddRows = false;
			this.agentsDataGridView.AllowUserToDeleteRows = false;
			this.agentsDataGridView.AllowUserToOrderColumns = true;
			this.agentsDataGridView.AllowUserToResizeRows = false;
			this.agentsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.agentsDataGridView.GridStyles.StyleDataCells = Krypton.Toolkit.GridStyle.Sheet;
			this.agentsDataGridView.Location = new System.Drawing.Point(0, 0);
			this.agentsDataGridView.MultiSelect = false;
			this.agentsDataGridView.Name = "agentsDataGridView";
			this.agentsDataGridView.Palette = this.fontPalette;
			this.agentsDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.agentsDataGridView.ReadOnly = true;
			this.controlResource.SetResourceKey(this.agentsDataGridView, null);
			this.agentsDataGridView.RowHeadersVisible = false;
			this.agentsDataGridView.RowTemplate.Height = 25;
			this.agentsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.agentsDataGridView.ShowCellErrors = false;
			this.agentsDataGridView.ShowCellToolTips = false;
			this.agentsDataGridView.ShowEditingIcon = false;
			this.agentsDataGridView.ShowRowErrors = false;
			this.agentsDataGridView.Size = new System.Drawing.Size(285, 264);
			this.agentsDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.agentsDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			this.agentsDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.agentsDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.agentsDataGridView.TabIndex = 0;
			this.agentsDataGridView.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.AgentsDataGridView_CellContentDoubleClick);
			this.agentsDataGridView.SelectionChanged += new System.EventHandler(this.AgentsDataGridView_SelectionChanged);
			// 
			// descriptionDataGridView
			// 
			this.descriptionDataGridView.AllowUserToAddRows = false;
			this.descriptionDataGridView.AllowUserToDeleteRows = false;
			this.descriptionDataGridView.AllowUserToResizeRows = false;
			this.descriptionDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.descriptionDataGridView.Location = new System.Drawing.Point(0, 0);
			this.descriptionDataGridView.Name = "descriptionDataGridView";
			this.descriptionDataGridView.Palette = this.fontPalette;
			this.descriptionDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.descriptionDataGridView.ReadOnly = true;
			this.controlResource.SetResourceKey(this.descriptionDataGridView, null);
			this.descriptionDataGridView.RowHeadersVisible = false;
			this.descriptionDataGridView.RowTemplate.Height = 25;
			this.descriptionDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.descriptionDataGridView.ShowCellErrors = false;
			this.descriptionDataGridView.ShowCellToolTips = false;
			this.descriptionDataGridView.ShowEditingIcon = false;
			this.descriptionDataGridView.ShowRowErrors = false;
			this.descriptionDataGridView.Size = new System.Drawing.Size(285, 297);
			this.descriptionDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.descriptionDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			this.descriptionDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.descriptionDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.descriptionDataGridView.TabIndex = 0;
			this.descriptionDataGridView.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.DescriptionDataGridView_ColumnWidthChanged);
			// 
			// settingsButton
			// 
			this.settingsButton.Enabled = false;
			this.settingsButton.Location = new System.Drawing.Point(4, 282);
			this.settingsButton.Name = "settingsButton";
			this.settingsButton.Palette = this.bigFontPalette;
			this.settingsButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.settingsButton, "IDS_SETTINGS_AGENTS_SETTINGS");
			this.settingsButton.Size = new System.Drawing.Size(138, 25);
			this.settingsButton.TabIndex = 2;
			this.settingsButton.Values.Text = "Settings";
			this.settingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
			// 
			// displayButton
			// 
			this.displayButton.Enabled = false;
			this.displayButton.Location = new System.Drawing.Point(153, 282);
			this.displayButton.Name = "displayButton";
			this.displayButton.Palette = this.bigFontPalette;
			this.displayButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.displayButton, "IDS_SETTINGS_AGENTS_DISPLAY");
			this.displayButton.Size = new System.Drawing.Size(138, 25);
			this.displayButton.TabIndex = 3;
			this.displayButton.Values.Text = "Display";
			this.displayButton.Click += new System.EventHandler(this.DisplayButton_Click);
			// 
			// agentsGroup
			// 
			this.agentsGroup.Location = new System.Drawing.Point(4, 8);
			this.agentsGroup.Name = "agentsGroup";
			// 
			// 
			// 
			this.agentsGroup.Panel.Controls.Add(this.agentsDataGridView);
			this.controlResource.SetResourceKey(this.agentsGroup, null);
			this.agentsGroup.Size = new System.Drawing.Size(287, 266);
			this.agentsGroup.TabIndex = 0;
			// 
			// descriptionGroup
			// 
			this.descriptionGroup.Location = new System.Drawing.Point(299, 8);
			this.descriptionGroup.Name = "descriptionGroup";
			// 
			// 
			// 
			this.descriptionGroup.Panel.Controls.Add(this.descriptionDataGridView);
			this.controlResource.SetResourceKey(this.descriptionGroup, null);
			this.descriptionGroup.Size = new System.Drawing.Size(287, 299);
			this.descriptionGroup.TabIndex = 1;
			// 
			// bigFontPalette
			// 
			this.bigFontPalette.BaseFontSize = 9F;
			// 
			// AgentsListUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.descriptionGroup);
			this.Controls.Add(this.agentsGroup);
			this.Controls.Add(this.displayButton);
			this.Controls.Add(this.settingsButton);
			this.Name = "AgentsListUserControl";
			this.controlResource.SetResourceKey(this, null);
			this.Size = new System.Drawing.Size(590, 315);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.agentsDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.descriptionDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.agentsGroup.Panel)).EndInit();
			this.agentsGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.agentsGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.descriptionGroup.Panel)).EndInit();
			this.descriptionGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.descriptionGroup)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonDataGridView agentsDataGridView;
		private DescriptionListControl descriptionDataGridView;
		private Krypton.Toolkit.KryptonButton settingsButton;
		private Krypton.Toolkit.KryptonButton displayButton;
		private Krypton.Toolkit.KryptonGroup agentsGroup;
		private Krypton.Toolkit.KryptonGroup descriptionGroup;
		private GuiKit.Components.FontPalette fontPalette;
		private GuiKit.Components.FontPalette bigFontPalette;
	}
}
