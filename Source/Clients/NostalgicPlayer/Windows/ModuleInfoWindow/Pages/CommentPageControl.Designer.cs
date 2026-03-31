namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages
{
	partial class CommentPageControl
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
			commentGroup = new Krypton.Toolkit.KryptonGroup();
			moduleInfoCommentReadOnlyTextBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.ReadOnlyTextBox();
			((System.ComponentModel.ISupportInitialize)commentGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)commentGroup.Panel).BeginInit();
			commentGroup.Panel.SuspendLayout();
			SuspendLayout();
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
			// CommentPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(commentGroup);
			Name = "CommentPageControl";
			Size = new System.Drawing.Size(282, 158);
			((System.ComponentModel.ISupportInitialize)commentGroup.Panel).EndInit();
			commentGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)commentGroup).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private Krypton.Toolkit.KryptonGroup commentGroup;
		private Controls.ReadOnlyTextBox moduleInfoCommentReadOnlyTextBox;
	}
}
