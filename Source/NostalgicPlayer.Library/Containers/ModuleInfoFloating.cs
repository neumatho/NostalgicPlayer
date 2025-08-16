/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Library.Containers
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
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal ModuleInfoFloating(int songNumber, DurationInfo durationInfo, string[] moduleInfo)
		{
			currentSong = songNumber;
			DurationInfo = durationInfo;
			songPosition = 0;
			SongLength = durationInfo != null ? durationInfo.PositionInfo.Length : 0;
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
				SongLength = newDurationInfo.PositionInfo.Length;
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
			get; private set;
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
