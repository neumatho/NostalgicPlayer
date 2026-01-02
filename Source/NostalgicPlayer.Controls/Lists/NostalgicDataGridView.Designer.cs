namespace Polycode.NostalgicPlayer.Controls.Lists
{
	partial class NostalgicDataGridView
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
			nostalgicDataGridViewInternal = new NostalgicDataGridViewInternal();
			nostalgicVScrollBar = new NostalgicVScrollBar();
			nostalgicHScrollBar = new NostalgicHScrollBar();
			cornerPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)nostalgicDataGridViewInternal).BeginInit();
			SuspendLayout();
			// 
			// nostalgicDataGridViewInternal
			// 
			nostalgicDataGridViewInternal.AllowUserToAddRows = false;
			nostalgicDataGridViewInternal.AllowUserToDeleteRows = false;
			nostalgicDataGridViewInternal.AllowUserToResizeRows = false;
			nostalgicDataGridViewInternal.BorderStyle = System.Windows.Forms.BorderStyle.None;
			nostalgicDataGridViewInternal.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
			nostalgicDataGridViewInternal.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			nostalgicDataGridViewInternal.EnableHeadersVisualStyles = false;
			nostalgicDataGridViewInternal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			nostalgicDataGridViewInternal.Location = new System.Drawing.Point(0, 0);
			nostalgicDataGridViewInternal.Name = "nostalgicDataGridViewInternal";
			nostalgicDataGridViewInternal.ReadOnly = true;
			nostalgicDataGridViewInternal.RowHeadersVisible = false;
			nostalgicDataGridViewInternal.ScrollBars = System.Windows.Forms.ScrollBars.None;
			nostalgicDataGridViewInternal.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			nostalgicDataGridViewInternal.ShowCellErrors = false;
			nostalgicDataGridViewInternal.ShowEditingIcon = false;
			nostalgicDataGridViewInternal.ShowRowErrors = false;
			nostalgicDataGridViewInternal.Size = new System.Drawing.Size(150, 150);
			nostalgicDataGridViewInternal.TabIndex = 0;
			nostalgicDataGridViewInternal.SelectionChanged += SelectionChangedHandler;
			nostalgicDataGridViewInternal.DoubleClick += DoubleClickHandler;
			// 
			// nostalgicVScrollBar
			// 
			nostalgicVScrollBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			nostalgicVScrollBar.Location = new System.Drawing.Point(133, 0);
			nostalgicVScrollBar.Name = "nostalgicVScrollBar";
			nostalgicVScrollBar.Size = new System.Drawing.Size(17, 133);
			nostalgicVScrollBar.TabIndex = 1;
			// 
			// nostalgicHScrollBar
			// 
			nostalgicHScrollBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nostalgicHScrollBar.Location = new System.Drawing.Point(0, 133);
			nostalgicHScrollBar.Name = "nostalgicHScrollBar";
			nostalgicHScrollBar.Size = new System.Drawing.Size(133, 17);
			nostalgicHScrollBar.TabIndex = 2;
			// 
			// cornerPanel
			// 
			cornerPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			cornerPanel.Location = new System.Drawing.Point(133, 133);
			cornerPanel.Name = "cornerPanel";
			cornerPanel.Size = new System.Drawing.Size(17, 17);
			cornerPanel.TabIndex = 3;
			// 
			// NostalgicDataGridView
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(cornerPanel);
			Controls.Add(nostalgicHScrollBar);
			Controls.Add(nostalgicVScrollBar);
			Controls.Add(nostalgicDataGridViewInternal);
			Name = "NostalgicDataGridView";
			((System.ComponentModel.ISupportInitialize)nostalgicDataGridViewInternal).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private NostalgicDataGridViewInternal nostalgicDataGridViewInternal;
		private NostalgicVScrollBar nostalgicVScrollBar;
		private NostalgicHScrollBar nostalgicHScrollBar;
		private System.Windows.Forms.Panel cornerPanel;
	}
}
