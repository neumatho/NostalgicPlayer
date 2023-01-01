/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Containers
{
	/// <summary>
	/// This class holds all information about the player which changes while playing
	/// </summary>
	public class ModuleInfoFloating
	{
		private int currentSong;
		private int songPosition;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoFloating()
		{
			currentSong = 0;
			DurationInfo = null;
			songPosition = 0;
			SongLength = 0;
			ModuleInformation = null;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor (for module players)
		/// </summary>
		/********************************************************************/
		internal ModuleInfoFloating(int songNumber, DurationInfo durationInfo, int songPosition, int songLength, string[] moduleInfo)
		{
			currentSong = songNumber;
			DurationInfo = durationInfo;
			this.songPosition = songPosition;
			SongLength = songLength;
			ModuleInformation = moduleInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor (for sample players)
		/// </summary>
		/********************************************************************/
		internal ModuleInfoFloating(DurationInfo durationInfo, int songPosition, int songLength, string[] moduleInfo)
		{
			currentSong = 0;
			DurationInfo = durationInfo;
			this.songPosition = songPosition;
			SongLength = songLength;
			ModuleInformation = moduleInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the current song playing
		/// </summary>
		/********************************************************************/
		public int CurrentSong
		{
			get
			{
				lock (this)
				{
					return currentSong;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Change the current song
		/// </summary>
		/********************************************************************/
		public void SetCurrentSong(int newSubSong, DurationInfo newDurationInfo)
		{
			lock (this)
			{
				currentSong = newSubSong;
				DurationInfo = newDurationInfo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Hold duration information about the current song
		/// </summary>
		/********************************************************************/
		public DurationInfo DurationInfo
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total time of the current song
		/// </summary>
		/********************************************************************/
		public TimeSpan SongTotalTime => DurationInfo?.TotalTime ?? new TimeSpan(0);



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
		/// Return the module information list of the current song
		/// </summary>
		/********************************************************************/
		public string[] ModuleInformation
		{
			get;
		}
	}
}
