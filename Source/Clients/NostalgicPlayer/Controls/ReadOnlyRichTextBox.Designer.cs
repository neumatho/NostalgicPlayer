
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	partial class ReadOnlyRichTextBox
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
			this.richTextBox = new Krypton.Toolkit.KryptonRichTextBox();
			this.looseFocusLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// richTextBox
			// 
			this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox.Location = new System.Drawing.Point(0, 0);
			this.richTextBox.Name = "richTextBox";
			this.richTextBox.ReadOnly = true;
			this.richTextBox.Size = new System.Drawing.Size(150, 150);
			this.richTextBox.TabIndex = 0;
			this.richTextBox.TabStop = false;
			this.richTextBox.Text = "";
			this.richTextBox.WordWrap = false;
			// 
			// looseFocusLabel
			// 
			this.looseFocusLabel.AutoSize = true;
			this.looseFocusLabel.Location = new System.Drawing.Point(0, 0);
			this.looseFocusLabel.Name = "looseFocusLabel";
			this.looseFocusLabel.Size = new System.Drawing.Size(15, 15);
			this.looseFocusLabel.TabIndex = 1;
			this.looseFocusLabel.Text = "ff";
			this.looseFocusLabel.Visible = false;
			// 
			// ReadOnlyRichTextBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.richTextBox);
			this.Controls.Add(this.looseFocusLabel);
			this.Name = "ReadOnlyRichTextBox";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Krypton.Toolkit.KryptonRichTextBox richTextBox;
		private System.Windows.Forms.Label looseFocusLabel;
	}
}
