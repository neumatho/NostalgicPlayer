/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Events
{
	/// <summary>
	/// </summary>
	public delegate void SongRowChangedEventHandler(object sender, SongRowChangedEventArgs e);

	/// <summary>
	/// Event class holding needed information when the song row changes
	/// </summary>
	public class SongRowChangedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SongRowChangedEventArgs(SongRowChangeInfo rowInfo)
		{
			RowInfo = rowInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the row change information
		/// </summary>
		/********************************************************************/
		public SongRowChangeInfo RowInfo
		{
			get;
		}
	}
}
