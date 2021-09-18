/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	/// <summary>
	/// A text box control which only allow numbers
	/// </summary>
	public class NumberTextBox : KryptonTextBox
	{
		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed
		/// </summary>
		/********************************************************************/
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
				e.Handled = true;

			base.OnKeyPress(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the text has been changed
		/// </summary>
		/********************************************************************/
		protected override void OnTextChanged(EventArgs e)
		{
			if (!Regex.IsMatch(Text, "^[0-9]*"))
				Text = string.Empty;

			base.OnTextChanged(e);
		}
	}
}
