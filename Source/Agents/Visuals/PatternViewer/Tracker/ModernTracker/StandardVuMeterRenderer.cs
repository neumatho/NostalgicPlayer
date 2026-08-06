//---------------------------------------------------------------------------------------
// <copyright file="StandardVuMeterRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Standard VU-Meter: segmented LED-style gradient (Purple → Blue → Cyan).
	/// </summary>
	internal sealed class StandardVuMeterRenderer : GradientVuMeterRenderer
	{
		private static readonly VuMeterGradientDefinition Gradient = CreateGradient();
		private readonly Color backgroundColor;

		public StandardVuMeterRenderer(Color backgroundColor) : base(Gradient)
		{
			this.backgroundColor = backgroundColor;
		}

		public override void Render(Graphics g, int x, int barY, int barWidth, int barHeight, int maxBarHeight,
			ref float ahxHueCounter, ref float ahxSaturationCounter, ref float ahxSaturationDirection,
			bool isFirstChannel, VuMeterBrushCache brushCache)
		{
			const int segmentHeight = 3;
			const int gapHeight = 1;
			const int stepHeight = segmentHeight + gapHeight;

			g.FillRectangle(brushCache.GetBrush(backgroundColor), x, barY, barWidth, barHeight);

			int barBottom = barY + barHeight;

			for (int segmentIndex = 0; segmentIndex < maxBarHeight / stepHeight; segmentIndex++)
			{
				int segmentBottom = barBottom - (segmentIndex * stepHeight);
				int segmentTop = segmentBottom - segmentHeight;

				if (segmentBottom <= barY)
				{
					break;
				}

				if (segmentTop >= barBottom)
				{
					continue;
				}

				int drawTop = Math.Max(segmentTop, barY);
				int drawBottom = Math.Min(segmentBottom, barBottom);
				int drawHeight = drawBottom - drawTop;

				if (drawHeight <= 0)
				{
					continue;
				}

				float positionInTotal = 1.0f - ((float)(segmentIndex * stepHeight) / maxBarHeight);

				Color barColor = GetGradientColor(Gradient.MainGradient, positionInTotal);
				g.FillRectangle(brushCache.GetBrush(barColor), x, drawTop, barWidth, drawHeight);

				if (Gradient.UseHighlightShadow)
				{
					const int highlightWidth = 2;

					Color highlightColor = GetGradientColor(Gradient.HighlightGradient, positionInTotal);
					g.FillRectangle(brushCache.GetBrush(highlightColor), x, drawTop, highlightWidth, drawHeight);

					Color shadowColor = GetGradientColor(Gradient.ShadowGradient, positionInTotal);
					g.FillRectangle(brushCache.GetBrush(shadowColor), x + barWidth - highlightWidth, drawTop, highlightWidth, drawHeight);
				}
			}
		}

		private static VuMeterGradientDefinition CreateGradient()
		{
			return new VuMeterGradientDefinition
			{
				UseHighlightShadow = true,
				MainGradient =
					new[]
					{
						new VuMeterColorStop(0.0f, Color.FromArgb(180, 60, 220)), new VuMeterColorStop(0.25f, Color.FromArgb(100, 80, 255)), new VuMeterColorStop(0.5f, Color.FromArgb(60, 140, 255)),
						new VuMeterColorStop(0.75f, Color.FromArgb(40, 200, 255)), new VuMeterColorStop(1.0f, Color.FromArgb(60, 220, 200))
					},
				HighlightGradient = new[]
				{
					new VuMeterColorStop(0.0f, Color.FromArgb(220, 120, 255)), new VuMeterColorStop(0.25f, Color.FromArgb(140, 120, 255)), new VuMeterColorStop(0.5f, Color.FromArgb(100, 180, 255)),
					new VuMeterColorStop(0.75f, Color.FromArgb(80, 230, 255)), new VuMeterColorStop(1.0f, Color.FromArgb(100, 250, 230))
				},
				ShadowGradient = new[]
				{
					new VuMeterColorStop(0.0f, Color.FromArgb(120, 30, 150)), new VuMeterColorStop(0.25f, Color.FromArgb(60, 50, 180)), new VuMeterColorStop(0.5f, Color.FromArgb(30, 90, 180)), new VuMeterColorStop(0.75f, Color.FromArgb(20, 140, 180)),
					new VuMeterColorStop(1.0f, Color.FromArgb(30, 160, 140))
				}
			};
		}
	}
}
