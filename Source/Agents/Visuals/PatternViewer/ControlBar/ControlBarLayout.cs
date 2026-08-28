//---------------------------------------------------------------------------------------
// <copyright file="ControlBarLayout.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar
{
	/// <summary>
	/// Layout metrics for control bar (scaled values)
	/// </summary>
	internal class ControlBarMetrics
	{
		public int ControlBarHeight
		{
			get;
			init;
		}

		public int ButtonSize
		{
			get;
			init;
		}

		public int ButtonSpacing
		{
			get;
			init;
		}

		public int ButtonMargin
		{
			get;
			init;
		}

		public int SliderHeight
		{
			get;
			init;
		}

		public int SliderMinWidth
		{
			get;
			init;
		}

		public int TimeDisplayWidth
		{
			get;
			init;
		}

		public int ButtonY
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Which button is currently pressed (if any)
	/// </summary>
	internal enum PressedButton
	{
		None,
		PrevModule,
		PrevSubSong,
		PrevSnapshot,
		Restart,
		PlayPause,
		Stop,
		NextSnapshot,
		NextSubSong,
		NextModule
	}

	/// <summary>
	/// State info for control bar click handling (to check disabled buttons)
	/// </summary>
	internal readonly struct ControlBarState
	{
		public int ModuleIndex
		{
			get;
			init;
		}

		public int ModuleCount
		{
			get;
			init;
		}

		public int SubSongCurrent
		{
			get;
			init;
		}

		public int SubSongCount
		{
			get;
			init;
		}

		public int SnapshotPosition
		{
			get;
			init;
		}

		public int SnapshotCount
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Calculated button rectangles for the control bar
	/// </summary>
	internal class ControlBarRects
	{
		public Rectangle PrevModule
		{
			get;
			set;
		}

		public Rectangle PrevSubSong
		{
			get;
			set;
		}

		public Rectangle PrevSnapshot
		{
			get;
			set;
		}

		public Rectangle Restart
		{
			get;
			set;
		}

		public Rectangle PlayPause
		{
			get;
			set;
		}

		public Rectangle Stop
		{
			get;
			set;
		}

		public Rectangle NextSnapshot
		{
			get;
			set;
		}

		public Rectangle NextSubSong
		{
			get;
			set;
		}

		public Rectangle NextModule
		{
			get;
			set;
		}

		public Rectangle Slider
		{
			get;
			set;
		}

		public Rectangle SliderThumb
		{
			get;
			set;
		}

		/// <summary>
		/// Current X position during layout
		/// </summary>
		public int CurrentX
		{
			get;
			set;
		}

		/// <summary>
		/// Currently pressed button (for visual feedback)
		/// </summary>
		public PressedButton Pressed
		{
			get;
			set;
		} = PressedButton.None;
	}

	/// <summary>
	/// Handles layout calculation, metrics, and click handling for control bar.
	/// This is shared between all tracker styles.
	/// </summary>
	internal static class ControlBarLayout
	{
		// Control bar base constants (at 100% scale)
		private const int BASE_CONTROL_BAR_HEIGHT = 32;
		private const int BASE_BUTTON_SIZE = 24;
		private const int BASE_BUTTON_SPACING = 4;
		private const int BASE_BUTTON_MARGIN = 8;
		private const int BASE_SLIDER_HEIGHT = 8;
		private const int BASE_SLIDER_MIN_WIDTH = 100;
		private const int BASE_TIME_ONLY_WIDTH = 100; // Width when only time is shown (e.g. "00:00 / 00:00")
		private const int BASE_POSITION_ONLY_WIDTH = 100; // Width when only position is shown (e.g. "Pos 000/000")
		private const int BASE_TIME_AND_POS_WIDTH = 200; // Width when both time and position are shown

		/********************************************************************/
		/// <summary>
		/// Get the height of the control bar (scaled)
		/// </summary>
		/********************************************************************/
		public static int GetHeight(float scaleFactor, int buttonSizeOverride = 0)
		{
			if (buttonSizeOverride > 0)
			{
				return buttonSizeOverride + (int)(8 * scaleFactor); // Button + padding
			}

			return (int)(BASE_CONTROL_BAR_HEIGHT * scaleFactor);
		}

		/********************************************************************/
		/// <summary>
		/// Calculate scaled metrics for the control bar
		/// </summary>
		/********************************************************************/
		public static ControlBarMetrics CalculateMetrics(float scale, int topOffset, ControlBarConfig config = null, int timeDisplayWidth = 0, int buttonSizeOverride = 0)
		{
			int buttonSize = buttonSizeOverride > 0 ? buttonSizeOverride : (int)(BASE_BUTTON_SIZE * scale);
			int controlBarHeight = buttonSize + (int)(8 * scale); // Button + padding

			// Calculate time display width if not provided
			if (timeDisplayWidth == 0 && config != null && (config.ShowTimeDisplay || config.ShowPositionDisplay))
			{
				int baseTimeWidth;
				if (config.ShowTimeDisplay && config.ShowPositionDisplay)
				{
					baseTimeWidth = BASE_TIME_AND_POS_WIDTH;
				}
				else if (config.ShowTimeDisplay)
				{
					baseTimeWidth = BASE_TIME_ONLY_WIDTH;
				}
				else
				{
					baseTimeWidth = BASE_POSITION_ONLY_WIDTH;
				}

				timeDisplayWidth = (int)(baseTimeWidth * scale);
			}

			return new ControlBarMetrics
			{
				ControlBarHeight = controlBarHeight,
				ButtonSize = buttonSize,
				ButtonSpacing = (int)(BASE_BUTTON_SPACING * scale),
				ButtonMargin = (int)(BASE_BUTTON_MARGIN * scale),
				SliderHeight = (int)(BASE_SLIDER_HEIGHT * scale),
				SliderMinWidth = (int)(BASE_SLIDER_MIN_WIDTH * scale),
				TimeDisplayWidth = timeDisplayWidth,
				ButtonY = topOffset + ((controlBarHeight - buttonSize) / 2)
			};
		}

		/********************************************************************/
		/// <summary>
		/// Calculate all button rectangles based on config
		/// </summary>
		/********************************************************************/
		public static ControlBarRects CalculateRects(ControlBarConfig config, ControlBarMetrics metrics, int panelWidth, int leftOffset = 0)
		{
			ControlBarRects rects = new();
			rects.CurrentX = leftOffset + metrics.ButtonMargin;

			// PrevModule
			if (config.ShowPrevModule)
			{
				rects.PrevModule = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// PrevSubSong
			if (config.ShowPrevSubSong)
			{
				rects.PrevSubSong = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// PrevSnapshot
			if (config.ShowPrevSnapshot)
			{
				rects.PrevSnapshot = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// Restart
			if (config.ShowRestart)
			{
				rects.Restart = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// PlayPause
			if (config.ShowPlayPause)
			{
				rects.PlayPause = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// Stop
			if (config.ShowStop)
			{
				rects.Stop = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// NextSnapshot
			if (config.ShowNextSnapshot)
			{
				rects.NextSnapshot = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// NextSubSong
			if (config.ShowNextSubSong)
			{
				rects.NextSubSong = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// NextModule
			if (config.ShowNextModule)
			{
				rects.NextModule = new Rectangle(rects.CurrentX, metrics.ButtonY, metrics.ButtonSize, metrics.ButtonSize);
				rects.CurrentX += metrics.ButtonSize + metrics.ButtonSpacing;
			}

			// Add margin after buttons
			rects.CurrentX += metrics.ButtonMargin - metrics.ButtonSpacing;

			// Calculate time display width (only if enabled)
			int actualTimeDisplayWidth = 0;
			if (config.ShowTimeDisplay || config.ShowPositionDisplay)
			{
				actualTimeDisplayWidth = metrics.TimeDisplayWidth;
			}

			// Calculate slider
			if (config.ShowSlider)
			{
				int sliderEndX = leftOffset + panelWidth - metrics.ButtonMargin - actualTimeDisplayWidth;
				int sliderWidth = sliderEndX - rects.CurrentX;

				if (sliderWidth >= metrics.SliderMinWidth)
				{
					int sliderY = metrics.ButtonY + ((metrics.ButtonSize - metrics.SliderHeight) / 2);
					rects.Slider = new Rectangle(rects.CurrentX, sliderY, sliderWidth, metrics.SliderHeight);
					rects.CurrentX = sliderEndX + metrics.ButtonMargin;
				}
				else
				{
					rects.CurrentX += metrics.ButtonMargin;
				}
			}
			else
			{
				rects.CurrentX += metrics.ButtonMargin;
			}

			return rects;
		}

		/********************************************************************/
		/// <summary>
		/// Calculate slider thumb rectangle
		/// </summary>
		/********************************************************************/
		public static Rectangle CalculateSliderThumb(Rectangle sliderRect, int position, int length, float scale)
		{
			if (sliderRect.IsEmpty || length <= 0)
			{
				return Rectangle.Empty;
			}

			int thumbWidth = (int)(8 * scale);
			int thumbExtraHeight = (int)(4 * scale);
			float ratio = (float)position / length;
			ratio = Math.Max(0, Math.Min(1, ratio));
			int thumbX = sliderRect.X + (int)(ratio * (sliderRect.Width - thumbWidth));

			return new Rectangle(thumbX, sliderRect.Y - (thumbExtraHeight / 2), thumbWidth, sliderRect.Height + thumbExtraHeight);
		}

		/********************************************************************/
		/// <summary>
		/// Determine which button is at the given location (for press feedback)
		/// Only returns enabled buttons - disabled buttons don't show pressed state
		/// </summary>
		/********************************************************************/
		public static PressedButton GetButtonAtLocation(ControlBarRects rects, Point location, ControlBarState state)
		{
			// PrevModule - disabled if at first module
			if (!rects.PrevModule.IsEmpty && rects.PrevModule.Contains(location))
			{
				if (state.ModuleIndex > 0)
				{
					return PressedButton.PrevModule;
				}

				return PressedButton.None;
			}

			// PrevSubSong - disabled if only one subsong or at first subsong
			if (!rects.PrevSubSong.IsEmpty && rects.PrevSubSong.Contains(location))
			{
				if (state.SubSongCount > 1 && state.SubSongCurrent > 1)
				{
					return PressedButton.PrevSubSong;
				}

				return PressedButton.None;
			}

			// PrevSnapshot - disabled if no snapshots or at first position
			if (!rects.PrevSnapshot.IsEmpty && rects.PrevSnapshot.Contains(location))
			{
				if (state.SnapshotCount > 1 && state.SnapshotPosition > 0)
				{
					return PressedButton.PrevSnapshot;
				}

				return PressedButton.None;
			}

			if (!rects.Restart.IsEmpty && rects.Restart.Contains(location))
			{
				return PressedButton.Restart;
			}

			if (!rects.PlayPause.IsEmpty && rects.PlayPause.Contains(location))
			{
				return PressedButton.PlayPause;
			}

			if (!rects.Stop.IsEmpty && rects.Stop.Contains(location))
			{
				return PressedButton.Stop;
			}

			// NextSnapshot - disabled if no snapshots or at last position
			if (!rects.NextSnapshot.IsEmpty && rects.NextSnapshot.Contains(location))
			{
				if (state.SnapshotCount > 1 && state.SnapshotPosition < state.SnapshotCount - 1)
				{
					return PressedButton.NextSnapshot;
				}

				return PressedButton.None;
			}

			// NextSubSong - disabled if only one subsong or at last subsong
			if (!rects.NextSubSong.IsEmpty && rects.NextSubSong.Contains(location))
			{
				if (state.SubSongCount > 1 && state.SubSongCurrent < state.SubSongCount)
				{
					return PressedButton.NextSubSong;
				}

				return PressedButton.None;
			}

			// NextModule - disabled if at last module
			if (!rects.NextModule.IsEmpty && rects.NextModule.Contains(location))
			{
				if (state.ModuleIndex < state.ModuleCount - 1)
				{
					return PressedButton.NextModule;
				}

				return PressedButton.None;
			}

			return PressedButton.None;
		}

		/********************************************************************/
		/// <summary>
		/// Handle click on control bar, returns action to perform
		/// </summary>
		/********************************************************************/
		public static ControlBarAction HandleClick(ControlBarRects rects, Point location, ControlBarState state)
		{
			// PrevModule - disabled if at first module
			if (!rects.PrevModule.IsEmpty && rects.PrevModule.Contains(location))
			{
				if (state.ModuleIndex > 0)
				{
					return ControlBarAction.PrevModule;
				}

				return ControlBarAction.None;
			}

			// PrevSubSong - disabled if only one subsong or at first subsong
			if (!rects.PrevSubSong.IsEmpty && rects.PrevSubSong.Contains(location))
			{
				if (state.SubSongCount > 1 && state.SubSongCurrent > 1)
				{
					return ControlBarAction.PrevSubSong;
				}

				return ControlBarAction.None;
			}

			// PrevSnapshot - disabled if no snapshots or at first position
			if (!rects.PrevSnapshot.IsEmpty && rects.PrevSnapshot.Contains(location))
			{
				if (state.SnapshotCount > 1 && state.SnapshotPosition > 0)
				{
					return ControlBarAction.PrevSnapshot;
				}

				return ControlBarAction.None;
			}

			if (!rects.Restart.IsEmpty && rects.Restart.Contains(location))
			{
				return ControlBarAction.Restart;
			}

			if (!rects.PlayPause.IsEmpty && rects.PlayPause.Contains(location))
			{
				return ControlBarAction.PlayPause;
			}

			if (!rects.Stop.IsEmpty && rects.Stop.Contains(location))
			{
				return ControlBarAction.Stop;
			}

			// NextSnapshot - disabled if no snapshots or at last position
			if (!rects.NextSnapshot.IsEmpty && rects.NextSnapshot.Contains(location))
			{
				if (state.SnapshotCount > 1 && state.SnapshotPosition < state.SnapshotCount - 1)
				{
					return ControlBarAction.NextSnapshot;
				}

				return ControlBarAction.None;
			}

			// NextSubSong - disabled if only one subsong or at last subsong
			if (!rects.NextSubSong.IsEmpty && rects.NextSubSong.Contains(location))
			{
				if (state.SubSongCount > 1 && state.SubSongCurrent < state.SubSongCount)
				{
					return ControlBarAction.NextSubSong;
				}

				return ControlBarAction.None;
			}

			// NextModule - disabled if at last module
			if (!rects.NextModule.IsEmpty && rects.NextModule.Contains(location))
			{
				if (state.ModuleIndex < state.ModuleCount - 1)
				{
					return ControlBarAction.NextModule;
				}

				return ControlBarAction.None;
			}

			if (!rects.Slider.IsEmpty && rects.Slider.Contains(location) && state.SnapshotCount > 0)
			{
				float ratio = (float)(location.X - rects.Slider.X) / rects.Slider.Width;
				int newPosition = (int)(ratio * state.SnapshotCount);
				return ControlBarAction.SetPosition(Math.Max(0, Math.Min(state.SnapshotCount - 1, newPosition)));
			}

			return ControlBarAction.None;
		}

		/********************************************************************/
		/// <summary>
		/// Format timespan as mm:ss
		/// </summary>
		/********************************************************************/
		public static string FormatTime(TimeSpan time)
		{
			int totalMinutes = (int)time.TotalMinutes;
			int seconds = time.Seconds;
			return $"{totalMinutes:00}:{seconds:00}";
		}

		/********************************************************************/
		/// <summary>
		/// Build time/position display text based on config
		/// </summary>
		/********************************************************************/
		public static string BuildTimeDisplayText(ControlBarConfig config, TimeSpan elapsed, TimeSpan total, int snapshotPosition, int snapshotCount)
		{
			string text = string.Empty;

			if (config.ShowTimeDisplay)
			{
				string elapsedStr = FormatTime(elapsed);
				if (total.TotalSeconds > 0)
				{
					string totalStr = FormatTime(total);
					text = $"{elapsedStr} / {totalStr}";
				}
				else
				{
					text = elapsedStr;
				}
			}

			if (config.ShowPositionDisplay)
			{
				string posText = $"Pos {snapshotPosition:D3} / {snapshotCount:D3}";
				if (!string.IsNullOrEmpty(text))
				{
					text += $"  {posText}";
				}
				else
				{
					text = posText;
				}
			}

			return text;
		}
	}
}
