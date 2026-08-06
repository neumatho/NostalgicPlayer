//---------------------------------------------------------------------------------------
// <copyright file="DisplayColors.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Color configuration for Modern Tracker (SystemLight/SystemDark styles)
	/// </summary>
	internal class DisplayColors
	{
		// Pen colors and widths
		public required Color GridColor
		{
			get;
			init;
		}

		public required Color PatternBoundaryColor
		{
			get;
			init;
		}

		public required Color ChannelSeparatorColor
		{
			get;
			init;
		}

		public required int ChannelSeparatorWidth
		{
			get;
			init;
		}

		public required bool DrawSeparatorAfterLastChannel
		{
			get;
			init;
		}

		public required Color ColumnSeparatorColor
		{
			get;
			init;
		}

		// Brush colors
		public required Color TextColor
		{
			get;
			init;
		}

		public required Color HeaderColor
		{
			get;
			init;
		}

		public required Color CurrentRowColor
		{
			get;
			init;
		}

		public required Color? RowBgColor
		{
			get;
			init;
		} // Background for normal rows (null = transparent)

		public required Color StatusBgColor
		{
			get;
			init;
		}

		public required Color HeaderBgColor
		{
			get;
			init;
		}

		public required Color ChannelStripeBgColor
		{
			get;
			init;
		}

		/********************************************************************/
		/// <summary>
		/// Get display colors for the specified Modern Tracker style
		/// </summary>
		/// <param name="isDark">True for dark mode, false for light mode</param>
		/********************************************************************/
		public static DisplayColors GetDisplayColors(bool isDark)
		{
			if (isDark)
			{
				return new DisplayColors
				{
					GridColor = Color.FromArgb(64, 64, 64),
					PatternBoundaryColor = Color.FromArgb(128, 128, 0),
					ChannelSeparatorColor = Color.FromArgb(48, 48, 48), // Same as HeaderBgColor
					ChannelSeparatorWidth = 3,
					DrawSeparatorAfterLastChannel = true,
					ColumnSeparatorColor = Color.FromArgb(40, 40, 40),
					TextColor = Color.LightGray,
					HeaderColor = Color.White,
					CurrentRowColor = Color.FromArgb(0, 64, 128),
					RowBgColor = null, // Transparent (shows panel background)
					StatusBgColor = Color.FromArgb(32, 32, 32),
					HeaderBgColor = Color.FromArgb(48, 48, 48),
					ChannelStripeBgColor = Color.FromArgb(16, 16, 16)
				};
			}

			return new DisplayColors
			{
				GridColor = Color.FromArgb(200, 200, 200),
				PatternBoundaryColor = Color.FromArgb(150, 150, 0),
				ChannelSeparatorColor = Color.FromArgb(230, 230, 230), // Same as HeaderBgColor
				ChannelSeparatorWidth = 3,
				DrawSeparatorAfterLastChannel = true,
				ColumnSeparatorColor = Color.FromArgb(220, 220, 220),
				TextColor = Color.Black,
				HeaderColor = Color.Black,
				CurrentRowColor = Color.FromArgb(200, 220, 255),
				RowBgColor = null, // Transparent (shows panel background)
				StatusBgColor = Color.FromArgb(245, 245, 245),
				HeaderBgColor = Color.FromArgb(230, 230, 230),
				ChannelStripeBgColor = Color.FromArgb(250, 250, 250)
			};
		}
	}
}
