/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Native;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	/// <summary>
	/// Wraps a RichTextBox and make it read only
	/// </summary>
	public partial class ReadOnlyRichTextBox : UserControl
	{
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
			richTextBox.RichTextBox.HandleCreated += (sender, args) => { User32.HideCaret(richTextBox.RichTextBox.Handle); };
			richTextBox.RichTextBox.GotFocus += (sender, args) => { User32.HideCaret(richTextBox.RichTextBox.Handle); looseFocusLabel.Focus(); };
			richTextBox.RichTextBox.MouseDown += (sender, args) => { User32.HideCaret(richTextBox.RichTextBox.Handle); };
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
