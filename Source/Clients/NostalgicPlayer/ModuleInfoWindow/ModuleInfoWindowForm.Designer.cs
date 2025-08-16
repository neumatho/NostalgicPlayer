
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleInfoWindowForm));
			moduleInfoInfoDataGridView = new ModuleInfoListControl();
			fontPalette = new Kit.Gui.Components.FontPalette(components);
			infoGroup = new Krypton.Toolkit.KryptonGroup();
			navigator = new Krypton.Navigator.KryptonNavigator();
			navigatorInfoPage = new Krypton.Navigator.KryptonPage();
			navigatorCommentPage = new Krypton.Navigator.KryptonPage();
			commentGroup = new Krypton.Toolkit.KryptonGroup();
			moduleInfoCommentReadOnlyTextBox = new Controls.ReadOnlyTextBox();
			navigatorLyricsPage = new Krypton.Navigator.KryptonPage();
			lyricsGroup = new Krypton.Toolkit.KryptonGroup();
			moduleInfoLyricsReadOnlyTextBox = new Controls.ReadOnlyTextBox();
			navigatorPicturePage = new Krypton.Navigator.KryptonPage();
			pictureGroup = new Krypton.Toolkit.KryptonGroup();
			previousPictureButton = new System.Windows.Forms.PictureBox();
			nextPictureButton = new System.Windows.Forms.PictureBox();
			pictureBox = new System.Windows.Forms.PictureBox();
			pictureLabelPictureBox = new System.Windows.Forms.PictureBox();
			animationTimer = new System.Windows.Forms.Timer(components);
			((System.ComponentModel.ISupportInitialize)moduleInfoInfoDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)infoGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)infoGroup.Panel).BeginInit();
			infoGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigator).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigatorInfoPage).BeginInit();
			navigatorInfoPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorCommentPage).BeginInit();
			navigatorCommentPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)commentGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)commentGroup.Panel).BeginInit();
			commentGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorLyricsPage).BeginInit();
			navigatorLyricsPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)lyricsGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)lyricsGroup.Panel).BeginInit();
			lyricsGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorPicturePage).BeginInit();
			navigatorPicturePage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureGroup.Panel).BeginInit();
			pictureGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)previousPictureButton).BeginInit();
			((System.ComponentModel.ISupportInitialize)nextPictureButton).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureLabelPictureBox).BeginInit();
			SuspendLayout();
			// 
			// moduleInfoInfoDataGridView
			// 
			moduleInfoInfoDataGridView.AllowUserToAddRows = false;
			moduleInfoInfoDataGridView.AllowUserToDeleteRows = false;
			moduleInfoInfoDataGridView.AllowUserToResizeRows = false;
			moduleInfoInfoDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			moduleInfoInfoDataGridView.Location = new System.Drawing.Point(0, 0);
			moduleInfoInfoDataGridView.Name = "moduleInfoInfoDataGridView";
			moduleInfoInfoDataGridView.Palette = fontPalette;
			moduleInfoInfoDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			moduleInfoInfoDataGridView.ReadOnly = true;
			moduleInfoInfoDataGridView.RowHeadersVisible = false;
			moduleInfoInfoDataGridView.RowTemplate.Height = 25;
			moduleInfoInfoDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			moduleInfoInfoDataGridView.ShowCellErrors = false;
			moduleInfoInfoDataGridView.ShowEditingIcon = false;
			moduleInfoInfoDataGridView.ShowRowErrors = false;
			moduleInfoInfoDataGridView.Size = new System.Drawing.Size(264, 140);
			moduleInfoInfoDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			moduleInfoInfoDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			moduleInfoInfoDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			moduleInfoInfoDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right;
			moduleInfoInfoDataGridView.TabIndex = 0;
			moduleInfoInfoDataGridView.CellContentClick += ModuleInfoInfoDataGridView_CellContentClick;
			// 
			// infoGroup
			// 
			infoGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoGroup.Location = new System.Drawing.Point(8, 8);
			infoGroup.Name = "infoGroup";
			// 
			// 
			// 
			infoGroup.Panel.Controls.Add(moduleInfoInfoDataGridView);
			infoGroup.Size = new System.Drawing.Size(266, 142);
			infoGroup.TabIndex = 0;
			// 
			// navigator
			// 
			navigator.AllowPageReorder = false;
			navigator.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			navigator.Button.ButtonDisplayLogic = Krypton.Navigator.ButtonDisplayLogic.Context;
			navigator.Button.CloseButtonAction = Krypton.Navigator.CloseButtonAction.RemovePageAndDispose;
			navigator.Button.CloseButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			navigator.Button.ContextButtonAction = Krypton.Navigator.ContextButtonAction.SelectPage;
			navigator.Button.ContextButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			navigator.Button.ContextMenuMapImage = Krypton.Navigator.MapKryptonPageImage.Small;
			navigator.Button.ContextMenuMapText = Krypton.Navigator.MapKryptonPageText.TextTitle;
			navigator.Button.NextButtonAction = Krypton.Navigator.DirectionButtonAction.ModeAppropriateAction;
			navigator.Button.NextButtonDisplay = Krypton.Navigator.ButtonDisplay.Logic;
			navigator.Button.PreviousButtonAction = Krypton.Navigator.DirectionButtonAction.ModeAppropriateAction;
			navigator.Button.PreviousButtonDisplay = Krypton.Navigator.ButtonDisplay.Logic;
			navigator.Location = new System.Drawing.Point(8, 8);
			navigator.Name = "navigator";
			navigator.NavigatorMode = Krypton.Navigator.NavigatorMode.BarTabGroup;
			navigator.PageBackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
			navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] { navigatorInfoPage, navigatorCommentPage, navigatorLyricsPage, navigatorPicturePage });
			navigator.Palette = fontPalette;
			navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			navigator.SelectedIndex = 0;
			navigator.Size = new System.Drawing.Size(284, 184);
			navigator.TabIndex = 1;
			// 
			// navigatorInfoPage
			// 
			navigatorInfoPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorInfoPage.Controls.Add(infoGroup);
			navigatorInfoPage.Flags = 65534;
			navigatorInfoPage.LastVisibleSet = true;
			navigatorInfoPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorInfoPage.Name = "navigatorInfoPage";
			navigatorInfoPage.Size = new System.Drawing.Size(282, 158);
			navigatorInfoPage.Text = "";
			navigatorInfoPage.ToolTipTitle = "Page ToolTip";
			navigatorInfoPage.UniqueName = "c99656c653d3472bb30017074959420b";
			// 
			// navigatorCommentPage
			// 
			navigatorCommentPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorCommentPage.Controls.Add(commentGroup);
			navigatorCommentPage.Flags = 65534;
			navigatorCommentPage.LastVisibleSet = true;
			navigatorCommentPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorCommentPage.Name = "navigatorCommentPage";
			navigatorCommentPage.Size = new System.Drawing.Size(282, 158);
			navigatorCommentPage.Text = "";
			navigatorCommentPage.ToolTipTitle = "Page ToolTip";
			navigatorCommentPage.UniqueName = "a6135a5732ce46a98b44f9d9f3bed53f";
			// 
			// commentGroup
			// 
			commentGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			commentGroup.Location = new System.Drawing.Point(8, 8);
			commentGroup.Name = "commentGroup";
			// 
			// 
			// 
			commentGroup.Panel.Controls.Add(moduleInfoCommentReadOnlyTextBox);
			commentGroup.Size = new System.Drawing.Size(266, 142);
			commentGroup.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			commentGroup.TabIndex = 0;
			// 
			// moduleInfoCommentReadOnlyTextBox
			// 
			moduleInfoCommentReadOnlyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			moduleInfoCommentReadOnlyTextBox.Location = new System.Drawing.Point(0, 0);
			moduleInfoCommentReadOnlyTextBox.Name = "moduleInfoCommentReadOnlyTextBox";
			moduleInfoCommentReadOnlyTextBox.Size = new System.Drawing.Size(266, 142);
			moduleInfoCommentReadOnlyTextBox.TabIndex = 0;
			// 
			// navigatorLyricsPage
			// 
			navigatorLyricsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorLyricsPage.Controls.Add(lyricsGroup);
			navigatorLyricsPage.Flags = 65534;
			navigatorLyricsPage.LastVisibleSet = true;
			navigatorLyricsPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorLyricsPage.Name = "navigatorLyricsPage";
			navigatorLyricsPage.Size = new System.Drawing.Size(282, 158);
			navigatorLyricsPage.Text = "";
			navigatorLyricsPage.ToolTipTitle = "Page ToolTip";
			navigatorLyricsPage.UniqueName = "c12067c426fe41ada484078b6b2d957f";
			// 
			// lyricsGroup
			// 
			lyricsGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			lyricsGroup.Location = new System.Drawing.Point(8, 8);
			lyricsGroup.Name = "lyricsGroup";
			// 
			// 
			// 
			lyricsGroup.Panel.Controls.Add(moduleInfoLyricsReadOnlyTextBox);
			lyricsGroup.Size = new System.Drawing.Size(266, 142);
			lyricsGroup.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			lyricsGroup.TabIndex = 0;
			// 
			// moduleInfoLyricsReadOnlyTextBox
			// 
			moduleInfoLyricsReadOnlyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			moduleInfoLyricsReadOnlyTextBox.Location = new System.Drawing.Point(0, 0);
			moduleInfoLyricsReadOnlyTextBox.Name = "moduleInfoLyricsReadOnlyTextBox";
			moduleInfoLyricsReadOnlyTextBox.Size = new System.Drawing.Size(266, 142);
			moduleInfoLyricsReadOnlyTextBox.TabIndex = 0;
			// 
			// navigatorPicturePage
			// 
			navigatorPicturePage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorPicturePage.Controls.Add(pictureGroup);
			navigatorPicturePage.Flags = 65534;
			navigatorPicturePage.LastVisibleSet = true;
			navigatorPicturePage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorPicturePage.Name = "navigatorPicturePage";
			navigatorPicturePage.Size = new System.Drawing.Size(282, 158);
			navigatorPicturePage.Text = "";
			navigatorPicturePage.ToolTipTitle = "Page ToolTip";
			navigatorPicturePage.UniqueName = "3facf91c879c4e6b8ef9b543ed9630a2";
			// 
			// pictureGroup
			// 
			pictureGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pictureGroup.Location = new System.Drawing.Point(8, 8);
			pictureGroup.Name = "pictureGroup";
			// 
			// 
			// 
			pictureGroup.Panel.Controls.Add(previousPictureButton);
			pictureGroup.Panel.Controls.Add(nextPictureButton);
			pictureGroup.Panel.Controls.Add(pictureBox);
			pictureGroup.Panel.Controls.Add(pictureLabelPictureBox);
			pictureGroup.Size = new System.Drawing.Size(266, 142);
			pictureGroup.TabIndex = 0;
			pictureGroup.Resize += PictureGroup_Resize;
			// 
			// previousPictureButton
			// 
			previousPictureButton.BackColor = System.Drawing.Color.Transparent;
			previousPictureButton.Image = Resources.IDB_PREVIOUS_PICTURE;
			previousPictureButton.Location = new System.Drawing.Point(8, 45);
			previousPictureButton.Name = "previousPictureButton";
			previousPictureButton.Size = new System.Drawing.Size(24, 24);
			previousPictureButton.TabIndex = 1;
			previousPictureButton.TabStop = false;
			previousPictureButton.Click += PreviousPictureButton_Click;
			// 
			// nextPictureButton
			// 
			nextPictureButton.BackColor = System.Drawing.Color.Transparent;
			nextPictureButton.Image = Resources.IDB_NEXT_PICTURE;
			nextPictureButton.Location = new System.Drawing.Point(234, 45);
			nextPictureButton.Name = "nextPictureButton";
			nextPictureButton.Size = new System.Drawing.Size(24, 24);
			nextPictureButton.TabIndex = 1;
			nextPictureButton.TabStop = false;
			nextPictureButton.Click += NextPictureButton_Click;
			// 
			// pictureBox
			// 
			pictureBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pictureBox.BackColor = System.Drawing.Color.Transparent;
			pictureBox.Location = new System.Drawing.Point(40, 8);
			pictureBox.Name = "pictureBox";
			pictureBox.Size = new System.Drawing.Size(186, 98);
			pictureBox.TabIndex = 1;
			pictureBox.TabStop = false;
			pictureBox.Paint += Picture_Paint;
			// 
			// pictureLabelPictureBox
			// 
			pictureLabelPictureBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pictureLabelPictureBox.BackColor = System.Drawing.Color.Transparent;
			pictureLabelPictureBox.Location = new System.Drawing.Point(40, 106);
			pictureLabelPictureBox.Name = "pictureLabelPictureBox";
			pictureLabelPictureBox.Size = new System.Drawing.Size(186, 28);
			pictureLabelPictureBox.TabIndex = 1;
			pictureLabelPictureBox.TabStop = false;
			pictureLabelPictureBox.Paint += PictureLabel_Paint;
			// 
			// animationTimer
			// 
			animationTimer.Interval = 20;
			animationTimer.Tick += AnimationTimer_Tick;
			// 
			// ModuleInfoWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(300, 200);
			Controls.Add(navigator);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(316, 239);
			Name = "ModuleInfoWindowForm";
			Palette = fontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			FormClosed += ModuleInfoWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)moduleInfoInfoDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)infoGroup.Panel).EndInit();
			infoGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)infoGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)navigator).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorInfoPage).EndInit();
			navigatorInfoPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorCommentPage).EndInit();
			navigatorCommentPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)commentGroup.Panel).EndInit();
			commentGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)commentGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorLyricsPage).EndInit();
			navigatorLyricsPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)lyricsGroup.Panel).EndInit();
			lyricsGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)lyricsGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorPicturePage).EndInit();
			navigatorPicturePage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureGroup.Panel).EndInit();
			pictureGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)previousPictureButton).EndInit();
			((System.ComponentModel.ISupportInitialize)nextPictureButton).EndInit();
			((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
			((System.ComponentModel.ISupportInitialize)pictureLabelPictureBox).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private ModuleInfoListControl moduleInfoInfoDataGridView;
		private Krypton.Toolkit.KryptonGroup infoGroup;
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorInfoPage;
		private Krypton.Navigator.KryptonPage navigatorCommentPage;
		private Krypton.Toolkit.KryptonGroup commentGroup;
		private Krypton.Navigator.KryptonPage navigatorLyricsPage;
		private Krypton.Toolkit.KryptonGroup lyricsGroup;
		private Kit.Gui.Components.FontPalette fontPalette;
		private Krypton.Navigator.KryptonPage navigatorPicturePage;
		private Krypton.Toolkit.KryptonGroup pictureGroup;
		private System.Windows.Forms.PictureBox previousPictureButton;
		private System.Windows.Forms.PictureBox nextPictureButton;
		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.PictureBox pictureLabelPictureBox;
		private System.Windows.Forms.Timer animationTimer;
		private Controls.ReadOnlyTextBox moduleInfoCommentReadOnlyTextBox;
		private Controls.ReadOnlyTextBox moduleInfoLyricsReadOnlyTextBox;
	}
}