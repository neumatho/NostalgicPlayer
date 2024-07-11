/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds information about a single position
	/// </summary>
	public class PositionInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor (used by module players)
		/// </summary>
		/********************************************************************/
		public PositionInfo(TimeSpan time, float playingFrequency, bool amigaFilter, ISnapshot snapshot)
		{
			Time = time;
			PlayingFrequency = playingFrequency;
			AmigaFilter = amigaFilter;
			Snapshot = snapshot;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor (used by sample players)
		/// </summary>
		/********************************************************************/
		public PositionInfo(TimeSpan time)
		{
			Time = time;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the time at the current position
		/// </summary>
		/********************************************************************/
		public TimeSpan Time
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the playing frequency at the current position
		/// </summary>
		/********************************************************************/
		internal float PlayingFrequency
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the state of the Amiga filter
		/// </summary>
		/********************************************************************/
		internal bool AmigaFilter
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the snapshot from the player
		/// </summary>
		/********************************************************************/
		internal ISnapshot Snapshot
		{
			get;
		}
	}
}
