//---------------------------------------------------------------------------------------
// <copyright file="PatternRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Factory and wrapper for tracker-specific renderers
	/// </summary>
	internal class PatternRenderer : IDisposable
	{
		private TrackerPainterBase currentRenderer;
		private string invalidStyleId;

		/********************************************************************/
		/// <summary>
		/// Constructor - creates default SystemLight renderer
		/// </summary>
		/********************************************************************/
		internal PatternRenderer()
		{
			// Initialize with SystemLight theme as default
			currentRenderer = new ModernTrackerPainter();
			CurrentStyleId = ModernTrackerPainter.StyleIdLight;
			currentRenderer.UpdateStyle(CurrentStyleId);
		}

		// Delegate all properties to current renderer
		internal VolumeBarState VolumeBarState => currentRenderer.VolumeBarState;
		internal bool[] LastEnabledState => currentRenderer.LastEnabledState;
		internal bool[] LastMutedState => currentRenderer.LastMutedState;
		internal bool[] MixerChannelsEnabled => currentRenderer.MixerChannelsEnabled;

		internal string[] RowNumbers => currentRenderer.RowNumbers;

		internal Rectangle LeftScrollButtonRect
		{
			get => currentRenderer.LeftScrollButtonRect;
			set => currentRenderer.LeftScrollButtonRect = value;
		}

		internal Rectangle RightScrollButtonRect
		{
			get => currentRenderer.RightScrollButtonRect;
			set => currentRenderer.RightScrollButtonRect = value;
		}

		internal Rectangle[] ChannelBarRects
		{
			get => currentRenderer.ChannelBarRects;
			set => currentRenderer.ChannelBarRects = value;
		}

		internal DisplayMode CurrentDisplayMode
		{
			get => currentRenderer.CurrentDisplayMode;
			set => currentRenderer.CurrentDisplayMode = value;
		}

		internal NumberDisplayMode RowNumberDisplayMode
		{
			get => currentRenderer.RowNumberDisplayMode;
			set => currentRenderer.RowNumberDisplayMode = value;
		}

		internal NumberDisplayMode InstrumentDisplayMode
		{
			get => currentRenderer.InstrumentDisplayMode;
			set => currentRenderer.InstrumentDisplayMode = value;
		}

		internal NumberDisplayMode VolumeDisplayMode
		{
			get => currentRenderer.VolumeDisplayMode;
			set => currentRenderer.VolumeDisplayMode = value;
		}

		internal NumberDisplayMode TrackPatternDisplayMode
		{
			get => currentRenderer.TrackPatternDisplayMode;
			set => currentRenderer.TrackPatternDisplayMode = value;
		}

		internal GridLinesMode GridLinesDisplayMode
		{
			get => currentRenderer.GridLinesDisplayMode;
			set => currentRenderer.GridLinesDisplayMode = value;
		}

		internal bool RollingPatterns
		{
			get => currentRenderer.RollingPatterns;
			set => currentRenderer.RollingPatterns = value;
		}

		internal int ChannelSeparatorWidth => currentRenderer.ChannelSeparatorWidth;
		internal bool DrawSeparatorAfterLastChannel => currentRenderer.DrawSeparatorAfterLastChannel;

		internal bool StripedChannels
		{
			get => currentRenderer.StripedChannels;
			set => currentRenderer.StripedChannels = value;
		}

		internal bool HideEmpty
		{
			get => currentRenderer.HideEmpty;
			set => currentRenderer.HideEmpty = value;
		}

		internal bool ShowDebugInfo
		{
			get => currentRenderer.ShowDebugInfo;
			set => currentRenderer.ShowDebugInfo = value;
		}

		internal bool DrawGrid
		{
			get => currentRenderer.DrawGrid;
			set => currentRenderer.DrawGrid = value;
		}


		internal ColorMode CurrentColorMode
		{
			get => currentRenderer.CurrentColorMode;
			set => currentRenderer.CurrentColorMode = value;
		}

		/// <summary>
		/// Current style ID from registry
		/// </summary>
		internal string CurrentStyleId
		{
			get;
			private set;
		}

		internal List<SongPatternViewData> SongData
		{
			get => currentRenderer.SongData;
			set => currentRenderer.SongData = value;
		}

		internal SongPatternViewData CurrentSongPattern
		{
			get => currentRenderer.CurrentSongPattern;
			set => currentRenderer.CurrentSongPattern = value;
		}

		internal SongRowChangeInfo CurrentRowInfo
		{
			get => currentRenderer.CurrentRowInfo;
			set => currentRenderer.CurrentRowInfo = value;
		}

		internal ChannelChanged[] ChannelInfo
		{
			get => currentRenderer.ChannelInfo;
			set => currentRenderer.ChannelInfo = value;
		}

		internal bool HasVolumeColumn
		{
			get => currentRenderer.HasVolumeColumn;
			set => currentRenderer.HasVolumeColumn = value;
		}

		internal bool HasTrackNumber
		{
			get => currentRenderer.HasTrackNumber;
			set => currentRenderer.HasTrackNumber = value;
		}

		internal bool HasTranspose
		{
			get => currentRenderer.HasTranspose;
			set => currentRenderer.HasTranspose = value;
		}

		internal NoteTransposeMode TransposeMode
		{
			get => currentRenderer.TransposeMode;
			set => currentRenderer.TransposeMode = value;
		}

		internal bool ShowTransposedNotes
		{
			get => currentRenderer.ShowTransposedNotes;
			set => currentRenderer.ShowTransposedNotes = value;
		}

		internal int MaxEffectCount
		{
			get => currentRenderer.MaxEffectCount;
			set => currentRenderer.MaxEffectCount = value;
		}

		internal int EffectCharCount
		{
			get => currentRenderer.EffectCharCount;
			set => currentRenderer.EffectCharCount = value;
		}

		internal int MaxRowCount
		{
			get => currentRenderer.MaxRowCount;
			set => currentRenderer.MaxRowCount = value;
		}

		internal int ChannelCount
		{
			get => currentRenderer.ChannelCount;
			set => currentRenderer.ChannelCount = value;
		}

		internal int CurrentRow
		{
			get => currentRenderer.CurrentRow;
			set => currentRenderer.CurrentRow = value;
		}

		internal int FirstVisibleChannel
		{
			get => currentRenderer.FirstVisibleChannel;
			set => currentRenderer.FirstVisibleChannel = value;
		}

		internal bool IsPaused
		{
			get => currentRenderer.IsPaused;
			set => currentRenderer.IsPaused = value;
		}

		internal int ManualRow
		{
			get => currentRenderer.ManualRow;
			set => currentRenderer.ManualRow = value;
		}

		internal int ManualSongPosition
		{
			get => currentRenderer.ManualSongPosition;
			set => currentRenderer.ManualSongPosition = value;
		}

		internal bool AllowPatternScrolling => currentRenderer.AllowPatternScrolling;

		internal string SongTitle
		{
			get => currentRenderer.SongTitle;
			set => currentRenderer.SongTitle = value;
		}

		internal string PlayerName
		{
			get => currentRenderer.PlayerName;
			set => currentRenderer.PlayerName = value;
		}

		internal string FileName
		{
			get => currentRenderer.FileName;
			set => currentRenderer.FileName = value;
		}

		internal string SongFormat
		{
			get => currentRenderer.SongFormat;
			set => currentRenderer.SongFormat = value;
		}

		internal int CurrentSongPosition
		{
			get => currentRenderer.CurrentSongPosition;
			set => currentRenderer.CurrentSongPosition = value;
		}

		internal int SongLength
		{
			get => currentRenderer.SongLength;
			set => currentRenderer.SongLength = value;
		}

		internal int SubSongCurrent
		{
			get => currentRenderer.SubSongCurrent;
			set => currentRenderer.SubSongCurrent = value;
		}

		internal int SubSongTotal
		{
			get => currentRenderer.SubSongTotal;
			set => currentRenderer.SubSongTotal = value;
		}

		internal int StartPosition
		{
			get => currentRenderer.StartPosition;
			set => currentRenderer.StartPosition = value;
		}

		internal int Speed
		{
			get => currentRenderer.Speed;
			set => currentRenderer.Speed = value;
		}

		internal int? Bpm
		{
			get => currentRenderer.Bpm;
			set => currentRenderer.Bpm = value;
		}

		internal bool LeftScrollButtonHover
		{
			get => currentRenderer.LeftScrollButtonHover;
			set => currentRenderer.LeftScrollButtonHover = value;
		}

		internal bool RightScrollButtonHover
		{
			get => currentRenderer.RightScrollButtonHover;
			set => currentRenderer.RightScrollButtonHover = value;
		}

		internal bool ShowControlBar
		{
			get => currentRenderer.ShowControlBar;
			set => currentRenderer.ShowControlBar = value;
		}

		internal bool AutoCompress
		{
			get => currentRenderer.AutoCompress;
			set => currentRenderer.AutoCompress = value;
		}

		internal TimeSpan ElapsedTime
		{
			get => currentRenderer.ElapsedTime;
			set => currentRenderer.ElapsedTime = value;
		}

		internal TimeSpan TotalTime
		{
			get => currentRenderer.TotalTime;
			set => currentRenderer.TotalTime = value;
		}

		internal int SnapshotPosition
		{
			get => currentRenderer.SnapshotPosition;
			set => currentRenderer.SnapshotPosition = value;
		}

		internal int SnapshotCount
		{
			get => currentRenderer.SnapshotCount;
			set => currentRenderer.SnapshotCount = value;
		}

		internal int ModuleIndex
		{
			get => currentRenderer.ModuleIndex;
			set => currentRenderer.ModuleIndex = value;
		}

		internal int ModuleCount
		{
			get => currentRenderer.ModuleCount;
			set => currentRenderer.ModuleCount = value;
		}

		internal Control PatternPanel
		{
			get => currentRenderer.PatternPanel;
			set => currentRenderer.PatternPanel = value;
		}

		internal PressedButton ControlBarPressedButton
		{
			get => currentRenderer.ControlBarPressedButton;
			set => currentRenderer.ControlBarPressedButton = value;
		}

		internal long TotalRenderTimeTicks => currentRenderer.TotalRenderTimeTicks;

		/********************************************************************/
		/// <summary>
		/// Dispose resources
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			currentRenderer?.Dispose();
		}

		/********************************************************************/
		/// <summary>
		/// Get error message - checks for invalid style first
		/// </summary>
		/********************************************************************/
		internal string GetErrorMessage()
		{
			if (!string.IsNullOrEmpty(invalidStyleId))
			{
				return $"Unknown style '{invalidStyleId}'";
			}

			return currentRenderer.SongErrorText;
		}

		internal void ResetRenderStats()
		{
			currentRenderer.ResetRenderStats();
		}

		internal bool IsChannelMuted(int channel)
		{
			return currentRenderer.IsChannelMuted(channel);
		}

		/********************************************************************/
		/// <summary>
		/// Update style using registry-based style ID
		/// </summary>
		/// <param name="styleId">The registry style ID (e.g., "ProTracker.ProTracker23")</param>
		/// <returns>TrackerStyleColors containing the background colors for this style</returns>
		/********************************************************************/
		internal TrackerStyleColors UpdateStyle(string styleId)
		{
			TrackerStyleRegistration registration = TrackerRegistry.Get(styleId);
			if (registration == null)
			{
				invalidStyleId = styleId;
				styleId = ModernTrackerPainter.StyleIdLight;
				registration = TrackerRegistry.Get(styleId);
			}
			else
			{
				invalidStyleId = null;
			}

			CurrentStyleId = styleId;

			// Check if we need to create a new renderer
			bool needsNewRenderer = currentRenderer?.GetType() != registration.RendererType;

			if (needsNewRenderer)
			{
				// Save current state
				TrackerPainterBase oldRenderer = currentRenderer;

				// Create new renderer
				currentRenderer = registration.CreateRenderer();

				// Copy state from old renderer
				if (oldRenderer != null)
				{
					CopyRendererState(oldRenderer, currentRenderer);
					oldRenderer.Dispose();
				}
			}

			// Call initialization (sets color scheme, etc.)
			registration.Initialize?.Invoke(currentRenderer);

			// Set style ID on renderer (and get colors)
			TrackerStyleColors styleColors = currentRenderer.UpdateStyle(styleId);

			// Invalidate bitmap cache on style change
			InvalidateCache();

			// Return style colors from current renderer
			return styleColors;
		}

		/********************************************************************/
		/// <summary>
		/// Copy state from old renderer to new renderer
		/// </summary>
		/********************************************************************/
		private void CopyRendererState(TrackerPainterBase from, TrackerPainterBase to)
		{
			// Copy all state properties
			to.CurrentDisplayMode = from.CurrentDisplayMode;
			to.RowNumberDisplayMode = from.RowNumberDisplayMode;
			to.InstrumentDisplayMode = from.InstrumentDisplayMode;
			to.VolumeDisplayMode = from.VolumeDisplayMode;
			to.TrackPatternDisplayMode = from.TrackPatternDisplayMode;
			to.GridLinesDisplayMode = from.GridLinesDisplayMode;
			to.VolumeBarState.DisplaySettings.Mode = from.VolumeBarState.DisplaySettings.Mode;
			to.VolumeBarState.DisplaySettings.VuMeterId = from.VolumeBarState.DisplaySettings.VuMeterId;
			to.RollingPatterns = from.RollingPatterns;
			to.StripedChannels = from.StripedChannels;
			to.HideEmpty = from.HideEmpty;
			to.ShowDebugInfo = from.ShowDebugInfo;
			to.DrawGrid = from.DrawGrid;
			to.CurrentColorMode = from.CurrentColorMode;

			to.SongData = from.SongData;
			to.CurrentSongPattern = from.CurrentSongPattern;
			to.ChannelInfo = from.ChannelInfo;
			to.HasVolumeColumn = from.HasVolumeColumn;
			to.HasTrackNumber = from.HasTrackNumber;
			to.HasTranspose = from.HasTranspose;
			to.TransposeMode = from.TransposeMode;
			to.ShowTransposedNotes = from.ShowTransposedNotes;
			to.MaxEffectCount = from.MaxEffectCount;
			to.EffectCharCount = from.EffectCharCount;
			to.MaxRowCount = from.MaxRowCount;

			to.ChannelCount = from.ChannelCount;
			to.CurrentRow = from.CurrentRow;
			to.FirstVisibleChannel = from.FirstVisibleChannel;

			to.IsPaused = from.IsPaused;
			to.ManualRow = from.ManualRow;
			to.ManualSongPosition = from.ManualSongPosition;

			to.SongTitle = from.SongTitle;
			to.PlayerName = from.PlayerName;
			to.FileName = from.FileName;
			to.SongFormat = from.SongFormat;
			to.CurrentPatternNumber = from.CurrentPatternNumber;
			to.CurrentSongPosition = from.CurrentSongPosition;
			to.SongLength = from.SongLength;
			to.Speed = from.Speed;
			to.Bpm = from.Bpm;
			to.SubSongCurrent = from.SubSongCurrent;
			to.SubSongTotal = from.SubSongTotal;

			to.LeftScrollButtonHover = from.LeftScrollButtonHover;
			to.RightScrollButtonHover = from.RightScrollButtonHover;

			to.ShowControlBar = from.ShowControlBar;
			to.ElapsedTime = from.ElapsedTime;
			to.TotalTime = from.TotalTime;

			to.PatternPanel = from.PatternPanel;

			// Copy volume bar state
			Array.Copy(from.VolumeBarState.AudioData.VolumeBarDecay, to.VolumeBarState.AudioData.VolumeBarDecay,
				from.VolumeBarState.AudioData.VolumeBarDecay.Length);
			Array.Copy(from.VolumeBarState.AudioData.RealVolumeSmoothed, to.VolumeBarState.AudioData.RealVolumeSmoothed,
				from.VolumeBarState.AudioData.RealVolumeSmoothed.Length);
			Array.Copy(from.VolumeBarState.AudioData.RealVolumeMeasured, to.VolumeBarState.AudioData.RealVolumeMeasured,
				from.VolumeBarState.AudioData.RealVolumeMeasured.Length);

			// Copy mixer channel enabled state
			Array.Copy(from.MixerChannelsEnabled, to.MixerChannelsEnabled, from.MixerChannelsEnabled.Length);
		}

		/********************************************************************/
		/// <summary>
		/// Draw simple grid (delegates to current renderer)
		/// </summary>
		/********************************************************************/
		internal void DrawSimpleGrid(Graphics g)
		{
			currentRenderer.DrawSimpleGrid(g);
		}

		/********************************************************************/
		/// <summary>
		/// Draw with bitmap caching (delegates to current renderer)
		/// </summary>
		/// <returns>True if pattern was re-rendered, false if cache was used</returns>
		/********************************************************************/
		internal bool DrawWithCache(Graphics g, Size panelSize)
		{
			return currentRenderer.DrawWithCache(g, panelSize);
		}

		/********************************************************************/
		/// <summary>
		/// Invalidate bitmap cache (call on row change, resize, etc.)
		/// </summary>
		/********************************************************************/
		internal void InvalidateCache()
		{
			currentRenderer.BitmapCache.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Animate volumes (delegates to current renderer)
		/// </summary>
		/********************************************************************/
		internal void AnimateVolumes()
		{
			currentRenderer.AnimateVolumes();
		}

		/********************************************************************/
		/// <summary>
		/// Import song module data and reset renderer state
		/// </summary>
		/********************************************************************/
		internal void ImportSongModule(SongModule data)
		{
			// Invalidate bitmap cache on song change
			InvalidateCache();

			// Clear effect sanitizer cache on song change
			currentRenderer.ClearEffectCache();

			// Reset all renderer state
			ChannelCount = 0;
			FirstVisibleChannel = 0;
			CurrentRow = 0;
			CurrentSongPosition = 0;
			ManualRow = 0;
			ManualSongPosition = 0;
			SongLength = 0;
			Speed = 6;
			Bpm = 125;
			SongTitle = "";
			PlayerName = data?.PlayerName ?? "";
			FileName = data?.FileName ?? "";
			SongFormat = data?.SongFormat ?? "";
			SongData = null;
			CurrentSongPattern = null;
			ChannelInfo = null;
			IsPaused = false;
		}

		/********************************************************************/
		/// <summary>
		/// Import song patterns data
		/// </summary>
		/********************************************************************/
		internal void ImportSongPatterns(SongPatterns moduleInfo)
		{
			if (moduleInfo != null)
			{
				// Store module info
				SongTitle = moduleInfo.ModuleTitle ?? string.Empty;
				SongLength = moduleInfo.SongLength;
				SubSongCurrent = moduleInfo.SubSongCurrent;
				SubSongTotal = moduleInfo.SubSongTotal;
				StartPosition = moduleInfo.StartPosition;
				Speed = moduleInfo.InitialSpeed;
				Bpm = moduleInfo.InitialBpm ?? 0;
				HasVolumeColumn = moduleInfo.HasVolumeColumn;
				HasTrackNumber = moduleInfo.HasTrackNumber;
				HasTranspose = moduleInfo.TransposeMode != NoteTransposeMode.NoTranspose;
				TransposeMode = moduleInfo.TransposeMode;
				AutoCompress = moduleInfo.AutoCompress;
				currentRenderer.SongErrorText = moduleInfo.ErrorMessage ?? string.Empty;

				// Store effect character count from player
				EffectCharCount = moduleInfo.EffectCharCount;

				// Store song data
				SongData = moduleInfo.SongData;

				// Validate effect lengths match EffectCharCount
				ValidateEffectLengths(moduleInfo.SongData, moduleInfo.EffectCharCount);

				// Calculate maximum effect count and row count
				(int maxEffectCount, int maxRowCount) =
					PatternControlHelper.CalculatePatternMetrics(moduleInfo.SongData);
				MaxEffectCount = maxEffectCount;
				MaxRowCount = maxRowCount;

				// Calculate maximum channel count across all patterns
				if (moduleInfo.SongData != null && moduleInfo.SongData.Count > 0)
				{
					ChannelCount = moduleInfo.SongData.Max(pattern => pattern.ChannelCount);
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Validate that all effect strings have the correct length
		/// </summary>
		/********************************************************************/
		private static void ValidateEffectLengths(List<SongPatternViewData> songData, int effectCharCount)
		{
			if (songData == null || effectCharCount == 0)
			{
				return;
			}

			foreach (SongPatternViewData pattern in songData)
			{
				if (pattern.Rows == null)
				{
					continue;
				}

				foreach (SongPatternViewRow row in pattern.Rows)
				{
					if (row?.Channels == null)
					{
						continue;
					}

					foreach (SongPatternViewChannel channel in row.Channels)
					{
						if (channel?.EffectText == null)
						{
							continue;
						}

						foreach (string effect in channel.EffectText)
						{
							if (!string.IsNullOrEmpty(effect) && effect.Length != effectCharCount)
							{
								throw new InvalidOperationException(
									$"Effect length mismatch! Expected {effectCharCount} chars, got {effect.Length} chars. " +
									$"Effect: '{effect}' in pattern {pattern.PatternNumber}, row {row.RowNumber}");
							}
						}
					}
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Clear all song data
		/// </summary>
		/********************************************************************/
		internal void ClearSongData()
		{
			// Invalidate bitmap cache
			InvalidateCache();

			SongTitle = "";
			FileName = "";
			SongData = null;
			VolumeBarState.ResetCounters();
		}
	}
}
