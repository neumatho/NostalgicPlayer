/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Controls.Lists
{
	/// <summary>
	/// Helper class to store list information
	/// </summary>
	public class NostalgicListItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicListItem()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicListItem(string text)
		{
			Text = text;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the text to be shown
		/// </summary>
		/********************************************************************/
		public string Text
		{
			get;

			set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds any information you like
		/// </summary>
		/********************************************************************/
		public object Tag
		{
			get;

			set;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Text;
		}
	}
}
