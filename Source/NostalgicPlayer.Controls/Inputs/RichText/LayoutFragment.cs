/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Inputs.RichText
{
	/// <summary>
	/// A single piece of text on a line, sharing one font and color
	/// </summary>
	internal sealed class LayoutFragment
	{
		/********************************************************************/
		/// <summary>
		/// The text of the fragment
		/// </summary>
		/********************************************************************/
		public string Text
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// The font to render with
		/// </summary>
		/********************************************************************/
		public Font Font
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// The color to render with, or null for the default text color
		/// </summary>
		/********************************************************************/
		public Color? Color
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// The absolute left pixel position
		/// </summary>
		/********************************************************************/
		public int X
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// The pixel width of the text
		/// </summary>
		/********************************************************************/
		public int Width
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// The global character index of the first character
		/// </summary>
		/********************************************************************/
		public int StartIndex
		{
			get; init;
		}
	}
}
