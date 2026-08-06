//---------------------------------------------------------------------------------------
// <copyright file="VuMeterRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	internal static class VuMeterRenderer
	{
		public static void RenderBar(Graphics g, int x, int barY, int barWidth, int barHeight, int maxBarHeight,
			string vuMeterId, ref float ahxHueCounter, ref float ahxSaturationCounter,
			ref float ahxSaturationDirection,
			bool isFirstChannel, VuMeterBrushCache brushCache)
		{
			IVuMeterStyleRenderer styleRenderer = VuMeterRegistry.GetRenderer(vuMeterId);
			styleRenderer?.Render(g, x, barY, barWidth, barHeight, maxBarHeight,
				ref ahxHueCounter, ref ahxSaturationCounter, ref ahxSaturationDirection,
				isFirstChannel, brushCache);
		}
	}
}
