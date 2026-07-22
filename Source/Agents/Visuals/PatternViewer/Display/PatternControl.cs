//---------------------------------------------------------------------------------------
// <copyright file="PatternControl.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows.Forms;
#if DEBUG
using System.Diagnostics;
#endif
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
	/// <summary>
	/// SongPattern display control
	/// </summary>
	internal partial class PatternControl : UserControl, IWantClientPlayerControl
	{
		// Context menu
		private readonly PatternContextMenu contextMenu;

#if DEBUG
		private readonly Timer paintCounterTimer;
#endif

		/// <summary>
		/// Fired once after a PatternControl is fully constructed. Optional companions
		/// (e.g. the Debugger) hook here to attach themselves to instance events.
		/// </summary>
		public static event Action<PatternControl> Created;

		/// <summary>
		/// Fired on every row change with the full row info.
		/// </summary>
		public event Action<SongRowChangeInfo, int> RowChanged;

		/// <summary>
		/// Fired when a new module is loaded and previous run-state should be cleared.
		/// </summary>
		public event Action SongDataReset;

		// SongPattern renderer
		private readonly PatternRenderer renderer;

		// Settings
		private readonly PatternViewerSettings settings;

		// Client player control surface for playback / navigation
		private IClientPlayerControl clientPlayer;

		// Current song module for pattern compression and compare
		private SongModule currentSongModule;

#if DEBUG
		private int lastPaintCount;
		private int lastPatternRenderCount;

		// Paint counter and pattern render counter (title bar diagnostics)
		private int paintCount;
		private int patternRenderCount;
#endif

		// Processed patterns (compressed or not, based on settings)
		private SongPatterns processedPatterns;

		// Channel count from InitVisual - remembered here because ImportSongModule
		// resets renderer.ChannelCount to 0 before the patterns are built
		private int initChannelCount;

		// Last elapsed second drawn for a dummy pattern, so the clock can be
		// refreshed once a second when the VU meters are off
		private int lastDummySecond = -1;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PatternControl(ISettingsFactory settingsFactory)
		{
			InitializeComponent();

			// Create renderer
			renderer = new PatternRenderer();
			renderer.PatternPanel = patternPanel;

			// Load settings
			settings = new PatternViewerSettings(settingsFactory);

			// Let any tracker that needs the host settings (for option persistence) pick them up
			TrackerRegistry.NotifySettingsReady(settings);

			// Apply loaded settings to renderer
			ApplySettings();

			// Create context menu
			contextMenu = new PatternContextMenu(
				renderer,
				settings,
				patternPanel,
				settings.TrackerStyleId,
				UpdateStyle);

			patternPanel.ContextMenuStrip = contextMenu.Menu;

			contextMenu.CompressPatternsChanged += OnCompressPatternsChanged;
			contextMenu.ShowTransposedNotesChanged += OnShowTransposedNotesChanged;

#if DEBUG
			// Paint counter timer - updates title every second (diagnostics)
			paintCounterTimer = new Timer(components);
			paintCounterTimer.Interval = 1000;
			paintCounterTimer.Tick += PaintCounterTimer_Tick;
			paintCounterTimer.Start();
#endif

			// Update style based on loaded tracker style
			UpdateStyle();

			// Enable keyboard input
			PreviewKeyDown += PatternControl_PreviewKeyDown;

			// Update form title when control is added to a form
			ParentChanged += (s, e) => UpdateFormTitle();

			// Invalidate cache on resize
			patternPanel.SizeChanged += (s, e) => renderer.InvalidateCache();

			// Let companions (e.g. Debugger) attach to this instance
			Created?.Invoke(this);
		}

		#region IWantClientPlayerControl
		/********************************************************************/
		/// <summary>
		/// Called by the host to provide the client player control instance.
		/// </summary>
		/********************************************************************/
		public void SetClientPlayerControl(IClientPlayerControl clientPlayerControl)
		{
			clientPlayer = clientPlayerControl;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Apply loaded settings to renderer
		/// </summary>
		/********************************************************************/
		private void ApplySettings()
		{
			renderer.CurrentDisplayMode = settings.DisplayMode;
			renderer.RowNumberDisplayMode = settings.RowNumberFormat;
			renderer.InstrumentDisplayMode = settings.InstrumentFormat;
			renderer.VolumeDisplayMode = settings.VolumeFormat;
			renderer.TrackPatternDisplayMode = settings.TrackPatternNumberFormat;
			renderer.GridLinesDisplayMode = settings.GridLinesMode;
			renderer.VolumeBarState.DisplaySettings.Mode = settings.VolumeBarMode;

			// Fall back to defaults if stored IDs no longer exist in the registries
			if (TrackerRegistry.Get(settings.TrackerStyleId) == null)
			{
				settings.TrackerStyleId = TrackerRegistry.DefaultStyleId;
			}

			if (VuMeterRegistry.GetById(settings.VuMeterId) == null)
			{
				settings.MatchTrackerVuMeter = true;
			}

			if (settings.MatchTrackerVuMeter)
			{
				TrackerStyleRegistration trackerReg = TrackerRegistry.Get(settings.TrackerStyleId);
				renderer.VolumeBarState.DisplaySettings.VuMeterId = trackerReg?.VuMeterId;
			}
			else
			{
				renderer.VolumeBarState.DisplaySettings.VuMeterId = settings.VuMeterId;
			}

			renderer.RollingPatterns = settings.RollingPatterns;
			renderer.StripedChannels = settings.StripedChannels;
			renderer.HideEmpty = settings.HideEmpty;
			renderer.ShowTransposedNotes = settings.ShowTransposedNotes;
			renderer.CurrentColorMode = settings.ColorMode;
			renderer.ShowControlBar = settings.PlayerControlBar;
		}

		#region Event Handlers
		/********************************************************************/
		/// <summary>
		/// Animation timer tick - called 50 times per second
		/// </summary>
		/********************************************************************/
		private void AnimationTimer_Tick(object sender, EventArgs e)
		{
			renderer.AnimateVolumes();

			if (renderer.IsPaused)
			{
				return;
			}

			VolumeBarMode mode = renderer.VolumeBarState.DisplaySettings.Mode;

			// A dummy pattern (format without patterns) gets no row-change
			// updates, so drive both the clock and the VU animation from here.
			// The control bar and VU meters are baked into the cached bitmap,
			// so the cache must be rebuilt for either to refresh
			if (processedPatterns?.HasPatterns == false)
			{
				UpdateTimeInfo();

				int second = (int)renderer.ElapsedTime.TotalSeconds;
				bool vuActive = mode != VolumeBarMode.Off;

				// Redraw every tick while the VU meters animate, otherwise only
				// when the displayed second changes
				if (vuActive || second != lastDummySecond)
				{
					lastDummySecond = second;
					renderer.InvalidateCache();
					patternPanel?.Invalidate();
				}
			}
			else if (mode == VolumeBarMode.RealVolume)
			{
				patternPanel?.Invalidate();
			}
		}

		// Font scale values for cycling through with Ctrl+/-
		private static readonly FontScale[] fontScaleValues = {FontScale.Scale100, FontScale.Scale125, FontScale.Scale150, FontScale.Scale175, FontScale.Scale200, FontScale.Scale300, FontScale.Scale400};

		/********************************************************************/
		/// <summary>
		/// Handle keyboard navigation
		/// </summary>
		/********************************************************************/
		private void PatternControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
			    e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
			    e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown ||
			    e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
			{
				e.IsInputKey = true;
			}

			// Handle Ctrl+Shift+/- for tracker style navigation
			if (e.Control && e.Shift)
			{
				if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
				{
					contextMenu.NavigateToNextStyle();
					return;
				}

				if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
				{
					contextMenu.NavigateToPreviousStyle();
					return;
				}
			}

			// Handle Ctrl+/- for font scaling
			if (e.Control && !e.Shift)
			{
				if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
				{
					ChangeScale(+1);
					return;
				}

				if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
				{
					ChangeScale(-1);
					return;
				}
			}

			if (PatternKeyboardHandler.HandleKeyDown(renderer, e.KeyCode, e.Control))
			{
				renderer.InvalidateCache();
				patternPanel.Invalidate();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Change font scale by direction (+1 = increase, -1 = decrease)
		/// </summary>
		/********************************************************************/
		private void ChangeScale(int direction)
		{
			// Find current index
			FontScale currentScale = BitmapFontRenderer.CurrentFontScale;
			int currentIndex = Array.IndexOf(fontScaleValues, currentScale);
			if (currentIndex < 0)
			{
				currentIndex = 0;
			}

			// Calculate new index
			int newIndex = currentIndex + direction;
			if (newIndex < 0 || newIndex >= fontScaleValues.Length)
			{
				return; // At boundary, don't change
			}

			// Set new scale (BitmapFontRenderer's setter notifies tracker-specific renderers via event)
			FontScale newScale = fontScaleValues[newIndex];
			settings.FontScale = newScale;
			BitmapFontRenderer.CurrentFontScale = newScale;

			// Re-initialize renderer with new scale
			renderer.UpdateStyle(renderer.CurrentStyleId);
			renderer.InvalidateCache();
			patternPanel.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Handle mouse clicks on scroll buttons and channel headers
		/// </summary>
		/********************************************************************/
		private void PatternPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			if (PatternMouseHandler.HandleMouseClick(renderer, e.Location, clientPlayer))
			{
				renderer.InvalidateCache();
				patternPanel?.Invalidate();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Handle mouse down for button press feedback
		/// </summary>
		/********************************************************************/
		private void PatternPanel_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			if (PatternMouseHandler.HandleMouseDown(renderer, e.Location))
			{
				renderer.InvalidateCache();
				patternPanel?.Invalidate();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Handle mouse up to release button press
		/// </summary>
		/********************************************************************/
		private void PatternPanel_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			if (PatternMouseHandler.HandleMouseUp(renderer))
			{
				renderer.InvalidateCache();
				patternPanel?.Invalidate();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Handle mouse movement for scroll button hover effects
		/// </summary>
		/********************************************************************/
		private void PatternPanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (PatternMouseHandler.HandleMouseMove(renderer, e.Location, out Cursor newCursor))
			{
				renderer.InvalidateCache();
				patternPanel?.Invalidate();
			}

			patternPanel.Cursor = newCursor;
		}

		/********************************************************************/
		/// <summary>
		/// Paint the pattern panel
		/// </summary>
		/********************************************************************/
		private void PatternPanel_Paint(object sender, PaintEventArgs e)
		{
			bool rendered = PatternDrawingHelper.PaintPatternPanel(e.Graphics, e.Graphics.ClipBounds, renderer);

#if DEBUG
			paintCount++;
			if (rendered)
			{
				patternRenderCount++;
			}
#endif
		}

#if DEBUG
		/********************************************************************/
		/// <summary>
		/// Update form title with paint/render counters per second
		/// </summary>
		/********************************************************************/
		private void PaintCounterTimer_Tick(object sender, EventArgs e)
		{
			int paintsPerSecond = paintCount - lastPaintCount;
			lastPaintCount = paintCount;

			int rendersPerSecond = patternRenderCount - lastPatternRenderCount;
			lastPatternRenderCount = patternRenderCount;

			// Calculate average render time
			string avgTimeStr = "";
			if (rendersPerSecond > 0)
			{
				double avgMs = (double)renderer.TotalRenderTimeTicks / rendersPerSecond / Stopwatch.Frequency * 1000.0;
				avgTimeStr = $", avg {avgMs:F2}ms";
			}

			renderer.ResetRenderStats();

			var form = FindForm();
			if (form != null)
			{
				var registration = TrackerRegistry.Get(contextMenu.CurrentStyleId);
				string styleName = registration?.DisplayName ?? contextMenu.CurrentStyleId;
				form.Text = $"Pattern Viewer - {styleName} [{paintsPerSecond} paints/s, {rendersPerSecond} renders/s{avgTimeStr}]";
			}
		}
#endif
		#endregion

		#region Public Methods
		/********************************************************************/
		/// <summary>
		/// Called when channel information has changed
		/// </summary>
		/********************************************************************/
		public void ChannelChange(ChannelChanged[] channels)
		{
			PatternChannelHelper.ProcessChannelChange(renderer, channels);
		}

		/********************************************************************/
		/// <summary>
		/// Called when mixer channel enabled states have changed
		/// </summary>
		/********************************************************************/
		public void UpdateMixerChannelStatus(bool[] channelsEnabled)
		{
			if (channelsEnabled == null)
			{
				return;
			}

			// Copy the mixer channel states to the renderer
			int count = Math.Min(channelsEnabled.Length, renderer.MixerChannelsEnabled.Length);
			for (int i = 0; i < count; i++)
			{
				renderer.MixerChannelsEnabled[i] = channelsEnabled[i];
			}

			renderer.InvalidateCache();
			patternPanel.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Cleanup the visualization
		/// </summary>
		/********************************************************************/
		public void CleanupVisualization()
		{
			animationTimer.Stop();
			renderer.ClearSongData();
			patternPanel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Initialize the visualization
		/// </summary>
		/********************************************************************/
		public void InitVisualization(int channels)
		{
			initChannelCount = channels;
			PatternChannelHelper.InitializeVisualization(renderer, channels);
			animationTimer.Start();
			Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Called when playback moves to a new row
		/// </summary>
		/********************************************************************/
		public void PatternRowChanged(SongRowChangeInfo rowInfo)
		{
			if (rowInfo == null)
			{
				return;
			}

			// Invalidate bitmap cache - pattern data changed
			renderer.InvalidateCache();

			PatternChannelHelper.ProcessRowChange(renderer, rowInfo);

			// Update time information from main window API
			UpdateTimeInfo();

			RowChanged?.Invoke(rowInfo, renderer.CurrentSongPattern?.RowCount ?? 0);

			UpdateStatusBar();
			patternPanel.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Set pause state - enables/disables manual scrolling
		/// </summary>
		/********************************************************************/
		public void SetPauseState(bool paused)
		{
			renderer.IsPaused = paused;

			if (paused)
			{
				renderer.ManualRow = renderer.CurrentRow;
				renderer.ManualSongPosition = renderer.CurrentSongPosition;
			}

			patternPanel.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Reset the viewer for a new module
		/// </summary>
		/********************************************************************/
		public void SongModuleLoaded(SongModule data)
		{
#if DEBUG
			// Reset paint counter on song start
			paintCount = 0;
			lastPaintCount = 0;
			patternRenderCount = 0;
			lastPatternRenderCount = 0;
#endif

			renderer.ImportSongModule(data);
			currentSongModule = data;

			SongDataReset?.Invoke();

			// Build processed patterns (with optional compression)
			BuildProcessedPatterns();

			if (processedPatterns != null)
			{
				renderer.ImportSongPatterns(processedPatterns);
				UpdateStatusBar();
			}

			patternPanel.Invalidate();
		}
		#endregion

		#region Private Methods
		/********************************************************************/
		/// <summary>
		/// Update parent form title with current tracker style
		/// </summary>
		/********************************************************************/
		private void UpdateFormTitle()
		{
			Form form = FindForm();
			if (form != null)
			{
				TrackerStyleRegistration registration = TrackerRegistry.Get(contextMenu.CurrentStyleId);
				string styleName = registration?.DisplayName ?? contextMenu.CurrentStyleId;
				form.Text = $"Pattern Viewer - {styleName}";
			}
		}

		/********************************************************************/
		/// <summary>
		/// Update the status bar display
		/// </summary>
		/********************************************************************/
		private void UpdateStatusBar()
		{
			patternPanel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Update time information from main window API
		/// </summary>
		/********************************************************************/
		private void UpdateTimeInfo()
		{
			if (clientPlayer == null)
			{
				return;
			}

			try
			{
				renderer.ElapsedTime = clientPlayer.ElapsedTime;
				renderer.TotalTime = clientPlayer.TotalTime;
				renderer.SnapshotPosition = clientPlayer.SnapshotPosition;
				renderer.SnapshotCount = clientPlayer.SnapshotCount;
				renderer.ModuleIndex = clientPlayer.ModuleIndex;
				renderer.ModuleCount = clientPlayer.ModuleCount;

				if (clientPlayer.SubSongCount > 0)
				{
					renderer.SubSongCurrent = clientPlayer.SubSongCurrent;
					renderer.SubSongTotal = clientPlayer.SubSongCount;
				}
			}
			catch
			{
				// Ignore errors if API is not available
			}
		}

		/********************************************************************/
		/// <summary>
		/// Update style based on current tracker style
		/// </summary>
		/********************************************************************/
		private void UpdateStyle()
		{
			// Update font scale before initializing renderers (tracker-specific renderers are notified via event)
			BitmapFontRenderer.CurrentFontScale = settings.FontScale;

			// Use registry-based style update
			TrackerStyleColors colors = renderer.UpdateStyle(contextMenu.CurrentStyleId);

			patternPanel.BackColor = colors.PatternPanelBackColor;
			BackColor = colors.ControlBackColor;
			UpdateFormTitle();
		}

		/********************************************************************/
		/// <summary>
		/// Get the source patterns based on pattern source mode setting
		/// </summary>
		/********************************************************************/
		private SongPatterns GetSourcePatterns(SongModule data)
		{
			if (data == null)
			{
				return null;
			}

			// Formats without patterns fall back to an empty dummy so the normal
			// pattern view (channel columns + VU meters) still renders - only the
			// note cells stay empty because there are no notes to show
			return data.Patterns ?? BuildDummyPatterns();
		}

		/********************************************************************/
		/// <summary>
		/// Build an empty dummy pattern for formats that do not provide any
		/// patterns. Uses the channel count reported by the player so the whole
		/// view renders exactly like a real module, just without notes
		/// </summary>
		/********************************************************************/
		private SongPatterns BuildDummyPatterns()
		{
			int channels = initChannelCount;
			if (channels <= 0)
			{
				return null;
			}

			// One empty row - keeps the channel count (needed for the columns and
			// VU meters) and gives a single note line, but no actual notes. Using
			// one row instead of zero keeps MaxRowCount at 1 (a zero row count
			// falls back to 64 in CalculatePatternMetrics)
			SongPatternViewChannel[] channelEntries = new SongPatternViewChannel[channels];

			for (int c = 0; c < channels; c++)
			{
				channelEntries[c] = new SongPatternViewChannel
				{
					Note = SongPatternViewNote.None,
					Octave = null,
					Instrument = null,
					Volume = null,
					EffectText = Array.Empty<string>()
				};
			}

			SongPatternViewData patternData = new() {PatternNumber = null, RowCount = 1, ChannelCount = channels, Rows = new[] {new SongPatternViewRow {RowNumber = 0, Channels = channelEntries}}};

			int subSongTotal = clientPlayer?.SubSongCount ?? 0;
			int subSongCurrent = clientPlayer?.SubSongCurrent ?? 0;

			return new SongPatterns
			{
				SongLength = 1,
				ModuleFormat = renderer.SongFormat ?? string.Empty,
				ModuleTitle = renderer.SongTitle ?? string.Empty,
				InitialSpeed = 0,
				InitialBpm = null,
				HasVolumeColumn = false,
				TransposeMode = NoteTransposeMode.NoTranspose,
				HasTrackNumber = false,
				EffectCharCount = 0,
				HasPatterns = false,
				SubSongCurrent = subSongCurrent > 0 ? subSongCurrent : 1,
				SubSongTotal = subSongTotal > 0 ? subSongTotal : 1,
				SongData = new List<SongPatternViewData> {patternData}
			};
		}

		/********************************************************************/
		/// <summary>
		/// Build processed patterns from source (with optional compression)
		/// </summary>
		/********************************************************************/
		private void BuildProcessedPatterns()
		{
			SongPatterns sourcePatterns = GetSourcePatterns(currentSongModule);
			if (sourcePatterns == null)
			{
				processedPatterns = null;
				return;
			}

			// Check if compression should be applied based on mode
			bool shouldCompress = settings.CompressPatterns.Resolve(sourcePatterns.AutoCompress);

			if (shouldCompress)
			{
				processedPatterns = CompressPatterns(sourcePatterns);
			}
			else
			{
				processedPatterns = CopyPatterns(sourcePatterns);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Create a copy of patterns (no compression)
		/// </summary>
		/********************************************************************/
		private SongPatterns CopyPatterns(SongPatterns source)
		{
			return new SongPatterns
			{
				SongLength = source.SongLength,
				StartPosition = source.StartPosition,
				ModuleFormat = source.ModuleFormat,
				ModuleTitle = source.ModuleTitle,
				InitialSpeed = source.InitialSpeed,
				InitialBpm = source.InitialBpm,
				HasVolumeColumn = source.HasVolumeColumn,
				TransposeMode = source.TransposeMode,
				HasTrackNumber = source.HasTrackNumber,
				EffectCharCount = source.EffectCharCount,
				ErrorMessage = source.ErrorMessage,
				SubSongCurrent = source.SubSongCurrent,
				SubSongTotal = source.SubSongTotal,
				AutoCompress = source.AutoCompress,
				HasPatterns = source.HasPatterns,
				SongData = source.SongData
			};
		}

		/********************************************************************/
		/// <summary>
		/// Compress patterns by removing empty rows
		/// </summary>
		/********************************************************************/
		private SongPatterns CompressPatterns(SongPatterns source)
		{
			// Create a copy with compressed song data
			SongPatterns compressed = new()
			{
				SongLength = source.SongLength,
				StartPosition = source.StartPosition,
				ModuleFormat = source.ModuleFormat,
				ModuleTitle = source.ModuleTitle,
				InitialSpeed = source.InitialSpeed,
				InitialBpm = source.InitialBpm,
				HasVolumeColumn = source.HasVolumeColumn,
				TransposeMode = source.TransposeMode,
				HasTrackNumber = source.HasTrackNumber,
				EffectCharCount = source.EffectCharCount,
				ErrorMessage = source.ErrorMessage,
				SubSongCurrent = source.SubSongCurrent,
				SubSongTotal = source.SubSongTotal,
				AutoCompress = source.AutoCompress,
				HasPatterns = source.HasPatterns,
				SongData = new List<SongPatternViewData>()
			};

			// Compress each pattern
			foreach (SongPatternViewData pattern in source.SongData)
			{
				SongPatternViewData compressedPattern = TryCompressPattern(pattern);
				compressed.SongData.Add(compressedPattern);
			}

			return compressed;
		}

		/********************************************************************/
		/// <summary>
		/// Try to compress a single pattern by removing empty odd rows
		/// </summary>
		/********************************************************************/
		private SongPatternViewData TryCompressPattern(SongPatternViewData pattern)
		{
			if (pattern.Rows == null || pattern.Rows.Length < 2)
			{
				return pattern;
			}

			int currentSkip = pattern.Skip;
			SongPatternViewRow[] currentRows = pattern.Rows;

			// Keep compressing while possible
			while (currentRows.Length >= 2 && CanCompressRows(currentRows))
			{
				// Take only even rows (0, 2, 4, ...)
				List<SongPatternViewRow> newRows = new();
				for (int i = 0; i < currentRows.Length; i += 2)
				{
					newRows.Add(currentRows[i]);
				}

				currentRows = newRows.ToArray();
				currentSkip *= 2;
			}

			// Return compressed pattern
			return new SongPatternViewData
			{
				PatternNumber = pattern.PatternNumber,
				RowCount = currentRows.Length,
				ChannelCount = pattern.ChannelCount,
				Rows = currentRows,
				Skip = currentSkip
			};
		}

		/********************************************************************/
		/// <summary>
		/// Check if all odd rows are empty (can be compressed)
		/// </summary>
		/********************************************************************/
		private bool CanCompressRows(SongPatternViewRow[] rows)
		{
			// Check all odd rows (1, 3, 5, ...)
			for (int i = 1; i < rows.Length; i += 2)
			{
				if (!IsRowEmpty(rows[i]))
				{
					return false;
				}
			}

			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Check if a row is empty (no notes, no effects)
		/// </summary>
		/********************************************************************/
		private bool IsRowEmpty(SongPatternViewRow row)
		{
			if (row?.Channels == null)
			{
				return true;
			}

			foreach (SongPatternViewChannel channel in row.Channels)
			{
				if (channel == null)
				{
					continue;
				}

				// Check if channel has any data
				if (channel.Note != SongPatternViewNote.None)
				{
					return false;
				}

				if (channel.Instrument.HasValue)
				{
					return false;
				}

				if (channel.Volume.HasValue)
				{
					return false;
				}

				if (channel.EffectText != null && channel.EffectText.Length > 0)
				{
					foreach (string effect in channel.EffectText)
					{
						if (!string.IsNullOrEmpty(effect) && effect.Trim().Length > 0)
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Handle compress patterns mode change
		/// </summary>
		/********************************************************************/
		private void OnCompressPatternsChanged()
		{
			if (currentSongModule == null)
			{
				return;
			}

			BuildProcessedPatterns();

			if (processedPatterns != null)
			{
				renderer.ImportSongPatterns(processedPatterns);
				UpdateStatusBar();
			}
			else
			{
				renderer.ClearSongData();
			}

			patternPanel.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Handle show transposed notes change
		/// </summary>
		/********************************************************************/
		private void OnShowTransposedNotesChanged()
		{
			renderer.ShowTransposedNotes = settings.ShowTransposedNotes;
			renderer.InvalidateCache();
			patternPanel.Invalidate();
		}


		/********************************************************************/
		/// <summary>
		/// Jump to a specific row when paused. Used by the debugger play-log to seek.
		/// </summary>
		/********************************************************************/
		public void JumpToRow(SongRowChangeInfo rowInfo)
		{
			if (!renderer.IsPaused || rowInfo == null)
			{
				return;
			}

			renderer.ManualRow = rowInfo.Row;
			renderer.ManualSongPosition = rowInfo.SongPosition - renderer.StartPosition;
			renderer.CurrentRow = rowInfo.Row;
			renderer.CurrentSongPosition = rowInfo.SongPosition - renderer.StartPosition;
			renderer.Speed = rowInfo.Speed;
			renderer.Bpm = rowInfo.Bpm;

			if (renderer.SongData != null && renderer.CurrentSongPosition >= 0 &&
			    renderer.CurrentSongPosition < renderer.SongData.Count)
			{
				renderer.CurrentSongPattern = renderer.SongData[renderer.CurrentSongPosition];
			}

			patternPanel?.Invalidate();
		}

		/// <summary>
		/// Currently-displayed song pattern. Exposed for companions that need pattern context.
		/// </summary>
		public SongPatternViewData CurrentSongPattern => renderer.CurrentSongPattern;

		/// <summary>
		/// Whether playback is paused (used by debugger seek logic).
		/// </summary>
		public bool IsPaused => renderer.IsPaused;
		#endregion
	}
}
