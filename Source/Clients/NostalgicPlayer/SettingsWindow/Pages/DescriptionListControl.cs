/******************************************************************************/
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

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// This class overrides some method in the DataGrid control
	/// to implement wanted features
	/// </summary>
	public class DescriptionListControl : KryptonDataGridView
	{
		public DescriptionListControl()
		{
			VerticalScrollBar.Visible = true;
			VerticalScrollBar.VisibleChanged += VerticalScrollBar_VisibleChanged;
//			this.VerticalScrollBar.SetBounds(this.VerticalScrollBar.Location.X, this.VerticalScrollBar.Location.Y, this.VerticalScrollBar.Width, this.Height);
		}

		private void VerticalScrollBar_VisibleChanged(object? sender, EventArgs e)
		{
			if (!VerticalScrollBar.Visible)
			{
				int width = VerticalScrollBar.Width;
				VerticalScrollBar.Location = new Point(ClientRectangle.Width - width - 2, 21);
				VerticalScrollBar.Size = new Size(width, ClientRectangle.Height - 21 - 2);
				VerticalScrollBar.Show();
			}
		}

		/********************************************************************/
		/// <summary>
		/// This is overridden and do not call the base method, which means
		/// nothing can be selected
		/// </summary>
		/********************************************************************/
		protected override void SetSelectedRowCore(int rowIndex, bool selected)
		{
		}
	}
}
