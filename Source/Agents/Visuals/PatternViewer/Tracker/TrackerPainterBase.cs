//---------------------------------------------------------------------------------------
// <copyright file="TrackerPainterBase.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker
{
	/// <summary>
	/// Color mode for pattern display (modern tracker only)
	/// </summary>
	internal enum ColorMode
	{
		Mono,
		Colored
	}

	/// <summary>
	/// Font scale for bitmap font rendering
	/// </summary>
	internal enum FontScale
	{
		Scale100 = 100,
		Scale125 = 125,
		Scale150 = 150,
		Scale175 = 175,
		Scale200 = 200,
		Scale300 = 300,
		Scale400 = 400
	}

	/// <summary>
	/// Base class for tracker-specific rendering implementations
	/// Contains all common data and delegates drawing to derived classes
	/// </summary>
	internal abstract class TrackerPainterBase : IDisposable
	{
		protected const float PATTERN_FONT_SIZE = 9.0f; // Font size for pattern display
		protected const int MIN_EFFECT_CHARS = 3; // Minimum effect column width in chars

		// Render time tracking

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected TrackerPainterBase()
		{
			VolumeBarState = new VolumeBarState();
			BitmapCache = new PatternBitmapCache();

			LastEnabledState = new bool[64];
			LastMutedState = new bool[64];
			// Initialize all channels as enabled by default
			for (int i = 0; i < 64; i++)
			{
				LastEnabledState[i] = true;
			}

			// Initialize all mixer channels as enabled by default
			for (int i = 0; i < 64; i++)
			{
				MixerChannelsEnabled[i] = true;
			}

			RowNumbers = new string[256];
			for (int i = 0; i < 256; i++)
			{
				RowNumbers[i] = i.ToString("00");
			}
		}

		// Volume bar state
		internal VolumeBarState VolumeBarState
		{
			get;
		}

		// Bitmap cache for pattern rendering
		internal PatternBitmapCache BitmapCache
		{
			get;
		}

		internal string[] RowNumbers
		{
			get;
		}

		internal bool[] LastEnabledState
		{
			get;
		}

		internal bool[] LastMutedState
		{
			get;
		}

		/// <summary>
		/// Mixer channel enabled states (from mixer settings, not player)
		/// </summary>
		internal bool[] MixerChannelsEnabled
		{
			get;
		} = new bool[64];

		internal Rectangle LeftScrollButtonRect
		{
			get;
			set;
		}

		internal Rectangle RightScrollButtonRect
		{
			get;
			set;
		}

		internal Rectangle[] ChannelBarRects
		{
			get;
			set;
		} = new Rectangle[64];

		// Display mode settings
		internal DisplayMode CurrentDisplayMode
		{
			get;
			set;
		} = DisplayMode.MultiEffect;

		internal NumberDisplayMode RowNumberDisplayMode
		{
			get;
			set;
		} = NumberDisplayMode.Auto;

		internal NumberDisplayMode InstrumentDisplayMode
		{
			get;
			set;
		} = NumberDisplayMode.Auto;

		internal NumberDisplayMode VolumeDisplayMode
		{
			get;
			set;
		} = NumberDisplayMode.Auto;

		internal NumberDisplayMode TrackPatternDisplayMode
		{
			get;
			set;
		} = NumberDisplayMode.Auto;

		internal GridLinesMode GridLinesDisplayMode
		{
			get;
			set;
		} = GridLinesMode.Auto;

		internal bool RollingPatterns
		{
			get;
			set;
		} = false;

		internal int ChannelSeparatorWidth
		{
			get;
			private protected set;
		} = 3;

		internal bool DrawSeparatorAfterLastChannel
		{
			get;
			private protected set;
		} = true;

		internal bool StripedChannels
		{
			get;
			set;
		} = false;

		internal bool HideEmpty
		{
			get;
			set;
		} = true;

		internal bool ShowDebugInfo
		{
			get;
			set;
		} = false;

		internal bool DrawGrid
		{
			get;
			set;
		} = true; // Deprecated - use GridLinesDisplayMode

		internal ColorMode CurrentColorMode
		{
			get;
			set;
		} = ColorMode.Mono;

		internal string CurrentStyleId
		{
			get;
			private protected set;
		}

		/// <summary>
		/// Default VU-Meter id (Family.Variant) for this renderer. Can be overridden per TrackerStyleRegistration.
		/// </summary>
		internal virtual string DefaultVuMeterId => ModernTrackerPainter.VuMeterIdLight;

		// Song pattern data
		internal List<SongPatternViewData> SongData
		{
			get;
			set;
		}

		internal SongPatternViewData CurrentSongPattern
		{
			get;
			set;
		}

		internal SongRowChangeInfo CurrentRowInfo
		{
			get;
			set;
		}

		internal ChannelChanged[] ChannelInfo
		{
			get => VolumeBarState.ChannelConfig.ChannelInfo;
			set => VolumeBarState.ChannelConfig.ChannelInfo = value;
		}

		internal bool HasVolumeColumn
		{
			get;
			set;
		} = false;

		internal bool HasTrackNumber
		{
			get;
			set;
		} = false;

		internal bool HasTranspose
		{
			get;
			set;
		} = false;

		/// <summary>
		/// How transpose is handled by the player (NoTranspose, NotesRaw, NotesTransposed)
		/// </summary>
		internal NoteTransposeMode TransposeMode
		{
			get;
			set;
		} = NoteTransposeMode.NoTranspose;

		/// <summary>
		/// Whether to show transposed notes (from settings)
		/// </summary>
		internal bool ShowTransposedNotes
		{
			get;
			set;
		} = true;

		/// <summary>
		/// Maximum number of effects per channel (e.g., 1, 2, 3)
		/// </summary>
		internal int MaxEffectCount
		{
			get;
			set;
		} = 1;

		/// <summary>
		/// Number of characters per effect as declared by the player.
		/// 0 means no effects are shown.
		/// </summary>
		internal int EffectCharCount
		{
			get;
			set;
		} = 3;

		internal int MaxRowCount
		{
			get;
			set;
		} = 64;

		// Display parameters
		internal int ChannelCount
		{
			get => VolumeBarState.ChannelConfig.ChannelCount;
			set => VolumeBarState.ChannelConfig.ChannelCount = value;
		}

		internal int CurrentRow
		{
			get;
			set;
		} = 0;

		internal int FirstVisibleChannel
		{
			get => VolumeBarState.ChannelConfig.FirstVisibleChannel;
			set => VolumeBarState.ChannelConfig.FirstVisibleChannel = value;
		}

		// Pause state
		internal bool IsPaused
		{
			get;
			set;
		} = false;

		internal int ManualRow
		{
			get;
			set;
		} = 0;

		internal int ManualSongPosition
		{
			get;
			set;
		} = 0;

		internal bool AllowPatternScrolling => IsPaused;

		// Song info
		internal string SongTitle
		{
			get;
			set;
		} = string.Empty;

		internal string PlayerName
		{
			get;
			set;
		} = string.Empty;

		internal string FileName
		{
			get;
			set;
		} = string.Empty;

		internal string SongFormat
		{
			get;
			set;
		} = string.Empty;

		internal string SongErrorText
		{
			get;
			set;
		} = string.Empty;

		internal int CurrentPatternNumber
		{
			get;
			set;
		} = 0;

		internal int CurrentSongPosition
		{
			get;
			set;
		} = 0;

		internal int SongLength
		{
			get;
			set;
		} = 128;

		internal int SubSongCurrent
		{
			get;
			set;
		} = 1;

		internal int SubSongTotal
		{
			get;
			set;
		} = 1;

		internal int StartPosition
		{
			get;
			set;
		} = 0;

		internal int Speed
		{
			get;
			set;
		} = 6;

		internal int? Bpm
		{
			get;
			set;
		} = null;

		// Hover state
		internal bool LeftScrollButtonHover
		{
			get;
			set;
		} = false;

		internal bool RightScrollButtonHover
		{
			get;
			set;
		} = false;

		// Control bar support
		internal bool ShowControlBar
		{
			get;
			set;
		} = false;

		internal ControlBarConfig ControlBarConfig
		{
			get;
			set;
		} = ControlBarConfig.Default;

		internal PressedButton ControlBarPressedButton
		{
			get;
			set;
		} = PressedButton.None;

		// Pattern compression support - indicates if player's format supports auto-compression
		internal bool AutoCompress
		{
			get;
			set;
		} = false;

		internal TimeSpan ElapsedTime
		{
			get;
			set;
		} = TimeSpan.Zero;

		internal TimeSpan TotalTime
		{
			get;
			set;
		} = TimeSpan.Zero;

		internal int SnapshotPosition
		{
			get;
			set;
		}

		internal int SnapshotCount
		{
			get;
			set;
		}

		internal int ModuleIndex
		{
			get;
			set;
		}

		internal int ModuleCount
		{
			get;
			set;
		}

		/// <summary>
		/// Effect text sanitizer for bitmap fonts
		/// </summary>
		internal EffectSanitizer EffectSanitizer
		{
			get;
		} = new();

		// Panel reference
		internal Control PatternPanel
		{
			get;
			set;
		}

		internal long TotalRenderTimeTicks
		{
			get;
			private set;
		}

		/********************************************************************/
		/// <summary>
		/// Dispose resources
		/// </summary>
		/********************************************************************/
		public abstract void Dispose();

		internal void ResetRenderStats()
		{
			TotalRenderTimeTicks = 0;
		}

		/********************************************************************/
		/// <summary>
		/// Clear effect cache (call on song change)
		/// </summary>
		/********************************************************************/
		internal void ClearEffectCache()
		{
			EffectSanitizer.ClearCache();
		}

		/********************************************************************/
		/// <summary>
		/// Check if a channel is muted in the mixer
		/// </summary>
		/********************************************************************/
		internal bool IsChannelMuted(int channel)
		{
			return channel >= 0 && channel < MixerChannelsEnabled.Length && !MixerChannelsEnabled[channel];
		}

		/********************************************************************/
		/// <summary>
		/// Format BPM for display (null -> "---")
		/// </summary>
		/********************************************************************/
		internal static string FormatBpm(int? bpm)
		{
			return PatternContentFormatter.FormatBpm(bpm);
		}

		/********************************************************************/
		/// <summary>
		/// Get pattern font size based on FontScale setting
		/// </summary>
		/********************************************************************/
		protected float GetPatternFontSize()
		{
			float baseSize = 9.0f;
			float scaleFactor = (int)BitmapFontRenderer.CurrentFontScale / 100f;
			return baseSize * scaleFactor;
		}

		/********************************************************************/
		/// <summary>
		/// Update style (creates new pens/brushes/fonts for the style)
		/// </summary>
		/// <param name="styleId">The style ID from registration</param>
		/// <returns>TrackerStyleColors containing the background colors for this style</returns>
		/********************************************************************/
		internal abstract TrackerStyleColors UpdateStyle(string styleId);

		/********************************************************************/
		/// <summary>
		/// Get the current style colors (background colors for pattern panel and control)
		/// Default implementation calls UpdateStyle with current style
		/// </summary>
		/********************************************************************/
		internal virtual TrackerStyleColors GetStyleColors()
		{
			return UpdateStyle(CurrentStyleId);
		}

		/********************************************************************/
		/// <summary>
		/// Draw the complete pattern grid (legacy entry point, called by RenderViewer)
		/// </summary>
		/********************************************************************/
		internal abstract void DrawSimpleGrid(Graphics g);

		/********************************************************************/
		/// <summary>
		/// Draw with bitmap caching - renders pattern to cache if invalid,
		/// then draws cached bitmap and VU meters on top
		/// </summary>
		/// <returns>True if pattern was re-rendered, false if cache was used</returns>
		/********************************************************************/
		internal bool DrawWithCache(Graphics g, Size panelSize)
		{
			bool renderedPattern = false;

			// Check if we need to re-render the pattern
			if (!BitmapCache.TryGetValid(out Bitmap cachedBitmap, out _))
			{
				// Get or create bitmap for rendering
				cachedBitmap = BitmapCache.GetOrCreateBitmap(panelSize);
				if (cachedBitmap == null)
				{
					return false;
				}

				// Render pattern to bitmap
				using (Graphics bitmapGraphics = Graphics.FromImage(cachedBitmap))
				{
					bitmapGraphics.Clear(Color.Transparent);

#if DEBUG
					long startTicks = Stopwatch.GetTimestamp();
#endif
					Rectangle panelRect = new(0, 0, panelSize.Width, panelSize.Height);

					// Outer margin around the pattern viewer content
					int outerMargin = OuterMarginSize;
					if (outerMargin > 0)
					{
						using SolidBrush marginBrush = new(OuterMarginColor);
						bitmapGraphics.FillRectangle(marginBrush, panelRect);
					}

					Rectangle contentRect = new(
						panelRect.X + outerMargin,
						panelRect.Y + outerMargin,
						panelRect.Width - (outerMargin * 2),
						panelRect.Height - (outerMargin * 2));

					GraphicsState clipState = bitmapGraphics.Save();
					bitmapGraphics.SetClip(contentRect);
					RenderViewerResult? viewerResult = RenderViewer(bitmapGraphics, contentRect);
					bitmapGraphics.Restore(clipState);
#if DEBUG
					TotalRenderTimeTicks += Stopwatch.GetTimestamp() - startTicks;
#endif

					// Store viewer result (new rect-based approach)
					BitmapCache.Parameters.ViewerResult = viewerResult;
				}

				BitmapCache.SetValid();
				renderedPattern = true;
			}

			// Draw cached bitmap to screen
			g.DrawImageUnscaled(cachedBitmap, 0, 0);

			return renderedPattern;
		}

		/********************************************************************/
		/// <summary>
		/// Animate volume values (called by timer at 50 Hz)
		/// </summary>
		/********************************************************************/
		internal void AnimateVolumes()
		{
			// Handle NoteKick mode decay (time-based, not row-based)
			if (VolumeBarState.DisplaySettings.Mode == VolumeBarMode.NoteKick)
			{
				for (int i = 0; i < VolumeBarState.AudioData.VolumeBarDecay.Length; i++)
				{
					if (VolumeBarState.AudioData.VolumeBarDecay[i] > 0)
					{
						VolumeBarState.AudioData.VolumeBarDecay[i]--; // Decay at 50Hz rate
					}
				}

				return;
			}

			// Handle RealVolume mode
			if (VolumeBarState.DisplaySettings.Mode == VolumeBarMode.RealVolume)
			{
				const int refreshRate = 50;
				float levelDecay = 1.0f - (float)Math.Exp(-1.0f / (refreshRate * 0.2f)); // ≈ 0.095

				for (int i = 0; i < VolumeBarState.AudioData.RealVolumeSmoothed.Length; i++)
				{
					// Get the measured level (peak hold)
					float newLevel = VolumeBarState.AudioData.RealVolumeMeasured[i];

					// Reset to 0 like Channel Meter does
					// This means we rely on getting continuous updates
					VolumeBarState.AudioData.RealVolumeMeasured[i] = 0.0f;

					// Apply smoothing (identical to Channel Meter)
					if (newLevel > VolumeBarState.AudioData.RealVolumeSmoothed[i])
					{
						VolumeBarState.AudioData.RealVolumeSmoothed[i] = newLevel; // Jump up immediately
					}
					else
					{
						VolumeBarState.AudioData.RealVolumeSmoothed[i] +=
							(newLevel - VolumeBarState.AudioData.RealVolumeSmoothed[i]) * levelDecay; // Fall slowly
					}
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Get the formatted track name for the given channel
		/// </summary>
		/********************************************************************/
		protected string GetTrackName(int channelIndex)
		{
			return PatternContentFormatter.GetTrackName(new GetTrackNameInput
			{
				ChannelIndex = channelIndex,
				CurrentDisplayMode = CurrentDisplayMode,
				EffectCharCount = EffectCharCount,
				AllowPatternScrolling = AllowPatternScrolling,
				ManualSongPosition = ManualSongPosition,
				CurrentSongPosition = CurrentSongPosition,
				ManualRow = ManualRow,
				CurrentRow = CurrentRow,
				SongData = SongData,
				CurrentSongPattern = CurrentSongPattern,
				CurrentRowInfo = CurrentRowInfo,
				HasTrackNumber = HasTrackNumber,
				HasTranspose = HasTranspose
			});
		}

		#region New Rect-based rendering (for new trackers)
		/********************************************************************/
		/// <summary>
		/// Render the complete viewer (new entry point with rect parameter)
		/// Override this in new trackers and leave DrawSimpleGrid empty
		/// </summary>
		/********************************************************************/
		internal virtual RenderViewerResult? RenderViewer(Graphics g, Rectangle panelRect)
		{
			// Default: call legacy method for backwards compatibility
			DrawSimpleGrid(g);
			return null;
		}

		/********************************************************************/
		/// <summary>
		/// Outer margin size around the pattern viewer content (default: 0)
		/// </summary>
		/********************************************************************/
		protected virtual int OuterMarginSize => 0;

		/********************************************************************/
		/// <summary>
		/// Outer margin color (default: Black)
		/// </summary>
		/********************************************************************/
		protected virtual Color OuterMarginColor => Color.Black;

		/********************************************************************/
		/// <summary>
		/// Render the header section (status bar + control bar)
		/// </summary>
		/// <returns>Result containing height consumed by header</returns>
		/********************************************************************/
		protected virtual RenderHeaderResult RenderHeader(Graphics g, Rectangle rect)
		{
			int totalHeight = 0;

			// Render header bar (song info: name, position, speed, etc.)
			Rectangle headerBarRect = new(rect.X, rect.Y, rect.Width, rect.Height);
			GraphicsState state1 = g.Save();
			try
			{
				g.SetClip(headerBarRect);
				RenderSectionResult headerBarResult = RenderHeaderBar(g, headerBarRect);
				totalHeight += headerBarResult.Height;
			}
			finally
			{
				g.Restore(state1);
			}

			// Render control bar if enabled
			if (ShowControlBar)
			{
				Rectangle controlBarRect = new(rect.X, rect.Y + totalHeight, rect.Width, rect.Height - totalHeight);
				GraphicsState state2 = g.Save();
				try
				{
					g.SetClip(controlBarRect);
					RenderSectionResult controlBarResult = RenderControlBar(g, controlBarRect);
					totalHeight += controlBarResult.Height;
				}
				finally
				{
					g.Restore(state2);
				}
			}

			return new RenderHeaderResult {Height = totalHeight};
		}

		/********************************************************************/
		/// <summary>
		/// Render the header bar (song info: name, position, speed, etc.)
		/// </summary>
		/// <returns>Result containing height consumed by header bar</returns>
		/********************************************************************/
		protected virtual RenderSectionResult RenderHeaderBar(Graphics g, Rectangle rect)
		{
			// Default implementation - override in trackers
			return new RenderSectionResult {Height = 0};
		}

		/********************************************************************/
		/// <summary>
		/// Render the control bar
		/// </summary>
		/// <returns>Result containing height consumed by control bar</returns>
		/********************************************************************/
		protected virtual RenderSectionResult RenderControlBar(Graphics g, Rectangle rect)
		{
			// Default implementation - override in trackers
			return new RenderSectionResult {Height = 0};
		}

		/********************************************************************/
		/// <summary>
		/// Render the pattern section (track headers + pattern rows)
		/// </summary>
		/********************************************************************/
		protected virtual void RenderPattern(Graphics g, Rectangle rect)
		{
			// Render track headers
			Rectangle trackHeadersRect = new(rect.X, rect.Y, rect.Width, rect.Height);
			RenderSectionResult trackHeadersResult;
			GraphicsState state1 = g.Save();
			try
			{
				g.SetClip(trackHeadersRect);
				trackHeadersResult = RenderTrackHeaders(g, trackHeadersRect);
			}
			finally
			{
				g.Restore(state1);
			}

			// Render pattern rows in remaining space
			Rectangle patternRowsRect = new(rect.X, rect.Y + trackHeadersResult.Height, rect.Width, rect.Height - trackHeadersResult.Height);
			GraphicsState state2 = g.Save();
			try
			{
				g.SetClip(patternRowsRect);
				RenderPatternRows(g, patternRowsRect);
			}
			finally
			{
				g.Restore(state2);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Render the track headers
		/// </summary>
		/// <returns>Result containing height consumed by track headers</returns>
		/********************************************************************/
		protected virtual RenderSectionResult RenderTrackHeaders(Graphics g, Rectangle rect)
		{
			// Default implementation - override in trackers
			return new RenderSectionResult {Height = 0};
		}

		/********************************************************************/
		/// <summary>
		/// Render the pattern rows
		/// </summary>
		/********************************************************************/
		protected virtual PatternRowsResult? RenderPatternRows(Graphics g, Rectangle rect)
		{
			// Default implementation - override in trackers
			return null;
		}
		#endregion
	}

	/// <summary>
	/// Result from RenderHeader
	/// </summary>
	internal readonly struct RenderHeaderResult
	{
		/// <summary>
		/// Height consumed by the header section
		/// </summary>
		public required int Height
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Result from rendering a section (status bar, control bar, track headers, etc.)
	/// </summary>
	internal readonly struct RenderSectionResult
	{
		/// <summary>
		/// Height consumed by the section
		/// </summary>
		public required int Height
		{
			get;
			init;
		}
	}
}
