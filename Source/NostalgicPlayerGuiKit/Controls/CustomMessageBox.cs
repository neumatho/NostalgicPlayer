﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
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
	}
}
