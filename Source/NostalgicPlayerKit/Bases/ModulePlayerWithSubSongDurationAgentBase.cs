/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Derive from this class in your player, if you want to calculate your
	/// sub-songs durations. This can also be used, if you only have one sub-song
	/// in the format, but uses different position lists for each track
	/// </summary>
	public abstract class ModulePlayerWithSubSongDurationAgentBase : ModulePlayerAgentBase, IModuleDurationPlayer
	{
		private DurationInfo[] allDurationInfo;

		private float[] positionTimes;
		private float currentTotalTime;

		private int currentSubSong;
		private DurationInfo currentDurationInfo;
		private TimeSpan playerRestartTime;
		private bool[] channelsDone;

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

				int totalSubSongs = SubSongs.Number;
				bool bufferMode = (SupportFlags & ModulePlayerSupportFlag.BufferMode) != 0;

				if (bufferMode)
					SetOutputFormat(44100, 2);

				for (currentSubSong = 0; currentSubSong < totalSubSongs; currentSubSong++)
				{
					HasEndReached = false;

					currentTotalTime = 0.0f;
					float lastSnapshotTime = 0.0f;

					InitDuration(currentSubSong);

					try
					{
						channelsDone = new bool[ModuleChannelCount];

						int totalPositions = GetTotalNumberOfPositions();
						positionTimes = new float[totalPositions];

						playerRestartTime = TimeSpan.Zero;

						List<PositionInfo> positionInfoList = new List<PositionInfo>();
						positionInfoList.Add(new PositionInfo(TimeSpan.Zero, PlayingFrequency, CreateSnapshot()));

						DateTime safeguardStartTime = DateTime.Now;

						for (;;)
						{
							// Check for time out
							if ((DateTime.Now - safeguardStartTime).Seconds >= 10)
								throw new Exception(Resources.IDS_ERR_DURATION_TIMEOUT);

							// Time to create a new snapshot?
							if ((currentTotalTime - lastSnapshotTime) >= IDurationPlayer.NumberOfSecondsBetweenEachSnapshot * 1000.0f)
							{
								positionInfoList.Add(new PositionInfo(new TimeSpan((long)(Math.Round(currentTotalTime, MidpointRounding.AwayFromZero) * TimeSpan.TicksPerMillisecond)), PlayingFrequency, CreateSnapshot()));
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
						result.Add(new DurationInfo(totalTime, positionInfoList.ToArray(), positionTimes.Select(x => new TimeSpan((long)(x * TimeSpan.TicksPerMillisecond))).ToArray(), playerRestartTime));
					}
					finally
					{
						CleanupDuration();
					}
				}

				// Clear the "end" flag again, so the module don't stop playing immediately
				HasEndReached = false;

				allDurationInfo = result.ToArray();

				return allDurationInfo;
			}
			finally
			{
				currentSubSong = -1;
				positionTimes = null;
				channelsDone = null;

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
			playerRestartTime = currentDurationInfo.RestartTime ?? TimeSpan.Zero;

			PositionInfo positionInfo = currentDurationInfo.PositionInfo[0];

			PlayingFrequency = positionInfo.PlayingFrequency;

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
			SetSnapshot(positionInfo.Snapshot, out _);

			for (int i = VirtualChannelCount - 1; i >= 0; i--)
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
		protected abstract void InitDuration(int subSong);



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
		/// Return the total number of positions. You only need to derive
		/// from this method, if your player have one position list for all
		/// channels and can restart on another position than 0
		/// </summary>
		/********************************************************************/
		protected virtual int GetTotalNumberOfPositions()
		{
			return 0;
		}



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
			channelsDone = null;

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

			base.CleanupSound();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override bool InitSound(int songNumber, out string errorMessage)
		{
			if (!base.InitSound(songNumber, out errorMessage))
				return false;

			channelsDone = new bool[ModuleChannelCount];

			return true;
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Mark that the given position has been played. You only need to
		/// call this method, if your player have one position list for all
		/// channels and can restart on another position than 0
		/// </summary>
		/********************************************************************/
		protected void MarkPositionAsVisited(int position)
		{
			if (positionTimes != null)
			{
				if (((position == 0) && (positionTimes.Length > 1) && (positionTimes[1] == 0)) || ((position != 0) && (positionTimes[position] == 0)))
					positionTimes[position] = currentTotalTime;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will set the current time as the restart time
		/// </summary>
		/********************************************************************/
		protected void SetRestartTime()
		{
			if (positionTimes != null)
				playerRestartTime = new TimeSpan((long)(currentTotalTime * TimeSpan.TicksPerMillisecond));
		}



		/********************************************************************/
		/// <summary>
		/// Will set the reset time to the time remembered for the given
		/// position
		/// </summary>
		/********************************************************************/
		protected void SetPositionTime(int position)
		{
			if (positionTimes != null)
				playerRestartTime = new TimeSpan((long)(positionTimes[position] * TimeSpan.TicksPerMillisecond));
		}



		/********************************************************************/
		/// <summary>
		/// Will restart the current sub-song from the beginning
		/// </summary>
		/********************************************************************/
		protected void RestartSong()
		{
			if (positionTimes == null)
			{
				SetSubSong(currentSubSong, out _);
				Array.Clear(channelsDone);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Call this when your player has reached the end of a single
		/// channel. Can be used, if your player have different position
		/// list on each channel
		/// </summary>
		/********************************************************************/
		protected void OnEndReached(int channel)
		{
			channelsDone[channel] = true;

			if (!channelsDone.Contains(false))
			{
				OnEndReached();
				Array.Clear(channelsDone);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Call this when your player has reached the end of all channels
		/// </summary>
		/********************************************************************/
		protected void OnEndReachedOnAllChannels(int restartPosition)
		{
			if ((currentDurationInfo != null) && (currentDurationInfo.PlayerPositionTime.Length > 0))
				playerRestartTime = currentDurationInfo.PlayerPositionTime[restartPosition];

			Array.Fill(channelsDone, true);

			OnEndReached(0);
		}
		#endregion
	}
}
