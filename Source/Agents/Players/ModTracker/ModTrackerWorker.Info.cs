/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker
{
	/// <summary>
	/// Pattern viewer methods - GetSongPatterns, NotifySongRowChanged
	/// </summary>
	internal partial class ModTrackerWorker
	{
		/********************************************************************/
		/// <summary>
		/// Notify pattern visuals about row change
		/// </summary>
		/********************************************************************/
		private void NotifySongRowChanged()
		{
			if (playingInfo == null)
				return;

			// Use helper to create row change info
			var rowInfo = ModTrackerPatternHelper.CreateRowChangeInfo(
				playingInfo.SongPos, playingInfo.PatternPos, positions,
				playingInfo.SpeedEven, playingInfo.SpeedOdd, playingInfo.Tempo, sequences, channelNum, showTracks);

			OnSongRowChanged(new SongRowChangedEventArgs(rowInfo));
		}



		/********************************************************************/
		/// <summary>
		/// Return pattern data for the current song
		/// </summary>
		/********************************************************************/
		public override SongPatternsResult GetSongPatterns()
		{
			if (positions != null && sequences != null && tracks != null)
			{
				return new SongPatternsResult(ModTrackerPatternHelper.CreateSongPatterns(
					positions, songLength, sequences, tracks, maxPattern,
					patternLength, channelNum, trackNum, currentModuleType,
					songName, initTempo, showTracks));
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the currently playing track number for each channel
		/// </summary>
		/********************************************************************/
		public override uint?[] GetCurrentChannelTracks()
		{
			// Only return tracks for formats that actually use separate tracks per channel
			// (IceTracker, SoundTracker 2.6). ProTracker uses patterns, not tracks.
			if (!showTracks)
				return null;

			if ((sequences == null) || (playingInfo == null))
				return null;

			List<uint?> tracks = new List<uint?>();

			for (int i = 0; i < channelNum; i++)
				tracks.Add(sequences[i, playingInfo.SongPos]);

			return tracks.ToArray();
		}
	}
}
