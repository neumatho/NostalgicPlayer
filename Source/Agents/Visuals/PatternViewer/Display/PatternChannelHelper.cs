//---------------------------------------------------------------------------------------
// <copyright file="PatternChannelHelper.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
	/// <summary>
	/// Helper class for channel-related operations in pattern viewer
	/// </summary>
	internal static class PatternChannelHelper
	{
		/********************************************************************/
		/// <summary>
		/// Process channel change information and update volume bar states
		/// </summary>
		/********************************************************************/
		public static void ProcessChannelChange(PatternRenderer renderer, ChannelChanged[] channels)
		{
			renderer.ChannelInfo = channels;

			if (channels == null)
			{
				return;
			}

			// Update volume bar decay for NoteKick mode
			if (renderer.VolumeBarState.DisplaySettings.Mode == VolumeBarMode.NoteKick)
			{
				for (int i = 0; i < channels.Length && i < renderer.VolumeBarState.AudioData.VolumeBarDecay.Length; i++)
				{
					if (channels[i] != null && channels[i].NoteKicked)
					{
						renderer.VolumeBarState.AudioData.VolumeBarDecay[i] = 20; // Start decay at full height
					}
				}
			}

			// Update debug counters for state changes
			UpdateDebugCounters(renderer, channels);

			// Update measured volume for RealVolume mode
			UpdateRealVolume(renderer, channels);
		}

		/********************************************************************/
		/// <summary>
		/// Update debug counters for enabled/muted state changes
		/// </summary>
		/********************************************************************/
		private static void UpdateDebugCounters(PatternRenderer renderer, ChannelChanged[] channels)
		{
			if (renderer.VolumeBarState.ChannelState.EnabledChangeCount == null ||
			    renderer.VolumeBarState.ChannelState.MutedChangeCount == null)
			{
				return;
			}

			for (int i = 0; i < channels.Length && i < 64; i++)
			{
				if (channels[i] == null)
				{
					continue;
				}

				// Check for enabled/disabled changes
				bool currentEnabled = channels[i].Enabled;
				if (currentEnabled != renderer.LastEnabledState[i])
				{
					renderer.VolumeBarState.ChannelState.EnabledChangeCount[i]++;
					renderer.LastEnabledState[i] = currentEnabled;
				}

				// Check for muted/unmuted changes
				bool currentMuted = channels[i].Muted;
				if (currentMuted != renderer.LastMutedState[i])
				{
					renderer.VolumeBarState.ChannelState.MutedChangeCount[i]++;
					renderer.LastMutedState[i] = currentMuted;
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Update measured volume for RealVolume mode (like ChannelLevelMeter.UpdateLevel)
		/// </summary>
		/********************************************************************/
		private static void UpdateRealVolume(PatternRenderer renderer, ChannelChanged[] channels)
		{
			if (renderer.VolumeBarState.DisplaySettings.Mode != VolumeBarMode.RealVolume)
			{
				return;
			}

			for (int i = 0; i < renderer.VolumeBarState.AudioData.RealVolumeMeasured.Length && i < channels.Length; i++)
			{
				if (channels[i] == null)
				{
					continue;
				}

				if (channels[i].Volume.HasValue)
				{
					float newLevel = channels[i].Volume.Value / 256.0f; // Normalize to 0.0-1.0

					// Keep peak value (like ChannelLevelMeter.UpdateLevel)
					if (newLevel > renderer.VolumeBarState.AudioData.RealVolumeMeasured[i])
					{
						renderer.VolumeBarState.AudioData.RealVolumeMeasured[i] = newLevel;
					}
				}
				// For PumaTracker: when muted but still has sound, keep current level
				// This prevents immediate drop to 0 when note ends but sound continues
				else if (channels[i].Muted && renderer.VolumeBarState.AudioData.RealVolumeMeasured[i] > 0)
				{
					// Don't update - let it decay naturally
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Initialize visualization for a new module
		/// </summary>
		/********************************************************************/
		public static void InitializeVisualization(PatternRenderer renderer, int channels)
		{
			renderer.ChannelCount = channels;
			renderer.FirstVisibleChannel = 0; // Reset to first channel when loading new module

			// Reset volume bar decay for all channels
			if (renderer.VolumeBarState.AudioData.VolumeBarDecay.Length >= channels)
			{
				for (int i = 0; i < channels; i++)
				{
					renderer.VolumeBarState.AudioData.VolumeBarDecay[i] = 0;
					renderer.VolumeBarState.AudioData.RealVolumeSmoothed[i] = 0.0f;
					renderer.VolumeBarState.AudioData.RealVolumeMeasured[i] = 0.0f;
				}
			}

			// Reset debug counters for all channels
			for (int i = 0; i < 64; i++)
			{
				renderer.VolumeBarState.ChannelState.EnabledChangeCount[i] = 0;
				renderer.VolumeBarState.ChannelState.MutedChangeCount[i] = 0;
				renderer.LastEnabledState[i] = true; // Assume enabled by default
				renderer.LastMutedState[i] = false; // Assume not muted by default
			}
		}

		/********************************************************************/
		/// <summary>
		/// Process pattern row change information
		/// </summary>
		/********************************************************************/
		public static void ProcessRowChange(PatternRenderer renderer, SongRowChangeInfo rowInfo)
		{
			if (rowInfo == null)
			{
				return;
			}

			// Convert absolute position to relative position for subsongs
			renderer.CurrentSongPosition = rowInfo.SongPosition - renderer.StartPosition;
			renderer.Speed = rowInfo.Speed;
			renderer.Bpm = rowInfo.Bpm;
			renderer.CurrentRowInfo = rowInfo;

			// Get current song position data
			if (renderer.SongData != null && renderer.CurrentSongPosition >= 0 &&
			    renderer.CurrentSongPosition < renderer.SongData.Count)
			{
				renderer.CurrentSongPattern = renderer.SongData[renderer.CurrentSongPosition];
			}

			// Divide row by skip factor to get correct array index in compressed pattern
			int skip = renderer.CurrentSongPattern?.Skip ?? 1;
			renderer.CurrentRow = rowInfo.Row / skip;
		}
	}
}
