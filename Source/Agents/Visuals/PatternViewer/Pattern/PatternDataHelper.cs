//---------------------------------------------------------------------------------------
// <copyright file="PatternDataHelper.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Helper methods for safe access to pattern data
	/// </summary>
	internal static class PatternDataHelper
	{
		/********************************************************************/
		/// <summary>
		/// Safely get a channel from pattern data with bounds checking
		/// </summary>
		/// <param name="pattern">The pattern data</param>
		/// <param name="row">The row index</param>
		/// <param name="channel">The channel index</param>
		/// <returns>The channel data, or null if out of bounds</returns>
		/********************************************************************/
		public static SongPatternViewChannel GetChannel(SongPatternViewData pattern, int row, int channel)
		{
			if (pattern?.Rows == null || row < 0 || row >= pattern.Rows.Length)
			{
				return null;
			}

			SongPatternViewRow rowData = pattern.Rows[row];
			if (rowData?.Channels == null || channel < 0 || channel >= rowData.Channels.Length)
			{
				return null;
			}

			return rowData.Channels[channel];
		}

		/********************************************************************/
		/// <summary>
		/// Get the display row number for a given array index.
		/// Multiplies the index by the skip factor for compressed patterns.
		/// </summary>
		/// <param name="pattern">The pattern data</param>
		/// <param name="rowIndex">The array index</param>
		/// <returns>The display row number (rowIndex * skip)</returns>
		/********************************************************************/
		public static int GetDisplayRowNumber(SongPatternViewData pattern, int rowIndex)
		{
			int skip = pattern?.Skip ?? 1;
			return rowIndex * skip;
		}
	}
}
