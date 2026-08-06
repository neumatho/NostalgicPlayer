//---------------------------------------------------------------------------------------
// <copyright file="DisplayMode.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers
{
	/// <summary>
	/// Display mode for pattern viewer
	/// </summary>
	internal enum DisplayMode
	{
		MultiEffect, // All effect columns
		SingleEffect, // Only first effect column
		Compact, // Only note + instrument
		NotesOnly // Only notes
	}

	/// <summary>
	/// Extension methods for DisplayMode
	/// </summary>
	internal static class DisplayModeExtensions
	{
		/// <summary>
		/// Returns true if this mode shows effects (MultiEffect or SingleEffect) and player has effects
		/// </summary>
		/// <param name="mode">Display mode</param>
		/// <param name="effectCharCount">Characters per effect (0 = no effects)</param>
		public static bool ShowsEffects(this DisplayMode mode, int effectCharCount)
		{
			return effectCharCount > 0 && (mode == DisplayMode.MultiEffect || mode == DisplayMode.SingleEffect);
		}

		/// <summary>
		/// Calculate the effect column width in characters based on display mode
		/// </summary>
		/// <param name="mode">Display mode</param>
		/// <param name="maxEffectCount">Maximum number of effects per channel</param>
		/// <param name="effectCharCount">Characters per effect (e.g., 3 for "C40", 4 for "10FF")</param>
		/// <returns>Total character width for effects column</returns>
		public static int GetEffectChars(this DisplayMode mode, int maxEffectCount, int effectCharCount)
		{
			// If no effects, return 0
			if (effectCharCount == 0)
			{
				return 0;
			}

			if (mode == DisplayMode.SingleEffect)
			{
				return effectCharCount;
			}

			// MultiEffect: effectCount * charCount + (effectCount - 1) spaces
			// e.g., 3 effects with 3 chars each: "000 000 000" = 3*3 + 2 = 11
			return (maxEffectCount * effectCharCount) + (maxEffectCount - 1);
		}
	}
}
