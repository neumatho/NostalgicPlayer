/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Controls.Navigation;

namespace Polycode.NostalgicPlayer.Controls.Events
{
	/// <summary></summary>
	public delegate void BreadcrumbPathChangedEventHandler(object sender, BreadcrumbPathChangedEventArgs e);

	/// <summary>
	/// Event class holding needed information when the path in a
	/// breadcrumb control has changed
	/// </summary>
	public class BreadcrumbPathChangedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BreadcrumbPathChangedEventArgs(IReadOnlyList<NostalgicBreadcrumbItem> oldItems, IReadOnlyList<NostalgicBreadcrumbItem> newItems)
		{
			OldItems = oldItems;
			NewItems = newItems;
		}



		/********************************************************************/
		/// <summary>
		/// Holding all the items before the change
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<NostalgicBreadcrumbItem> OldItems
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holding all the items after the change
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<NostalgicBreadcrumbItem> NewItems
		{
			get;
		}
	}
}
