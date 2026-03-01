/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// A notification bar control for displaying warnings/info with action buttons
	/// </summary>
	internal sealed class InfoBarControl : UserControl
	{
		private readonly Button actionButton;
		private readonly Button closeButton;
		private readonly Button ignoreButton;
		private readonly Label messageLabel;

		private string serviceId;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public InfoBarControl()
		{
			// Control settings
			BackColor = Color.FromArgb(255, 243, 205); // Soft yellow/orange
			Height = 34;
			Dock = DockStyle.Top;
			Padding = new Padding(8, 4, 8, 4);

			// Create table layout for proper arrangement
			TableLayoutPanel layout1 = new() {Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, BackColor = Color.Transparent};
			layout1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Message takes remaining space
			layout1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Action button
			layout1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Ignore button
			layout1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Close button
			layout1.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

			// Message label
			messageLabel = new Label
			{
				AutoSize = false,
				Dock = DockStyle.Fill,
				ForeColor = Color.FromArgb(133, 100, 4), // Dark amber text
				TextAlign = ContentAlignment.MiddleLeft
			};

			// Action button (Update Now)
			actionButton = new Button
			{
				FlatStyle = FlatStyle.Flat,
				AutoSize = true,
				Cursor = Cursors.Hand,
				BackColor = Color.FromArgb(133, 100, 4),
				ForeColor = Color.White,
				Margin = new Padding(4, 0, 4, 0)
			};
			actionButton.FlatAppearance.BorderSize = 0;
			actionButton.Click += ActionButton_Click;

			// Ignore button
			ignoreButton = new Button
			{
				FlatStyle = FlatStyle.Flat,
				AutoSize = true,
				Cursor = Cursors.Hand,
				ForeColor = Color.FromArgb(133, 100, 4),
				BackColor = Color.Transparent,
				Margin = new Padding(4, 0, 4, 0)
			};
			ignoreButton.FlatAppearance.BorderSize = 1;
			ignoreButton.FlatAppearance.BorderColor = Color.FromArgb(133, 100, 4);
			ignoreButton.Click += IgnoreButton_Click;

			// Close/Dismiss button
			closeButton = new Button
			{
				FlatStyle = FlatStyle.Flat,
				AutoSize = true,
				Cursor = Cursors.Hand,
				ForeColor = Color.FromArgb(133, 100, 4),
				BackColor = Color.Transparent,
				Margin = new Padding(4, 0, 0, 0)
			};
			closeButton.FlatAppearance.BorderSize = 0;
			closeButton.Click += CloseButton_Click;

			// Add controls to layout
			layout1.Controls.Add(messageLabel, 0, 0);
			layout1.Controls.Add(actionButton, 1, 0);
			layout1.Controls.Add(ignoreButton, 2, 0);
			layout1.Controls.Add(closeButton, 3, 0);

			Controls.Add(layout1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/

		public event EventHandler<InfoBarActionEventArgs> ActionClicked;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler<InfoBarActionEventArgs> IgnoreClicked;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler<InfoBarActionEventArgs> CloseClicked;



		/********************************************************************/
		/// <summary>
		/// Handle action button click
		/// </summary>
		/********************************************************************/
		private void ActionButton_Click(object sender, EventArgs e)
		{
			ActionClicked?.Invoke(this, new InfoBarActionEventArgs(serviceId));
		}



		/********************************************************************/
		/// <summary>
		/// Handle ignore button click
		/// </summary>
		/********************************************************************/
		private void IgnoreButton_Click(object sender, EventArgs e)
		{
			IgnoreClicked?.Invoke(this, new InfoBarActionEventArgs(serviceId));
		}



		/********************************************************************/
		/// <summary>
		/// Handle close button click
		/// </summary>
		/********************************************************************/
		private void CloseButton_Click(object sender, EventArgs e)
		{
			CloseClicked?.Invoke(this, new InfoBarActionEventArgs(serviceId));
		}



		/********************************************************************/
		/// <summary>
		/// Configure the info bar with message and button texts
		/// </summary>
		/********************************************************************/
		public void Configure(string newServiceId, string message, string actionText, string ignoreText, string dismissText)
		{
			serviceId = newServiceId;
			messageLabel.Text = message;
			actionButton.Text = actionText;
			ignoreButton.Text = ignoreText;
			closeButton.Text = dismissText;
		}
	}



	/********************************************************************/
	/// <summary>
	/// Event args for InfoBar button clicks
	/// </summary>
	/********************************************************************/
	internal class InfoBarActionEventArgs(string serviceId) : EventArgs
	{
		public string ServiceId
		{
			get;
		} = serviceId;
	}
}
