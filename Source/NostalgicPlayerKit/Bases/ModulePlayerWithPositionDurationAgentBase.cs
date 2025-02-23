/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Derive from this class in your player, if you want to find sub-songs and calculate
	/// their durations based on the position list
	/// </summary>
	public abstract class ModulePlayerWithPositionDurationAgentBase : ModulePlayerAgentBase, IModuleDurationPlayer
	{
		private int[] originalPositionSubSong;
		private DurationInfo[] allDurationInfo;
		private SubSongInfo subSongInfo;

		private int[] workingPositionSubSong;

		private float[] positionTimes;
		private float currentTotalTime;

		private int currentSubSong;
		private DurationInfo currentDurationInfo;
		private TimeSpan playerRestartTime;

		#region IModuleDurationPlayer implementation
		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public DurationInfo[] CalculateDuration()
		{
			doNotTrigEvents = true;

			try
			{
				List<DurationInfo> result = new List<DurationInfo>();

				int totalPositions = GetTotalNumberOfPositions();
				bool bufferMode = (SupportFlags & ModulePlayerSupportFlag.BufferMode) != 0;

				if (bufferMode)
					SetOutputFormat(44100, 2);

				originalPositionSubSong = new int[totalPositions];
				Array.Fill(originalPositionSubSong, -1);

				currentSubSong = 0;
				positionTimes = new float[totalPositions];

				for (;;)
				{
					HasEndReached = false;

					int startPosition = Array.IndexOf(originalPositionSubSong, -1);
					if (startPosition == -1)
						break;

					currentTotalTime = 0.0f;
					float lastSnapshotTime = 0.0f;

					startPosition = InitDuration(currentSubSong, startPosition);
					if (startPosition < 0)
						break;

					try
					{
						List<PositionInfo> positionInfoList = new List<PositionInfo>();
						positionInfoList.Add(new PositionInfoForPositionDuration(TimeSpan.Zero, PlayingFrequency, AmigaFilter, originalPositionSubSong, CreateSnapshot()));

						DateTime safeguardStartTime = DateTime.Now;

						for (;;)
						{
							// Check for time out
							if ((DateTime.Now - safeguardStartTime).Seconds >= 10)
								throw new Exception(Resources.IDS_ERR_DURATION_TIMEOUT);

							// Time to create a new snapshot?
							if ((currentTotalTime - lastSnapshotTime) >= IDurationPlayer.NumberOfSecondsBetweenEachSnapshot * 1000.0f)
							{
								positionInfoList.Add(new PositionInfoForPositionDuration(new TimeSpan((long)(currentTotalTime * TimeSpan.TicksPerMillisecond)), PlayingFrequency, AmigaFilter, originalPositionSubSong, CreateSnapshot()));
								lastSnapshotTime = currentTotalTime;
							}

							// "Play" a single tick
							Play();

							if (HasEndReached)
								break;

							// Add the tick time
							if (bufferMode)
								currentTotalTime += (VirtualChannels[0].GetSampleLength() * 1000.0f) / mixerFreq;
							else
								currentTotalTime += 1000.0f / PlayingFrequency;
						}

						// Calculate the total time of the song
						TimeSpan totalTime = new TimeSpan((long)(currentTotalTime * TimeSpan.TicksPerMillisecond));

						// Remember the song
						result.Add(new DurationInfo(totalTime, positionInfoList.ToArray(), positionTimes.Select(x => new TimeSpan((long)(x * TimeSpan.TicksPerMillisecond))).ToArray()));
						currentSubSong++;
					}
					finally
					{
						CleanupDuration();
					}
				}

				// Clear the "end" flag again, so the module don't stop playing immediately
				HasEndReached = false;

				if (result.Count == 0)
					result.Add(new DurationInfo(TimeSpan.Zero, Array.Empty<PositionInfo>(), Array.Empty<TimeSpan>()));

				subSongInfo = new SubSongInfo(result.Count, 0);
				allDurationInfo = result.ToArray();

				// ReSharper disable once CoVariantArrayConversion
				return allDurationInfo;
			}
			finally
			{
				currentSubSong = -1;
				positionTimes = null;

				doNotTrigEvents = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize player to play the given sub-song
		/// </summary>
		/********************************************************************/
		public bool SetSubSong(int subSong, out string errorMessage)
		{
			currentSubSong = subSong;
			currentDurationInfo = allDurationInfo[subSong];
			playerRestartTime = TimeSpan.Zero;

			PositionInfoForPositionDuration positionInfo = GetPositionInfo(0);
			if (positionInfo == null)
			{
				errorMessage = string.Empty;
				return true;
			}

			InitializeWorkingPositionSubSongArray(positionInfo);

			PlayingFrequency = positionInfo.PlayingFrequency;
			AmigaFilter = positionInfo.AmigaFilter;

			return SetSnapshot(positionInfo.Snapshot, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Will tell the player to change its current state to match the
		/// position given
		/// </summary>
		/********************************************************************/
		public void SetSongPosition(PositionInfo positionInfo)
		{
			PlayingFrequency = positionInfo.PlayingFrequency;
			AmigaFilter = positionInfo.AmigaFilter;

			SetSnapshot(positionInfo.Snapshot, out _);

			InitializeWorkingPositionSubSongArray((PositionInfoForPositionDuration)positionInfo);

			for (int i = VirtualChannels.Length - 1; i >= 0; i--)
				VirtualChannels[i].Mute();
		}



		/********************************************************************/
		/// <summary>
		/// Return the time into the song when restarting
		/// </summary>
		/********************************************************************/
		public TimeSpan GetRestartTime()
		{
			return playerRestartTime;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected abstract int InitDuration(int songNumber, int startPosition);



		/********************************************************************/
		/// <summary>
		/// Cleanup needed stuff after a sub-song calculation
		/// </summary>
		/********************************************************************/
		protected virtual void CleanupDuration()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected abstract int GetTotalNumberOfPositions();



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected abstract ISnapshot CreateSnapshot();



		/********************************************************************/
		/// <summary>
		/// Initialize internal structures based on the snapshot given
		/// </summary>
		/********************************************************************/
		protected abstract bool SetSnapshot(ISnapshot snapshot, out string errorMessage);

		#region ModulePlayerAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public override ModulePlayerSupportFlag SupportFlags => base.SupportFlags | ModulePlayerSupportFlag.SetPosition;



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			originalPositionSubSong = null;

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			currentSubSong = -1;
			workingPositionSubSong = null;

			base.CleanupSound();
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => subSongInfo;
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Tells if the given position has already been played
		/// </summary>
		/********************************************************************/
		protected bool HasPositionBeenVisited(int position)
		{
			int[] array = GetPositionSubSongArray();

			if (position >= array.Length)
				return true;

			return array[position] != -1;
		}



		/********************************************************************/
		/// <summary>
		/// Mark that the given position has been played
		/// </summary>
		/********************************************************************/
		protected void MarkPositionAsVisited(int position)
		{
			int[] array = GetPositionSubSongArray();
			if (position < array.Length)
			{
				if (array[position] == -1)
				{
					array[position] = currentSubSong;

					if (positionTimes != null)
						positionTimes[position] = currentTotalTime;
				}

				if (!doNotTrigEvents)
				{
					int subSong = array[position];
					if (subSong != currentSubSong)
					{
						currentSubSong = subSong;
						currentDurationInfo = allDurationInfo[subSong];

						OnSubSongChanged(new SubSongChangedEventArgs(currentSubSong, currentDurationInfo));
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will restart the current sub-song from the beginning
		/// </summary>
		/********************************************************************/
		protected void RestartSong()
		{
			if (workingPositionSubSong != null)
				SetSubSong(currentSubSong, out _);
		}



		/********************************************************************/
		/// <summary>
		/// Call this when your player has reached the end of the module
		/// </summary>
		/********************************************************************/
		protected void OnEndReached(int restartPosition)
		{
			if (currentDurationInfo != null)
			{
				if (restartPosition < currentDurationInfo.PlayerPositionTime.Length)
				{
					playerRestartTime = currentDurationInfo.PlayerPositionTime[restartPosition];
					InitializeWorkingPositionSubSongArray(GetPositionInfo((int)(playerRestartTime.TotalSeconds / IDurationPlayer.NumberOfSecondsBetweenEachSnapshot)));
				}
				else
					playerRestartTime = TimeSpan.Zero;
			}

			OnEndReached();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return the position sub-song array to use
		/// </summary>
		/********************************************************************/
		private int[] GetPositionSubSongArray()
		{
			return workingPositionSubSong ?? originalPositionSubSong;
		}



		/********************************************************************/
		/// <summary>
		/// Return the right PositionInfo object
		/// </summary>
		/********************************************************************/
		private PositionInfoForPositionDuration GetPositionInfo(int index)
		{
			return index < currentDurationInfo.PositionInfo.Length ? (PositionInfoForPositionDuration)currentDurationInfo.PositionInfo[index] : null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize working position array
		/// </summary>
		/********************************************************************/
		private void InitializeWorkingPositionSubSongArray(PositionInfoForPositionDuration positionInfo)
		{
			workingPositionSubSong = ArrayHelper.CloneArray(positionInfo.PositionSubSongs);
		}
		#endregion
	}
}
