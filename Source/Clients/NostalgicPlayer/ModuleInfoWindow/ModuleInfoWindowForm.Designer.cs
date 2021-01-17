
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
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.moduleInfoDataGridView = new Polycode.NostalgicPlayer.Client.GuiPlayer.ModuleInfoWindow.ModuleInfoListControl();
			this.listGroup = new Krypton.Toolkit.KryptonGroup();
			((System.ComponentModel.ISupportInitialize)(this.moduleInfoDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.listGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.listGroup.Panel)).BeginInit();
			this.listGroup.Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// moduleInfoDataGridView
			// 
			this.moduleInfoDataGridView.AllowUserToAddRows = false;
			this.moduleInfoDataGridView.AllowUserToDeleteRows = false;
			this.moduleInfoDataGridView.AllowUserToResizeRows = false;
			this.moduleInfoDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.moduleInfoDataGridView.Location = new System.Drawing.Point(0, 0);
			this.moduleInfoDataGridView.Name = "moduleInfoDataGridView";
			this.moduleInfoDataGridView.ReadOnly = true;
			this.moduleInfoDataGridView.RowHeadersVisible = false;
			this.moduleInfoDataGridView.RowTemplate.Height = 25;
			this.moduleInfoDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.moduleInfoDataGridView.Size = new System.Drawing.Size(282, 182);
			this.moduleInfoDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.moduleInfoDataGridView.StateCommon.DataCell.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleInfoDataGridView.StateCommon.DataCell.Content.Padding = new System.Windows.Forms.Padding(0);
			this.moduleInfoDataGridView.StateCommon.HeaderColumn.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.moduleInfoDataGridView.TabIndex = 0;
			// 
			// listGroup
			// 
			this.listGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listGroup.Location = new System.Drawing.Point(8, 8);
			this.listGroup.Name = "listGroup";
			// 
			// 
			// 
			this.listGroup.Panel.Controls.Add(this.moduleInfoDataGridView);
			this.listGroup.Size = new System.Drawing.Size(284, 184);
			this.listGroup.TabIndex = 1;
			// 
			// ModuleInfoWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(300, 200);
			this.Controls.Add(this.listGroup);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(316, 239);
			this.Name = "ModuleInfoWindowForm";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ModuleInfoWindowForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.moduleInfoDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.listGroup.Panel)).EndInit();
			this.listGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listGroup)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private ModuleInfoListControl moduleInfoDataGridView;
		private Krypton.Toolkit.KryptonGroup listGroup;
	}
}