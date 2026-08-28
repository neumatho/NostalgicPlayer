//---------------------------------------------------------------------------------------
// <copyright file="GraphicsExtensions.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Tile direction for DrawTiled
	/// </summary>
	internal enum TileDirection
	{
		Horizontal,
		Vertical,
		Both
	}

	/// <summary>
	/// Extension methods for Graphics
	/// </summary>
	internal static class GraphicsExtensions
	{
		/********************************************************************/
		/// <summary>
		/// Execute a drawing action with a clipping rectangle applied.
		/// Properly saves and restores the graphics state.
		/// </summary>
		/// <param name="g">The graphics context</param>
		/// <param name="clipRect">The rectangle to clip to</param>
		/// <param name="drawAction">The drawing action to execute</param>
		/// <param name="padding">Optional padding to shrink the clip rect on all sides</param>
		/********************************************************************/
		public static void DrawClipped(this Graphics g, Rectangle clipRect, Action drawAction, int padding = 0)
		{
			g.DrawClipped(clipRect, drawAction, padding, padding, padding, padding);
		}

		/********************************************************************/
		/// <summary>
		/// Execute a drawing action with a clipping rectangle applied.
		/// Properly saves and restores the graphics state.
		/// </summary>
		/// <param name="g">The graphics context</param>
		/// <param name="clipRect">The rectangle to clip to</param>
		/// <param name="drawAction">The drawing action to execute</param>
		/// <param name="horizontal">Padding on left and right sides</param>
		/// <param name="vertical">Padding on top and bottom sides</param>
		/********************************************************************/
		public static void DrawClipped(this Graphics g, Rectangle clipRect, Action drawAction,
			int horizontal, int vertical)
		{
			g.DrawClipped(clipRect, drawAction, horizontal, vertical, horizontal, vertical);
		}

		/********************************************************************/
		/// <summary>
		/// Execute a drawing action with a clipping rectangle applied.
		/// Properly saves and restores the graphics state.
		/// </summary>
		/// <param name="g">The graphics context</param>
		/// <param name="clipRect">The rectangle to clip to</param>
		/// <param name="drawAction">The drawing action to execute</param>
		/// <param name="left">Padding on the left side</param>
		/// <param name="top">Padding on the top side</param>
		/// <param name="right">Padding on the right side</param>
		/// <param name="bottom">Padding on the bottom side</param>
		/********************************************************************/
		public static void DrawClipped(this Graphics g, Rectangle clipRect, Action drawAction,
			int left, int top, int right, int bottom)
		{
			if (left > 0 || top > 0 || right > 0 || bottom > 0)
			{
				clipRect = new Rectangle(
					clipRect.X + left,
					clipRect.Y + top,
					clipRect.Width - left - right,
					clipRect.Height - top - bottom);
			}

			GraphicsState state = g.Save();
			try
			{
				g.SetClip(clipRect);
				drawAction();
			}
			finally
			{
				g.Restore(state);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Draw a bitmap tiled to fill the destination rectangle.
		/// Clips to the destination rectangle.
		/// </summary>
		/// <param name="g">The graphics context</param>
		/// <param name="bitmap">The bitmap to tile</param>
		/// <param name="destRect">The destination rectangle to fill</param>
		/// <param name="direction">Tile direction (Horizontal, Vertical, or Both)</param>
		/********************************************************************/
		public static void DrawTiled(this Graphics g, Bitmap bitmap, Rectangle destRect,
			TileDirection direction = TileDirection.Both)
		{
			if (bitmap == null)
			{
				return;
			}

			bool tileHorizontal = direction == TileDirection.Horizontal || direction == TileDirection.Both;
			bool tileVertical = direction == TileDirection.Vertical || direction == TileDirection.Both;

			int bitmapWidth = bitmap.Width;
			int bitmapHeight = bitmap.Height;

			GraphicsState state = g.Save();
			try
			{
				g.SetClip(destRect);

				int startX = destRect.X;
				int startY = destRect.Y;
				int endX = destRect.Right;
				int endY = destRect.Bottom;

				int currentY = startY;
				while (currentY < endY)
				{
					int drawHeight = tileVertical ? Math.Min(bitmapHeight, endY - currentY) : Math.Min(bitmapHeight, destRect.Height);

					int currentX = startX;
					while (currentX < endX)
					{
						int drawWidth = tileHorizontal ? Math.Min(bitmapWidth, endX - currentX) : Math.Min(bitmapWidth, destRect.Width);

						g.DrawImage(bitmap,
							new Rectangle(currentX, currentY, drawWidth, drawHeight),
							new Rectangle(0, 0, drawWidth, drawHeight),
							GraphicsUnit.Pixel);

						currentX += drawWidth;

						if (!tileHorizontal)
						{
							break;
						}
					}

					currentY += drawHeight;

					if (!tileVertical)
					{
						break;
					}
				}
			}
			finally
			{
				g.Restore(state);
			}
		}
	}
}
