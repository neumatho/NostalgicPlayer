/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for module player agents
	/// </summary>
	public abstract class ModulePlayerAgentBase : PlayerAgentBase, IModulePlayerAgent
	{
		private static readonly SubSongInfo subSongInfo = new SubSongInfo(1, 0);

		/// <summary>
		/// Holds the mixer frequency. It is only set if BufferMode is set in the SupportFlags.
		/// </summary>
		protected uint mixerFreq;

		/// <summary>
		/// Holds the number of channels to output. It is only set if BufferMode is set in the SupportFlags.
		/// </summary>
		protected int mixerChannels;

		/// <summary>
		/// Holds the duration information for the current playing song
		/// </summary>
		protected DurationInfo currentDurationInfo;

		private DurationInfo[] allDurations;
		private List<int> positionPlayOrder;
		private bool[] visitedPositions;

		private int currentSong;
		private SubSongInfo currentSubSongInfo;

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
		/// This flag is set to true, when end is reached
		/// </summary>
		/********************************************************************/
		public override bool HasEndReached
		{
			set
			{
				base.HasEndReached = value;

				if (!value && (visitedPositions != null))
					Array.Clear(visitedPositions);
			}
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
		public virtual bool InitPlayer(out string errorMessage)
		{
			errorMessage = string.Empty;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public virtual void CleanupPlayer()
		{
			allDurations = null;
			positionPlayOrder = null;
			visitedPositions = null;

			currentSubSongInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public virtual bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (visitedPositions != null)
				Array.Clear(visitedPositions);

			currentSong = songNumber;
			currentDurationInfo = durationInfo;

			return true;
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
		/// Is only called if BufferMode is set in the SupportFlags. It tells
		/// your player what frequency the NostalgicPlayer mixer is using.
		/// You can use it if you want or you can use your own output
		/// frequency, but if you also using BufferDirect, you need to use
		/// this frequency and number of channels
		/// </summary>
		/********************************************************************/
		public virtual void SetOutputFormat(uint mixerFrequency, int channels)
		{
			mixerFreq = mixerFrequency;
			mixerChannels = channels;
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
		public virtual SubSongInfo SubSongs => currentSubSongInfo ?? subSongInfo;



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
			if (visitedPositions != null)
			{
				Array.Clear(visitedPositions);

				for (int pos = allDurations[positionInfo.SubSong].StartPosition; ; pos++)
				{
					int playingIndex = positionPlayOrder[pos];
					visitedPositions[playingIndex] = true;

					if (playingIndex == position)
						break;
				}

				ChangeSubSong(positionInfo.SubSong);
			}
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
		/// Holds all the virtual channel instances used to play the samples
		/// </summary>
		/********************************************************************/
		public virtual IChannel[] VirtualChannels
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Return an effect master instance if the player adds extra mixer
		/// effects to the output
		/// </summary>
		/********************************************************************/
		public virtual IEffectMaster EffectMaster => null;



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



		/********************************************************************/
		/// <summary>
		/// Event called when the player change sub-song
		/// </summary>
		/********************************************************************/
		public event SubSongChangedEventHandler SubSongChanged;

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this every time your player change it's position
		/// </summary>
		/********************************************************************/
		protected void OnPositionChanged()
		{
			if (!doNotTrigEvents && (PositionChanged != null))
				PositionChanged(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Call this every time your player change the sub-song
		/// </summary>
		/********************************************************************/
		protected void OnSubSongChanged(SubSongChangedEventArgs e)
		{
			if (!doNotTrigEvents && (SubSongChanged != null))
				SubSongChanged(this, e);
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



		/********************************************************************/
		/// <summary>
		/// Tells if the given position has already been played
		/// </summary>
		/********************************************************************/
		protected bool HasPositionBeenVisited(int position)
		{
			return visitedPositions[position];
		}



		/********************************************************************/
		/// <summary>
		/// Mark that the given position has been played
		/// </summary>
		/********************************************************************/
		protected void MarkPositionAsVisited(int position)
		{
			visitedPositions[position] = true;

			if (currentDurationInfo != null)
				ChangeSubSong(currentDurationInfo.PositionInfo[position].SubSong);
		}



		/********************************************************************/
		/// <summary>
		/// Will change the sub-song if needed
		/// </summary>
		/********************************************************************/
		protected void ChangeSubSong(int subSong)
		{
			if (subSong != currentSong)
			{
				currentSong = subSong;
				currentDurationInfo = allDurations[subSong];

				OnSubSongChanged(new SubSongChangedEventArgs(subSong, currentDurationInfo));
			}
		}
		#endregion

		#region Duration calculation helpers
		/********************************************************************/
		/// <summary>
		/// Calculate the duration of each sub-song. The sub-songs are found
		/// by using the same position array for all songs
		/// </summary>
		/********************************************************************/
		protected DurationInfo[] CalculateDurationBySongPosition(bool playerTellsWhenToStop = false)
		{
			doNotTrigEvents = true;

			try
			{
				List<DurationInfo> result = new List<DurationInfo>();

				PositionInfo[] positionTimes = new PositionInfo[SongLength];
				positionPlayOrder = new List<int>(positionTimes.Length);
				visitedPositions = new bool[positionTimes.Length];

				int songStartPos = 0;
				int subSong = 0;
				DateTime startTime = DateTime.Now;

				do
				{
					HasEndReached = false;

					songStartPos = InitDurationCalculationByStartPos(songStartPos);
					if (songStartPos < 0)
						break;

					int previousPos = -1;
					float total = 0.0f;

					byte currentSpeed = GetCurrentSpeed();
					ushort currentBpm = GetCurrentBpm();
					object extraInfo = GetExtraPositionInfo();

					for (;;)
					{
						// Check for time out
						if ((DateTime.Now - startTime).Seconds >= 5)
							throw new Exception(Resources.IDS_ERR_DURATION_TIMEOUT);

						int currentPos = GetSongPosition();
						if (currentPos != previousPos)
						{
							if (playerTellsWhenToStop)
							{
								if (positionTimes[currentPos] != null)
								{
									if (positionTimes[currentPos].SubSong != subSong)
										break;
								}
							}
							else
							{
								if (positionTimes[currentPos] != null)	// Position has already been taken by another sub-song
									break;
							}

							previousPos = currentPos;

							// Add position information to the list
							PositionInfo posInfo = new PositionInfo(currentSpeed, currentBpm, new TimeSpan((long)total * TimeSpan.TicksPerMillisecond), subSong, extraInfo);
							positionTimes[currentPos] = posInfo;

							positionPlayOrder.Add(currentPos);
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
						total += 1000.0f / PlayingFrequency;
					}

					// Calculate the total time of the song
					TimeSpan totalTime = new TimeSpan((long)total * TimeSpan.TicksPerMillisecond);

					// Remember the song
					//
					// Note that the same positionTimes array is used for all sub-songs,
					// so even if it's not totally updated now, it will be later
					result.Add(new DurationInfo(totalTime, positionTimes, songStartPos));

					CleanupDurationCalculation();
					subSong++;

					// Find new start position
					songStartPos = Array.FindIndex(positionTimes, item => item == null);
				}
				while (playerTellsWhenToStop || (songStartPos != -1));

				// Clear the "end" flag again, so the module don't stop playing immediately
				HasEndReached = false;

				allDurations = result.ToArray();
				currentSubSongInfo = new SubSongInfo(allDurations.Length, 0);

				return allDurations;
			}
			finally
			{
				doNotTrigEvents = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration of each sub-song
		/// </summary>
		/********************************************************************/
		protected DurationInfo[] CalculateDurationBySubSongs()
		{
			doNotTrigEvents = true;

			try
			{
				List<DurationInfo> result = new List<DurationInfo>();

				int numSongs = SubSongs.Number;
				DateTime startTime = DateTime.Now;

				for (int song = 0; song < numSongs; song++)
				{
					HasEndReached = false;

					InitDurationCalculationBySubSong(song);

					int prevPos = -1;
					float total = 0.0f;

					byte currentSpeed = GetCurrentSpeed();
					ushort currentBpm = GetCurrentBpm();
					object extraInfo = GetExtraPositionInfo();

					List<PositionInfo> positionTimes = new List<PositionInfo>();

					for (;;)
					{
						// Check for time out
						if ((DateTime.Now - startTime).Seconds >= 5)
							throw new Exception(Resources.IDS_ERR_DURATION_TIMEOUT);

						int currentPos = GetSongPosition();
						if (prevPos != currentPos)
						{
							prevPos = currentPos;

							// Add position information to the list
							PositionInfo posInfo = new PositionInfo(currentSpeed, currentBpm, new TimeSpan((long)total * TimeSpan.TicksPerMillisecond), song, extraInfo);

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
						total += 1000.0f / PlayingFrequency;
					}

					// Calculate the total time of the song
					TimeSpan totalTime = new TimeSpan((long)total * TimeSpan.TicksPerMillisecond);

					// Fill the rest of the list with total time
					for (int i = positionTimes.Count; i < SongLength; i++)
						positionTimes.Add(new PositionInfo(currentSpeed, currentBpm, totalTime, song, extraInfo));

					// Remember the song
					result.Add(new DurationInfo(totalTime, positionTimes.ToArray(), 0));

					CleanupDurationCalculation();
				}

				// Clear the "end" flag again, so the module don't stop playing immediately
				HasEndReached = false;

				return result.ToArray();
			}
			finally
			{
				doNotTrigEvents = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected virtual int InitDurationCalculationByStartPos(int startPosition)
		{
			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected virtual void InitDurationCalculationBySubSong(int subSong)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup needed stuff after a sub-song calculation
		/// </summary>
		/********************************************************************/
		protected virtual void CleanupDurationCalculation()
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
