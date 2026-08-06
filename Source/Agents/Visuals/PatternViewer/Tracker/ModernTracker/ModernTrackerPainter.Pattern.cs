//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerPainter.Pattern.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using System.Drawing.Drawing2D;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Modern tracker painter - Pattern rows drawing
	/// </summary>
	internal partial class ModernTrackerPainter
	{
		/********************************************************************/
		/// <summary>
		/// Draw current row highlight (simple style for modern themes)
		/// </summary>
		/********************************************************************/
		private void DrawCurrentRowHighlight(Graphics g, Rectangle rect, int highlightY, int visibleChannels, RenderMetrics metrics)
		{
			// Draw row number highlight
			g.FillRectangle(currentRowBrush, rect.X, highlightY, metrics.RowNumberWidth, metrics.RowHeight);

			// Draw channel highlights
			for (int chIdx = 0; chIdx < visibleChannels; chIdx++)
			{
				int channelX = rect.X + metrics.RowNumberWidth + (chIdx * metrics.ChannelWidth);
				g.FillRectangle(currentRowBrush, channelX + ChannelSeparatorWidth, highlightY,
					metrics.ChannelWidth - ChannelSeparatorWidth, metrics.RowHeight);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Draw the pattern rows
		/// </summary>
		/********************************************************************/
		protected override PatternRowsResult? RenderPatternRows(Graphics g, Rectangle rect)
		{
			// Build metrics
			RenderMetrics metrics = CreateRenderMetrics(g);

			// Use manual position when scrolling is allowed
			DisplayPositionInfo displayPos = VisibleAreaCalculator.GetDisplayPosition(
				AllowPatternScrolling, ManualRow, CurrentRow, ManualSongPosition, CurrentSongPosition,
				SongData, CurrentSongPattern);

			// Calculate visible channels
			int visibleChannels = VisibleAreaCalculator.CalculateVisibleChannels(
				rect.Width, metrics.RowNumberWidth, metrics.ChannelWidth, FirstVisibleChannel, ChannelCount);

			// Calculate visible rows
			VisibleRowsInfo rowsInfo = VisibleAreaCalculator.CalculateVisibleRows(
				rect.Height, 0, 0, metrics.RowHeight, displayPos.DisplayRow);

			return RenderPatternRows(g, rect, displayPos, visibleChannels, rowsInfo, metrics);
		}

		/********************************************************************/
		/// <summary>
		/// Draw all pattern rows (with clipping)
		/// </summary>
		/********************************************************************/
		private PatternRowsResult RenderPatternRows(Graphics g, Rectangle rect,
			DisplayPositionInfo displayPos,
			int visibleChannels, VisibleRowsInfo rowsInfo, RenderMetrics metrics)
		{
			PatternRenderResult patternResult;

			GraphicsState state = g.Save();
			try
			{
				g.SetClip(rect);

				// Draw all visible rows
				patternResult = DrawPatternRows(g, rect, displayPos.DisplaySongPosition, displayPos.DisplaySongPattern,
					visibleChannels, rowsInfo.VisibleRows, rowsInfo.CenterRow, metrics);
			}
			finally
			{
				g.Restore(state);
			}

			// Build channel X positions
			int[] channelXPositions = new int[visibleChannels];
			for (int i = 0; i < visibleChannels; i++)
			{
				channelXPositions[i] = rect.X + metrics.RowNumberWidth + (i * metrics.ChannelWidth);
			}

			return new PatternRowsResult {ChannelXPositions = channelXPositions, ChannelWidth = metrics.ChannelWidth, VuBarTop = rect.Y + metrics.RowHeight, VuBarBottom = patternResult.VuBarBottom};
		}

		/********************************************************************/
		/// <summary>
		/// Draw all pattern rows
		/// </summary>
		/********************************************************************/
		private PatternRenderResult DrawPatternRows(Graphics g, Rectangle rect, int displaySongPosition,
			SongPatternViewData displaySongPattern,
			int visibleChannels, int visibleRows, int centerRow, RenderMetrics metrics)
		{
			for (int i = 0; i < visibleRows; i++)
			{
				int row = scrollPosition + i;
				int y = rect.Y + (i * metrics.RowHeight);

				// Draw row background (if needed, but not on highlighted row)
				if (rowBgBrush != null && i != centerRow)
				{
					g.FillRectangle(rowBgBrush, rect.X, y, metrics.RowNumberWidth + (visibleChannels * metrics.ChannelWidth), metrics.RowHeight);
				}

				// Get actual pattern and row for rolling patterns
				RollingPatternInfo rollingInfo = PatternRenderingHelpers.GetRollingPatternInfo(displaySongPosition, row,
					displaySongPattern, SongData, RollingPatterns);
				SongPatternViewData actualPattern = rollingInfo.SongPattern;
				int actualRow = rollingInfo.Row;

				// Draw pattern boundaries
				PatternBoundaryRenderer.Draw(g, rect, new PatternBoundaryInput
				{
					Y = y,
					Row = row,
					DisplaySongPosition = displaySongPosition,
					DisplaySongPattern = displaySongPattern,
					ActualPattern = actualPattern,
					ActualRow = actualRow,
					RowHeight = metrics.RowHeight,
					RollingPatterns = RollingPatterns,
					SongData = SongData,
					PatternBoundaryPen = patternBoundaryPen
				});

				// Draw striped channel backgrounds
				if (StripedChannels)
				{
					for (int chIdx = 0; chIdx < visibleChannels; chIdx++)
					{
						if (chIdx % 2 == 1)
						{
							int channelX = rect.X + metrics.RowNumberWidth + (chIdx * metrics.ChannelWidth);
							g.FillRectangle(channelStripeBgBrush, channelX + ChannelSeparatorWidth, y,
								metrics.ChannelWidth - ChannelSeparatorWidth, metrics.RowHeight);
						}
					}
				}

				// Draw horizontal grid line (before channel separators so they paint over it)
				if (metrics.DrawGrid)
				{
					g.DrawLine(gridPen, rect.X, y + metrics.RowHeight, rect.X + metrics.RowNumberWidth + (visibleChannels * metrics.ChannelWidth), y + metrics.RowHeight);
				}

				// Draw channel separators
				ChannelSeparatorRenderer.DrawSimple(g, rect.X, y, metrics.RowHeight, visibleChannels, metrics.RowNumberWidth,
					metrics.ChannelWidth, ChannelSeparatorWidth, DrawSeparatorAfterLastChannel, channelSeparatorPen);

				// Draw pattern data if valid row
				bool isValidRow = actualPattern != null && actualRow >= 0 && actualRow < actualPattern.RowCount;
				if (isValidRow)
				{
					DrawPatternRow(g, actualPattern, actualRow, y, rect.X, metrics, visibleChannels, i == centerRow);
				}
			}

			return new PatternRenderResult {VuBarBottom = rect.Y + (centerRow * metrics.RowHeight)};
		}

		/********************************************************************/
		/// <summary>
		/// Draw a complete pattern row
		/// </summary>
		/********************************************************************/
		private void DrawPatternRow(Graphics g, SongPatternViewData actualSongPattern, int actualRow, int y, int xOffset,
			RenderMetrics metrics, int visibleChannels, bool isCurrentRow)
		{
			using (StringFormat sf = new(StringFormat.GenericTypographic))
			{
				// Draw row number
				int displayRow = PatternDataHelper.GetDisplayRowNumber(actualSongPattern, actualRow);
				string rowNumStr =
					PatternRenderingHelpers.FormatRowNumber(displayRow, metrics.RowNumberFormat, MaxRowCount);
				g.DrawString(rowNumStr, metrics.PatternFont, textBrush, xOffset + metrics.TextPaddingWidth,
					y + metrics.TextPaddingHeight, sf);

				// Draw pattern data for all visible channels
				for (int chIdx = 0; chIdx < visibleChannels; chIdx++)
				{
					int ch = FirstVisibleChannel + chIdx;
					if (ch >= ChannelCount)
					{
						break;
					}

					int channelX = xOffset + metrics.RowNumberWidth + (chIdx * metrics.ChannelWidth);

					// Check if channel exists in the actual pattern
					if (actualSongPattern != null && ch >= actualSongPattern.ChannelCount)
						// Channel doesn't exist in this pattern - don't draw anything
					{
						continue;
					}

					// Get channel with bounds checking
					SongPatternViewChannel channel = PatternDataHelper.GetChannel(actualSongPattern, actualRow, ch);

					DrawPatternChannel(g, channel, channelX, y, metrics, textBrush, sf);
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Draw pattern data for a single channel
		/// </summary>
		/********************************************************************/
		private void DrawPatternChannel(Graphics g, SongPatternViewChannel channel, int channelX, int y,
			RenderMetrics metrics, Brush textColor, StringFormat sf)
		{
			int textY = y + metrics.TextPaddingHeight;

			// Select brush based on color mode
			Brush noteBrush = CurrentColorMode == ColorMode.Colored ? noteColorBrush : textColor;
			Brush instrumentBrush = CurrentColorMode == ColorMode.Colored ? instrumentColorBrush : textColor;
			Brush volumeBrush = CurrentColorMode == ColorMode.Colored ? volumeColorBrush : textColor;
			Brush effectBrush = CurrentColorMode == ColorMode.Colored ? effectColorBrush : textColor;

			// Draw note
			if (metrics.NotePos.Text.HasValue)
			{
				string note = HideEmpty
					? PatternRenderingHelpers.FormatNoteDisplay(channel?.Note ?? SongPatternViewNote.None,
						channel?.Octave)
					: channel == null || channel.Note == SongPatternViewNote.None || !channel.Octave.HasValue
						? "---"
						: PatternRenderingHelpers.FormatNoteDisplay(channel.Note, channel.Octave);

				g.DrawString(note, metrics.PatternFont, noteBrush, channelX + metrics.NotePos.Text.Value, textY, sf);
			}

			if (metrics.NotePos.Separator.HasValue && metrics.DrawGrid)
			{
				int sepX = channelX + metrics.NotePos.Separator.Value;
				g.DrawLine(columnSeparatorPen, sepX, y, sepX, y + metrics.RowHeight);
			}

			// Draw instrument
			if (metrics.InstrumentPos.Text.HasValue)
			{
				string format = metrics.InstrumentFormat == NumberFormat.Hexadecimal ? "X2" : "D2";
				string instrument = HideEmpty
					? channel?.Instrument.HasValue ?? false ? channel.Instrument.Value.ToString(format) : "  "
					: channel == null || !channel.Instrument.HasValue
						? metrics.InstrumentFormat == NumberFormat.Hexadecimal ? "00" : "00"
						: channel.Instrument.Value.ToString(format);

				g.DrawString(instrument, metrics.PatternFont, instrumentBrush,
					channelX + metrics.InstrumentPos.Text.Value,
					textY, sf);
			}

			if (metrics.InstrumentPos.Separator.HasValue && metrics.DrawGrid)
			{
				int sepX = channelX + metrics.InstrumentPos.Separator.Value;
				g.DrawLine(columnSeparatorPen, sepX, y, sepX, y + metrics.RowHeight);
			}

			// Draw volume
			if (metrics.VolumePos.Text.HasValue)
			{
				string format = metrics.VolumeFormat == NumberFormat.Hexadecimal ? "X2" : "D2";
				string volume = HideEmpty
					? channel?.Volume.HasValue ?? false ? channel.Volume.Value.ToString(format) : "  "
					: channel == null || !channel.Volume.HasValue
						? metrics.VolumeFormat == NumberFormat.Hexadecimal ? "00" : "00"
						: channel.Volume.Value.ToString(format);

				g.DrawString(volume, metrics.PatternFont, volumeBrush, channelX + metrics.VolumePos.Text.Value, textY,
					sf);
			}

			if (metrics.VolumePos.Separator.HasValue && metrics.DrawGrid)
			{
				int sepX = channelX + metrics.VolumePos.Separator.Value;
				g.DrawLine(columnSeparatorPen, sepX, y, sepX, y + metrics.RowHeight);
			}

			// Draw effects
			if (metrics.EffectPos.Text.HasValue)
			{
				string effectText = RenderEffectHelper.GetEffectTextForRendering(
					channel?.EffectText, HideEmpty, CurrentDisplayMode, MaxEffectCount, EffectCharCount);

				if (!string.IsNullOrEmpty(effectText))
				{
					g.DrawString(effectText, metrics.PatternFont, effectBrush, channelX + metrics.EffectPos.Text.Value,
						textY, sf);
				}
			}
		}
	}
}
