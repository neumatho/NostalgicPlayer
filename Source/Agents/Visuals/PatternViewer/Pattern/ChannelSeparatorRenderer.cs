//---------------------------------------------------------------------------------------
// <copyright file="ChannelSeparatorRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Renders channel separators in different styles
	/// </summary>
	internal static class ChannelSeparatorRenderer
	{
		/********************************************************************/
		/// <summary>
		/// Draw simple channel separators (single line style)
		/// Used by: Oktalyzer, ScreamTracker, ImpulseTracker
		/// </summary>
		/********************************************************************/
		internal static void DrawSimple(Graphics g, int leftOffset, int y, int rowHeight,
			int visibleChannels, int rowNumberWidth, int channelWidth,
			int channelSeparatorWidth, bool drawSeparatorAfterLastChannel, Pen pen)
		{
			int maxSep = drawSeparatorAfterLastChannel ? visibleChannels : visibleChannels - 1;
			for (int sep = 0; sep <= maxSep; sep++)
			{
				int sepX = leftOffset + rowNumberWidth + (sep * channelWidth);
				int x = sepX + (channelSeparatorWidth / 2);
				g.DrawLine(pen, x, y, x, y + rowHeight);
			}
		}
	}
}
