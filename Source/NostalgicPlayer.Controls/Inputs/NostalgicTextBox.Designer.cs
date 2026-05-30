namespace Polycode.NostalgicPlayer.Controls.Inputs
{
	partial class NostalgicTextBox
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
			nostalgicRichTextBox = new NostalgicRichTextBox();
			SuspendLayout();
			// 
			// nostalgicRichTextBox
			// 
			nostalgicRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			nostalgicRichTextBox.Location = new System.Drawing.Point(0, 0);
			nostalgicRichTextBox.Name = "nostalgicRichTextBox";
			nostalgicRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			nostalgicRichTextBox.SingleLine = true;
			nostalgicRichTextBox.Size = new System.Drawing.Size(150, 20);
			nostalgicRichTextBox.TabIndex = 0;
			nostalgicRichTextBox.Text = "";
			nostalgicRichTextBox.WordWrap = false;
			// 
			// NostalgicTextBox
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(nostalgicRichTextBox);
			Name = "NostalgicTextBox";
			Size = new System.Drawing.Size(150, 20);
			ResumeLayout(false);
		}

		#endregion

		private NostalgicRichTextBox nostalgicRichTextBox;
	}
}
