/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Inputs.RichText
{
	/// <summary>
	/// A single run of text sharing the same font and color
	/// </summary>
	internal sealed class RichTextRun
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public RichTextRun(string text, Font font, Color? color)
		{
			Text = text;
			Font = font;
			Color = color;
		}



		/********************************************************************/
		/// <summary>
		/// The text of the run
		/// </summary>
		/********************************************************************/
		public string Text
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The font to render the run with, or null to use the control's
		/// default font
		/// </summary>
		/********************************************************************/
		public Font Font
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The color to render the run with, or null to use the theme's
		/// default text color
		/// </summary>
		/********************************************************************/
		public Color? Color
		{
			get;
		}
	}
}
