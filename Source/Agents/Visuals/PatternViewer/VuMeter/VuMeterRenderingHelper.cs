//---------------------------------------------------------------------------------------
// <copyright file="VuMeterRenderingHelper.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Helper for VU meter rendering with new input struct
	/// </summary>
	internal static class VuMeterRenderingHelper
	{
		/********************************************************************/
		/// <summary>
		/// Render volume bars using the new VuRenderInput struct
		/// </summary>
		/********************************************************************/
		internal static void RenderVolumeBars(Graphics g, Rectangle contentRect, VuRenderInput input, VolumeBarState state, VuRenderDebugOptions debugOptions)
		{
			if (state.ChannelConfig.ChannelInfo == null)
			{
				return;
			}

			// If VU meters are off but debug info is enabled, still show debug info
			bool skipVuMeters = state.DisplaySettings.Mode == VolumeBarMode.Off;
			if (skipVuMeters && !debugOptions.ShowDebugInfo)
			{
				return;
			}

			int barWidth = (int)(16 * BitmapFontRenderer.ScaleFactor);
			int maxBarHeight = input.VuBarBottom - input.VuBarTop;
			int visibleChannels = input.ChannelXPositions.Length;

			for (int i = 0; i < visibleChannels; i++)
			{
				int ch = state.ChannelConfig.FirstVisibleChannel + i;
				if (ch >= state.ChannelConfig.ChannelCount || ch >= state.ChannelConfig.ChannelInfo.Length)
				{
					break;
				}

				ChannelChanged channelInfo = state.ChannelConfig.ChannelInfo[ch];

				// Track channelInfo null occurrences
				bool isCurrentlyNull = channelInfo == null;
				bool wasNullLastFrame = state.ChannelState.WasChannelInfoNull[ch];

				if (isCurrentlyNull && !wasNullLastFrame)
				{
					state.ChannelState.ChannelInfoNullCount[ch]++;
				}

				// Update state for next frame
				state.ChannelState.WasChannelInfoNull[ch] = isCurrentlyNull;

				// Remember if channel is disabled for later
				bool isDisabled = channelInfo != null && !channelInfo.Enabled;

				// Calculate X position - center the bar in the channel
				int x = input.ChannelXPositions[i] + (input.ChannelWidth / 2) - (barWidth / 2);
				int barHeight = 0;

				// Calculate bar height based on mode (skip if VU meters are off)
				if (!skipVuMeters && state.DisplaySettings.Mode == VolumeBarMode.NoteKick)
				{
					if (ch < state.AudioData.VolumeBarDecay.Length && state.AudioData.VolumeBarDecay[ch] > 0)
					{
						barHeight = state.AudioData.VolumeBarDecay[ch] * maxBarHeight / 20;
					}
				}
				else if (!skipVuMeters && state.DisplaySettings.Mode == VolumeBarMode.RealVolume)
				{
					if (ch < state.AudioData.RealVolumeSmoothed.Length && state.AudioData.RealVolumeSmoothed[ch] > 0.0f)
					{
						float levelDb = PatternRenderingVuHelpers.GainToDecibels(state.AudioData.RealVolumeSmoothed[ch]);
						int endPos = PatternRenderingVuHelpers.PositionForLevel(levelDb, maxBarHeight);
						barHeight = maxBarHeight - endPos;
					}
				}

				// Draw VU bar only if not skipped and has height
				if (!skipVuMeters && barHeight > 0)
				{
					int barY = input.VuBarBottom - barHeight;
					float ahxHueCounter = state.AhxAnimation.HueCounter;
					float ahxSaturationCounter = state.AhxAnimation.SaturationCounter;
					float ahxSaturationDirection = state.AhxAnimation.SaturationDirection;

					VuMeterRenderer.RenderBar(g, x, barY, barWidth, barHeight, maxBarHeight,
						state.DisplaySettings.VuMeterId,
						ref ahxHueCounter, ref ahxSaturationCounter, ref ahxSaturationDirection, i == 0,
						state.BrushCache);

					// Update animation values
					state.AhxAnimation.HueCounter = ahxHueCounter;
					state.AhxAnimation.SaturationCounter = ahxSaturationCounter;
					state.AhxAnimation.SaturationDirection = ahxSaturationDirection;
				}

				// Draw debug info box when enabled
				if (debugOptions.ShowDebugInfo)
				{
					SongPatternViewChannel patternChannel = null;
					if (debugOptions.CurrentPattern != null &&
					    debugOptions.CurrentPattern.Rows != null &&
					    debugOptions.CurrentRow >= 0 && debugOptions.CurrentRow < debugOptions.CurrentPattern.Rows.Length)
					{
						SongPatternViewRow row = debugOptions.CurrentPattern.Rows[debugOptions.CurrentRow];
						if (row != null && row.Channels != null && ch < row.Channels.Length)
						{
							patternChannel = row.Channels[ch];
						}
					}

					DrawDebugInfoBox(g, x, barWidth, input.VuBarTop, maxBarHeight,
						channelInfo, isDisabled, ch,
						patternChannel,
						state.AudioData.RealVolumeSmoothed, state.AudioData.RealVolumeMeasured,
						state.ChannelState.EnabledChangeCount, state.ChannelState.MutedChangeCount,
						state.ChannelState.ChannelInfoNullCount,
						barHeight, debugOptions.ShowDebugInfo, skipVuMeters);
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Build VuRenderInput from PatternRowsResult
		/// </summary>
		/********************************************************************/
		internal static VuRenderInput BuildFromPatternRowsResult(PatternRowsResult result)
		{
			return new VuRenderInput {ChannelXPositions = result.ChannelXPositions, ChannelWidth = result.ChannelWidth, VuBarTop = result.VuBarTop, VuBarBottom = result.VuBarBottom};
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Draw debug information box for a channel
		/// </summary>
		/********************************************************************/
		private static void DrawDebugInfoBox(Graphics g, int x, int barWidth, int vuBarTop, int maxBarHeight,
			ChannelChanged channelInfo, bool isDisabled, int channelIndex,
			SongPatternViewChannel patternChannel,
			float[] realVolumeSmoothed, float[] realVolumeMeasured,
			int[] enabledChangeCount, int[] mutedChangeCount, int[] channelInfoNullCount,
			int currentBarHeight, bool showDebugInfo, bool skipVuMeters)
		{
			using (Font font = new("Consolas", 9, FontStyle.Bold))
			{
				int textY = vuBarTop + maxBarHeight + 20;

				List<(string text, bool isRed)> lines = new();

				if (patternChannel != null)
				{
					lines.Add(patternChannel.TrackNumber.HasValue
						? ($"Trk: {patternChannel.TrackNumber.Value:D3}", false)
						: ("Trk: ---", false));

					lines.Add(patternChannel.PlayingPosition.HasValue
						? ($"Pos: {patternChannel.PlayingPosition.Value:D3}", false)
						: ("Pos: ---", false));
				}
				else
				{
					lines.Add(("Trk: N/A", false));
					lines.Add(("Pos: N/A", false));
				}

				if (channelInfo != null)
				{
					if (!skipVuMeters)
					{
						lines.Add(channelInfo.Volume.HasValue
							? ($"V: {channelInfo.Volume.Value:D3}", false)
							: ("V: null", false));
					}

					lines.Add(($"Mut: {(channelInfo.Muted ? "yes" : "nop")}", false));
					lines.Add(($"Dis: {(isDisabled ? "yes" : "nop")}", false));
				}
				else
				{
					if (!skipVuMeters)
					{
						lines.Add(("V: N/A", false));
					}

					lines.Add(("Mut: N/A", false));
					lines.Add(("Dis: N/A", false));
				}

				if (!skipVuMeters && channelIndex < realVolumeSmoothed.Length)
				{
					float measured = channelIndex < realVolumeSmoothed.Length ? realVolumeMeasured[channelIndex] : 0f;
					float smoothed = realVolumeSmoothed[channelIndex];
					float smoothedDb = PatternRenderingVuHelpers.GainToDecibels(smoothed);
					lines.Add(($"M: {measured:F2}", false));
					lines.Add(($"S: {smoothed:F2}", false));
					lines.Add(($"{smoothedDb,4:F0} dB", false));

					float pctOfMax = maxBarHeight > 0 ? currentBarHeight * 100.0f / maxBarHeight : 0f;
					lines.Add(($"H: {pctOfMax,3:F0}%", false));
				}

				if (channelInfoNullCount != null && channelIndex < channelInfoNullCount.Length)
				{
					int nCount = channelInfoNullCount[channelIndex];
					lines.Add(($"N#: {nCount}", nCount != 0));
				}

				if (enabledChangeCount != null && mutedChangeCount != null && channelIndex < enabledChangeCount.Length)
				{
					int eCount = enabledChangeCount[channelIndex];
					int mCount = mutedChangeCount[channelIndex];
					lines.Add(($"E#: {eCount}", eCount != 0));
					lines.Add(($"M#: {mCount}", mCount != 0));
				}

				float lineHeight = 14f;
				string testString = "Pos: 999";
				int boxWidth = (int)(g.MeasureString(testString, font).Width + 8);
				int boxHeight = (int)((lines.Count * lineHeight) + 4);
				int boxX = x + (barWidth / 2) - (boxWidth / 2);
				int boxY = textY;

				using (SolidBrush bgBrush = new(Color.FromArgb(200, 0, 0, 0)))
				{
					g.FillRectangle(bgBrush, boxX, boxY, boxWidth, boxHeight);
				}

				using (Pen borderPen = new(Color.FromArgb(150, 255, 255, 255), 1))
				{
					g.DrawRectangle(borderPen, boxX, boxY, boxWidth, boxHeight);
				}

				using (SolidBrush whiteBrush = new(Color.White))
				using (SolidBrush redBrush = new(Color.Red))
				{
					int currentY = boxY + 2;
					foreach ((string text, bool isRed) in lines)
					{
						SolidBrush brush = isRed ? redBrush : whiteBrush;
						SizeF size = g.MeasureString(text, font);
						float textX = boxX + ((boxWidth - size.Width) / 2);
						g.DrawString(text, font, brush, textX, currentY);
						currentY += (int)lineHeight;
					}
				}
			}
		}
		#endregion
	}

	/// <summary>
	/// Debug options for VU meter rendering
	/// </summary>
	internal readonly struct VuRenderDebugOptions
	{
		/// <summary>
		/// Current display mode
		/// </summary>
		public required DisplayMode CurrentDisplayMode
		{
			get;
			init;
		}

		/// <summary>
		/// Whether to show debug information
		/// </summary>
		public required bool ShowDebugInfo
		{
			get;
			init;
		}

		/// <summary>
		/// Current pattern data
		/// </summary>
		public SongPatternViewData CurrentPattern
		{
			get;
			init;
		}

		/// <summary>
		/// Current row number
		/// </summary>
		public int CurrentRow
		{
			get;
			init;
		}
	}
}
