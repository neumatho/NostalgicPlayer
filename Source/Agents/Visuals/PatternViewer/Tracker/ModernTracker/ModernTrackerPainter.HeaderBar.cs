//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerPainter.HeaderBar.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Modern tracker painter - Header bar drawing
	/// </summary>
	internal partial class ModernTrackerPainter
	{
		/********************************************************************/
		/// <summary>
		/// Draw header bar section
		/// </summary>
		/********************************************************************/
		private void DrawHeaderBarSection(Graphics g, Rectangle rect, int displaySongPosition, int displayRow,
			SongPatternViewData displaySongPattern, RenderMetrics metrics)
		{
			int lineHeight = metrics.StatusLineHeight;
			int statusHeight = metrics.StatusHeight;
			int yOffset = rect.Y;

			// Draw background
			g.FillRectangle(statusBgBrush, rect);

			using (StringFormat sf = new(StringFormat.GenericTypographic))
			{
				// Line 1: Song title
				string titleText =
					PatternRenderingHelpers.FormatTitleText(SongTitle, PlayerName, IsPaused, FileName, "Name: ",
						SubSongCurrent, SubSongTotal);

				g.DrawString(titleText, metrics.StatusFont, headerBrush, rect.X + 5, yOffset + 3, sf);

				// Line 2: Position and pattern info
				int? patternNum = displaySongPattern?.PatternNumber;
				int rowCount = displaySongPattern?.RowCount ?? 0;
				int skip = displaySongPattern?.Skip ?? 1;
				string patternStr = patternNum.HasValue ? $"  Pattern: {patternNum.Value:00}" : "";
				string rowDisplay =
					PatternRenderingHelpers.FormatRowDisplay(displayRow, rowCount, metrics.RowNumberFormat,
						MaxRowCount, skip);

				string posText =
					$"Position: {displaySongPosition + 1:000}/{SongLength:000}{patternStr}  Row: {rowDisplay}";
				g.DrawString(posText, metrics.StatusFont, headerBrush, rect.X + 5, yOffset + 3 + lineHeight, sf);

				// Line 3: Speed, BPM, Channels
				int channels = displaySongPattern?.ChannelCount ?? ChannelCount;
				string speedText = $"Speed: {PatternRenderingHelpers.FormatSpeed(Speed, skip)}  BPM: {FormatBpm(Bpm)}  Tracks: {channels:00}";
				g.DrawString(speedText, metrics.StatusFont, headerBrush, rect.X + 5, yOffset + 3 + (lineHeight * 2), sf);
			}

			// Draw separator line
			g.DrawLine(gridPen, rect.X, yOffset + statusHeight - 1, rect.X + rect.Width, yOffset + statusHeight - 1);
		}
	}
}
