//---------------------------------------------------------------------------------------
// <copyright file="IVuMeterStyleRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Renders a single VU-Meter bar in a specific tracker style.
	/// Each tracker provides its own implementation in its own folder.
	/// </summary>
	internal interface IVuMeterStyleRenderer
	{
		void Render(Graphics g, int x, int barY, int barWidth, int barHeight, int maxBarHeight,
			ref float ahxHueCounter, ref float ahxSaturationCounter, ref float ahxSaturationDirection,
			bool isFirstChannel, VuMeterBrushCache brushCache);
	}
}
