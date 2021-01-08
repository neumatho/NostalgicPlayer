/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Containers
{
	/// <summary>
	/// This class holds all information about the player which changes while playing
	/// </summary>
	public class ModuleInfoFloating
	{
		private int songPosition;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoFloating()
		{
			CurrentSong = 0;
			TotalTime = new TimeSpan(0);
			songPosition = 0;
			SongLength = 0;
			PositionTimes = null;
			ModuleInformation = null;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal ModuleInfoFloating(int songNumber, TimeSpan totalTime, int songLength, TimeSpan[] positionTimes, string[] moduleInfo)
		{
			CurrentSong = songNumber;
			TotalTime = totalTime;
			songPosition = 0;
			SongLength = songLength;
			PositionTimes = positionTimes;
			ModuleInformation = moduleInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the current song playing
		/// </summary>
		/********************************************************************/
		public int CurrentSong
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the total time of the current song
		/// </summary>
		/********************************************************************/
		public TimeSpan TotalTime
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the current song position of the current module
		/// </summary>
		/********************************************************************/
		public int SongPosition
		{
			get
			{
				lock (this)
				{
					return songPosition;
				}
			}

			set
			{
				lock (this)
				{
					songPosition = value;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public int SongLength
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the position time table for the current song
		/// </summary>
		/********************************************************************/
		public TimeSpan[] PositionTimes
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the module information list of the current song
		/// </summary>
		/********************************************************************/
		public string[] ModuleInformation
		{
			get;
		}
	}
}
