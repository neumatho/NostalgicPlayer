//---------------------------------------------------------------------------------------
// <copyright file="RenderEffects.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Helper class for rendering effects in pattern data
	/// </summary>
	internal static class RenderEffectHelper
	{
		/********************************************************************/
		/// <summary>
		/// Format effect text array to display string
		/// </summary>
		/// <param name="effectText">Array of effect strings</param>
		/// <returns>Formatted effect string with spaces between effects</returns>
		/********************************************************************/
		internal static string FormatEffectText(string[] effectText)
		{
			if (effectText == null || effectText.Length == 0)
			{
				return string.Empty;
			}

			return string.Join(" ", effectText);
		}

		/********************************************************************/
		/// <summary>
		/// Get effect text for rendering, handling hideEmpty and display mode
		/// </summary>
		/// <param name="effectText">Array of effect strings from channel data</param>
		/// <param name="hideEmpty">Whether to hide empty effects</param>
		/// <param name="displayMode">Current display mode</param>
		/// <param name="maxEffectCount">Maximum number of effects per channel</param>
		/// <param name="effectCharCount">Characters per effect (e.g., 3 for "000", 4 for "0000")</param>
		/// <returns>Formatted effect string ready for rendering</returns>
		/********************************************************************/
		internal static string GetEffectTextForRendering(string[] effectText, bool hideEmpty, DisplayMode displayMode, int maxEffectCount, int effectCharCount)
		{
			// No effects configured for this player
			if (effectCharCount == 0)
			{
				return string.Empty;
			}

			// SingleEffect mode - just show first effect
			if (displayMode == DisplayMode.SingleEffect)
			{
				if (effectText != null && effectText.Length > 0)
				{
					return effectText[0];
				}

				return hideEmpty ? "" : new string('0', effectCharCount);
			}

			// MultiEffect mode
			if (effectText == null || effectText.Length == 0)
			{
				// No effects at all
				return hideEmpty ? "" : GeneratePlaceholderEffects(maxEffectCount, effectCharCount, displayMode);
			}

			// hideEmpty = true: just show what we have, no padding
			if (hideEmpty)
			{
				return string.Join(" ", effectText);
			}

			// hideEmpty = false: pad missing effects with zeros to maxEffectCount
			string placeholder = new('0', effectCharCount);
			string[] paddedEffects = new string[maxEffectCount];

			for (int i = 0; i < maxEffectCount; i++)
			{
				if (i < effectText.Length && !string.IsNullOrEmpty(effectText[i]))
				{
					paddedEffects[i] = effectText[i];
				}
				else
				{
					paddedEffects[i] = placeholder;
				}
			}

			return string.Join(" ", paddedEffects);
		}

		/********************************************************************/
		/// <summary>
		/// Generate placeholder effects string based on effect count and char length
		/// </summary>
		/// <param name="maxEffectCount">Number of effect blocks</param>
		/// <param name="effectCharCount">Characters per effect block</param>
		/// <param name="displayMode">Current display mode</param>
		/// <returns>Placeholder string like "000 000 000" for 3 effects with 3 chars each</returns>
		/********************************************************************/
		internal static string GeneratePlaceholderEffects(int maxEffectCount, int effectCharCount, DisplayMode displayMode)
		{
			// No effects
			if (effectCharCount == 0)
			{
				return string.Empty;
			}

			// Single block placeholder (e.g., "000" or "0000")
			string singleBlock = new('0', effectCharCount);

			// SingleEffect mode only shows one effect
			if (displayMode == DisplayMode.SingleEffect)
			{
				return singleBlock;
			}

			// Build multiple blocks with spaces between them
			// e.g., maxEffectCount=3, effectCharCount=3 → "000 000 000"
			string[] blocks = new string[maxEffectCount];
			for (int i = 0; i < maxEffectCount; i++)
			{
				blocks[i] = singleBlock;
			}

			return string.Join(" ", blocks);
		}
	}
}
