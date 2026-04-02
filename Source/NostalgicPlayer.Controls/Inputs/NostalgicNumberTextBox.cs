/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Controls.Inputs
{
	/// <summary>
	/// Themed number text box with custom rendering
	/// </summary>
	public class NostalgicNumberTextBox : NostalgicTextBox
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
