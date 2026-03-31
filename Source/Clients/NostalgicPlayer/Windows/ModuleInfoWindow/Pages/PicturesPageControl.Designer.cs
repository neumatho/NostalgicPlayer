namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages
{
	partial class PicturesPageControl
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
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			animationTimer = new System.Windows.Forms.Timer(components);
			pictureGroup = new Krypton.Toolkit.KryptonGroup();
			previousPictureButton = new System.Windows.Forms.PictureBox();
			nextPictureButton = new System.Windows.Forms.PictureBox();
			pictureBox = new System.Windows.Forms.PictureBox();
			pictureLabelPictureBox = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)pictureGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureGroup.Panel).BeginInit();
			pictureGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)previousPictureButton).BeginInit();
			((System.ComponentModel.ISupportInitialize)nextPictureButton).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureLabelPictureBox).BeginInit();
			SuspendLayout();
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// animationTimer
			// 
			animationTimer.Interval = 20;
			animationTimer.Tick += AnimationTimer_Tick;
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
			// PicturesPageControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(pictureGroup);
			Name = "PicturesPageControl";
			Size = new System.Drawing.Size(282, 158);
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
		private Krypton.Toolkit.KryptonGroup pictureGroup;
		private System.Windows.Forms.PictureBox previousPictureButton;
		private System.Windows.Forms.PictureBox nextPictureButton;
		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.PictureBox pictureLabelPictureBox;
		private System.Windows.Forms.Timer animationTimer;
		private Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette fontPalette;
	}
}
