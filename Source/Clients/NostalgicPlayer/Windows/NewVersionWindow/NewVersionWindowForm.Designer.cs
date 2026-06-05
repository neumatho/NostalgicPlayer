
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.NewVersionWindow
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewVersionWindowForm));
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			label = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			fontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			okButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicButton();
			historyRichTextBox = new Polycode.NostalgicPlayer.Controls.Inputs.NostalgicRichTextView();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// label
			// 
			label.Location = new System.Drawing.Point(8, 8);
			label.Name = "label";
			controlResource.SetResourceKey(label, "IDS_NEWVERSION_MESSAGE");
			label.Size = new System.Drawing.Size(451, 31);
			label.TabIndex = 0;
			label.Text = "Congratulations! A new version of NostalgicPlayer has been installed. See below\r\nwhat has changed since your previous version.";
			label.UseFont = fontConfiguration;
			// 
			// fontConfiguration
			// 
			fontConfiguration.FontSize = 1;
			// 
			// okButton
			// 
			okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			okButton.Location = new System.Drawing.Point(369, 255);
			okButton.Name = "okButton";
			controlResource.SetResourceKey(okButton, "IDS_BUT_OK");
			okButton.Size = new System.Drawing.Size(90, 25);
			okButton.TabIndex = 2;
			okButton.Text = "Ok";
			okButton.UseFont = fontConfiguration;
			// 
			// historyRichTextBox
			// 
			historyRichTextBox.Location = new System.Drawing.Point(8, 47);
			historyRichTextBox.Name = "historyRichTextBox";
			historyRichTextBox.Size = new System.Drawing.Size(451, 200);
			historyRichTextBox.TabIndex = 3;
			// 
			// NewVersionWindowForm
			// 
			AllowResizing = false;
			ClientSize = new System.Drawing.Size(467, 288);
			Controls.Add(historyRichTextBox);
			Controls.Add(okButton);
			Controls.Add(label);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "NewVersionWindowForm";
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);

		}

		#endregion
		private Kit.Gui.Designer.ControlResource controlResource;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel label;
		private NostalgicPlayer.Controls.Buttons.NostalgicButton okButton;
		private NostalgicPlayer.Controls.Inputs.NostalgicRichTextView historyRichTextBox;
		private NostalgicPlayer.Controls.Components.FontConfiguration fontConfiguration;
	}
}