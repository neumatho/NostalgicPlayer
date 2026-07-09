/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;

namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker
{
	/// <summary>
	/// Pattern viewer methods - GetSongPatterns, NotifySongRowChanged
	/// </summary>
	internal partial class DavidWhittakerWorker
	{
		private readonly DavidWhittakerRecorder patternRecorder = new();

		private int currentRow;

		/********************************************************************/
		/// <summary>
		/// Get the row number based on current playback tick
		/// </summary>
		/********************************************************************/
		private int GetRowFromPlaybackTick()
		{
			if (patternRecorder == null || !patternRecorder.HasTickMapping(currentSong))
				return currentRow;

			return patternRecorder.GetRowFromTick(currentSong);
		}

		/********************************************************************/
		/// <summary>
		/// Reset the current row counter. Sets it to -1 because it will be
		/// incremented immediately in the same Play() iteration
		/// </summary>
		/********************************************************************/
		private void ResetCurrentRow()
		{
			currentRow = -1;
		}

		/********************************************************************/
		/// <summary>
		/// Increment the current row counter
		/// </summary>
		/********************************************************************/
		private void IncrementCurrentRow()
		{
			currentRow++;
			NotifySongRowChanged();
		}

		/********************************************************************/
		/// <summary>
		/// Called before duration calculation starts
		/// </summary>
		/********************************************************************/
		public override void BeforeCalculateDuration()
		{
			patternRecorder.StartRecording();
		}



		/********************************************************************/
		/// <summary>
		/// Called after duration calculation is complete
		/// </summary>
		/********************************************************************/
		public override void AfterCalculateDuration()
		{
			patternRecorder.StopRecording();
		}



		/********************************************************************/
		/// <summary>
		/// Return the time into the song when restarting (song loops)
		/// </summary>
		/********************************************************************/
		public override TimeSpan GetRestartTime()
		{
			patternRecorder.ResetTick();

			return base.GetRestartTime();
		}



		/********************************************************************/
		/// <summary>
		/// Return song patterns
		/// </summary>
		/********************************************************************/
		public override SongPatternsResult GetSongPatterns()
		{
			return new SongPatternsResult(null, patternRecorder?.GetSongPatterns(currentSong));
		}



		/********************************************************************/
		/// <summary>
		/// Notify pattern viewer about row change
		/// </summary>
		/********************************************************************/
		private void NotifySongRowChanged()
		{
			if (channels == null || channels.Length == 0)
				return;

			if (songInfoList == null || currentSong >= songInfoList.Count)
				return;

			SongInfo song = songInfoList[currentSong];
			if (song == null)
				return;

			// Find the fastest (minimum) speed across all channels
			// The fastest channel determines the visible tempo
			ushort currentSpeed = ushort.MaxValue;
			for (int i = 0; i < channels.Length; i++)
			{
				if (channels[i].Speed > 0 && channels[i].Speed < currentSpeed)
					currentSpeed = channels[i].Speed;
			}

			// Fallback if no valid speed found
			if (currentSpeed == ushort.MaxValue)
				currentSpeed = song.Speed;

			// Record pattern data
			patternRecorder.RecordRow(currentRow, channels, trackNumbers);

			// Get current positions for each channel
			int[] channelPositions = new int[numberOfChannels];
			for (int i = 0; i < numberOfChannels && i < channels.Length; i++)
			{
				channelPositions[i] = channels[i].CurrentPosition;
			}

			// We have one long pattern (position 0), use tick-based row for consistent position across loops
			SongRowChangeInfo rowInfo = DavidWhittakerPatternHelper.CreateRowChangeInfo(
				0,  // Single pattern at position 0
				GetRowFromPlaybackTick(),
				currentSpeed,
			song.PositionLists,
			trackNumbers,
			numberOfChannels,
			channelPositions);

			OnSongRowChanged(new SongRowChangedEventArgs(rowInfo));
		}



		/********************************************************************/
		/// <summary>
		/// Return the current channel track numbers
		/// </summary>
		/********************************************************************/
		public override uint?[] GetCurrentChannelTracks()
		{
			if (channels == null || trackNumbers == null)
				return null;

			List<uint?> tracks = new List<uint?>();

			for (int i = 0; i < numberOfChannels; i++)
				tracks.Add((uint?)trackNumbers[channels[i].PositionList[channels[i].CurrentPosition - 1]]);

			return tracks.ToArray();
		}
	}
}
