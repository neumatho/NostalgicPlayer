/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Controls.Inputs.RichText
{
	/// <summary>
	/// A paragraph holds a list of runs plus paragraph level indentation.
	/// A paragraph is the unit that word wrapping is performed within
	/// </summary>
	internal sealed class RichTextParagraph
	{
		/********************************************************************/
		/// <summary>
		/// The distance in pixels between the left edge of the text area and
		/// the left edge of the paragraph (applies to all lines)
		/// </summary>
		/********************************************************************/
		public int Indent
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// The additional distance in pixels that wrapped lines (all lines
		/// except the first) are indented compared to the first line
		/// </summary>
		/********************************************************************/
		public int HangingIndent
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// The runs that make up the paragraph text
		/// </summary>
		/********************************************************************/
		public List<RichTextRun> Runs
		{
			get;
		} = new List<RichTextRun>();



		/********************************************************************/
		/// <summary>
		/// The total number of characters in the paragraph
		/// </summary>
		/********************************************************************/
		public int Length
		{
			get
			{
				int length = 0;

				foreach (RichTextRun run in Runs)
					length += run.Text.Length;

				return length;
			}
		}
	}
}
