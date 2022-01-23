/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;

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

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CustomMessageBox()
		{
			InitializeComponent();
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
			pictureBox.Image = imageList.Images[(int)iconType];
		}



		/********************************************************************/
		/// <summary>
		/// Change the message text
		/// </summary>
		/********************************************************************/
		public void SetMessage(string message)
		{
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
		public char GetButtonResult()
		{
			return result;
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
				string message = originalMessage;

				List<string> lines = new List<string>();

				using (Graphics g = Graphics.FromHwnd(messageLabel.Handle))
				{
					while (!string.IsNullOrEmpty(message))
					{
						string tempStr;

						// See if there is any newlines
						int index = message.IndexOf('\n');
						if (index != -1)
						{
							// There is, get the line
							tempStr = message.Substring(0, index);
							message = message.Substring(index + 1);
						}
						else
						{
							tempStr = message;
							message = string.Empty;
						}

						// Adjust the description line
						tempStr = tempStr.Trim();

						// Just add empty lines
						if (string.IsNullOrEmpty(tempStr))
							lines.Add(string.Empty);
						else
						{
							do
							{
								int lineWidth = (int)g.MeasureString(tempStr, messageLabel.Font).Width;

								string tempStr1 = string.Empty;

								while (lineWidth > maxWidth)
								{
									// We need to split the line
									index = tempStr.LastIndexOf(' ');
									if (index != -1)
									{
										// Found a space, check if the line can be showed now
										tempStr1 = tempStr.Substring(index) + tempStr1;
										tempStr = tempStr.Substring(0, index);

										lineWidth = (int)g.MeasureString(tempStr, messageLabel.Font).Width;
									}
									else
									{
										// Well, the line can't be showed and we can't split it :-(
										break;
									}
								}

								// Adjust the description line
								tempStr = tempStr.Trim();

								// Add the line in the grid
								lines.Add(tempStr);

								tempStr = tempStr1.Trim();
							}
							while (!string.IsNullOrEmpty(tempStr));
						}
					}
				}

				messageLabel.Text = string.Join("\r\n", lines);
			}
		}
	}
}
