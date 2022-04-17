
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModuleInfoWindow
{
	partial class ModuleInfoWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleInfoWindowForm));
			this.moduleInfoInfoDataGridView = new Polycode.NostalgicPlayer.Client.GuiPlayer.ModuleInfoWindow.ModuleInfoListControl();
			this.fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(this.components);
			this.infoGroup = new Krypton.Toolkit.KryptonGroup();
			this.navigator = new Krypton.Navigator.KryptonNavigator();
			this.navigatorInfoPage = new Krypton.Navigator.KryptonPage();
			this.navigatorCommentPage = new Krypton.Navigator.KryptonPage();
			this.commentGroup = new Krypton.Toolkit.KryptonGroup();
			this.moduleInfoCommentReadOnlyRichTextBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.ReadOnlyRichTextBox();
			this.navigatorLyricsPage = new Krypton.Navigator.KryptonPage();
			this.lyricsGroup = new Krypton.Toolkit.KryptonGroup();
			this.moduleInfoLyricsReadOnlyRichTextBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.ReadOnlyRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.moduleInfoInfoDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.infoGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.infoGroup.Panel)).BeginInit();
			this.infoGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.navigator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorInfoPage)).BeginInit();
			this.navigatorInfoPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.navigatorCommentPage)).BeginInit();
			this.navigatorCommentPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.commentGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.commentGroup.Panel)).BeginInit();
			this.commentGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.navigatorLyricsPage)).BeginInit();
			this.navigatorLyricsPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lyricsGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lyricsGroup.Panel)).BeginInit();
			this.lyricsGroup.Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// moduleInfoInfoDataGridView
			// 
			this.moduleInfoInfoDataGridView.AllowUserToAddRows = false;
			this.moduleInfoInfoDataGridView.AllowUserToDeleteRows = false;
			this.moduleInfoInfoDataGridView.AllowUserToResizeRows = false;
			this.moduleInfoInfoDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.moduleInfoInfoDataGridView.Location = new System.Drawing.Point(0, 0);
			this.moduleInfoInfoDataGridView.Name = "moduleInfoInfoDataGridView";
			this.moduleInfoInfoDataGridView.Palette = this.fontPalette;
			this.moduleInfoInfoDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.moduleInfoInfoDataGridView.ReadOnly = true;
			this.moduleInfoInfoDataGridView.RowHeadersVisible = false;
			this.moduleInfoInfoDataGridView.RowTemplate.Height = 25;
			this.moduleInfoInfoDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.moduleInfoInfoDataGridView.ShowCellErrors = false;
			this.moduleInfoInfoDataGridView.ShowEditingIcon = false;
			this.moduleInfoInfoDataGridView.ShowRowErrors = false;
			this.moduleInfoInfoDataGridView.Size = new System.Drawing.Size(264, 140);
			this.moduleInfoInfoDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.moduleInfoInfoDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			this.moduleInfoInfoDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.moduleInfoInfoDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.moduleInfoInfoDataGridView.TabIndex = 0;
			this.moduleInfoInfoDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ModuleInfoInfoDataGridView_CellContentClick);
			// 
			// infoGroup
			// 
			this.infoGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.infoGroup.Location = new System.Drawing.Point(8, 8);
			this.infoGroup.Name = "infoGroup";
			// 
			// 
			// 
			this.infoGroup.Panel.Controls.Add(this.moduleInfoInfoDataGridView);
			this.infoGroup.Size = new System.Drawing.Size(266, 142);
			this.infoGroup.TabIndex = 0;
			// 
			// navigator
			// 
			this.navigator.AllowPageReorder = false;
			this.navigator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.navigator.Button.CloseButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			this.navigator.Button.ContextButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			this.navigator.Location = new System.Drawing.Point(8, 8);
			this.navigator.Name = "navigator";
			this.navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] {
            this.navigatorInfoPage,
            this.navigatorCommentPage,
            this.navigatorLyricsPage});
			this.navigator.Palette = this.fontPalette;
			this.navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.navigator.SelectedIndex = 0;
			this.navigator.Size = new System.Drawing.Size(284, 184);
			this.navigator.TabIndex = 1;
			this.navigator.SelectedPageChanged += new System.EventHandler(this.Navigator_SelectedPageChanged);
			// 
			// navigatorInfoPage
			// 
			this.navigatorInfoPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorInfoPage.Controls.Add(this.infoGroup);
			this.navigatorInfoPage.Flags = 65534;
			this.navigatorInfoPage.LastVisibleSet = true;
			this.navigatorInfoPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorInfoPage.Name = "navigatorInfoPage";
			this.navigatorInfoPage.Size = new System.Drawing.Size(282, 158);
			this.navigatorInfoPage.Text = "";
			this.navigatorInfoPage.ToolTipTitle = "Page ToolTip";
			this.navigatorInfoPage.UniqueName = "c99656c653d3472bb30017074959420b";
			// 
			// navigatorCommentPage
			// 
			this.navigatorCommentPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorCommentPage.Controls.Add(this.commentGroup);
			this.navigatorCommentPage.Flags = 65534;
			this.navigatorCommentPage.LastVisibleSet = true;
			this.navigatorCommentPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorCommentPage.Name = "navigatorCommentPage";
			this.navigatorCommentPage.Size = new System.Drawing.Size(282, 157);
			this.navigatorCommentPage.Text = "";
			this.navigatorCommentPage.ToolTipTitle = "Page ToolTip";
			this.navigatorCommentPage.UniqueName = "a6135a5732ce46a98b44f9d9f3bed53f";
			// 
			// commentGroup
			// 
			this.commentGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.commentGroup.Location = new System.Drawing.Point(8, 8);
			this.commentGroup.Name = "commentGroup";
			// 
			// 
			// 
			this.commentGroup.Panel.Controls.Add(this.moduleInfoCommentReadOnlyRichTextBox);
			this.commentGroup.Size = new System.Drawing.Size(266, 141);
			this.commentGroup.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.commentGroup.TabIndex = 0;
			// 
			// moduleInfoCommentReadOnlyRichTextBox
			// 
			this.moduleInfoCommentReadOnlyRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.moduleInfoCommentReadOnlyRichTextBox.Lines = new string[0];
			this.moduleInfoCommentReadOnlyRichTextBox.Location = new System.Drawing.Point(0, 0);
			this.moduleInfoCommentReadOnlyRichTextBox.Name = "moduleInfoCommentReadOnlyRichTextBox";
			this.moduleInfoCommentReadOnlyRichTextBox.Size = new System.Drawing.Size(266, 141);
			this.moduleInfoCommentReadOnlyRichTextBox.TabIndex = 0;
			// 
			// navigatorLyricsPage
			// 
			this.navigatorLyricsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorLyricsPage.Controls.Add(this.lyricsGroup);
			this.navigatorLyricsPage.Flags = 65534;
			this.navigatorLyricsPage.LastVisibleSet = true;
			this.navigatorLyricsPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorLyricsPage.Name = "navigatorLyricsPage";
			this.navigatorLyricsPage.Size = new System.Drawing.Size(282, 157);
			this.navigatorLyricsPage.Text = "";
			this.navigatorLyricsPage.ToolTipTitle = "Page ToolTip";
			this.navigatorLyricsPage.UniqueName = "c12067c426fe41ada484078b6b2d957f";
			// 
			// lyricsGroup
			// 
			this.lyricsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lyricsGroup.Location = new System.Drawing.Point(8, 8);
			this.lyricsGroup.Name = "lyricsGroup";
			// 
			// 
			// 
			this.lyricsGroup.Panel.Controls.Add(this.moduleInfoLyricsReadOnlyRichTextBox);
			this.lyricsGroup.Size = new System.Drawing.Size(266, 141);
			this.lyricsGroup.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.lyricsGroup.TabIndex = 0;
			// 
			// moduleInfoLyricsReadOnlyRichTextBox
			// 
			this.moduleInfoLyricsReadOnlyRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.moduleInfoLyricsReadOnlyRichTextBox.Lines = new string[0];
			this.moduleInfoLyricsReadOnlyRichTextBox.Location = new System.Drawing.Point(0, 0);
			this.moduleInfoLyricsReadOnlyRichTextBox.Name = "moduleInfoLyricsReadOnlyRichTextBox";
			this.moduleInfoLyricsReadOnlyRichTextBox.Size = new System.Drawing.Size(266, 141);
			this.moduleInfoLyricsReadOnlyRichTextBox.TabIndex = 0;
			// 
			// ModuleInfoWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(300, 200);
			this.Controls.Add(this.navigator);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(316, 239);
			this.Name = "ModuleInfoWindowForm";
			this.Palette = this.fontPalette;
			this.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ModuleInfoWindowForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.moduleInfoInfoDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.infoGroup.Panel)).EndInit();
			this.infoGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.infoGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorInfoPage)).EndInit();
			this.navigatorInfoPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.navigatorCommentPage)).EndInit();
			this.navigatorCommentPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.commentGroup.Panel)).EndInit();
			this.commentGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.commentGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorLyricsPage)).EndInit();
			this.navigatorLyricsPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.lyricsGroup.Panel)).EndInit();
			this.lyricsGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.lyricsGroup)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private ModuleInfoListControl moduleInfoInfoDataGridView;
		private Krypton.Toolkit.KryptonGroup infoGroup;
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorInfoPage;
		private Krypton.Navigator.KryptonPage navigatorCommentPage;
		private Krypton.Toolkit.KryptonGroup commentGroup;
		private Controls.ReadOnlyRichTextBox moduleInfoCommentReadOnlyRichTextBox;
		private Krypton.Navigator.KryptonPage navigatorLyricsPage;
		private Controls.ReadOnlyRichTextBox moduleInfoLyricsReadOnlyRichTextBox;
		private Krypton.Toolkit.KryptonGroup lyricsGroup;
		private GuiKit.Components.FontPalette fontPalette;
	}
}