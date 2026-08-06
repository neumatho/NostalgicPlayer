//---------------------------------------------------------------------------------------
// <copyright file="ControlBarIcons.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar
{
	/// <summary>
	/// Icon type for control bar buttons
	/// </summary>
	internal enum ControlBarIconType
	{
		PrevModule, // |<
		PrevSubSong, // |<<
		PrevSnapshot, // <<
		Restart, // <|
		Play, // >
		Pause, // ||
		Stop, // []
		NextSnapshot, // >>
		NextSubSong, // >>|
		NextModule // >|
	}

	/// <summary>
	/// Provides icon geometry calculations for control bar buttons.
	/// All methods return Point arrays that can be filled with any brush/style.
	/// Icon sizes are calculated relative to button size for proper scaling.
	/// </summary>
	internal static class ControlBarIcons
	{
		// Icon size as fraction of button size
		private const float ICON_SIZE_RATIO = 0.25f;

		/********************************************************************/
		/// <summary>
		/// Calculate icon size based on button dimensions
		/// </summary>
		/********************************************************************/
		private static int GetIconSize(Rectangle rect)
		{
			return (int)(Math.Min(rect.Width, rect.Height) * ICON_SIZE_RATIO);
		}

		/********************************************************************/
		/// <summary>
		/// Get play icon points (triangle pointing right)
		/// </summary>
		/********************************************************************/
		public static Point[] GetPlayIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = (int)(GetIconSize(rect) * 1.33f); // Play icon slightly larger

			return new[] {new Point(cx - (size / 3), cy - (size / 2)), new Point(cx + (size / 2), cy), new Point(cx - (size / 3), cy + (size / 2))};
		}

		/********************************************************************/
		/// <summary>
		/// Get pause icon rectangles (two vertical bars)
		/// Returns array of 2 rectangles
		/// </summary>
		/********************************************************************/
		public static Rectangle[] GetPauseIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = GetIconSize(rect);
			int barWidth = Math.Max(2, size / 3);
			int barHeight = size * 2;
			int gap = Math.Max(2, size / 3);

			return new[] {new Rectangle(cx - gap - barWidth, cy - (barHeight / 2), barWidth, barHeight), new Rectangle(cx + gap - barWidth + 1, cy - (barHeight / 2), barWidth, barHeight)};
		}

		/********************************************************************/
		/// <summary>
		/// Get stop icon rectangle (square)
		/// </summary>
		/********************************************************************/
		public static Rectangle GetStopIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = (int)(GetIconSize(rect) * 1.6f);

			return new Rectangle(cx - (size / 2), cy - (size / 2), size, size);
		}

		/********************************************************************/
		/// <summary>
		/// Get previous module icon (bar + triangle pointing left)
		/// Returns: bar rectangle, triangle points
		/// </summary>
		/********************************************************************/
		public static (Rectangle bar, Point[] triangle) GetPrevModuleIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = GetIconSize(rect);
			int offset = Math.Max(1, size / 3);
			int barWidth = Math.Max(2, size / 3);

			Rectangle bar = new(cx - size - offset, cy - size, barWidth, size * 2);

			Point[] triangle = new[] {new Point(cx + size - offset, cy - size), new Point(cx - offset, cy), new Point(cx + size - offset, cy + size)};

			return (bar, triangle);
		}

		/********************************************************************/
		/// <summary>
		/// Get previous subsong icon (bar + two triangles pointing left)
		/// Returns: bar rectangle, two triangle point arrays
		/// </summary>
		/********************************************************************/
		public static (Rectangle bar, Point[] tri1, Point[] tri2) GetPrevSubSongIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = GetIconSize(rect);
			int offset = Math.Max(1, size / 5);
			int barWidth = Math.Max(2, size / 3);

			Rectangle bar = new(cx - size - offset - barWidth, cy - size, barWidth, size * 2);

			Point[] tri1 = new[] {new Point(cx - offset, cy - size), new Point(cx - size - offset, cy), new Point(cx - offset, cy + size)};

			Point[] tri2 = new[] {new Point(cx + size - offset, cy - size), new Point(cx - offset, cy), new Point(cx + size - offset, cy + size)};

			return (bar, tri1, tri2);
		}

		/********************************************************************/
		/// <summary>
		/// Get rewind/prev snapshot icon (two triangles pointing left, no bar)
		/// </summary>
		/********************************************************************/
		public static (Point[] tri1, Point[] tri2) GetRewindIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = GetIconSize(rect);
			int offset = Math.Max(1, size / 3);

			Point[] tri1 = new[] {new Point(cx - offset, cy - size), new Point(cx - size - offset, cy), new Point(cx - offset, cy + size)};

			Point[] tri2 = new[] {new Point(cx + size - offset, cy - size), new Point(cx - offset, cy), new Point(cx + size - offset, cy + size)};

			return (tri1, tri2);
		}

		/********************************************************************/
		/// <summary>
		/// Get restart icon (triangle pointing left + bar on right)
		/// </summary>
		/********************************************************************/
		public static (Point[] triangle, Rectangle bar) GetRestartIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2) - GetIconSize(rect);
			int cy = rect.Y + (rect.Height / 2);
			int size = GetIconSize(rect);
			int offset = Math.Max(1, size / 3);
			int barWidth = Math.Max(2, size / 3);

			Point[] triangle = new[] {new Point(cx + size - offset, cy - size), new Point(cx - offset, cy), new Point(cx + size - offset, cy + size)};

			Rectangle bar = new(cx + size + offset, cy - size, barWidth, size * 2);

			return (triangle, bar);
		}

		/********************************************************************/
		/// <summary>
		/// Get fast forward/next snapshot icon (two triangles pointing right, no bar)
		/// </summary>
		/********************************************************************/
		public static (Point[] tri1, Point[] tri2) GetFastForwardIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = GetIconSize(rect);
			int offset = Math.Max(1, size / 3);

			Point[] tri1 = new[] {new Point(cx - size + offset, cy - size), new Point(cx + offset, cy), new Point(cx - size + offset, cy + size)};

			Point[] tri2 = new[] {new Point(cx + offset, cy - size), new Point(cx + size + offset, cy), new Point(cx + offset, cy + size)};

			return (tri1, tri2);
		}

		/********************************************************************/
		/// <summary>
		/// Get next subsong icon (two triangles pointing right + bar)
		/// </summary>
		/********************************************************************/
		public static (Point[] tri1, Point[] tri2, Rectangle bar) GetNextSubSongIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = GetIconSize(rect);
			int offset = Math.Max(1, size / 5);
			int barWidth = Math.Max(2, size / 3);

			Point[] tri1 = new[] {new Point(cx - size + offset, cy - size), new Point(cx + offset, cy), new Point(cx - size + offset, cy + size)};

			Point[] tri2 = new[] {new Point(cx + offset, cy - size), new Point(cx + size + offset, cy), new Point(cx + offset, cy + size)};

			Rectangle bar = new(cx + size + offset, cy - size, barWidth, size * 2);

			return (tri1, tri2, bar);
		}

		/********************************************************************/
		/// <summary>
		/// Get next module icon (triangle pointing right + bar)
		/// </summary>
		/********************************************************************/
		public static (Point[] triangle, Rectangle bar) GetNextModuleIcon(Rectangle rect, float scale)
		{
			int cx = rect.X + (rect.Width / 2);
			int cy = rect.Y + (rect.Height / 2);
			int size = GetIconSize(rect);
			int offset = Math.Max(1, size / 3);
			int barWidth = Math.Max(2, size / 3);

			Point[] triangle = new[] {new Point(cx - size + offset, cy - size), new Point(cx + offset, cy), new Point(cx - size + offset, cy + size)};

			Rectangle bar = new(cx + size, cy - size, barWidth, size * 2);

			return (triangle, bar);
		}
	}
}
