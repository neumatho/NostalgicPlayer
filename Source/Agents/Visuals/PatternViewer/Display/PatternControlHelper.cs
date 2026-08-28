//---------------------------------------------------------------------------------------
// <copyright file="PatternControlHelper.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
	/// <summary>
	/// Helper methods for pattern control operations
	/// </summary>
	internal static class PatternControlHelper
	{
		/********************************************************************/
		/// <summary>
		/// Calculate pattern metrics: maximum effect count per channel and maximum row count
		/// </summary>
		/********************************************************************/
		public static (int maxEffectCount, int maxRowCount) CalculatePatternMetrics(List<SongPatternViewData> songData)
		{
			if (songData == null || songData.Count == 0)
			{
				return (1, 64); // Default values
			}

			int maxEffects = 0;
			int maxRows = 0;

			foreach (SongPatternViewData patternData in songData)
			{
				if (patternData == null)
				{
					continue;
				}

				// Track maximum displayed row number (considering skip factor)
				int skip = patternData.Skip;
				int maxDisplayedRow = (patternData.RowCount - 1) * skip;
				if (maxDisplayedRow + 1 > maxRows)
				{
					maxRows = maxDisplayedRow + 1;
				}

				// Track maximum effect count per channel
				if (patternData.Rows != null)
				{
					foreach (SongPatternViewRow row in patternData.Rows)
					{
						if (row.Channels == null)
						{
							continue;
						}

						foreach (SongPatternViewChannel channel in row.Channels)
						{
							if (channel.EffectText != null && channel.EffectText.Length > maxEffects)
							{
								maxEffects = channel.EffectText.Length;
							}
						}
					}
				}
			}

			return (Math.Max(1, maxEffects), maxRows > 0 ? maxRows : 64);
		}
	}
}
