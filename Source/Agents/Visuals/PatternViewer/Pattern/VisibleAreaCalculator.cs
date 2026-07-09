//---------------------------------------------------------------------------------------
// <copyright file="VisibleAreaCalculator.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Result of visible rows calculation
	/// </summary>
	internal class VisibleRowsInfo
	{
		public int CenterRow
		{
			get;
			set;
		}

		public int VisibleRows
		{
			get;
			set;
		}

		public int ScrollPosition
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Result of display position calculation
	/// </summary>
	internal class DisplayPositionInfo
	{
		public int DisplayRow
		{
			get;
			set;
		}

		public int DisplaySongPosition
		{
			get;
			set;
		}

		public SongPatternViewData DisplaySongPattern
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Calculates visible channels, rows and scroll positions
	/// </summary>
	internal static class VisibleAreaCalculator
	{
		/********************************************************************/
		/// <summary>
		/// Calculate the number of visible channels based on available width
		/// </summary>
		/********************************************************************/
		internal static int CalculateVisibleChannels(int panelWidth, int rowNumberWidth, int channelWidth,
			int firstVisibleChannel, int channelCount)
		{
			int availableWidth = panelWidth - rowNumberWidth;
			int maxVisibleChannels = availableWidth / channelWidth;
			int channelsToShow = maxVisibleChannels;

			if (availableWidth > maxVisibleChannels * channelWidth &&
			    firstVisibleChannel + maxVisibleChannels < channelCount)
			{
				channelsToShow++;
			}

			return Math.Min(channelsToShow, channelCount - firstVisibleChannel);
		}

		/********************************************************************/
		/// <summary>
		/// Calculate visible rows and scroll position
		/// </summary>
		/********************************************************************/
		internal static VisibleRowsInfo CalculateVisibleRows(int panelHeight, int topOffset, int headerHeight,
			int rowHeight, int displayRow)
		{
			int fullVisibleRows = (panelHeight - topOffset - headerHeight) / rowHeight;
			return new VisibleRowsInfo
			{
				CenterRow = fullVisibleRows / 2,
				VisibleRows = fullVisibleRows + 1, // Draw one extra row partially at bottom
				ScrollPosition = displayRow - (fullVisibleRows / 2)
			};
		}

		/********************************************************************/
		/// <summary>
		/// Get display position and pattern based on scrolling state
		/// </summary>
		/********************************************************************/
		internal static DisplayPositionInfo GetDisplayPosition(bool allowPatternScrolling, int manualRow, int currentRow,
			int manualSongPosition, int currentSongPosition,
			List<SongPatternViewData> songData, SongPatternViewData currentSongPattern)
		{
			int displayRow = allowPatternScrolling ? manualRow : currentRow;
			int displaySongPosition = allowPatternScrolling ? manualSongPosition : currentSongPosition;
			SongPatternViewData displaySongPattern = allowPatternScrolling && songData != null &&
			                                         displaySongPosition >= 0 && displaySongPosition < songData.Count
				? songData[displaySongPosition]
				: currentSongPattern;

			return new DisplayPositionInfo {DisplayRow = displayRow, DisplaySongPosition = displaySongPosition, DisplaySongPattern = displaySongPattern};
		}
	}
}
