//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerPainter.ChannelBar.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Modern tracker painter - Channel bar drawing
	/// </summary>
	internal partial class ModernTrackerPainter
	{
		/********************************************************************/
		/// <summary>
		/// Draw channel headers section
		/// </summary>
		/********************************************************************/
		private void DrawChannelHeadersSection(Graphics g, Rectangle rect, SongPatternViewData displaySongPattern,
			int visibleChannels, RenderMetrics metrics)
		{
			int topOffset = rect.Y;

			// Initialize channel bar rects array
			ChannelBarRects = new Rectangle[ChannelCount];

			// Draw header background
			g.FillRectangle(headerBgBrush, rect);

			using (StringFormat sf = new(StringFormat.GenericTypographic))
			{
				// Draw scroll buttons with triangles using shared helper
				LeftScrollButtonRect =
					new Rectangle(rect.X + 1, topOffset + 1, metrics.ScrollButtonWidth, metrics.HeaderHeight - 2);
				bool canScrollLeft = FirstVisibleChannel > 0;

				// Draw scroll buttons with theme colors
				Color arrowColor = ((SolidBrush)headerBrush).Color;
				ChannelScrollButtonRenderer.DrawScrollButton(g, LeftScrollButtonRect, true, canScrollLeft,
					LeftScrollButtonHover,
					scrollButtonHoverColor,
					scrollButtonNormalColor,
					scrollButtonDisabledColor,
					arrowColor);

				RightScrollButtonRect = new Rectangle(rect.X + 1 + metrics.ScrollButtonWidth + 1, topOffset + 1,
					metrics.ScrollButtonWidth, metrics.HeaderHeight - 2);
				bool canScrollRight = FirstVisibleChannel < ChannelCount - 1;

				ChannelScrollButtonRenderer.DrawScrollButton(g, RightScrollButtonRect, false, canScrollRight,
					RightScrollButtonHover,
					scrollButtonHoverColor,
					scrollButtonNormalColor,
					scrollButtonDisabledColor,
					arrowColor);
			}

			// Draw channel headers
			for (int i = 0; i < visibleChannels; i++)
			{
				int ch = FirstVisibleChannel + i;
				if (ch >= ChannelCount)
				{
					break;
				}

				int x = rect.X + metrics.RowNumberWidth + (i * metrics.ChannelWidth);
				Rectangle headerRect = new(x, topOffset, metrics.ChannelWidth, metrics.HeaderHeight);
				ChannelBarRects[ch] = headerRect;

				string channelName = GetTrackName(ch);

				bool isMuted = IsChannelMuted(ch);
				Brush brushToUse = isMuted ? headerMutedBrush : headerBrush;

				// Use smaller font when track number + transpose is shown (longer header text)
				Font fontToUse = HasTrackNumber && HasTranspose ? headerFontSmall : headerFont;

				g.DrawClipped(headerRect, () =>
				{
					using (StringFormat
					       centerFormat = new(StringFormat.GenericTypographic) {Alignment = StringAlignment.Center})
					{
						g.DrawString(channelName, fontToUse, brushToUse, x + (metrics.ChannelWidth / 2.0f),
							topOffset + metrics.TextPaddingHeight, centerFormat);
					}
				}, RenderConstants.ChannelBarClipPaddingHorizontal, RenderConstants.ChannelBarClipPaddingVertical);
			}

			// Draw header line
			g.DrawLine(gridPen, rect.X, topOffset + metrics.HeaderHeight, rect.X + rect.Width,
				topOffset + metrics.HeaderHeight);
		}
	}
}
