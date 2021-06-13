/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
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
		public DurationInfo(TimeSpan totalTime, PositionInfo[] posInfo)
		{
			TotalTime = totalTime;
			PositionInfo = posInfo;
			StartPosition = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DurationInfo(TimeSpan totalTime, PositionInfo[] posInfo, int startPosition)
		{
			TotalTime = totalTime;
			PositionInfo = posInfo;
			StartPosition = startPosition;
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
		/// Holds the start position (only needed for position sub-songs)
		/// </summary>
		/********************************************************************/
		public int StartPosition
		{
			get;
		}
	}
}
