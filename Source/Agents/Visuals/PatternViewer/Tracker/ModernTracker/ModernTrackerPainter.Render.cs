//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerPainter.Render.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Modern tracker painter - Main rendering orchestration
	/// </summary>
	internal partial class ModernTrackerPainter
	{
		/********************************************************************/
		/// <summary>
		/// Render the complete viewer with rect-based layout
		/// </summary>
		/********************************************************************/
		internal override RenderViewerResult? RenderViewer(Graphics g, Rectangle panelRect)
		{
			// Fill background
			using (SolidBrush bgBrush = new(bgColor))
			{
				g.FillRectangle(bgBrush, panelRect);
			}

			// Calculate metrics
			RenderMetrics metrics = CreateRenderMetrics(g);

			int topOffset = panelRect.Y;

			// Use manual position when scrolling is allowed
			DisplayPositionInfo displayPos = VisibleAreaCalculator.GetDisplayPosition(
				AllowPatternScrolling, ManualRow, CurrentRow, ManualSongPosition, CurrentSongPosition,
				SongData, CurrentSongPattern);

			// Draw header bar first (at the top)
			Rectangle headerBarRect = new(panelRect.X, topOffset, panelRect.Width, metrics.StatusHeight);
			DrawHeaderBarSection(g, headerBarRect, displayPos.DisplaySongPosition, displayPos.DisplayRow, displayPos.DisplaySongPattern, metrics);

			topOffset += metrics.StatusHeight;

			// Draw control bar if enabled (between status bar and channel headers)
			if (ShowControlBar)
			{
				float scaleFactor = (int)BitmapFontRenderer.CurrentFontScale / 100f;
				int buttonSize = metrics.StatusLineHeight * 2;
				int controlBarHeight = ModernTrackerControlBarRenderer.GetHeight(scaleFactor, buttonSize);

				// 1px gap between status bar and control bar
				int gap = 1;

				// Draw background for gap + control bar area
				g.FillRectangle(statusBgBrush, panelRect.X, topOffset, panelRect.Width, gap + controlBarHeight);
				topOffset += gap;

				Rectangle controlBarRect = new(panelRect.X, topOffset, panelRect.Width, controlBarHeight);
				DrawControlBar(g, controlBarRect, metrics);

				topOffset += controlBarHeight;
			}

			// Calculate visible channels
			int visibleChannels = VisibleAreaCalculator.CalculateVisibleChannels(
				panelRect.Width, metrics.RowNumberWidth, metrics.ChannelWidth, FirstVisibleChannel, ChannelCount);

			// Draw channel headers
			Rectangle channelHeadersRect = new(panelRect.X, topOffset, panelRect.Width, metrics.HeaderHeight);
			DrawChannelHeadersSection(g, channelHeadersRect, displayPos.DisplaySongPattern, visibleChannels, metrics);

			// Calculate visible rows (add 1 to allow partial row at bottom for better appearance)
			int patternTop = topOffset + metrics.HeaderHeight;
			int patternHeight = panelRect.Height - patternTop;
			VisibleRowsInfo rowsInfo = VisibleAreaCalculator.CalculateVisibleRows(
				patternHeight, 0, 0, metrics.RowHeight, displayPos.DisplayRow);
			scrollPosition = rowsInfo.ScrollPosition;

			// Draw current row highlight
			int highlightY = patternTop + (rowsInfo.CenterRow * metrics.RowHeight);
			DrawCurrentRowHighlight(g, panelRect, highlightY, visibleChannels, metrics);

			// Draw all visible rows
			Rectangle patternRowsRect = new(panelRect.X, patternTop, panelRect.Width, patternHeight);
			PatternRowsResult patternRowsResult = RenderPatternRows(g, patternRowsRect, displayPos, visibleChannels, rowsInfo, metrics);

			// Draw VU meters on top of pattern rows
			if (!IsPaused)
			{
				VuRenderInput vuInput = VuMeterRenderingHelper.BuildFromPatternRowsResult(patternRowsResult);

				VuRenderDebugOptions debugOptions = new() {CurrentDisplayMode = CurrentDisplayMode, ShowDebugInfo = ShowDebugInfo, CurrentPattern = CurrentSongPattern, CurrentRow = CurrentRow};

				VuMeterRenderingHelper.RenderVolumeBars(g, patternRowsRect, vuInput, VolumeBarState, debugOptions);
			}

			return new RenderViewerResult {PatternRows = patternRowsResult};
		}
	}
}
