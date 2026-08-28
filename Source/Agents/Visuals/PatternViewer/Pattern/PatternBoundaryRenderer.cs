//---------------------------------------------------------------------------------------
// <copyright file="PatternBoundaryRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Drawing;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Input parameters for pattern boundary rendering
	/// </summary>
	internal readonly struct PatternBoundaryInput
	{
		public int X
		{
			get;
			init;
		}

		public required int Y
		{
			get;
			init;
		}

		public required int Row
		{
			get;
			init;
		}

		public required int DisplaySongPosition
		{
			get;
			init;
		}

		public SongPatternViewData DisplaySongPattern
		{
			get;
			init;
		}

		public SongPatternViewData ActualPattern
		{
			get;
			init;
		}

		public required int ActualRow
		{
			get;
			init;
		}

		public required int RowHeight
		{
			get;
			init;
		}

		public required bool RollingPatterns
		{
			get;
			init;
		}

		public required List<SongPatternViewData> SongData
		{
			get;
			init;
		}

		public required Pen PatternBoundaryPen
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Renders pattern boundary lines when rolling patterns is enabled
	/// </summary>
	internal static class PatternBoundaryRenderer
	{
		/********************************************************************/
		/// <summary>
		/// Draw pattern boundaries (horizontal lines at pattern start/end)
		/// </summary>
		/********************************************************************/
		internal static void Draw(Graphics g, Rectangle contentRect, PatternBoundaryInput input)
		{
			if (!input.RollingPatterns)
			{
				return;
			}

			int panelWidth = contentRect.Width;

			if (input.ActualPattern != null && input.ActualRow == 0)
			{
				RollingPatternInfo prevInfo = PatternRenderingHelpers.GetRollingPatternInfo(input.DisplaySongPosition, input.Row - 1,
					input.DisplaySongPattern, input.SongData, input.RollingPatterns);
				if (prevInfo.SongPattern == null || prevInfo.SongPattern != input.ActualPattern)
				{
					g.DrawLine(input.PatternBoundaryPen, 0, input.Y, panelWidth, input.Y);
				}
			}

			if (input.ActualPattern != null && input.ActualRow == input.ActualPattern.RowCount - 1)
			{
				RollingPatternInfo nextInfo = PatternRenderingHelpers.GetRollingPatternInfo(input.DisplaySongPosition, input.Row + 1,
					input.DisplaySongPattern, input.SongData, input.RollingPatterns);
				if ((nextInfo.IsAtBoundary && nextInfo.Row == 0) || nextInfo.SongPattern == null)
				{
					g.DrawLine(input.PatternBoundaryPen, 0, input.Y + input.RowHeight, panelWidth, input.Y + input.RowHeight);
				}
			}
		}
	}
}
