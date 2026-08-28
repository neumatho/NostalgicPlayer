//---------------------------------------------------------------------------------------
// <copyright file="ChannelScrollButtonRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Renders channel scroll buttons with triangle arrows (shared by multiple renderers)
	/// </summary>
	internal static class ChannelScrollButtonRenderer
	{
		/********************************************************************/
		/// <summary>
		/// Draw a scroll button with triangle arrow
		/// Supports hover states and active/disabled colors
		/// </summary>
		/********************************************************************/
		public static void DrawScrollButton(Graphics g, Rectangle buttonRect, bool pointsLeft,
			bool canScroll, bool isHovering,
			Color activeHoverColor, Color activeNormalColor, Color disabledColor,
			Color arrowColor)
		{
			if (buttonRect.Width <= 0 || buttonRect.Height <= 0)
			{
				return;
			}

			// Determine background color based on state
			Color backgroundColor;
			if (!canScroll)
			{
				backgroundColor = disabledColor;
			}
			else if (isHovering)
			{
				backgroundColor = activeHoverColor;
			}
			else
			{
				backgroundColor = activeNormalColor;
			}

			// Draw button background
			using (SolidBrush bgBrush = new(backgroundColor))
			{
				g.FillRectangle(bgBrush, buttonRect);
			}

			// Draw arrow triangle
			using (SolidBrush arrowBrush = new(arrowColor))
			{
				int arrowCenterX = buttonRect.X + (buttonRect.Width / 2);
				int arrowCenterY = buttonRect.Y + (buttonRect.Height / 2);
				int arrowSize = Math.Min(buttonRect.Width, buttonRect.Height) / 3;

				Point[] arrow;
				if (pointsLeft)
					// Left-pointing triangle
				{
					arrow = new[] {new Point(arrowCenterX - (arrowSize / 2), arrowCenterY), new Point(arrowCenterX + (arrowSize / 2), arrowCenterY - arrowSize), new Point(arrowCenterX + (arrowSize / 2), arrowCenterY + arrowSize)};
				}
				else
					// Right-pointing triangle
				{
					arrow = new[] {new Point(arrowCenterX + (arrowSize / 2), arrowCenterY), new Point(arrowCenterX - (arrowSize / 2), arrowCenterY - arrowSize), new Point(arrowCenterX - (arrowSize / 2), arrowCenterY + arrowSize)};
				}

				g.FillPolygon(arrowBrush, arrow);
			}
		}
	}
}
