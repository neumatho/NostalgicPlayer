//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerPainter.ControlBar.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Modern tracker painter - Control bar drawing
	/// </summary>
	internal partial class ModernTrackerPainter
	{
		/********************************************************************/
		/// <summary>
		/// Draw the control bar
		/// </summary>
		/********************************************************************/
		private void DrawControlBar(Graphics g, Rectangle rect, RenderMetrics metrics)
		{
			float scaleFactor = (int)BitmapFontRenderer.CurrentFontScale / 100f;

			// Calculate time display width by measuring actual text width with the font
			int timeDisplayWidth = 0;
			using (StringFormat sf = StringFormat.GenericTypographic)
			{
				if (ControlBarConfig.ShowTimeDisplay)
				{
					// Measure "00:00 / 00:00" (13 chars)
					SizeF size = g.MeasureString("00:00 / 00:00", metrics.StatusFont, PointF.Empty, sf);
					timeDisplayWidth += (int)size.Width;
				}

				if (ControlBarConfig.ShowPositionDisplay)
				{
					if (timeDisplayWidth > 0)
					{
						// Measure separator "  " (2 chars)
						SizeF sepSize = g.MeasureString("  ", metrics.StatusFont, PointF.Empty, sf);
						timeDisplayWidth += (int)sepSize.Width;
					}

					// Measure "Pos 000 / 000" (13 chars)
					SizeF posSize = g.MeasureString("Pos 000 / 000", metrics.StatusFont, PointF.Empty, sf);
					timeDisplayWidth += (int)posSize.Width;
				}
			}

			if (timeDisplayWidth > 0)
			{
				timeDisplayWidth += (int)(10 * scaleFactor); // Right padding
			}

			int buttonSize = metrics.StatusLineHeight * 2;

			ModernTrackerControlBarRenderer.DrawControlBar(g, rect, new ControlBarInput
			{
				ScaleFactor = scaleFactor,
				ButtonSize = buttonSize,
				IsPlaying = !IsPaused && SongData != null && SongData.Count > 0,
				IsPaused = IsPaused,
				SubSongCurrent = SubSongCurrent,
				SubSongCount = SubSongTotal,
				ModuleIndex = ModuleIndex,
				ModuleCount = ModuleCount,
				SnapshotPosition = SnapshotPosition,
				SnapshotCount = SnapshotCount,
				ElapsedTime = ElapsedTime,
				TotalTime = TotalTime,
				BackgroundBrush = statusBgBrush,
				SeparatorPen = gridPen,
				ButtonBrush = headerBgBrush,
				ButtonBorderPen = gridPen,
				IconBrush = headerBrush,
				DisabledBrush = new SolidBrush(Color.Gray),
				SliderTrackBrush = new SolidBrush(Color.FromArgb(100, 100, 100)),
				SliderThumbBrush = headerBrush,
				TextBrush = headerBrush,
				Font = metrics.StatusFont,
				Config = ControlBarConfig,
				PressedButton = ControlBarPressedButton,
				TimeDisplayWidth = timeDisplayWidth,
				DrawTimeText = (graphics, text, x, y) =>
				{
					using (SolidBrush brush = new(((SolidBrush)headerBrush).Color))
					using (StringFormat sf = StringFormat.GenericTypographic)
					{
						graphics.DrawString(text, metrics.StatusFont, brush, x, y, sf);
					}
				}
			});
		}
	}
}
