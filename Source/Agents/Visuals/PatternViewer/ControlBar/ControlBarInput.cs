//---------------------------------------------------------------------------------------
// <copyright file="ControlBarInput.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar
{
	/// <summary>
	/// Delegate for drawing time/position text using tracker-specific font
	/// </summary>
	/// <param name="g">Graphics context</param>
	/// <param name="text">Text to draw</param>
	/// <param name="x">X position</param>
	/// <param name="y">Y position</param>
	internal delegate void DrawTimeTextDelegate(Graphics g, string text, int x, int y);

	/// <summary>
	/// Input parameters for control bar rendering
	/// </summary>
	internal readonly struct ControlBarInput
	{
		public required float ScaleFactor
		{
			get;
			init;
		}

		public required bool IsPlaying
		{
			get;
			init;
		}

		public required bool IsPaused
		{
			get;
			init;
		}

		public required int SubSongCurrent
		{
			get;
			init;
		}

		public required int SubSongCount
		{
			get;
			init;
		}

		public required int ModuleIndex
		{
			get;
			init;
		}

		public required int ModuleCount
		{
			get;
			init;
		}

		public required int SnapshotPosition
		{
			get;
			init;
		}

		public required int SnapshotCount
		{
			get;
			init;
		}

		public required TimeSpan ElapsedTime
		{
			get;
			init;
		}

		public required TimeSpan TotalTime
		{
			get;
			init;
		}

		public required Brush BackgroundBrush
		{
			get;
			init;
		}

		public required Pen SeparatorPen
		{
			get;
			init;
		}

		public required Brush ButtonBrush
		{
			get;
			init;
		}

		public required Pen ButtonBorderPen
		{
			get;
			init;
		}

		public required Brush IconBrush
		{
			get;
			init;
		}

		public required Brush DisabledBrush
		{
			get;
			init;
		}

		public required Brush SliderTrackBrush
		{
			get;
			init;
		}

		public required Brush SliderThumbBrush
		{
			get;
			init;
		}

		public required Brush TextBrush
		{
			get;
			init;
		}

		public required Font Font
		{
			get;
			init;
		}

		public required ControlBarConfig Config
		{
			get;
			init;
		}

		public required PressedButton PressedButton
		{
			get;
			init;
		}

		/// <summary>
		/// Width of the time/position display in pixels (calculated by tracker)
		/// </summary>
		public int TimeDisplayWidth
		{
			get;
			init;
		}

		/// <summary>
		/// Override button size in pixels (0 = use default scaled size)
		/// </summary>
		public int ButtonSize
		{
			get;
			init;
		}

		/// <summary>
		/// Height of the time/position display font in pixels (for vertical centering)
		/// </summary>
		public int TimeDisplayTextHeight
		{
			get;
			init;
		}

		/// <summary>
		/// Optional delegate to draw time/position text using tracker-specific font
		/// </summary>
		public DrawTimeTextDelegate DrawTimeText
		{
			get;
			init;
		}
	}
}
