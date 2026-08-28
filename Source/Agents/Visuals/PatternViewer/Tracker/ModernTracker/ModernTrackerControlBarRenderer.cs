//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerControlBarRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Renders the control bar with modern flat style for SystemLight and SystemDark themes
	/// </summary>
	internal static class ModernTrackerControlBarRenderer
	{
		// Cached rects for click handling
		private static ControlBarRects cachedRects;

		/// <summary>
		/// Get the height of the control bar (scaled)
		/// </summary>
		public static int GetHeight(float scaleFactor, int buttonSize = 0)
		{
			return ControlBarLayout.GetHeight(scaleFactor, buttonSize);
		}

		/// <summary>
		/// Get cached button rectangles (for click detection)
		/// </summary>
		public static ControlBarRects GetCachedRects()
		{
			return cachedRects;
		}

		/********************************************************************/
		/// <summary>
		/// Draw the control bar
		/// </summary>
		/********************************************************************/
		public static void DrawControlBar(Graphics g, Rectangle rect, ControlBarInput input)
		{
			ControlBarConfig config = input.Config;
			float scale = input.ScaleFactor;
			int x = rect.X;
			int y = rect.Y;
			int width = rect.Width;

			// Calculate metrics and layout
			ControlBarMetrics metrics = ControlBarLayout.CalculateMetrics(scale, y, config, input.TimeDisplayWidth, input.ButtonSize);
			ControlBarRects rects = ControlBarLayout.CalculateRects(config, metrics, width, x);
			cachedRects = rects;

			// Draw background
			g.FillRectangle(input.BackgroundBrush, rect);

			// Draw bottom separator line
			g.DrawLine(input.SeparatorPen, x, y + metrics.ControlBarHeight - 1, x + width, y + metrics.ControlBarHeight - 1);

			PressedButton pressed = input.PressedButton;

			// Draw buttons
			if (config.ShowPrevModule)
			{
				bool disabled = input.ModuleIndex <= 0;
				DrawButtonWithIcon(g, input, rects.PrevModule, ControlBarIconType.PrevModule, disabled, scale, pressed == PressedButton.PrevModule);
			}

			if (config.ShowPrevSubSong)
			{
				bool disabled = input.SubSongCount <= 1 || input.SubSongCurrent <= 1;
				DrawButtonWithIcon(g, input, rects.PrevSubSong, ControlBarIconType.PrevSubSong, disabled, scale, pressed == PressedButton.PrevSubSong);
			}

			if (config.ShowPrevSnapshot)
			{
				bool disabled = input.SnapshotPosition <= 0;
				DrawButtonWithIcon(g, input, rects.PrevSnapshot, ControlBarIconType.PrevSnapshot, disabled, scale, pressed == PressedButton.PrevSnapshot);
			}

			if (config.ShowRestart)
			{
				DrawButtonWithIcon(g, input, rects.Restart, ControlBarIconType.Restart, false, scale, pressed == PressedButton.Restart);
			}

			if (config.ShowPlayPause)
			{
				ControlBarIconType iconType = input.IsPaused || !input.IsPlaying ? ControlBarIconType.Play : ControlBarIconType.Pause;
				DrawButtonWithIcon(g, input, rects.PlayPause, iconType, false, scale, pressed == PressedButton.PlayPause);
			}

			if (config.ShowStop)
			{
				DrawButtonWithIcon(g, input, rects.Stop, ControlBarIconType.Stop, !input.IsPlaying, scale, pressed == PressedButton.Stop);
			}

			if (config.ShowNextSnapshot)
			{
				bool disabled = input.SnapshotPosition >= input.SnapshotCount - 1;
				DrawButtonWithIcon(g, input, rects.NextSnapshot, ControlBarIconType.NextSnapshot, disabled, scale, pressed == PressedButton.NextSnapshot);
			}

			if (config.ShowNextSubSong)
			{
				bool disabled = input.SubSongCount <= 1 || input.SubSongCurrent >= input.SubSongCount;
				DrawButtonWithIcon(g, input, rects.NextSubSong, ControlBarIconType.NextSubSong, disabled, scale, pressed == PressedButton.NextSubSong);
			}

			if (config.ShowNextModule)
			{
				bool disabled = input.ModuleIndex >= input.ModuleCount - 1;
				DrawButtonWithIcon(g, input, rects.NextModule, ControlBarIconType.NextModule, disabled, scale, pressed == PressedButton.NextModule);
			}

			// Draw slider
			if (config.ShowSlider && !rects.Slider.IsEmpty)
			{
				DrawSlider(g, rects.Slider, input.SliderTrackBrush, input.SliderThumbBrush,
					input.SnapshotPosition, input.SnapshotCount, scale);
			}

			// Draw time display
			if (config.ShowTimeDisplay || config.ShowPositionDisplay)
			{
				string text = ControlBarLayout.BuildTimeDisplayText(config, input.ElapsedTime, input.TotalTime,
					input.SnapshotPosition, input.SnapshotCount);

				if (!string.IsNullOrEmpty(text))
				{
					int textHeight = (int)input.Font.GetHeight(g);
					int timeY = y + ((metrics.ControlBarHeight - textHeight) / 2);
					if (input.DrawTimeText != null)
					{
						input.DrawTimeText(g, text, rects.CurrentX, timeY);
					}
					else
					{
						g.DrawString(text, input.Font, input.TextBrush, rects.CurrentX, timeY);
					}
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Handle click on control bar, returns action to perform
		/// </summary>
		/********************************************************************/
		public static ControlBarAction HandleClick(Point location, ControlBarState state)
		{
			if (cachedRects == null)
			{
				return ControlBarAction.None;
			}

			return ControlBarLayout.HandleClick(cachedRects, location, state);
		}

		#region Private Drawing Methods
		/********************************************************************/
		/// <summary>
		/// Draw a button with its icon (modern flat style)
		/// When pressed, the button background is darkened
		/// </summary>
		/********************************************************************/
		private static void DrawButtonWithIcon(Graphics g, ControlBarInput input, Rectangle rect, ControlBarIconType iconType, bool disabled, float scale, bool pressed)
		{
			Brush brush = disabled ? input.DisabledBrush : input.IconBrush;

			// Draw flat button background (darker when pressed)
			if (pressed && !disabled)
			{
				// Darken the button background when pressed
				Color buttonColor = ((SolidBrush)input.ButtonBrush).Color;
				Color pressedColor = Color.FromArgb(
					Math.Max(0, buttonColor.R - 40),
					Math.Max(0, buttonColor.G - 40),
					Math.Max(0, buttonColor.B - 40));
				using (SolidBrush pressedBrush = new(pressedColor))
				{
					g.FillRectangle(pressedBrush, rect);
				}
			}
			else
			{
				g.FillRectangle(input.ButtonBrush, rect);
			}

			if (!disabled)
			{
				g.DrawRectangle(input.ButtonBorderPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
			}

			// Draw icon
			g.SmoothingMode = SmoothingMode.AntiAlias;

			switch (iconType)
			{
				case ControlBarIconType.Play:
					g.FillPolygon(brush, ControlBarIcons.GetPlayIcon(rect, scale));
					break;

				case ControlBarIconType.Pause:
					foreach (Rectangle pauseRect in ControlBarIcons.GetPauseIcon(rect, scale))
					{
						g.FillRectangle(brush, pauseRect);
					}

					break;

				case ControlBarIconType.Stop:
					g.FillRectangle(brush, ControlBarIcons.GetStopIcon(rect, scale));
					break;

				case ControlBarIconType.PrevModule:
					(Rectangle bar, Point[] triangle) prevMod = ControlBarIcons.GetPrevModuleIcon(rect, scale);
					g.FillRectangle(brush, prevMod.bar);
					g.FillPolygon(brush, prevMod.triangle);
					break;

				case ControlBarIconType.PrevSubSong:
					(Rectangle bar, Point[] tri1, Point[] tri2) prevSub = ControlBarIcons.GetPrevSubSongIcon(rect, scale);
					g.FillRectangle(brush, prevSub.bar);
					g.FillPolygon(brush, prevSub.tri1);
					g.FillPolygon(brush, prevSub.tri2);
					break;

				case ControlBarIconType.PrevSnapshot:
					(Point[] tri1, Point[] tri2) rewind = ControlBarIcons.GetRewindIcon(rect, scale);
					g.FillPolygon(brush, rewind.tri1);
					g.FillPolygon(brush, rewind.tri2);
					break;

				case ControlBarIconType.Restart:
					(Point[] triangle, Rectangle bar) restart = ControlBarIcons.GetRestartIcon(rect, scale);
					g.FillPolygon(brush, restart.triangle);
					g.FillRectangle(brush, restart.bar);
					break;

				case ControlBarIconType.NextSnapshot:
					(Point[] tri1, Point[] tri2) ff = ControlBarIcons.GetFastForwardIcon(rect, scale);
					g.FillPolygon(brush, ff.tri1);
					g.FillPolygon(brush, ff.tri2);
					break;

				case ControlBarIconType.NextSubSong:
					(Point[] tri1, Point[] tri2, Rectangle bar) nextSub = ControlBarIcons.GetNextSubSongIcon(rect, scale);
					g.FillPolygon(brush, nextSub.tri1);
					g.FillPolygon(brush, nextSub.tri2);
					g.FillRectangle(brush, nextSub.bar);
					break;

				case ControlBarIconType.NextModule:
					(Point[] triangle, Rectangle bar) nextMod = ControlBarIcons.GetNextModuleIcon(rect, scale);
					g.FillPolygon(brush, nextMod.triangle);
					g.FillRectangle(brush, nextMod.bar);
					break;
			}

			g.SmoothingMode = SmoothingMode.None;
		}

		/********************************************************************/
		/// <summary>
		/// Draw position slider (modern style with round thumb)
		/// </summary>
		/********************************************************************/
		private static void DrawSlider(Graphics g, Rectangle rect, Brush trackBrush, Brush thumbBrush,
			int position, int length, float scale)
		{
			// Draw track
			g.FillRectangle(trackBrush, rect);

			// Draw thumb if we have valid position info
			if (length > 0)
			{
				Rectangle thumbRect = ControlBarLayout.CalculateSliderThumb(rect, position, length, scale);
				if (!thumbRect.IsEmpty)
				{
					g.SmoothingMode = SmoothingMode.AntiAlias;
					g.FillEllipse(thumbBrush, thumbRect);
					g.SmoothingMode = SmoothingMode.None;
				}
			}
		}
		#endregion
	}
}
