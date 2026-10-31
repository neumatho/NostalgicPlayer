/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Controls.Inputs.RichText
{
	/// <summary>
	/// A single visual line (after word wrapping)
	/// </summary>
	internal sealed class LayoutLine
	{
		/********************************************************************/
		/// <summary>
		/// The content relative top position (0 = top of first line)
		/// </summary>
		/********************************************************************/
		public int Top
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// The height of the line
		/// </summary>
		/********************************************************************/
		public int Height
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// The absolute left pixel position where the line begins
		/// </summary>
		/********************************************************************/
		public int Left
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// The global character index of the first character on the line
		/// </summary>
		/********************************************************************/
		public int FirstIndex
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// The global character index right after the last character on the line
		/// </summary>
		/********************************************************************/
		public int EndIndex
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// The fragments on the line, left to right
		/// </summary>
		/********************************************************************/
		public List<LayoutFragment> Fragments
		{
			get;
		} = new List<LayoutFragment>();
	}
}
