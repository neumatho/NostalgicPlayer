//---------------------------------------------------------------------------------------
// <copyright file="GradientVuMeterRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Reusable VU-Meter renderer that draws a vertical 2-pixel-segmented gradient bar.
	/// Per-style renderers inherit and supply their own VuMeterGradientDefinition.
	/// </summary>
	internal class GradientVuMeterRenderer : IVuMeterStyleRenderer
	{
		private readonly VuMeterGradientDefinition gradient;

		protected GradientVuMeterRenderer(VuMeterGradientDefinition gradient)
		{
			this.gradient = gradient;
		}

		public virtual void Render(Graphics g, int x, int barY, int barWidth, int barHeight, int maxBarHeight,
			ref float ahxHueCounter, ref float ahxSaturationCounter, ref float ahxSaturationDirection,
			bool isFirstChannel, VuMeterBrushCache brushCache)
		{
			for (int py = 0; py < barHeight; py += 2)
			{
				int absolutePositionFromBottom = maxBarHeight - barHeight + py;
				float positionInTotal = (float)absolutePositionFromBottom / maxBarHeight;
				int segHeight = Math.Min(2, barHeight - py);

				Color barColor = GetGradientColor(gradient.MainGradient, positionInTotal);
				g.FillRectangle(brushCache.GetBrush(barColor), x, barY + py, barWidth, segHeight);

				if (gradient.UseHighlightShadow)
				{
					const int highlightWidth = 2;

					Color highlightColor = GetGradientColor(gradient.HighlightGradient, positionInTotal);
					g.FillRectangle(brushCache.GetBrush(highlightColor), x, barY + py, highlightWidth, segHeight);

					Color shadowColor = GetGradientColor(gradient.ShadowGradient, positionInTotal);
					g.FillRectangle(brushCache.GetBrush(shadowColor), x + barWidth - highlightWidth, barY + py, highlightWidth, segHeight);
				}
			}
		}

		protected static Color GetGradientColor(VuMeterColorStop[] gradient, float position)
		{
			if (gradient == null || gradient.Length == 0)
			{
				return Color.White;
			}

			position = Math.Max(0.0f, Math.Min(1.0f, position));

			for (int i = 0; i < gradient.Length - 1; i++)
			{
				if (position >= gradient[i].Position && position <= gradient[i + 1].Position)
				{
					float t = (position - gradient[i].Position) / (gradient[i + 1].Position - gradient[i].Position);
					Color c1 = gradient[i].Color;
					Color c2 = gradient[i + 1].Color;

					return Color.FromArgb(
						(int)(c1.A + ((c2.A - c1.A) * t)),
						(int)(c1.R + ((c2.R - c1.R) * t)),
						(int)(c1.G + ((c2.G - c1.G) * t)),
						(int)(c1.B + ((c2.B - c1.B) * t))
					);
				}
			}

			return gradient[gradient.Length - 1].Color;
		}
	}
}
