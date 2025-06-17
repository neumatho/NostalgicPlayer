/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Events
{
	/// <summary></summary>
	public delegate void TrackEventHandler(object sender, TrackEventArgs e);

	/// <summary>
	/// Event class holding needed information when playing a track
	/// </summary>
	public class TrackEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TrackEventArgs(AudiusListItem item)
		{
			Items = [ item ];
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TrackEventArgs(AudiusListItem[] items)
		{
			Items = items;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the track item
		/// </summary>
		/********************************************************************/
		public AudiusListItem[] Items
		{
			get;
		}
	}
}
