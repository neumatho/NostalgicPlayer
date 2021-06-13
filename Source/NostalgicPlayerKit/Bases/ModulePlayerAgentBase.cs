/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for module player agents
	/// </summary>
	public abstract class ModulePlayerAgentBase : PlayerAgentBase, IModulePlayerAgent
	{
		private static readonly SubSongInfo subSongInfo = new SubSongInfo(1, 0); 

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected ModulePlayerAgentBase()
		{
			VirtualChannels = null;
			PlayingFrequency = 50.0f;
		}



		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public virtual ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.None;



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage);



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public virtual bool InitPlayer()
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public virtual void CleanupPlayer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public virtual void InitSound(int songNumber, DurationInfo durationInfo)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public virtual void CleanupSound()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public virtual DurationInfo[] CalculateDuration()
		{
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public abstract void Play();



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module want to reserve
		/// </summary>
		/********************************************************************/
		public virtual int VirtualChannelCount => ModuleChannelCount;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public virtual int ModuleChannelCount => 4;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public virtual SubSongInfo SubSongs => subSongInfo;



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public virtual int SongLength => 0;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public virtual int GetSongPosition()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public virtual void SetSongPosition(int position, PositionInfo positionInfo)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the instruments available in the module. If none,
		/// null is returned
		/// </summary>
		/********************************************************************/
		public virtual InstrumentInfo[] Instruments => null;



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public virtual SampleInfo[] Samples => null;



		/********************************************************************/
		/// <summary>
		/// Return the total size of all the extra files loaded
		/// </summary>
		/********************************************************************/
		public virtual long ExtraFilesSizes
		{
			get; protected set;
		} = 0;



		/********************************************************************/
		/// <summary>
		/// Holds all the virtual channel instances used to play the samples
		/// </summary>
		/********************************************************************/
		public virtual IChannel[] VirtualChannels
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current playing frequency
		/// </summary>
		/********************************************************************/
		public virtual float PlayingFrequency
		{
			get; protected set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current state of the Amiga filter
		/// </summary>
		/********************************************************************/
		public bool AmigaFilter
		{
			get; protected set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Event called when the player change position
		/// </summary>
		/********************************************************************/
		public event EventHandler PositionChanged;

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this every time your player change it's position
		/// </summary>
		/********************************************************************/
		protected void OnPositionChanged()
		{
			if (PositionChanged != null)
				PositionChanged(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the frequency to the BPM you give and change the
		/// playing speed
		/// </summary>
		/********************************************************************/
		protected void SetBpmTempo(ushort bpm)
		{
			PlayingFrequency = bpm / 2.5f;
		}
		#endregion

		#region Duration calculation helpers
		/********************************************************************/
		/// <summary>
		/// Calculate the duration of each sub-song. The sub-songs are found
		/// by using the same position array for all songs
		/// </summary>
		/********************************************************************/
		protected DurationInfo[] CalculateDurationBySongPosition()
		{
			List<DurationInfo> result = new List<DurationInfo>();

			int songStartPos = 0;

			do
			{
				InitDurationCalculation(songStartPos);

				int prevPos = -1;
				float total = 0.0f;

				byte currentSpeed = GetCurrentSpeed();
				ushort currentBpm = GetCurrentBpm();
				object extraInfo = GetExtraPositionInfo();

				List<PositionInfo> positionTimes = new List<PositionInfo>();

				// Well, fill the position time list with empty times until
				// we reach the sub-song position
				for (int i = 0; i < songStartPos; i++)
					positionTimes.Add(new PositionInfo(currentSpeed, currentBpm, new TimeSpan(0), extraInfo));

				HasEndReached = false;

				for (;;)
				{
					if (prevPos < GetSongPosition())
					{
						prevPos = GetSongPosition();

						// Add position information to the list
						PositionInfo posInfo = new PositionInfo(currentSpeed, currentBpm, new TimeSpan((long)total * TimeSpan.TicksPerMillisecond), extraInfo);

						// Need to make a while, in case there is a position jump
						// that jumps forward, then we're missing some items in the list
						while (prevPos >= positionTimes.Count)
							positionTimes.Add(posInfo);
					}

					// "Play" a single tick
					Play();

					// Update information
					currentSpeed = GetCurrentSpeed();
					currentBpm = GetCurrentBpm();
					extraInfo = GetExtraPositionInfo();

					if (HasEndReached)
						break;

					// Add the tick time
					total += (1000.0f / (currentBpm / 2.5f));
				}

				// Calculate the total time of the song
				TimeSpan totalTime = new TimeSpan((long)total * TimeSpan.TicksPerMillisecond);

				// Find new start position
				int newStartPosition = positionTimes.Count;

				// Fill the rest of the list with total time
				for (int i = positionTimes.Count; i < SongLength; i++)
					positionTimes.Add(new PositionInfo(currentSpeed, currentBpm, totalTime, extraInfo));

				// Remember the song
				result.Add(new DurationInfo(totalTime, positionTimes.ToArray(), songStartPos));

				songStartPos = newStartPosition;
			}
			while (songStartPos < SongLength - 1);

			// Clear the "end" flag again, so the module don't stop playing immediately
			HasEndReached = false;

			return result.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected virtual void InitDurationCalculation(int startPosition)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return the current speed
		/// </summary>
		/********************************************************************/
		protected virtual byte GetCurrentSpeed()
		{
			return 6;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current BPM
		/// </summary>
		/********************************************************************/
		protected virtual ushort GetCurrentBpm()
		{
			return 125;
		}



		/********************************************************************/
		/// <summary>
		/// Return extra information for the current position
		/// </summary>
		/********************************************************************/
		protected virtual object GetExtraPositionInfo()
		{
			return null;
		}
		#endregion
	}
}
