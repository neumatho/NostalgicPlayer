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
			nostalgicTextBoxInternal = new NostalgicTextBoxInternal();
			SuspendLayout();
			// 
			// nostalgicTextBoxInternal
			// 
			nostalgicTextBoxInternal.Dock = System.Windows.Forms.DockStyle.Fill;
			nostalgicTextBoxInternal.Location = new System.Drawing.Point(0, 0);
			nostalgicTextBoxInternal.Name = "nostalgicTextBoxInternal";
			nostalgicTextBoxInternal.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			nostalgicTextBoxInternal.SingleLine = true;
			nostalgicTextBoxInternal.Size = new System.Drawing.Size(150, 20);
			nostalgicTextBoxInternal.TabIndex = 0;
			nostalgicTextBoxInternal.Text = "";
			nostalgicTextBoxInternal.WordWrap = false;
			// 
			// NostalgicTextBox
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(nostalgicTextBoxInternal);
			Name = "NostalgicTextBox";
			Size = new System.Drawing.Size(150, 20);
			ResumeLayout(false);
		}

		#endregion

		private NostalgicTextBoxInternal nostalgicTextBoxInternal;
	}
}
