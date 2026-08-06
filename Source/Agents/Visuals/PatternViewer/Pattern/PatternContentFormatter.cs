//---------------------------------------------------------------------------------------
// <copyright file="PatternContentFormatter.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Static helper class for formatting content displayed in the pattern viewer
	/// </summary>
	internal static class PatternContentFormatter
	{
		/********************************************************************/
		/// <summary>
		/// Format BPM for display (null -> "---")
		/// </summary>
		/********************************************************************/
		public static string FormatBpm(int? bpm)
		{
			return bpm.HasValue ? bpm.Value.ToString("000") : "---";
		}

		/********************************************************************/
		/// <summary>
		/// Get the formatted track name for the given channel
		/// </summary>
		/********************************************************************/
		public static string GetTrackName(GetTrackNameInput input)
		{
			if (!input.CurrentDisplayMode.ShowsEffects(input.EffectCharCount))
			{
				return $"#{input.ChannelIndex + 1:00}";
			}

			// Get the correct pattern and row based on whether we're in manual scroll mode or playback
			int displaySongPosition = input.AllowPatternScrolling ? input.ManualSongPosition : input.CurrentSongPosition;
			int displayRow = input.AllowPatternScrolling ? input.ManualRow : input.CurrentRow;
			SongPatternViewData displayPattern = input.AllowPatternScrolling && input.SongData != null && displaySongPosition >= 0 &&
			                                     displaySongPosition < input.SongData.Count
				? input.SongData[displaySongPosition]
				: input.CurrentSongPattern;

			// Get current values from row data
			int? trackNumber = null;
			sbyte? transpose = null;

			// Try to get track number from pattern row data
			if (displayPattern?.Rows != null &&
			    displayRow >= 0 && displayRow < displayPattern.Rows.Length)
			{
				SongPatternViewRow row = displayPattern.Rows[displayRow];
				if (row?.Channels != null && input.ChannelIndex < row.Channels.Length)
				{
					SongPatternViewChannel entry = row.Channels[input.ChannelIndex];
					if (entry?.TrackNumber.HasValue == true)
					{
						trackNumber = entry.TrackNumber.Value;
					}

					if (entry?.Transpose.HasValue == true)
					{
						transpose = entry.Transpose.Value;
					}
				}
			}

			// Try to get track number from CurrentRowInfo (unified way)
			if (!trackNumber.HasValue &&
			    input.CurrentRowInfo?.ChannelTracks != null &&
			    input.ChannelIndex < input.CurrentRowInfo.ChannelTracks.Length &&
			    input.CurrentRowInfo.ChannelTracks[input.ChannelIndex].HasValue)
			{
				trackNumber = (int)input.CurrentRowInfo.ChannelTracks[input.ChannelIndex].Value;
			}

			// Build header based on format flags (not values)
			if (input.HasTrackNumber && input.HasTranspose)
			{
				string trackStr = trackNumber.HasValue ? $"{trackNumber.Value:000}" : "---";
				string transposeStr = transpose.HasValue
					? transpose.Value >= 0 ? $"+{transpose.Value}" : $"{transpose.Value}"
					: "+-";
				return $"#{input.ChannelIndex + 1:00} [{trackStr}{transposeStr}]";
			}

			if (input.HasTrackNumber)
			{
				string trackStr = trackNumber.HasValue ? $"{trackNumber.Value:000}" : "---";
				return $"#{input.ChannelIndex + 1:00} [{trackStr}]";
			}

			if (input.HasTranspose)
			{
				string transposeStr = transpose.HasValue
					? transpose.Value >= 0 ? $"+{transpose.Value}" : $"{transpose.Value}"
					: "+-";
				return $"#{input.ChannelIndex + 1:00} [{transposeStr}]";
			}

			return $"TRACK#{input.ChannelIndex + 1:00}";
		}
	}

	/// <summary>
	/// Input parameters for GetTrackName
	/// </summary>
	internal readonly struct GetTrackNameInput
	{
		public required int ChannelIndex
		{
			get;
			init;
		}

		public required DisplayMode CurrentDisplayMode
		{
			get;
			init;
		}

		public required int EffectCharCount
		{
			get;
			init;
		}

		public required bool AllowPatternScrolling
		{
			get;
			init;
		}

		public required int ManualSongPosition
		{
			get;
			init;
		}

		public required int CurrentSongPosition
		{
			get;
			init;
		}

		public required int ManualRow
		{
			get;
			init;
		}

		public required int CurrentRow
		{
			get;
			init;
		}

		public IReadOnlyList<SongPatternViewData> SongData
		{
			get;
			init;
		}

		public SongPatternViewData CurrentSongPattern
		{
			get;
			init;
		}

		public SongRowChangeInfo CurrentRowInfo
		{
			get;
			init;
		}

		public required bool HasTrackNumber
		{
			get;
			init;
		}

		public required bool HasTranspose
		{
			get;
			init;
		}
	}
}
