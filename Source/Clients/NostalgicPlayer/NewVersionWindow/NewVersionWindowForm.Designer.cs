
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.HelpWindow
{
	partial class NewVersionWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewVersionWindowForm));
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.label = new Krypton.Toolkit.KryptonLabel();
			this.urlTextBox = new Krypton.Toolkit.KryptonTextBox();
			this.kryptonButton1 = new Krypton.Toolkit.KryptonButton();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			this.SuspendLayout();
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// label
			// 
			this.label.Location = new System.Drawing.Point(8, 8);
			this.label.Name = "label";
			this.controlResource.SetResourceKey(this.label, "IDS_NEWVERSION_MESSAGE");
			this.label.Size = new System.Drawing.Size(351, 47);
			this.label.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label.TabIndex = 0;
			this.label.Values.Text = "Congratulations! A new version of NostalgicPlayer has been\r\ninstalled. Copy the U" +
    "RL below and insert into a browser to see\r\nwhat is new.";
			// 
			// urlTextBox
			// 
			this.urlTextBox.Location = new System.Drawing.Point(12, 67);
			this.urlTextBox.Name = "urlTextBox";
			this.urlTextBox.ReadOnly = true;
			this.controlResource.SetResourceKey(this.urlTextBox, null);
			this.urlTextBox.Size = new System.Drawing.Size(360, 16);
			this.urlTextBox.StateCommon.Back.Color1 = System.Drawing.SystemColors.Control;
			this.urlTextBox.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.urlTextBox.StateCommon.Content.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.urlTextBox.StateCommon.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Inherit;
			this.urlTextBox.TabIndex = 1;
			this.urlTextBox.Text = "https://www.nostalgicplayer.dk";
			// 
			// kryptonButton1
			// 
			this.kryptonButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.kryptonButton1.Location = new System.Drawing.Point(282, 105);
			this.kryptonButton1.Name = "kryptonButton1";
			this.controlResource.SetResourceKey(this.kryptonButton1, "IDS_BUT_OK");
			this.kryptonButton1.Size = new System.Drawing.Size(90, 25);
			this.kryptonButton1.TabIndex = 2;
			this.kryptonButton1.Values.Text = "Ok";
			// 
			// NewVersionWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(384, 142);
			this.Controls.Add(this.kryptonButton1);
			this.Controls.Add(this.urlTextBox);
			this.Controls.Add(this.label);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NewVersionWindowForm";
			this.controlResource.SetResourceKey(this, null);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonLabel label;
		private Krypton.Toolkit.KryptonTextBox urlTextBox;
		private Krypton.Toolkit.KryptonButton kryptonButton1;
	}
}