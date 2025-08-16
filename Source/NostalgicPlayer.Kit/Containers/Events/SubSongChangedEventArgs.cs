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
	public delegate void SubSongChangedEventHandler(object sender, SubSongChangedEventArgs e);

	/// <summary>
	/// Event class holding needed information when sending a sub-song changed event
	/// </summary>
	public class SubSongChangedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SubSongChangedEventArgs(int newSubSong, DurationInfo durationInfo)
		{
			SubSong = newSubSong;
			DurationInfo = durationInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the sub-song that has been changed to
		/// </summary>
		/********************************************************************/
		public int SubSong
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the duration information
		/// </summary>
		/********************************************************************/
		public DurationInfo DurationInfo
		{
			get;
		}
	}
}
