/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Buttons;
using Polycode.NostalgicPlayer.Controls.Extensions;
using Polycode.NostalgicPlayer.Controls.Forms;
using Polycode.NostalgicPlayer.Controls.Images;

namespace Polycode.NostalgicPlayer.Controls.Dialogs
{
	/// <summary>
	/// Show a message box with customized buttons
	/// </summary>
	public partial class CustomMessageBox : NostalgicForm
	{
		/// <summary></summary>
		public enum IconType
		{
			/// <summary></summary>
			Information,
			/// <summary></summary>
			Question,
			/// <summary></summary>
			Warning,
			/// <summary></summary>
			Error
		}

		private const int ButtonSpacing = 5;

		private INostalgicImageBank imageBank;

		private int buttonNumber = 1;
		private char result;

		private int lastWidth = 0;
		private string originalMessage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal CustomMessageBox()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the form
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeForm(string message, string title, IconType icon, INostalgicImageBank imageBank)
		{
			this.imageBank = imageBank;

			result = '\0';		// Default, if the window is closed without pressing any button

			SetMessage(message);
			SetTitle(title);
			SetIcon(icon);
		}



		/********************************************************************/
		/// <summary>
		/// Change the title text
		/// </summary>
		/********************************************************************/
		public void SetTitle(string title)
		{
			Text = title;
		}



		/********************************************************************/
		/// <summary>
		/// Change the icon
		/// </summary>
		/********************************************************************/
		public void SetIcon(IconType iconType)
		{
			switch (iconType)
			{
				case IconType.Error:
				{
					pictureBox.Image = imageBank.General.Error;
					break;
				}

				case IconType.Warning:
				{
					pictureBox.Image = imageBank.General.Warning;
					break;
				}

				case IconType.Information:
				{
					pictureBox.Image = imageBank.General.Information;
					break;
				}

				case IconType.Question:
				{
					pictureBox.Image = imageBank.General.Question;
					break;
				}

				default:
					throw new NotImplementedException($"Icon type of {iconType} not implemented");
			}
		}



		/********************************************************************/
		/// <summary>
		/// Change the message text
		/// </summary>
		/********************************************************************/
		public void SetMessage(string message)
		{
			message = message.Replace("&", "&&");

			messageLabel.Text = message;
			originalMessage = message;
		}



		/********************************************************************/
		/// <summary>
		/// Will add an extra button
		/// </summary>
		/********************************************************************/
		public void AddButton(string label, char result)
		{
			NostalgicButton button = new NostalgicButton();
			button.Name = $"button_{buttonNumber}";
			button.TabIndex = buttonNumber;
			button.Text = label;
			button.Tag = result;
			button.AutoSize = true;
			button.DialogResult = DialogResult.Cancel;
			button.Click += Button_Click;

			// The horizontal position is set in LayoutButtons() once the
			// buttons have their final auto-sized width (the theme font is not
			// applied until the form handle is created, so the width is not
			// known yet here)
			button.Location = new Point(0, 0);

			if (char.IsUpper(result))
			{
				AcceptButton = button;
				button.DialogResult = DialogResult.OK;
				button.TabIndex = 0;
			}

			buttonPanel.Controls.Add(button);

			buttonNumber++;
		}



		/********************************************************************/
		/// <summary>
		/// Will get the button clicked
		/// </summary>
		/********************************************************************/
		public char GetButtonResult(char resultWhenClosing)
		{
			return result == '\0' ? resultWhenClosing : result;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a button is clicked
		/// </summary>
		/********************************************************************/
		private void Button_Click(object sender, EventArgs e)
		{
			result = (char)((NostalgicButton)sender).Tag;
		}



		/********************************************************************/
		/// <summary>
		/// Lay out the buttons in a row and position the button panel just
		/// below the tallest of the icon and the message. Because the icon is
		/// taller than two lines of text, one and two line messages keep the
		/// same height, while three or more lines push the buttons down and
		/// grow the form
		/// </summary>
		/********************************************************************/
		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (!IsHandleCreated)
				return;

			base.OnLayout(levent);

			if (buttonPanel == null)
				return;

			LayoutButtons();
			PositionButtonPanel();
		}



		/********************************************************************/
		/// <summary>
		/// Place the buttons next to each other from left to right, using
		/// their final auto-sized widths
		/// </summary>
		/********************************************************************/
		private void LayoutButtons()
		{
			int x = 0;

			foreach (Control button in buttonPanel.Controls)
			{
				if (button.Left != x)
					button.Left = x;

				x += button.Width + ButtonSpacing;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Position the button panel just below the icon and the message
		/// </summary>
		/********************************************************************/
		private void PositionButtonPanel()
		{
			int contentBottom = Math.Max(pictureBox.Bottom, messagePanel.Bottom);
			int desiredTop = contentBottom + buttonPanel.Margin.Top;

			if (buttonPanel.Top != desiredTop)
				buttonPanel.Top = desiredTop;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the window is resized
		/// </summary>
		/********************************************************************/
		private void CustomMessageBox_Resize(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(originalMessage) && (ClientSize.Width != lastWidth))
			{
				lastWidth = ClientSize.Width;

				// Check each line to see if it viewable and if not, add extra new lines
				int maxWidth = ClientSize.Width - messagePanel.Left;
				messageLabel.Text = string.Join("\r\n", originalMessage.SplitIntoLines(messageLabel.Handle, maxWidth, messageLabel.Font));
			}
		}
	}
}
