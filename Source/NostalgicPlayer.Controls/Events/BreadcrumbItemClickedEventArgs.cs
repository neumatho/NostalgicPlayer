/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Controls.Navigation;

namespace Polycode.NostalgicPlayer.Controls.Events
{
	/// <summary></summary>
	public delegate void BreadcrumbItemClickedEventHandler(object sender, BreadcrumbItemClickedEventArgs e);

	/// <summary>
	/// Event class holding needed information when an item in a breadcrumb
	/// control is clicked
	/// </summary>
	public class BreadcrumbItemClickedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal BreadcrumbItemClickedEventArgs(NostalgicBreadcrumbItem item, int index)
		{
			Item = item;
			Index = index;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the item that was clicked
		/// </summary>
		/********************************************************************/
		public NostalgicBreadcrumbItem Item
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the index of the item that was clicked
		/// </summary>
		/********************************************************************/
		public int Index
		{
			get;
		}
	}
}
