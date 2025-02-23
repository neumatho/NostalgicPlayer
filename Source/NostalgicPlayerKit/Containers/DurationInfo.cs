/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds information about a single sub-song and the duration of it
	/// </summary>
	public class DurationInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DurationInfo(TimeSpan totalTime, PositionInfo[] positionInfo, TimeSpan[] playerPositionTime, TimeSpan? restartTime = null)
		{
			TotalTime = TimeSpan.FromSeconds(Math.Round(totalTime.TotalSeconds, MidpointRounding.AwayFromZero));
			PositionInfo = positionInfo;
			PlayerPositionTime = playerPositionTime;
			RestartTime = restartTime;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DurationInfo(TimeSpan totalTime, PositionInfo[] positionInfo) : this(totalTime, positionInfo, Array.Empty<TimeSpan>())
		{
		}



		/********************************************************************/
		/// <summary>
		/// Holds the total time of the song
		/// </summary>
		/********************************************************************/
		public TimeSpan TotalTime
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds information for each position
		/// </summary>
		/********************************************************************/
		public PositionInfo[] PositionInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the time on each player specific position
		/// </summary>
		/********************************************************************/
		public TimeSpan[] PlayerPositionTime
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the restart time if any
		/// </summary>
		/********************************************************************/
		public TimeSpan? RestartTime
		{
			get;
		}
	}
}
