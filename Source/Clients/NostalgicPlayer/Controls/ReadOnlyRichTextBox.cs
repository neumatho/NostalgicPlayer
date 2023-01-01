/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	/// <summary>
	/// Wraps a RichTextBox and make it read only
	/// </summary>
	public partial class ReadOnlyRichTextBox : UserControl
	{
		[DllImport("user32.dll")]
		private static extern bool HideCaret(IntPtr hWnd);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReadOnlyRichTextBox()
		{
			InitializeComponent();

			richTextBox.StateCommon.Border.Color1 = Color.FromArgb(133, 158, 191);

			// Setup events
			richTextBox.RichTextBox.HandleCreated += (sender, args) => { HideCaret(richTextBox.RichTextBox.Handle); };
			richTextBox.RichTextBox.GotFocus += (sender, args) => { HideCaret(richTextBox.RichTextBox.Handle); looseFocusLabel.Focus(); };
			richTextBox.RichTextBox.MouseDown += (sender, args) => { HideCaret(richTextBox.RichTextBox.Handle); };
		}



		/********************************************************************/
		/// <summary>
		/// Clear all the text
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			richTextBox.Text = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the collection of all the text lines
		/// </summary>
		/********************************************************************/
		public string[] Lines
		{
			get
			{
				return richTextBox.Lines;
			}

			set
			{
				richTextBox.Lines = value;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Hold the font to be used
		/// </summary>
		/********************************************************************/
		public void SetFont(Font font)
		{
			richTextBox.StateCommon.Content.Font = font;
			richTextBox.RichTextBox.Invalidate();
		}
	}
}
