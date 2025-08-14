﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.GuiKit.Extensions;

namespace Polycode.NostalgicPlayer.GuiKit.Controls
{
	/// <summary>
	/// Show a message box with customized buttons
	/// </summary>
	public partial class CustomMessageBox : KryptonForm
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

		private int buttonNumber = 1;
		private char result;

		private int lastWidth = 0;
		private string originalMessage;

		private readonly List<Image> imageList;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CustomMessageBox()
		{
			InitializeComponent();

			imageList = new List<Image>
			{
				Resources.Information,
				Resources.Question,
				Resources.Warning,
				Resources.Error
			};

			result = '\0';		// Default, if the window is closed without pressing any button
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CustomMessageBox(string message, string title, IconType icon) : this()
		{
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
			pictureBox.Image = imageList[(int)iconType];
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
			KryptonButton button = new KryptonButton();
			button.Name = $"button_{buttonNumber}";
			button.TabIndex = buttonNumber;
			button.Text = label;
			button.Tag = result;
			button.AutoSize = true;
			button.DialogResult = DialogResult.Cancel;
			button.LocalCustomPalette = fontPalette;
			button.Click += Button_Click;

			if (buttonNumber == 1)
				button.Location = new Point(0, 0);
			else
			{
				KryptonButton previousButton = (KryptonButton)buttonPanel.Controls[buttonNumber - 2];
				button.Location = new Point(previousButton.Location.X + previousButton.Size.Width + 5, 0);
			}

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
			result = (char)((KryptonButton)sender).Tag;
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
