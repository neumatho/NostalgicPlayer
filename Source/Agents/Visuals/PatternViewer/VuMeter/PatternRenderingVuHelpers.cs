//---------------------------------------------------------------------------------------
// <copyright file="PatternRenderingVuHelpers.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Static helper methods for VU meter / audio level calculations
	/// </summary>
	internal static class PatternRenderingVuHelpers
	{
		/********************************************************************/
		/// <summary>
		/// Converts a gain level into a dBFS value (same as ChannelLevelMeter)
		/// </summary>
		/********************************************************************/
		internal static float GainToDecibels(float gain)
		{
			const float minusInfinityDb = -60.0f;
			return gain > 0.0f ? Math.Max(minusInfinityDb, (float)Math.Log10(gain) * 20.0f) : minusInfinityDb;
		}

		/********************************************************************/
		/// <summary>
		/// Remaps a value from a source range to a target range (same as ChannelLevelMeter)
		/// </summary>
		/********************************************************************/
		internal static float Map(float sourceValue, float sourceRangeMin, float sourceRangeMax, float targetRangeMin,
			float targetRangeMax)
		{
			return targetRangeMin + ((targetRangeMax - targetRangeMin) * (sourceValue - sourceRangeMin) /
			                         (sourceRangeMax - sourceRangeMin));
		}

		/********************************************************************/
		/// <summary>
		/// Convert dB level to pixel position (same as ChannelLevelMeter)
		/// </summary>
		/********************************************************************/
		internal static int PositionForLevel(float dbLevel, int maxHeight)
		{
			const float maxDb = 0.0f;
			const float minDb = -60.0f;
			return (int)Math.Round(Map(dbLevel, maxDb, minDb, 0, maxHeight), MidpointRounding.AwayFromZero);
		}
	}
}
