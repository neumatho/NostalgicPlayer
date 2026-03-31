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
			commentText = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicText();
			SuspendLayout();
			// 
			// commentText
			// 
			commentText.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			commentText.Location = new System.Drawing.Point(8, 8);
			commentText.Name = "commentText";
			commentText.Size = new System.Drawing.Size(266, 142);
			commentText.TabIndex = 0;
			// 
			// CommentPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(commentText);
			Name = "CommentPageControl";
			Size = new System.Drawing.Size(282, 158);
			ResumeLayout(false);
		}

		#endregion
		private Polycode.NostalgicPlayer.Controls.Lists.NostalgicText commentText;
	}
}
